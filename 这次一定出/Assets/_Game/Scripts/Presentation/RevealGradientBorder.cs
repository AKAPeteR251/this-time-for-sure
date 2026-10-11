using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
namespace ThisTime.Game
{
    // Sprite alpha preserves the authored open frame; vertex colors animate independently of text.
    public sealed class RevealGradientBorder : Image
    {
        public float speed=.12f;
        public float blinkStrength,blinkFrequency=1.6f;
        public float Phase {get;private set;}
        public float Pulse {get;private set;}
        private void Update(){Phase=Mathf.Repeat(Time.unscaledTime*speed,1);Pulse=(Mathf.Sin(Time.unscaledTime*blinkFrequency*Mathf.PI*2)+1)*.5f;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(sprite==null)return;
            Rect r=rectTransform.rect;Vector4 uv=DataUtility.GetOuterUV(sprite);
            const int columns=32;
            for(int i=0;i<columns;i++)
            {
                float a=i/(float)columns,b=(i+1)/(float)columns;
                Color ca=Tint(a),cb=Tint(b);int n=vh.currentVertCount;
                vh.AddVert(new Vector3(Mathf.Lerp(r.xMin,r.xMax,a),r.yMin),ca,new Vector2(Mathf.Lerp(uv.x,uv.z,a),uv.y));
                vh.AddVert(new Vector3(Mathf.Lerp(r.xMin,r.xMax,a),r.yMax),ca,new Vector2(Mathf.Lerp(uv.x,uv.z,a),uv.w));
                vh.AddVert(new Vector3(Mathf.Lerp(r.xMin,r.xMax,b),r.yMax),cb,new Vector2(Mathf.Lerp(uv.x,uv.z,b),uv.w));
                vh.AddVert(new Vector3(Mathf.Lerp(r.xMin,r.xMax,b),r.yMin),cb,new Vector2(Mathf.Lerp(uv.x,uv.z,b),uv.y));
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
        private Color Tint(float x)
        {
            Color[] colors={new Color(.25f,.9f,1),new Color(.65f,.48f,1),new Color(1,.52f,.78f),new Color(1,.87f,.46f),new Color(.25f,.9f,1)};
            float t=Mathf.Repeat(x+Phase,1)*4;int i=Mathf.FloorToInt(t);Color c=Color.Lerp(colors[i],colors[i+1],t-i);
            c=Color.Lerp(c,Color.white,Pulse*blinkStrength*.3f);c.a=color.a*(1-blinkStrength+Pulse*blinkStrength);return c;
        }
    }
}
