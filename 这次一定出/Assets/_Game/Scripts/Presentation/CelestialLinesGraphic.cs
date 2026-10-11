using UnityEngine;
using UnityEngine.UI;

namespace ThisTime.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CelestialLinesGraphic : MaskableGraphic
    {
        public float Progress { get; private set; }
        public void Animate(float progress) { Progress = Mathf.Clamp01(progress); SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); Rect r = rectTransform.rect;
            Vector2 center = r.center + Vector2.up * r.height * .06f;
            float p = Progress;
            float rise = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.06f, .38f, p));
            float ribbonAlpha = Mathf.Sin(rise * Mathf.PI) * color.a;
            Vector2 previous = new Vector2(center.x, r.yMin + r.height * .27f);
            for (int i = 1; i <= 28; i++)
            {
                float u = i / 28f * rise;
                Vector2 point = new Vector2(center.x + Mathf.Sin(u * Mathf.PI * 2) * r.width * .045f * (1-u),
                    Mathf.Lerp(r.yMin + r.height * .27f, center.y + r.height * .2f, u));
                Line(mesh, previous, point, 3 + 5 * (1-u), new Color(1, .79f, .30f, ribbonAlpha)); previous = point;
            }
        }
        private static void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
        {
            Vector2 normal = new Vector2(-(b-a).y, (b-a).x).normalized * width * .5f;
            int n = mesh.currentVertCount;
            mesh.AddVert(a-normal,tint,Vector2.zero); mesh.AddVert(a+normal,tint,Vector2.zero);
            mesh.AddVert(b+normal,tint,Vector2.zero); mesh.AddVert(b-normal,tint,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2); mesh.AddTriangle(n,n+2,n+3);
        }
    }
}
