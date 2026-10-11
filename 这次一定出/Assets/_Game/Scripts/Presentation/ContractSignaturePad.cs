using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThisTime.Game
{
    // 一笔指针轨迹：不做识字、评分或签名持久化。
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ContractSignaturePad : MaskableGraphic, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private readonly List<Vector2> points = new List<Vector2>();
        private bool drawing;
        private int pointerId;
        public bool AcceptInput = true;
        public float penWidth = 4;
        public Action StrokeAccepted;
        public IReadOnlyList<Vector2> Points { get { return points.AsReadOnly(); } }
        public void ClearStroke() { drawing = false; points.Clear(); SetVerticesDirty(); }
        public void CopyStroke(IReadOnlyList<Vector2> source)
        { ClearStroke(); for (int i = 0; i < source.Count; i++) points.Add(source[i]); SetVerticesDirty(); }
        public void OnPointerDown(PointerEventData data)
        {
            if (!AcceptInput || data.button != PointerEventData.InputButton.Left) return;
            ClearStroke(); drawing = true; pointerId = data.pointerId; AddPoint(data);
        }
        public void OnDrag(PointerEventData data)
        { if (drawing && AcceptInput && data.pointerId == pointerId) AddPoint(data); }
        public void OnPointerUp(PointerEventData data)
        {
            if (!drawing || !AcceptInput || data.pointerId != pointerId) return;
            AddPoint(data); drawing = false;
            if (points.Count > 0 && StrokeAccepted != null) StrokeAccepted();
        }
        private void AddPoint(PointerEventData data)
        {
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out local)) return;
            Rect rect = rectTransform.rect;
            Vector2 point = new Vector2(Mathf.Clamp01((local.x - rect.xMin) / Mathf.Max(1, rect.width)),
                Mathf.Clamp01((local.y - rect.yMin) / Mathf.Max(1, rect.height)));
            if (points.Count == 0 || Vector2.Distance(points[points.Count - 1], point) > 0.001f) points.Add(point);
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            // 空白时也保留透明区域的网格边界，让 GraphicRaycaster 可以落笔。
            Rect area = rectTransform.rect;
            mesh.AddVert(new Vector2(area.xMin, area.yMin), Color.clear, Vector2.zero);
            mesh.AddVert(new Vector2(area.xMin, area.yMax), Color.clear, Vector2.zero);
            mesh.AddVert(new Vector2(area.xMax, area.yMax), Color.clear, Vector2.zero);
            mesh.AddVert(new Vector2(area.xMax, area.yMin), Color.clear, Vector2.zero);
            mesh.AddTriangle(0, 1, 2); mesh.AddTriangle(0, 2, 3);
            Rect rect = rectTransform.rect;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 a = rect.min + Vector2.Scale(points[i], rect.size);
                Vector2 b = i + 1 < points.Count ? rect.min + Vector2.Scale(points[i + 1], rect.size) : a + Vector2.right * penWidth;
                Vector2 direction = (b - a).normalized;
                if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
                Vector2 normal = new Vector2(-direction.y, direction.x) * penWidth * 0.5f;
                int index = mesh.currentVertCount;
                mesh.AddVert(a - normal, color, Vector2.zero); mesh.AddVert(a + normal, color, Vector2.zero);
                mesh.AddVert(b + normal, color, Vector2.zero); mesh.AddVert(b - normal, color, Vector2.zero);
                mesh.AddTriangle(index, index + 1, index + 2); mesh.AddTriangle(index, index + 2, index + 3);
            }
        }
    }
}

