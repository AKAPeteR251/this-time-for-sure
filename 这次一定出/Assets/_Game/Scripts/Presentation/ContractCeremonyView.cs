using UnityEngine;
using UnityEngine.UI;

namespace ThisTime.Game
{
    // 所有动态图层/签名均由 Unity 实时合成，时间线只消费已确定批次的最高信号。
    public sealed class ContractCeremonyView
    {
        private readonly Image waxShadow;
        private readonly bool realistic;
        private readonly Vector4 signatureRect;
        private readonly Image stamp, giant, outer, inner, beam, impact, burst, flash, paperImage;
        private readonly Text declaration, signature;
        private readonly ContractSignaturePad ink;
        private readonly RectTransform paper, world;
        private readonly CanvasGroup paperAlpha;
        private readonly ContractBurnView burnView;
        private readonly ContractPaperMotion motion;
        private readonly RectTransform goldMask;
        private readonly CelestialLinesGraphic lines;
        private readonly P04FeelConfig config;
        private readonly SummonAudio audio;
        private float previousSeconds, duration;
        private Color signal;
        public float Progress { get; private set; }
        public ContractCeremonyView(RectTransform root, PrototypeUITheme theme, Font textFont, Font handwriting, P04FeelConfig config = null, SummonAudio audio = null)
        {
            this.config=config; this.audio=audio;
            if(config!=null)RectAt(root,"ContractEmbers",0,0,1,1).gameObject.AddComponent<ContractEmbersGraphic>().Build(config);
            world=RectAt(root,"RitualWorld",0,0,1,1);
            giant=Layer(world,"CelestialGiantRing",config==null?theme.sparkleGold:config.giantRing,.04f,-.08f,.96f,1.14f);
            outer=Layer(world,"CelestialOuterRing",config==null?theme.sparkleGold:config.outerRing,.12f,.03f,.88f,1.03f);
            inner=Layer(world,"CelestialInnerRing",config==null?theme.sparkleBlue:config.innerRing,.22f,.12f,.78f,.94f);
            beam=Layer(world,"StarGateLight",config==null?null:config.lightPillar,.34f,-.13f,.66f,1.22f);
            realistic=ContractPaperLayout.Available(config);
            signatureRect=realistic?config.contractSignatureRect:new Vector4(.10f,.16f,.90f,.41f);
            if(realistic)
            {
                var slot=RectAt(root,"RisingContractSlot",.205f,.085f,.795f,.91f);
                motion=slot.gameObject.AddComponent<ContractPaperMotion>();motion.Build(config);
                slot.SetSiblingIndex(world.GetSiblingIndex()+(config.contractBurnShader!=null?1:0));
                paper=ContractPaperLayout.Fit(slot,"RisingContract",config);
                paperAlpha=paper.gameObject.AddComponent<CanvasGroup>();
                paperImage=paper.Find("Paper").GetComponent<Image>();
            }
            else
            {
            paper=RectAt(root,"RisingContract",.205f,.085f,.795f,.91f);
            paper.SetSiblingIndex(world.GetSiblingIndex());
            paperAlpha=paper.gameObject.AddComponent<CanvasGroup>();
            paperImage=ImageAt(paper,"Paper",Color.white,0,0,1,1);
            paperImage.sprite=config==null?null:config.contractPaper;
            if(paperImage.sprite==null)paperImage.color=new Color(.96f,.92f,.78f,1);
            Image border=Layer(paper,"PaperBorder",config==null?null:config.contractBorder,0,0,1,1);border.preserveAspect=false;
            Text title=TextAt(paper,"ContractTitle",textFont,42,.09f,.73f,.91f,.88f);title.text="群星召唤契约";title.color=new Color(.24f,.16f,.07f);
            Text contract=TextAt(paper,"ContractWords",textFont,27,.08f,.46f,.92f,.7f);contract.text="愿以此名，唤醒沉睡于群星之间的回响。\n此约既成，群星为证。";contract.color=new Color(.30f,.21f,.09f);
            }
            goldMask=RectAt(paper,"GoldenSignatureMask",.10f,.16f,.90f,.41f);ContractPaperLayout.Place(goldMask,signatureRect);goldMask.gameObject.AddComponent<RectMask2D>();
            ink=RectAt(goldMask,"GoldenInk",0,0,1,1).gameObject.AddComponent<ContractSignaturePad>();
            ink.AcceptInput=false;ink.raycastTarget=false;ink.color=new Color(.95f,.68f,.14f);
            // 名称保留 U03 消费的稳定节点；遮罩收窄，文本宽度保持完整。
            signature=TextAt(paper,"SignatureName",handwriting,56,.10f,.16f,.90f,.41f);
            ContractPaperLayout.Place(signature.rectTransform,signatureRect);
            signature.color=new Color(.7f,.4f,.02f,1);signature.resizeTextForBestFit=true;signature.resizeTextMinSize=24;signature.resizeTextMaxSize=56;
            if(realistic)
            {
                waxShadow=ContractPaperLayout.Layer(paper,"WaxContactShadow",config.contractWaxShadow,config.contractWaxRect);
                stamp=ContractPaperLayout.Layer(paper,"GiantStarSeal",config.starSeal,config.contractWaxRect);
            }
            else stamp=Layer(world,"GiantStarSeal",config==null?theme.sparkleGold:config.starSeal,.30f,.19f,.70f,.84f);
            impact=Layer(world,"SealShockwave",config==null?theme.sparkleGold:config.shockwave,.29f,.20f,.71f,.82f);
            lines=RectAt(world,"AscendingSignatureGoldRibbon",0,0,1,1).gameObject.AddComponent<CelestialLinesGraphic>();lines.raycastTarget=false;lines.color=new Color(1,.83f,.4f,.9f);
            burst=Layer(world,"StarGateShockwave",config==null?theme.sparkleGold:config.shockwave,.14f,.02f,.86f,1.04f);
            flash=ImageAt(world,"GateFlash",Color.clear,0,0,1,1);
            Image subtitleShade=ImageAt(root,"DeclarationShade",new Color(.008f,.016f,.045f,.9f),.12f,.04f,.88f,.16f);
            declaration=TextAt(root,"SacredDeclaration",textFont,37,.14f,.055f,.86f,.15f);
            if(realistic&&config.contractBurnShader!=null)
            {burnView=paper.gameObject.AddComponent<ContractBurnView>();burnView.Build(config);}
        }
        public void Begin(ContractSignaturePad source,string name,CardRarity highest=CardRarity.R,float seconds=6.8f,ContractPaperMotion sourceMotion=null)
        {
            world.localScale=Vector3.one;world.localRotation=Quaternion.identity;
            if(motion!=null)motion.Adopt(sourceMotion);
            duration=seconds;previousSeconds=-1;signal=highest==CardRarity.UR?new Color(1,.46f,.15f):highest==CardRarity.SSR?new Color(1,.78f,.35f):highest==CardRarity.SR?new Color(.73f,.5f,1):new Color(.42f,.72f,1);
            ink.CopyStroke(source.Points);signature.text=name;paper.localRotation=Quaternion.identity;
            if(audio!=null){if(config!=null&&config.crystalInteraction)audio.BeginPrelude();else audio.BeginCeremony(seconds);}Animate(0);
        }
        public void Animate(float p)
        {
            Progress=p=Mathf.Clamp01(p);bool crystal=config!=null&&config.crystalInteraction;
            if(motion!=null)motion.ApplyNow();
            float t=p*(crystal?config.preludeSeconds:config==null?6.8f:config.referenceSeconds);
            float impactTime=config==null? .72f:config.sealImpact;
            float ringTime=config==null?1.35f:config.ringsAppear;
            float contractTime=config==null?4.25f:config.contraction;
            float burstTime=config==null?5.85f:config.gateBurst;
            float power=config==null? .85f:config.effectStrength;
            if(previousSeconds<impactTime && t>=impactTime && audio!=null)audio.SealImpact();
            if(!crystal&&previousSeconds<burstTime && t>=burstTime && audio!=null)audio.GateBurst();
            if(crystal&&previousSeconds<config.voiceSecondLine&&t>=config.voiceSecondLine&&audio!=null)audio.SecondDeclaration();
            previousSeconds=t;
            float redraw=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.48f,t));
            goldMask.anchorMax=new Vector2(Mathf.Lerp(signatureRect.x,signatureRect.z,redraw),signatureRect.w);
            ink.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,paper.rect.width*(signatureRect.z-signatureRect.x));
            ink.rectTransform.anchorMin=Vector2.zero;ink.rectTransform.anchorMax=new Vector2(0,1);ink.rectTransform.pivot=new Vector2(0,.5f);ink.rectTransform.anchoredPosition=Vector2.zero;
            signature.color=Color.Lerp(new Color(.24f,.14f,.04f),new Color(1,.72f,.13f),redraw);
            float lift=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.05f,2.4f,t));
            if(burnView!=null)
            {
                float burn=Mathf.InverseLerp(config.contractBurnStart,config.contractBurnStart+config.contractBurnSeconds,t);
                paper.localScale=Vector3.one;paper.anchoredPosition=Vector2.zero;paper.localRotation=Quaternion.identity;
                paperAlpha.alpha=burn>=1?0:1;
                burnView.Tick(burn,Mathf.Max(0,t-config.contractBurnStart));
            }
            else
            {
                paper.localScale=Vector3.one*Mathf.Lerp(1,.12f,lift);
                paper.anchoredPosition=new Vector2(0,lift*460);
                paper.localRotation=Quaternion.Euler(0,lift*25,0);
                paperAlpha.alpha=1-Mathf.InverseLerp(1.8f,2.65f,t);
            }
            float fall=Mathf.Clamp01(Mathf.InverseLerp(.40f,impactTime,t));
            if(realistic)
            {
                stamp.rectTransform.anchoredPosition=Vector2.up*Mathf.Lerp(800,0,fall*fall);
                stamp.rectTransform.localScale=Vector3.one*Mathf.Lerp(3.4f,1,fall);
                stamp.color=new Color(1,1,1,t<.39f?0:1);
                waxShadow.color=new Color(1,1,1,t<impactTime?0:config.contractWaxShadowAlpha);
                impact.rectTransform.position=stamp.rectTransform.position;
            }
            else
            {
            stamp.rectTransform.anchoredPosition=Vector2.up*Mathf.Lerp(800,-60,fall*fall);
            stamp.rectTransform.localScale=Vector3.one*Mathf.Lerp(3.4f,.78f,fall);
            stamp.color=new Color(1,.89f,.64f,t<.39f?0:1-Mathf.InverseLerp(impactTime+.13f,2.4f,t));
            if(crystal&&t>impactTime){float rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(impactTime+.15f,2.4f,t));stamp.rectTransform.anchoredPosition=Vector2.up*Mathf.Lerp(-60,650,rise);}
            }
            float wave=Mathf.Clamp01((t-impactTime)/.65f);
            impact.rectTransform.localScale=Vector3.one*Mathf.Lerp(.2f,3.9f,wave);
            impact.color=new Color(1,.82f,.42f,t<impactTime?0:Mathf.Sin(wave*Mathf.PI)*power);
            float establish=Mathf.SmoothStep(0,1,Mathf.InverseLerp(ringTime,2.35f,t));
            float lockIn=Mathf.SmoothStep(0,1,Mathf.InverseLerp(contractTime,burstTime-.13f,t));
            float explosion=Mathf.Clamp01((t-burstTime)/.70f);
            float ringsAlpha=establish*(1-Mathf.InverseLerp(burstTime+.10f,6.65f,t));
            float scale=(config==null?1.65f:config.giantRingScale)*Mathf.Lerp(.62f,1.28f,establish)*Mathf.Lerp(1,.37f,lockIn)*(1+explosion*3);
            giant.rectTransform.localScale=Vector3.one*scale;
            outer.rectTransform.localScale=Vector3.one*scale*.96f;
            inner.rectTransform.localScale=Vector3.one*scale*.91f;
            float rotation=(config==null?110:config.ringRotation);
            giant.rectTransform.localRotation=Quaternion.Euler(44-20*establish,0,t*rotation*.35f);
            outer.rectTransform.localRotation=Quaternion.Euler(12+Mathf.Sin(t)*30,0,-t*rotation*.6f);
            inner.rectTransform.localRotation=Quaternion.Euler(25,0,t*rotation);
            giant.color=new Color(1,.85f,.54f,ringsAlpha*.9f*power);outer.color=new Color(.91f,.73f,1,ringsAlpha*.87f*power);inner.color=new Color(.55f,.77f,1,ringsAlpha*.85f*power);
            float pillar=Mathf.Sin(Mathf.InverseLerp(2.25f,4.3f,t)*Mathf.PI)*power;
            float aperture=Mathf.SmoothStep(0,1,Mathf.InverseLerp(burstTime-.15f,burstTime+.28f,t));
            beam.rectTransform.localScale=new Vector3(Mathf.Lerp(.12f,4.5f,aperture),1.3f,1);
            beam.color=new Color(signal.r,signal.g,signal.b,(pillar*.72f+Mathf.Sin(explosion*Mathf.PI)*.95f)*power);
            burst.rectTransform.localScale=Vector3.one*Mathf.Lerp(.35f,4.3f,explosion);
            burst.color=new Color(signal.r,signal.g,signal.b,t<burstTime?0:(1-explosion)*power);
            float sealFlash=t>=impactTime && t<impactTime+.18f?(1-(t-impactTime)/.18f)*.18f:0;
            flash.color=new Color(1,.94f,.76f,(sealFlash+(t>=burstTime?Mathf.Exp(-explosion*13)*.42f:0))*power);
            float shake=(t>=impactTime&&t<impactTime+.2f?Mathf.Exp(-(t-impactTime)*18):0)+(t>=burstTime?Mathf.Exp(-(t-burstTime)*15):0);
            float amplitude=(config==null?12:config.shakePixels)*shake;
            world.anchoredPosition=new Vector2(Mathf.Sin(t*111)*amplitude,Mathf.Cos(t*97)*amplitude*.5f);
            lines.Animate(p);lines.color=new Color(1,.8f,.38f,(1-Mathf.InverseLerp(.91f,1,p))*power);
            float second=config==null?1.75f:config.voiceSecondLine;float last=config==null?5:config.voiceLastLine;
            declaration.text=t<second?"以此真名，缔结星约。":t<last?"让群星回应，让命运见证——":"星门，为您而开！";
            declaration.color=Color.Lerp(Color.white,new Color(1,.9f,.62f),Mathf.InverseLerp(last,burstTime,t));
        }
        public void PortalTransition(float progress)
        {
            if(config==null||!config.crystalPortalCamera)return;
            float p=Mathf.SmoothStep(0,1,Mathf.Clamp01(progress));
            world.localScale=Vector3.one*Mathf.Lerp(1,config.crystalPortalScale,p);
            world.localRotation=Quaternion.Euler(Mathf.Sin(p*Mathf.PI)*8,0,-Mathf.Sin(p*Mathf.PI)*3);
        }
        public void Finish()
        {
            stamp.color=giant.color=outer.color=inner.color=beam.color=impact.color=burst.color=flash.color=Color.clear;
            if(waxShadow!=null)waxShadow.color=Color.clear;
            if(burnView!=null){burnView.Clear();paperAlpha.alpha=0;}
            lines.color=Color.clear;world.anchoredPosition=Vector2.zero;
            world.localScale=Vector3.one;world.localRotation=Quaternion.identity;
            if(audio!=null)audio.FinishPhase();
        }
        private static RectTransform RectAt(Transform parent,string name,float x0,float y0,float x1,float y1)
        { var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;return r; }
        private static Image Layer(Transform parent,string name,Sprite sprite,float x0,float y0,float x1,float y1)
        { var i=ImageAt(parent,name,Color.white,x0,y0,x1,y1);i.sprite=sprite;i.preserveAspect=true;if(sprite==null)i.enabled=false;return i; }
        private static Image ImageAt(Transform parent,string name,Color color,float x0,float y0,float x1,float y1)
        {var i=RectAt(parent,name,x0,y0,x1,y1).gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=false;return i;}
        private static Text TextAt(Transform parent,string name,Font font,int size,float x0,float y0,float x1,float y1)
        {var t=RectAt(parent,name,x0,y0,x1,y1).gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;}
    }
}

