using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    // 原批次只读表现：剪影/碎屑 → 角色与名字 → 稀有度 → 新确认。
    public sealed class FastRevealView
    {
        private readonly Image backdrop,artwork,silhouette,transition,veil,iceBackdrop,rarityIcon,newIcon;
        private readonly Text identity,rarityLabel,newLabel,signal;
        private readonly RectTransform artworkSlot,info;
        private readonly RevealBurstGraphic burst;
        private readonly RevealSky sky;
        private readonly RevealArrivalBurst arrivalBurst;private readonly Image arrivalFlash;
        public float SmallBurstPulse {get{return arrivalBurst==null?0:arrivalBurst.SmallPulse;}}
        public float BigBurstPulse {get{return arrivalBurst==null?0:arrivalBurst.BigPulse;}}
        public float BurstAlpha {get{return arrivalBurst==null?0:arrivalBurst.ActiveAlpha;}}
        private readonly Image rarityAccent,newArtwork,newGlint;
        private readonly RevealGradientBorder newFrame;
        private readonly P04FeelConfig config;
        private readonly PrototypeUITheme theme;
        private readonly SummonAudio audio;
        private PrototypeCard card;private bool isNew,impactPlayed;
        public float Progress {get;private set;}
        public float NameAlpha {get{return identity.color.a;}}
        public float RarityAlpha {get{return rarityLabel.color.a;}}
        public Sprite Artwork {get{return artwork.sprite;}}
        public string CharacterId {get{return card==null?"":card.Id;}}
        public CardRarity Rarity {get{return card.Rarity;}}
        public FastRevealView(RectTransform page,PrototypeUITheme theme,Font font,Font titleFont,P04FeelConfig config=null,SummonAudio audio=null)
        {
            this.theme=theme;this.config=config;this.audio=audio;
            backdrop=ImageAt(page,"RevealBackdrop",0,0,1,1);backdrop.color=new Color(.025f,.04f,.08f,1);
            burst=RectAt(page,"OutwardShardsAndStars",0,0,1,1).gameObject.AddComponent<RevealBurstGraphic>();burst.raycastTarget=false;
            if(config!=null&&config.revealStarfield!=null)
            {burst.enabled=false;sky=RectAt(page,"LayeredRevealSky",0,0,1,1).gameObject.AddComponent<RevealSky>();sky.Build(config);}
            if(config!=null&&config.revealArrivalShader!=null){arrivalBurst=RectAt(page,"RevealDoubleBurst",0,0,1,1).gameObject.AddComponent<RevealArrivalBurst>();arrivalBurst.Build(config);}
            iceBackdrop=ImageAt(page,"IceMageSnowflake",.22f,.07f,.82f,.95f);iceBackdrop.preserveAspect=true;iceBackdrop.sprite=config==null?null:config.iceMageBackdrop;
            artworkSlot=RectAt(page,"RevealArtworkSlot",.26f,.10f,.81f,.96f);
            artwork=ImageAt(artworkSlot,"RevealArtwork",0,0,1,1);artwork.preserveAspect=true;
            silhouette=ImageAt(artworkSlot,"UnknownSilhouette",.2f,.08f,.8f,.92f);
            veil=ImageAt(page,"BlackRevealVeil",0,0,1,1);veil.color=Color.clear;
            arrivalFlash=ImageAt(page,"ArrivalFrontFlash",0,0,1,1);arrivalFlash.color=Color.clear;
            info=RectAt(page,"RevealInformation",.075f,.19f,.28f,.59f);
            identity=TextAt(page,"RevealIdentity",titleFont,72,.075f,.44f,.35f,.59f);identity.alignment=TextAnchor.MiddleLeft;
            rarityIcon=ImageAt(page,"SsrCrest",.065f,.19f,.245f,.43f);rarityIcon.preserveAspect=true;rarityIcon.sprite=config==null?null:config.revealSsrBadge;
            rarityLabel=TextAt(rarityIcon.rectTransform,"RarityLabel",font,40,.28f,.39f,.72f,.62f);rarityLabel.fontStyle=FontStyle.Bold;
            newIcon=ImageAt(page,"NewPlaque",.075f,.60f,.255f,.68f);newIcon.preserveAspect=true;newIcon.sprite=config==null?null:config.revealNewPlaque;
            newLabel=TextAt(newIcon.rectTransform,"NewLabel",font,31,.30f,.20f,.77f,.75f);newLabel.fontStyle=FontStyle.Bold;
            rarityAccent=ImageAt(page,"RarityAccent",.075f,.355f,.295f,.39f);rarityAccent.preserveAspect=true;
            newArtwork=ImageAt(page,"NewArtwork",.075f,.605f,.19f,.66f);newArtwork.preserveAspect=true;
            newFrame=RectAt(page,"NewGradientFrame",.075f,.595f,.19f,.67f).gameObject.AddComponent<RevealGradientBorder>();newFrame.raycastTarget=false;
            newGlint=ImageAt(page,"NewGlint",0,0,0,0);newGlint.rectTransform.sizeDelta=new Vector2(13,13);
            newGlint.rectTransform.anchorMin=newGlint.rectTransform.anchorMax=new Vector2(.074f,.655f);
            if(config!=null)
            {
                rarityAccent.sprite=config.rarityAccent;newArtwork.sprite=config.newText;newFrame.sprite=config.newBorder;
                newFrame.speed=config.newGradientSpeed;newGlint.sprite=config.starCross;
                newFrame.blinkStrength=config.newBlinkStrength;newFrame.blinkFrequency=config.newBlinkFrequency;
                if(config.raritySSR!=null){rarityIcon.rectTransform.anchorMin=new Vector2(.05f,.16f);rarityIcon.rectTransform.anchorMax=new Vector2(.32f,.40f);rarityLabel.gameObject.SetActive(false);}
                if(config.newText!=null){newIcon.gameObject.SetActive(false);newLabel.gameObject.SetActive(false);}
            }
            RectAt(page,"FutureExtrasArea",.80f,.26f,.97f,.67f);
            signal=TextAt(page,"RevealSignal",font,22,0,0,0,0);signal.text="";
            transition=ImageAt(page,"CardTransition",0,0,1,1);transition.color=Color.clear;
        }
        public void Begin(PrototypeCard result,Sprite portrait,bool newCharacter=false)
        {
            card=result;isNew=newCharacter;impactPlayed=false;artwork.sprite=portrait;artwork.enabled=portrait!=null;
            if(config!=null&&config.raritySSR!=null)rarityIcon.sprite=card.Rarity==CardRarity.R?config.rarityR:card.Rarity==CardRarity.SR?config.raritySR:card.Rarity==CardRarity.UR?config.rarityUR:config.raritySSR;
            silhouette.enabled=portrait==null;identity.text=card.Name;rarityLabel.text=card.Rarity.ToString();newLabel.text=isNew?"NEW":"";
            transition.color=Color.clear;artworkSlot.localScale=Vector3.one;artworkSlot.anchoredPosition=Vector2.zero;
            if(arrivalBurst!=null)arrivalBurst.Begin(card.Id=="SSR001"?new Color(.20f,.57f,1):theme.Rarity(card.Rarity));if(audio!=null)audio.BeginCard(card.Rarity);Animate(0);
        }
        public void Animate(float progress)
        {
            Progress=Mathf.Clamp01(progress);if(card==null)return;
            if(sky!=null)sky.Animate(Progress);
            if(arrivalBurst!=null){arrivalBurst.Animate(Progress);arrivalFlash.color=new Color(.72f,.86f,1,arrivalBurst.BigPulse*.18f*Mathf.Clamp01(1-(Progress-config.characterBurstStart)/.12f));}
            Color tint=theme.Rarity(card.Rarity);burst.signal=tint;burst.Animate(Progress);
            float second=config==null?.46f:config.characterBurstStart;
            bool ice=card.Id=="SSR001"&&iceBackdrop.sprite!=null;
            burst.showHalo=!ice;iceBackdrop.enabled=ice;
            iceBackdrop.color=new Color(1,1,1,(config==null?.22f:config.iceBackdropAlpha)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(second+.08f,second+.28f,Progress)));
            rarityIcon.enabled=rarityIcon.sprite!=null&&(config!=null&&config.raritySSR!=null||card.Rarity==CardRarity.SSR);
            float reveal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(second,second+.20f,Progress));
            artwork.color=Color.Lerp(Color.black,Color.white,reveal);
            var missing=silhouette.GetComponentInChildren<UICharacterSilhouette>();
            if(missing!=null)missing.color=Color.Lerp(Color.black,new Color(.38f,.46f,.58f),reveal);
            float zoom=card.Id=="SSR001"&&config!=null?(Progress<second?1:Mathf.Lerp(config.faceZoom,1,Mathf.SmoothStep(0,1,Mathf.InverseLerp(second,second+.28f,Progress)))):Mathf.Lerp(1.12f,1,reveal);
            artworkSlot.localScale=Vector3.one*zoom;
            if(card.Id=="SSR001"&&config!=null)
            {
                var root=artworkSlot.parent.GetComponent<RectTransform>();Vector2 size=artworkSlot.rect.size;
                float aspect=artwork.sprite==null?2f/3:artwork.sprite.rect.width/artwork.sprite.rect.height;
                Vector2 fitted=size.x/size.y>aspect?new Vector2(size.y*aspect,size.y):new Vector2(size.x,size.x/aspect);
                Vector2 center=Vector2.Scale((artworkSlot.anchorMin+artworkSlot.anchorMax)*.5f-new Vector2(.5f,.5f),root.rect.size);
                Vector2 target=Vector2.Scale(config.faceTarget-new Vector2(.5f,.5f),root.rect.size);
                Vector2 face=Vector2.Scale(config.faceUv-new Vector2(.5f,.5f),fitted)*config.faceZoom;
                float p=Progress<second?0:1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(second,second+.28f,Progress));artworkSlot.anchoredPosition=(target-center-face)*p;
            }
            else artworkSlot.anchoredPosition=Vector2.right*(1-reveal)*30;
            float name=Mathf.SmoothStep(0,1,Mathf.InverseLerp(second+.08f,second+.26f,Progress));
            identity.color=new Color(.97f,.98f,1,name);identity.rectTransform.anchoredPosition=Vector2.up*(1-name)*18;
            float rarity=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.80f,.91f,Progress));
            rarityLabel.color=new Color(1,.92f,.67f,rarity);rarityIcon.color=new Color(1,1,1,rarity);
            rarityIcon.rectTransform.localScale=Vector3.one*Mathf.Lerp(1.15f,1,rarity);
            rarityAccent.color=new Color(tint.r,tint.g,tint.b,rarity);rarityAccent.enabled=rarityAccent.sprite!=null;
            float fresh=isNew?Mathf.InverseLerp(.92f,.99f,Progress):0;
            newIcon.enabled=isNew&&newIcon.sprite!=null;newIcon.color=new Color(1,1,1,fresh);newLabel.color=new Color(1,.94f,.75f,fresh);
            newArtwork.enabled=isNew&&newArtwork.sprite!=null;newArtwork.color=new Color(1,.95f,.81f,fresh);
            newFrame.enabled=isNew&&newFrame.sprite!=null;newFrame.color=new Color(1,1,1,fresh);newGlint.enabled=isNew&&newGlint.sprite!=null;newGlint.color=new Color(1,.97f,.84f,fresh);
            veil.color=new Color(0,0,0,(1-reveal)*.14f);
            if(!impactPlayed&&Progress>=second+.04f){impactPlayed=true;if(audio!=null)audio.CardImpact(card.Rarity);}
        }
        public void Finish(){if(arrivalBurst!=null)arrivalBurst.Clear();arrivalFlash.color=Color.clear;veil.color=transition.color=Color.clear;artworkSlot.localScale=Vector3.one;artworkSlot.anchoredPosition=Vector2.zero;if(audio!=null)audio.FinishPhase();}
        public void TickIdle(float time)
        {
            if(card!=null)burst.Animate(1);
            if(isNew&&newGlint.enabled)
            {
                float p=Mathf.Repeat(time*(config==null?.12f:config.newGradientSpeed),1)*4;Vector2 a,b;
                if(p<1){a=new Vector2(.082f,.661f);b=new Vector2(.183f,.661f);}
                else if(p<2){a=new Vector2(.183f,.661f);b=new Vector2(.183f,.605f);}
                else if(p<3){a=new Vector2(.183f,.605f);b=new Vector2(.082f,.605f);}
                else{a=new Vector2(.082f,.605f);b=new Vector2(.082f,.661f);}
                newGlint.rectTransform.anchorMin=newGlint.rectTransform.anchorMax=Vector2.Lerp(a,b,p-Mathf.Floor(p));newGlint.rectTransform.anchoredPosition=Vector2.zero;
            }
        }
        public void Transition(float progress){transition.color=new Color(.015f,.025f,.055f,Mathf.Sin(progress*Mathf.PI)*.95f);}
        private static RectTransform RectAt(Transform p,string n,float x,float y,float xx,float yy){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(xx,yy);r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        private static Image ImageAt(Transform p,string n,float x,float y,float xx,float yy){var i=RectAt(p,n,x,y,xx,yy).gameObject.AddComponent<Image>();i.raycastTarget=false;return i;}
        private static Text TextAt(Transform p,string n,Font f,int s,float x,float y,float xx,float yy){var t=RectAt(p,n,x,y,xx,yy).gameObject.AddComponent<Text>();t.font=f;t.fontSize=s;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.raycastTarget=false;return t;}
    }
}
