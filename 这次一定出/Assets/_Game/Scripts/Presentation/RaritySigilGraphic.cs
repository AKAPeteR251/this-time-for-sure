using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RaritySigilGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Vector2 center=rectTransform.rect.center;float r=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.42f;
            for(int i=0;i<64;i++)
            {
                float a=i*Mathf.PI*2/64,b=(i+1)*Mathf.PI*2/64;
                Quad(vh,center+V(a)*r*.75f,center+V(b)*r*.75f,center+V(b)*r*.79f,center+V(a)*r*.79f,color);
                Quad(vh,center+V(a)*r*.89f,center+V(b)*r*.89f,center+V(b)*r*.91f,center+V(a)*r*.91f,color);
            }
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4;Vector2 tip=center+V(a)*r,side=center+V(a+.22f)*r*.26f,other=center+V(a-.22f)*r*.26f;
                Triangle(vh,center,side,tip,color);Triangle(vh,center,tip,other,new Color(1,.94f,.71f,color.a));
            }
            // 几何月桂翼，左右镜像，无参考商用徽记。
            for(int side=-1;side<=1;side+=2)for(int j=0;j<5;j++)
            {Vector2 p=center+new Vector2(side*r*(.67f+j*.09f),r*(-.5f+j*.2f));Quad(vh,p,p+new Vector2(side*r*.27f,r*.16f),p+new Vector2(side*r*.17f,r*.23f),p+Vector2.up*r*.07f,color);}
        }
        private static Vector2 V(float a){return new Vector2(Mathf.Cos(a),Mathf.Sin(a));}
        private static void Triangle(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color col){int n=v.currentVertCount;v.AddVert(a,col,Vector2.zero);v.AddVert(b,col,Vector2.zero);v.AddVert(c,col,Vector2.zero);v.AddTriangle(n,n+1,n+2);}
        private static void Quad(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color col){int n=v.currentVertCount;v.AddVert(a,col,Vector2.zero);v.AddVert(b,col,Vector2.zero);v.AddVert(c,col,Vector2.zero);v.AddVert(d,col,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
    }
}
