using UnityEngine;
using UnityEngine.UI;

namespace ThisTime.Game
{
    // 无立绘的角色使用同一个中性轮廓，角色名和稀有度仍由卡面显示。
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UICharacterSilhouette : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            Ellipse(mesh, bounds, new Vector2(0.5f, 0.70f), new Vector2(0.20f, 0.22f));
            Ellipse(mesh, bounds, new Vector2(0.5f, 0.13f), new Vector2(0.45f, 0.39f));
        }
        private void Ellipse(VertexHelper mesh, Rect bounds, Vector2 center, Vector2 radius)
        {
            int start = mesh.currentVertCount;
            Add(mesh, bounds, center);
            const int segments = 48;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                Add(mesh, bounds, center + new Vector2(Mathf.Cos(angle) * radius.x, Mathf.Sin(angle) * radius.y));
                if (i > 0) mesh.AddTriangle(start, start + i, start + i + 1);
            }
        }
        private void Add(VertexHelper mesh, Rect bounds, Vector2 point)
        {
            point = new Vector2(Mathf.Clamp01(point.x), Mathf.Clamp01(point.y));
            mesh.AddVert(new Vector3(bounds.xMin + bounds.width * point.x, bounds.yMin + bounds.height * point.y), color, Vector2.zero);
        }
    }
}
