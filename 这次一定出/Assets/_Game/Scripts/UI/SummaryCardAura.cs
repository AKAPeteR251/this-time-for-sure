using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SummaryCardAura : MaskableGraphic
    {
        public Color auraColor=new Color(1,.35f,.74f);
        public float strength=.75f,width=.28f;
        public float Motion {get;private set;}
        private void Update(){Motion=Time.unscaledTime*.12f;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();Vector2[] corners={new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),new Vector2(0,0)};
            for(int band=0;band<16;band++)for(int side=0;side<4;side++)
            {
                float a=band/16f,b=(band+1)/16f;
                Color ca=auraColor,cb=auraColor;
                float breath=.88f+.12f*Mathf.Sin(Time.unscaledTime*1.4f);
                ca.a=strength*breath*Mathf.Pow(1-a,2)*.32f;cb.a=strength*breath*Mathf.Pow(1-b,2)*.32f;
                Quad(mesh,Expand(corners[side],1+a*width),Expand(corners[side+1],1+a*width),Expand(corners[side+1],1+b*width),Expand(corners[side],1+b*width),ca,cb);
            }
            Rect r=rectTransform.rect;
            for(int i=0;i<10;i++)
            {
                float travel=Mathf.Repeat(i*.391f+Motion*.19f,1)*4;int side=Mathf.FloorToInt(travel);
                Vector2 edge=Vector2.Lerp(corners[side],corners[side+1],travel-side);
                Vector3 centre=Expand(edge,1.12f+.09f*Mathf.Sin(Motion*2+i*1.7f));
                float size=Mathf.Min(r.width,r.height)*(.013f+.006f*Mathf.Sin(i*3.1f));
                float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*.9f+i*2.3f);
                Color white=new Color(1,1,1,.30f+.65f*pulse);int n=mesh.currentVertCount;
                mesh.AddVert(centre+new Vector3(-size,0),white,Vector2.zero);
                mesh.AddVert(centre+new Vector3(0,size*1.6f),white,Vector2.zero);
                mesh.AddVert(centre+new Vector3(size,0),white,Vector2.zero);
                mesh.AddVert(centre-new Vector3(0,size*1.6f),white,Vector2.zero);
                mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            }
        }
        private Vector3 Expand(Vector2 p,float scale)
        {Rect r=rectTransform.rect;return new Vector3(r.xMin+r.width*(.5f+(p.x*.86f+p.y*.14f-.5f)*scale),r.yMin+r.height*(.5f+(p.y-.5f)*scale));}
        private void Quad(VertexHelper mesh,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color inner,Color outer)
        {int n=mesh.currentVertCount;mesh.AddVert(a,inner,Vector2.zero);mesh.AddVert(b,inner,Vector2.zero);mesh.AddVert(c,outer,Vector2.zero);mesh.AddVert(d,outer,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);}
    }
}
