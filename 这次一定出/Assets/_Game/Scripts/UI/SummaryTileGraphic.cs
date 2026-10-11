using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
namespace ThisTime.Game
{
    // All layers share the same slight shear; no texture resampling or new portrait assets.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SummaryTileGraphic : MaskableGraphic
    {
        public Sprite artwork;
        public Rect crop = new Rect(.18f,.56f,.64f,.42f);
        public Rect area = new Rect(0,0,1,1);
        protected override void OnEnable() { base.OnEnable(); SetAllDirty(); }
        public override Texture mainTexture { get { return artwork != null ? artwork.texture : Texture2D.whiteTexture; } }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect uv=new Rect(0,0,1,1);
            if(artwork!=null)
            {
                Rect c=crop;
                float desired=rectTransform.rect.width*.86f*area.width/(rectTransform.rect.height*area.height);
                float aspect=artwork.rect.width*c.width/(artwork.rect.height*c.height);
                if(aspect>desired){float w=c.width*desired/aspect;c.x+=(c.width-w)*.5f;c.width=w;}
                else{float h=c.height*aspect/desired;c.y+=(c.height-h)*.5f;c.height=h;}
                Vector4 full=DataUtility.GetOuterUV(artwork);
                uv=new Rect(Mathf.Lerp(full.x,full.z,c.x),Mathf.Lerp(full.y,full.w,c.y),c.width*(full.z-full.x),c.height*(full.w-full.y));
            }
            vh.AddVert(Point(area.xMin,area.yMin),color,new Vector2(uv.xMin,uv.yMin));
            vh.AddVert(Point(area.xMin,area.yMax),color,new Vector2(uv.xMin,uv.yMax));
            vh.AddVert(Point(area.xMax,area.yMax),color,new Vector2(uv.xMax,uv.yMax));
            vh.AddVert(Point(area.xMax,area.yMin),color,new Vector2(uv.xMax,uv.yMin));
            vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);
        }
        private Vector3 Point(float x,float y){Rect r=rectTransform.rect;return new Vector3(r.xMin+r.width*(x*.86f+y*.14f),r.yMin+r.height*y);}
    }
}
