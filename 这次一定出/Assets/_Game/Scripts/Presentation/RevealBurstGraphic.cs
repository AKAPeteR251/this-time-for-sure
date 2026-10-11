using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RevealBurstGraphic : MaskableGraphic
    {
        public float progress;
        public bool showHalo=true;
        public Color signal=new Color(.5f,.7f,1);
        public void Animate(float p){progress=p;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();Rect rect=rectTransform.rect;Vector2 center=rect.min+Vector2.Scale(rect.size,new Vector2(.61f,.55f));
            float unit=Mathf.Min(rect.width,rect.height);
            for(int i=0;i<64;i++)
            {
                float a=i*Mathf.PI*2/64,b=(i+1)*Mathf.PI*2/64;
                Tri(v,center,center+Dir(a)*unit*.95f,center+Dir(b)*unit*.95f,new Color(.16f,.22f,.35f,.75f),new Color(.025f,.04f,.08f,0));
            }
            // 原创不规则放射面；中心亮、外缘黑，衬托剪影与角色。
            float reveal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.18f,.45f,progress));
            for(int i=0;i<(showHalo?56:0);i++)
            {
                float a=i*Mathf.PI*2/56,b=(i+1)*Mathf.PI*2/56;
                float r=unit*(.32f+.11f*Mathf.Sin(i*7.13f));
                float rr=unit*(.32f+.11f*Mathf.Sin((i+1)*7.13f));
                Tri(v,center,center+Dir(a)*r,center+Dir(b)*rr,new Color(.95f,.98f,1,.25f+reveal*.45f),new Color(.58f,.72f,.90f,.12f+reveal*.32f));
            }
            float travel=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.8f,progress));
            for(int i=0;i<32;i++)
            {
                float a=i*2.399963f;float distance=unit*Mathf.Lerp(.05f,.55f+(i%4)*.08f,travel);
                Vector2 p=center+Dir(a)*distance;Vector2 along=Dir(a+.18f)*unit*(.025f+(i%3)*.008f),side=Dir(a+Mathf.PI*.5f)*unit*.008f;
                Color col=new Color(.58f+signal.r*.3f,.68f+signal.g*.2f,.80f+signal.b*.15f,(1-Mathf.InverseLerp(.62f,1,progress))*.8f);
                Tri(v,p-along,p+side,p+along,col,col);Tri(v,p-along,p+along,p-side,col,new Color(col.r*.45f,col.g*.55f,col.b*.7f,col.a));
            }
            for(int i=0;i<75;i++)
            {
                float x=Mathf.Repeat(i*.618034f,1),y=Mathf.Repeat(i*.414214f,1);Vector2 p=rect.min+Vector2.Scale(rect.size,new Vector2(x,y));
                float s=unit*(i%7==0?.003f:.0012f);Color c=new Color(.6f,.73f,.95f,.2f+(i%4)*.11f);
                Tri(v,p+Vector2.up*s,p+Vector2.left*s,p+Vector2.down*s,c,c);Tri(v,p+Vector2.up*s,p+Vector2.down*s,p+Vector2.right*s,c,c);
            }
        }
        private static Vector2 Dir(float a){return new Vector2(Mathf.Cos(a),Mathf.Sin(a));}
        private static void Tri(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color ca,Color cb){int n=v.currentVertCount;v.AddVert(a,ca,Vector2.zero);v.AddVert(b,cb,Vector2.zero);v.AddVert(c,cb,Vector2.zero);v.AddTriangle(n,n+1,n+2);}
    }
}
