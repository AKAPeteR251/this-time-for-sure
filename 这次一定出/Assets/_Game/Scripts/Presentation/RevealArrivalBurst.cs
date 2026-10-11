using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    // Stateless normalized reveal time: animation completion/skip clears both waves immediately.
    public sealed class RevealArrivalBurst : MonoBehaviour
    {
        private P04FeelConfig config;
        private Material smallMaterial,bigMaterial;
        private RawImage smallLight,bigLight;
        private Image[] pieces,trails;
        private Color tint;
        public float SmallPulse {get;private set;}
        public float BigPulse {get;private set;}
        public float ActiveAlpha {get;private set;}
        public float FragmentAlpha {get;private set;}
        public void Build(P04FeelConfig cfg)
        {
            config=cfg;
            smallLight=Light("SilhouetteInnerLight",out smallMaterial);
            bigLight=Light("CharacterInnerLight",out bigMaterial);
            pieces=new Image[52];trails=new Image[52];
            for(int i=0;i<pieces.Length;i++)
            {
                trails[i]=ImageAt("BurstTrail"+i,cfg.revealBurstStreak!=null?cfg.revealBurstStreak:cfg.starSoft);
                pieces[i]=ImageAt("BurstShard"+i,cfg.revealBurstShards!=null&&cfg.revealBurstShards.Length>0?cfg.revealBurstShards[i%cfg.revealBurstShards.Length]:cfg.revealShard);
            }
            Clear();
        }
        public void Begin(Color color){tint=color;Animate(0);}
        public void Animate(float progress)
        {
            float s=Mathf.InverseLerp(config.silhouetteBurstStart,config.silhouetteBurstEnd,progress);
            float b=Mathf.Clamp01((progress-config.characterBurstStart)/config.characterBurstSpan);
            SmallPulse=Pulse(s);BigPulse=progress<config.characterBurstStart?0:Pulse(b);
            ActiveAlpha=Mathf.Max(SmallPulse,BigPulse);
            DrawLight(smallLight,smallMaterial,s,SmallPulse,config.silhouetteBurstRadius);
            DrawLight(bigLight,bigMaterial,b,BigPulse,config.characterBurstRadius);
            FragmentAlpha=0;
            float aspect=Mathf.Max(1,((RectTransform)transform).rect.width)/Mathf.Max(1,((RectTransform)transform).rect.height);
            for(int i=0;i<pieces.Length;i++)
            {
                bool large=i>=20;int n=large?i-20:i;int count=large?32:20;
                float phase=large?b:s;
                float p=Mathf.Clamp01(phase/config.revealShardTravelFraction);
                float alpha=(large&&progress<config.characterBurstStart)?0:Mathf.SmoothStep(0,1,p/.06f)*(1-Mathf.SmoothStep(.12f,1,p));
                FragmentAlpha=Mathf.Max(FragmentAlpha,alpha);
                float angle=(n+Hash(i,3)*.65f)*Mathf.PI*2/count;
                float radius=Mathf.Lerp(large?.32f:.28f,large?1.25f:1.02f,1-Mathf.Pow(1-p,2))*(.78f+Hash(i,4)*.34f);
                Vector2 center=new Vector2(.535f,.52f);
                Vector2 anchor=center+new Vector2(Mathf.Cos(angle)/aspect,Mathf.Sin(angle))*radius;
                float length=(large?205:145)*( .65f+Hash(i,7)*.75f)*(1-p*.45f);
                Place(pieces[i],anchor,new Vector2(length*.34f,length*(1+p*.9f)),angle*Mathf.Rad2Deg-90+p*(Hash(i,9)-.5f)*60);
                pieces[i].color=new Color(.65f+tint.r*.35f,.68f+tint.g*.32f,.75f+tint.b*.25f,alpha*.55f*config.revealBurstStrength);
                Vector2 tail=anchor-new Vector2(Mathf.Cos(angle)/aspect,Mathf.Sin(angle))*(large?.035f:.02f);
                Place(trails[i],tail,new Vector2(length*.55f,length*3.6f),angle*Mathf.Rad2Deg-90);
                trails[i].color=new Color(tint.r,tint.g,tint.b,alpha*.75f*config.revealBurstStrength);
            }
        }
        private void DrawLight(RawImage image,Material material,float phase,float pulse,float radius)
        {
            if(material==null){image.color=Color.clear;return;}
            // The face close-up is larger at the start of the second wave: move its light out too.
            material.SetFloat("_Gap",config.revealLightGap+(material==bigMaterial?.12f*(1-phase):0));
            material.SetFloat("_Softness",config.revealLightSoftness);material.SetFloat("_Scatter",config.revealLightScatter);
            material.SetFloat("_Phase",phase);material.SetFloat("_Radius",radius);
            material.SetFloat("_Aspect",Mathf.Max(1,((RectTransform)transform).rect.width)/Mathf.Max(1,((RectTransform)transform).rect.height));
            material.SetColor("_SignalColor",tint);image.color=new Color(1,1,1,pulse*config.revealBurstStrength);
        }
        private static float Pulse(float t){return Mathf.SmoothStep(0,1,t/.16f)*(1-Mathf.SmoothStep(.28f,1,t));}
        public void Clear()
        {
            SmallPulse=BigPulse=ActiveAlpha=FragmentAlpha=0;
            if(smallLight!=null)smallLight.color=bigLight.color=Color.clear;
            if(pieces!=null)for(int i=0;i<pieces.Length;i++)pieces[i].color=trails[i].color=Color.clear;
        }
        private RawImage Light(string name,out Material material)
        {
            var image=Rect(name).gameObject.AddComponent<RawImage>();image.raycastTarget=false;image.texture=Texture2D.whiteTexture;
            material=config.revealArrivalShader==null?null:new Material(config.revealArrivalShader);
            image.material=material;return image;
        }
        private Image ImageAt(string name,Sprite sprite){var i=Rect(name).gameObject.AddComponent<Image>();i.sprite=sprite;i.raycastTarget=false;return i;}
        private RectTransform Rect(string name){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        private static void Place(Image image,Vector2 anchor,Vector2 size,float angle){var r=image.rectTransform;r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=Vector2.zero;r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);}
        private static float Hash(int i,int s){return Mathf.Repeat(Mathf.Sin(i*17.13f+s*31.7f)*4387.23f,1);}
        private void OnDestroy(){if(smallMaterial!=null)Destroy(smallMaterial);if(bigMaterial!=null)Destroy(bigMaterial);}
    }
}
