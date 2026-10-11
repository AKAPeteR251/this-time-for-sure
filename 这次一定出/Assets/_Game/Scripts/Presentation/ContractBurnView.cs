using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThisTime.Game
{
    // 所有动态字迹与独立印面使用同一纸面坐标烧穿，禁止只烧底图留下文字。
    public sealed class ContractBurnView : MonoBehaviour
    {
        private readonly List<Material> materials=new List<Material>();
        private RectTransform paper;
        private ContractAshGraphic ash;
        private P04FeelConfig cfg;
        public float Progress {get;private set;}
        public void Build(P04FeelConfig config)
        {
            cfg=config;paper=(RectTransform)transform;
            foreach(var graphic in GetComponentsInChildren<Graphic>(true))
            {
                var material=new Material(cfg.contractBurnShader){name="P26_"+graphic.name};
                material.SetFloat("_Glow",graphic.name=="Paper"?1:0);
                graphic.material=material;materials.Add(material);
            }
            var r=new GameObject("ContractAsh",typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(paper,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            ash=r.gameObject.AddComponent<ContractAshGraphic>();ash.raycastTarget=false;
            Tick(0,0);
        }
        public void Tick(float progress,float seconds)
        {
            Progress=Mathf.Clamp01(progress);
            var corners=new Vector3[4];paper.GetWorldCorners(corners);
            var canvas=paper.GetComponentInParent<Canvas>();
            Camera camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            Vector2 min=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
            Vector2 right=RectTransformUtility.WorldToScreenPoint(camera,corners[3])-min;
            Vector2 up=RectTransformUtility.WorldToScreenPoint(camera,corners[1])-min;
            Vector2 origin=new Vector2(min.x/Screen.width,min.y/Screen.height);
            right=new Vector2(right.x/Screen.width,right.y/Screen.height);up=new Vector2(up.x/Screen.width,up.y/Screen.height);
            float determinant=right.x*up.y-right.y*up.x;
            if(Mathf.Abs(determinant)<.000001f)determinant=.000001f;
            foreach(var m in materials)
            {
                m.SetVector("_PaperScreenRect",new Vector4(origin.x,origin.y,0,0));
                m.SetVector("_PaperScreenAxisX",new Vector4(up.y/determinant,-up.x/determinant,0,0));
                m.SetVector("_PaperScreenAxisY",new Vector4(-right.y/determinant,right.x/determinant,0,0));
                m.SetFloat("_Burn",Progress);m.SetFloat("_BurnTime",seconds);m.SetFloat("_Edge",cfg.contractBurnEdge);
            }
            ash.Animate(Progress,seconds,cfg.contractBurnSeconds);
        }
        public void Clear(){Tick(0,0);ash.Animate(0,0,cfg.contractBurnSeconds);}
        private void OnDestroy(){foreach(var m in materials)if(m!=null)Destroy(m);}
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ContractAshGraphic : MaskableGraphic
    {
        private float progress,seconds,duration;
        public void Animate(float p,float t,float d){progress=p;seconds=t;duration=d;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(progress<=0||progress>=1)return;
            Rect r=rectTransform.rect;
            for(int i=0;i<62;i++)
            {
                float seed=Mathf.Repeat(i*.618034f,.98f);
                float birth=(.06f+Mathf.Repeat(i*.371f,.84f))*duration;
                float age=seconds-birth,lifetime=.45f+seed*.5f;
                if(age<0||age>lifetime)continue;
                float u=Mathf.Repeat(i*.754877f,.92f)+.04f;
                float y=Mathf.Lerp(-.14f,1.18f,birth/duration);
                Vector2 pos=r.min+Vector2.Scale(new Vector2(u,y),r.size);
                pos+=new Vector2(Mathf.Sin(i*2.4f+age*5)*age*34,age*(80+seed*95));
                float size=1.5f+seed*3;float a=Mathf.Sin(age/lifetime*Mathf.PI)*.8f;
                Color tint=i%4==0?new Color(1,.45f,.08f,a):new Color(.20f,.14f,.09f,a);
                Vector2 x=new Vector2(Mathf.Cos(age*9+i),Mathf.Sin(age*9+i))*size;
                Vector2 z=new Vector2(-x.y,x.x)*.65f;int n=mesh.currentVertCount;
                mesh.AddVert(pos-x,tint,Vector2.zero);mesh.AddVert(pos+z,tint,Vector2.zero);
                mesh.AddVert(pos+x,tint,Vector2.zero);mesh.AddVert(pos-z,tint,Vector2.zero);
                mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
