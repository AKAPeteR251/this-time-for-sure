using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SummaryRainbowFrame : MaskableGraphic
    {
        public float speed=.35f,blinkStrength=.32f,blinkFrequency=1.6f;
        public float Phase {get;private set;}
        public float Pulse {get;private set;}
        private void Update(){Phase=Mathf.Repeat(Time.unscaledTime*speed,1);Pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*blinkFrequency*Mathf.PI*2);SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Vector2[] outside={new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),new Vector2(0,0)};
            Vector2[] inside={new Vector2(.022f,.019f),new Vector2(.022f,.981f),new Vector2(.978f,.981f),new Vector2(.978f,.019f),new Vector2(.022f,.019f)};
            for(int side=0;side<4;side++)for(int i=0;i<16;i++)
            {
                float a=i/16f,b=(i+1)/16f;int n=mesh.currentVertCount;
                Color ca=Tint((side+a)*.25f),cb=Tint((side+b)*.25f);
                mesh.AddVert(Point(Vector2.Lerp(outside[side],outside[side+1],a)),ca,Vector2.zero);
                mesh.AddVert(Point(Vector2.Lerp(outside[side],outside[side+1],b)),cb,Vector2.zero);
                mesh.AddVert(Point(Vector2.Lerp(inside[side],inside[side+1],b)),cb,Vector2.zero);
                mesh.AddVert(Point(Vector2.Lerp(inside[side],inside[side+1],a)),ca,Vector2.zero);
                mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            }
        }
        private Vector3 Point(Vector2 p){Rect r=rectTransform.rect;return new Vector3(r.xMin+r.width*(p.x*.86f+p.y*.14f),r.yMin+r.height*p.y);}
        private Color Tint(float t)
        {
            Color c=Color.HSVToRGB(Mathf.Repeat(t+Phase,1),.62f,1);
            c=Color.Lerp(c,Color.white,Pulse*blinkStrength*.45f);c.a=color.a*(1-blinkStrength+Pulse*blinkStrength);return c;
        }
    }
}
