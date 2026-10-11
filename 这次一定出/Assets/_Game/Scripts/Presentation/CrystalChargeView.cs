using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    public sealed class CrystalChargeView
    {
        public readonly RectTransform Root;
        private readonly RectTransform crystal;
        private readonly Image background,intact,cracks,deep,leak,rupture,white;
        private readonly Text declaration;
        private readonly P04FeelConfig cfg;
        private readonly CrystalStage3D stage3D;private readonly CanvasGroup entryAlpha;
        public CrystalStage3D Stage3D {get{return stage3D;}}
        public bool Uses3D {get{return stage3D!=null;}}
        public float TransitionAlpha {get{return entryAlpha.alpha;}}
        private Color signal;
        private float lastHit=-100,lastTick;
        public float Energy {get;private set;}
        public float ShakeAmplitude {get;private set;}
        public int AcceptedHits {get;private set;}
        public float WhiteAlpha {get{return white.color.a;}}
        public bool Contains(Vector2 screen,Camera camera){return stage3D!=null?stage3D.Contains(screen,camera):RectTransformUtility.RectangleContainsScreenPoint(crystal,screen,camera);}
        public CrystalChargeView(RectTransform parent,P04FeelConfig config,Font font)
        {
            cfg=config;Root=Rect(parent,"CrystalStage",0,0,1,1);
            entryAlpha=Root.gameObject.AddComponent<CanvasGroup>();entryAlpha.blocksRaycasts=false;
            if(cfg.crystalStageModel!=null)
            {
                if(cfg.altarEnvironmentModel==null){var sky=Rect(Root,"CrystalLivingSky",0,0,1,1).gameObject.AddComponent<RevealSky>();sky.Build(cfg);sky.Animate(1);}
                stage3D=Rect(Root,"CrystalReal3D",0,0,1,1).gameObject.AddComponent<CrystalStage3D>();stage3D.Build(cfg);
            }
            background=Img(Root,"GiantRingsV004",0,0,1,1,cfg.crystalBackground);background.preserveAspect=false;
            crystal=Rect(Root,"CrystalBody",.22f,.10f,.78f,.96f);
            intact=Img(crystal,"Intact",0,0,1,1,cfg.crystalIntact);
            cracks=Img(crystal,"Cracks",0,0,1,1,cfg.crystalCracks);
            deep=Img(crystal,"DeepCracks",0,0,1,1,cfg.crystalDeep);
            leak=Img(crystal,"GoldenLeak",0,0,1,1,cfg.crystalLeak);
            rupture=Img(crystal,"Rupture",0,0,1,1,null);
            white=Img(Root,"CrystalWhite",0,0,1,1,null);
            declaration=Rect(Root,"FinalDeclaration",.14f,.05f,.84f,.15f).gameObject.AddComponent<Text>();
            declaration.font=font;declaration.fontSize=37;declaration.alignment=TextAnchor.MiddleCenter;declaration.raycastTarget=false;
            Clear();
        }
        public void Begin(CardRarity rarity)
        {
            Root.gameObject.SetActive(true);entryAlpha.alpha=1;background.enabled=stage3D==null;Energy=0;AcceptedHits=0;lastHit=-100;lastTick=Time.unscaledTime;
            signal=rarity==CardRarity.UR?new Color(1,.42f,.17f):rarity==CardRarity.SSR?new Color(1,.827f,.431f):rarity==CardRarity.SR?new Color(.718f,.518f,1):new Color(.459f,.788f,1);
            declaration.text="";rupture.color=white.color=Color.clear;if(stage3D!=null)stage3D.Begin(signal);Tick();
        }
        public void BeginPreview(CardRarity rarity){Begin(rarity);Transition(0);}
        public void Transition(float progress)
        {
            entryAlpha.alpha=cfg.crystalPortalCamera&&cfg.crystalPortalShader!=null?1:Mathf.SmoothStep(0,1,progress);
            if(stage3D!=null)stage3D.SetEntry(progress);
        }
        public float CeremonyFade(float progress)
        {return cfg.crystalPortalCamera&&cfg.crystalPortalShader!=null?1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.60f,.94f,progress)):1-entryAlpha.alpha;}
        public bool Hit()
        {
            if(Energy>=1)return false;
            float now=Time.unscaledTime,interval=now-lastHit;
            float combo=Mathf.Lerp(1,cfg.comboMultiplier,1-Mathf.Clamp01(interval/cfg.comboWindow));
            Energy=Mathf.Min(1,Energy+cfg.hitEnergy*combo);AcceptedHits++;lastHit=now;Tick();return true;
        }
        public void Tick()
        {
            float t=Time.unscaledTime,age=t-lastHit;
            float pulse=Mathf.Clamp01(1-age/cfg.hitShakeSeconds);
            ShakeAmplitude=Mathf.Lerp(2,cfg.crystalShakePixels,Energy)*pulse;
            // Canvas参考1920；规格震幅以1280为准，换算1.5。
            crystal.anchoredPosition=new Vector2(Mathf.Sin(t*137)*ShakeAmplitude*1.5f,Mathf.Cos(t*113)*ShakeAmplitude*.75f+Mathf.Sin(t*.85f)*3);
            crystal.localScale=Vector3.one*(1+Energy*.065f+pulse*.018f);
            float c=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.18f,.45f,Energy));
            float d=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.45f,.78f,Energy));
            intact.color=new Color(1,1,1,1-c);cracks.color=new Color(1,1,1,c*(1-d));deep.color=new Color(1,1,1,d);
            leak.color=new Color(signal.r,signal.g,signal.b,Mathf.InverseLerp(.24f,1,Energy)*(.55f+Mathf.Sin(t*4)*.07f+pulse*.25f));
            background.rectTransform.localScale=Vector3.one*(1.08f+Energy*.08f);
            if(stage3D!=null){stage3D.Pose(Energy,ShakeAmplitude);intact.color=cracks.color=deep.color=leak.color=Color.clear;}
            lastTick=t;
        }
        public void Terminal(float seconds)
        {
            Tick();declaration.text="星门，为您而开！";declaration.color=new Color(1,.9f,.65f);
            crystal.localScale=Vector3.one*Mathf.Lerp(1.06f,.86f,Mathf.Clamp01(seconds/cfg.finalChargeSeconds));
            crystal.anchoredPosition=new Vector2(Mathf.Sin(seconds*155)*16,Mathf.Cos(seconds*121)*9);if(stage3D!=null)stage3D.Terminal(seconds);
        }
        public void Burst(float seconds)
        {
            background.enabled=false;if(stage3D!=null)stage3D.HideGeometry();
            intact.color=cracks.color=deep.color=leak.color=Color.clear;
            declaration.text="";crystal.localScale=Vector3.one;crystal.anchoredPosition=Vector2.zero;
            int index=Mathf.FloorToInt(seconds*cfg.ruptureFps);
            if(cfg.crystalRupture!=null&&index<cfg.crystalRupture.Length){rupture.sprite=cfg.crystalRupture[index];rupture.color=new Color(1,1,1,1-Mathf.InverseLerp(.2f,.85f,seconds));}else rupture.color=Color.clear;
            white.color=new Color(1,1,1,Mathf.Clamp01(1-seconds/cfg.whiteSeconds));
        }
        public void Clear(){if(stage3D!=null)stage3D.HideGeometry();Root.gameObject.SetActive(false);white.color=rupture.color=Color.clear;ShakeAmplitude=0;crystal.anchoredPosition=Vector2.zero;}
        private static RectTransform Rect(Transform p,string n,float x,float y,float xx,float yy){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(xx,yy);r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        private static Image Img(Transform p,string n,float x,float y,float xx,float yy,Sprite s){var i=Rect(p,n,x,y,xx,yy).gameObject.AddComponent<Image>();i.sprite=s;i.preserveAspect=true;i.raycastTarget=false;return i;}
    }
}
