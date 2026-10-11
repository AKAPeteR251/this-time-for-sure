using UnityEngine;
using UnityEngine.UI;

namespace ThisTime.Game
{
    public sealed class ContractPaperMotion : MonoBehaviour
    {
        private RectTransform root;
        private CanvasGroup alpha;
        private P04FeelConfig cfg;
        private float born;
        public float Entry {get;private set;}
        public void Build(P04FeelConfig config)
        {cfg=config;root=(RectTransform)transform;alpha=gameObject.AddComponent<CanvasGroup>();born=Time.unscaledTime-config.contractArriveSeconds;ApplyNow();}
        public void BeginEntrance(){born=Time.unscaledTime;ApplyNow();}
        public void Adopt(ContractPaperMotion source)
        {born=source==null?Time.unscaledTime-cfg.contractArriveSeconds:source.born;ApplyNow();}
        private void LateUpdate(){ApplyNow();}
        public void ApplyNow()
        {
            if(cfg==null)return;
            float t=Time.unscaledTime;
            Entry=Mathf.Clamp01((t-born)/Mathf.Max(.1f,cfg.contractArriveSeconds));
            float ease=1-Mathf.Pow(1-Entry,3);
            float slow=t*cfg.contractFloatSpeed;
            var drift=new Vector2(Mathf.Sin(slow*.79f)*cfg.contractFloatPixels*.65f,Mathf.Sin(slow+1.2f)*cfg.contractFloatPixels);
            root.anchoredPosition=Vector2.Lerp(new Vector2(-230,-640),Vector2.zero,ease)+drift;
            root.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(-9,0,ease)+Mathf.Sin(slow*.63f)*.8f);
            root.localScale=Vector3.one;
            alpha.alpha=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.32f,Entry));
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ContractEmbersGraphic : MaskableGraphic
    {
        private P04FeelConfig cfg;
        public void Build(P04FeelConfig config){cfg=config;raycastTarget=false;}
        private void Update(){SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(cfg==null)return;Rect r=rectTransform.rect;float t=Time.unscaledTime;
            for(int i=0;i<cfg.contractEmberCount;i++)
            {
                float seed=Mathf.Repeat(i*.618034f+.17f,1),speed=.018f+seed*.019f;
                float cycle=Mathf.Repeat(t*speed+i*.381966f,1);
                float x=Mathf.Repeat(i*.754877f+.11f,1)+Mathf.Sin(t*.19f+i*2.1f)*.025f;
                Vector2 p=r.min+Vector2.Scale(new Vector2(x,cycle),r.size);
                float opacity=Mathf.Sin(cycle*Mathf.PI)*(.6f+.4f*Mathf.Sin(t*1.3f+i)*Mathf.Sin(t*1.3f+i))*cfg.contractEmberAlpha;
                float size=1.4f+seed*2.4f;
                int n=mesh.currentVertCount;var c=new Color(1,.3f+seed*.3f,.06f,opacity);
                mesh.AddVert(p,c,Vector2.zero);
                for(int k=0;k<8;k++){float a=k*Mathf.PI*.25f;mesh.AddVert(p+new Vector2(Mathf.Cos(a)*size,Mathf.Sin(a)*size*1.5f),new Color(c.r,c.g,c.b,0),Vector2.zero);}
                for(int k=0;k<8;k++)mesh.AddTriangle(n,n+1+k,n+1+(k+1)%8);
            }
        }
    }
}
