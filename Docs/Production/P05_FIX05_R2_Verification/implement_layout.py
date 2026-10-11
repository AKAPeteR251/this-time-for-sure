from pathlib import Path
root=Path(r'D:/vibe/这次一定出/这次一定出/Assets/_Game')
p=root/'Scripts/Presentation/FastRevealView.cs';s=p.read_text(encoding='utf-8')
s=s.replace('backdrop,artwork,silhouette,transition,veil','backdrop,artwork,silhouette,transition,veil,iceBackdrop,rarityIcon,newIcon').replace('identity,title,rarityLabel,newLabel,signal','identity,rarityLabel,newLabel,signal').replace('        private readonly RaritySigilGraphic sigil;\n','')
s=s.replace('            artworkSlot=RectAt(page,"RevealArtworkSlot",.36f,.10f,.90f,.96f);','''            iceBackdrop=ImageAt(page,"IceMageSnowflake",.22f,.07f,.82f,.95f);iceBackdrop.preserveAspect=true;iceBackdrop.sprite=config==null?null:config.iceMageBackdrop;
            artworkSlot=RectAt(page,"RevealArtworkSlot",.26f,.10f,.81f,.96f);''')
s=s.replace('.075f,.19f,.43f,.61f','.075f,.19f,.28f,.59f').replace('72,.075f,.44f,.43f,.59f','72,.075f,.44f,.35f,.59f')
a=s.index('            title=TextAt(');b=s.index('            signal=TextAt(',a)
s=s[:a]+'''            rarityIcon=ImageAt(page,"SsrCrest",.065f,.19f,.245f,.43f);rarityIcon.preserveAspect=true;rarityIcon.sprite=config==null?null:config.revealSsrBadge;
            rarityLabel=TextAt(rarityIcon.rectTransform,"RarityLabel",font,40,.28f,.39f,.72f,.62f);rarityLabel.fontStyle=FontStyle.Bold;
            newIcon=ImageAt(page,"NewPlaque",.075f,.60f,.255f,.68f);newIcon.preserveAspect=true;newIcon.sprite=config==null?null:config.revealNewPlaque;
            newLabel=TextAt(newIcon.rectTransform,"NewLabel",font,31,.30f,.20f,.77f,.75f);newLabel.fontStyle=FontStyle.Bold;
            RectAt(page,"FutureExtrasArea",.80f,.26f,.97f,.67f);
''' +s[b:]
s=s.replace('identity.text=card.Name;title.text=card.Title;','identity.text=card.Name;')
s=s.replace('            Color tint=theme.Rarity(card.Rarity);burst.signal=tint;burst.Animate(Progress);','''            Color tint=theme.Rarity(card.Rarity);burst.signal=tint;burst.Animate(Progress);
            bool ice=card.Id=="SSR001"&&iceBackdrop.sprite!=null;
            burst.showHalo=!ice;iceBackdrop.enabled=ice;
            iceBackdrop.color=new Color(1,1,1,(config==null?.22f:config.iceBackdropAlpha)*Mathf.Lerp(.35f,1,Mathf.InverseLerp(.16f,.50f,Progress)));
            rarityIcon.enabled=card.Rarity==CardRarity.SSR&&rarityIcon.sprite!=null;''')
s='\n'.join(l for l in s.split('\n') if 'title.color=' not in l)
s=s.replace('            rarityLabel.color=new Color(tint.r,tint.g,tint.b,rarity);sigil.color=new Color(tint.r,tint.g,tint.b,rarity);\n            sigil.rectTransform.localScale=Vector3.one*Mathf.Lerp(1.35f,1,rarity);','''            rarityLabel.color=new Color(1,.92f,.67f,rarity);rarityIcon.color=new Color(1,1,1,rarity);
            rarityIcon.rectTransform.localScale=Vector3.one*Mathf.Lerp(1.15f,1,rarity);''')
s=s.replace('            newLabel.color=new Color(1,.83f,.45f,Mathf.InverseLerp(.84f,.95f,Progress));','''            float fresh=isNew?Mathf.InverseLerp(.84f,.95f,Progress):0;
            newIcon.enabled=isNew&&newIcon.sprite!=null;newIcon.color=new Color(1,1,1,fresh);newLabel.color=new Color(1,.94f,.75f,fresh);''')
p.write_text(s,encoding='utf-8')
p=root/'Scripts/UI/PrototypeGame.cs';s=p.read_text(encoding='utf-8');s=s.replace('autoSignatureText, revealCount','autoSignatureText').replace('        private Button confirmCardButton;\n','')
s=s.replace('identity.anchorMax = new Vector2(0.43f, 0.59f)','identity.anchorMax = new Vector2(0.35f, 0.59f)')
s=s.replace(' confirmedCard = true; confirmCardButton.gameObject.SetActive(false); summonAudio.Confirm();',' confirmedCard = true; summonAudio.Confirm();')
s='\n'.join(l for l in s.split('\n') if 'revealCount' not in l and 'confirmCardButton' not in l)
p.write_text(s,encoding='utf-8')
p=root/'Scripts/Presentation/RevealBurstGraphic.cs';s=p.read_text(encoding='utf-8');s=s.replace('        public float progress;','        public float progress;\n        public bool showHalo=true;').replace('            for(int i=0;i<56;i++)','            for(int i=0;i<(showHalo?56:0);i++)');p.write_text(s,encoding='utf-8')
print('Names-only layout, blank-click progression, central character and generated sprite slots implemented')
