from pathlib import Path
import re, subprocess, json, hashlib
root=Path(__file__).resolve().parents[3]
here=Path(__file__).resolve().parent
project=root/'这次一定出'
old=(project/'Assets/_Game/Editor/P05Fix05R2Setup.cs').read_text(encoding='utf-8-sig')
text=old.replace('P05Fix05R2','P06RevealR3').replace('P05_FIX05_R2_Verification','P06_REVEAL_R3_Verification').replace('Fix05R2.request','RevealR3.request').replace('ThisTime.Fix05R2Verify','ThisTime.RevealR3Verify').replace('P05-FIX05-R2','P06-REVEAL-R3')
start=text.index('        const string art=')
end=text.index('        var scene=EditorSceneManager.OpenScene(ScenePath);',start)
prepare='''        const string art="Assets/_Game/Art/Effects/F05/RevealR3/";
        foreach(string path in Directory.GetFiles(art,"*.png"))
        {
            var t=AssetImporter.GetAtPath(path.Replace('\\\\','/')) as TextureImporter;
            t.textureType=path.Contains("nebula")||path.Contains("starfield")?TextureImporterType.Default:TextureImporterType.Sprite;
            t.spriteImportMode=SpriteImportMode.Single;t.spritePivot=new Vector2(.5f,.5f);t.alphaIsTransparency=!path.Contains("starfield");
            t.wrapMode=path.Contains("nebula")?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
            t.mipmapEnabled=false;t.maxTextureSize=path.Contains("starfield")?4096:2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.filterMode=FilterMode.Bilinear;t.SaveAndReimport();
        }
        var config=AssetDatabase.LoadAssetAtPath<P04FeelConfig>(ConfigPath);
        config.revealStarfield=AssetDatabase.LoadAssetAtPath<Texture2D>(art+"reveal_starfield_base_v003.png");
        config.nebulaFar=AssetDatabase.LoadAssetAtPath<Texture2D>(art+"reveal_nebula_far_v003.png");config.nebulaNear=AssetDatabase.LoadAssetAtPath<Texture2D>(art+"reveal_nebula_near_v003.png");
        config.starSoft=Sprite(art+"star_soft_v003.png");config.starCross=Sprite(art+"star_cross_v003.png");config.meteorTrail=Sprite(art+"meteor_trail_v003.png");config.revealShard=Sprite(art+"reveal_ice_shard_v003.png");
        config.rarityR=Sprite(art+"rarity_R_v003.png");config.raritySR=Sprite(art+"rarity_SR_v003.png");config.raritySSR=Sprite(art+"rarity_SSR_v003.png");config.rarityUR=Sprite(art+"rarity_UR_v003.png");
        config.rarityAccent=Sprite(art+"rarity_accent_v003.png");config.newText=Sprite(art+"new_text_v003.png");config.newBorder=Sprite(art+"new_border_mask_v003.png");config.iceMageBackdrop=Sprite(art+"SSR001_snowflake_backdrop_v003.png");
        config.iceBackdropAlpha=.32f;config.faceTarget=new Vector2(.55f,.68f);EditorUtility.SetDirty(config);
'''
text=text[:start]+prepare+text[end:]
text=text.replace('        ui.portraits.Single(p=>p.characterId=="UR001").portrait=Sprite(ur);','')
text=text.replace('Main经Editor保存：v004水晶/24帧、透明冰法师、P05连续蓄能、分段中文旁白；U03字体与六卡保持。','Main保存15项R3分层素材，保留现有水晶/语音/人物引用。')
text=text.replace('ui.feelConfig.revealSsrBadge!=null&&ui.feelConfig.revealNewPlaque!=null&&ui.feelConfig.iceMageBackdrop!=null','ui.feelConfig.rarityR!=null&&ui.feelConfig.raritySR!=null&&ui.feelConfig.raritySSR!=null&&ui.feelConfig.rarityUR!=null&&ui.feelConfig.newBorder!=null&&ui.feelConfig.revealStarfield!=null')
text=text.replace('Main三张生成参考实际引用','Main R3四档与分层背景引用')
text=text.replace('main_names_only_reveal','main_r3_layered_reveal')
text=text.replace('yield return new WaitForSecondsRealtime(.4f);Require(ui.RevealIndex==0,','''var sky=page.GetComponentInChildren<RevealSky>();var border=page.GetComponentInChildren<RevealGradientBorder>();
        Vector2 cloud=sky.CloudOffset;float star=sky.FirstStarAlpha,phase=border.Phase,meteorPeak=0;
        for(float t=0;t<4.0f;t+=Time.unscaledDeltaTime){meteorPeak=Mathf.Max(meteorPeak,sky.MeteorAlpha);yield return null;}
        Require((sky.CloudOffset-cloud).sqrMagnitude>.00001f,"等待中双层星云持续漂移");Require(Mathf.Abs(sky.FirstStarAlpha-star)>.001f,"等待中恒星闪烁");Require(Mathf.Abs(border.Phase-phase)>.001f,"NEW边框动态渐变");Require(meteorPeak>.05f,"实际流星出现并移动");
        yield return Capture("1280x720_ssr_dynamic_wait");Require(ui.RevealIndex==0,''')
needle='        Resize(1280,720);yield return null;yield return null;ui.RequestSummon(1);'
insert='''        Resize(1280,720);yield return null;yield return null;
        foreach(CardRarity tier in Enum.GetValues(typeof(CardRarity)))
        {
            ui.PlayBatchForDebug(new List<PrototypePull>{new PrototypePull{Card=state.Cards.First(c=>c.Rarity==tier),IsNew=true}});yield return null;ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
            while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}
            yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,7);
            var icon=Page(ui,"Reveal").GetComponentsInChildren<Image>().Single(x=>x.name=="SsrCrest");
            Sprite expected=tier==CardRarity.R?ui.feelConfig.rarityR:tier==CardRarity.SR?ui.feelConfig.raritySR:tier==CardRarity.SSR?ui.feelConfig.raritySSR:ui.feelConfig.rarityUR;
            Require(icon.sprite==expected,"本张真实稀有度字图:"+tier);yield return Capture("1280x720_rarity_"+tier);ui.HandleEscape();yield return null;
        }
        ui.RequestSummon(1);'''
assert needle in text;text=text.replace(needle,insert)
text=text.replace('生成SSR/NEW/雪花引用、三窗口','R3分层动态、四档字图、三窗口')
(project/'Assets/_Game/Editor/P06RevealR3Setup.cs').write_text(text,encoding='utf-8-sig')
for kind in ['runtime','editor']:
    oldrsp=(root/'Docs/Production/P05_FIX05_R2_Verification'/(kind+'.rsp')).read_text(encoding='utf-8-sig')
    lines=[s for s in oldrsp.splitlines() if not s.startswith('"') and not s.startswith('/out:') and not ('/runtime.dll"' in s or '\\runtime.dll"' in s)]
    lines.append('/out:"'+str(here/(kind+'.dll'))+'"')
    if kind=='editor':lines.append('/reference:"'+str(here/'runtime.dll')+'"')
    files=list((project/'Assets/_Game').rglob('*.cs'))
    files=[f for f in files if ('Editor' in f.parts)==(kind=='editor')]
    lines += ['"'+str(f)+'"' for f in files]
    (here/(kind+'.rsp')).write_text('\n'.join(lines),encoding='utf-8-sig')
    proc=subprocess.run(['E:/Unity/2022.3.53f1c1/Editor/Data/NetCoreRuntime/dotnet.exe','E:/Unity/2022.3.53f1c1/Editor/Data/DotNetSdkRoslyn/csc.dll','@'+str(here/(kind+'.rsp'))],cwd=root,capture_output=True,text=True,encoding='utf-8',errors='replace')
    (here/(kind+'_compile.log')).write_text(proc.stdout+proc.stderr,encoding='utf-8');print(kind,proc.returncode,proc.stdout[-1500:]);assert proc.returncode==0
before=json.loads((root/'Docs/Production/P05_CRYSTAL_Verification/protected_before.json').read_text(encoding='utf-8-sig'))
checks=[dict(file=p,unchanged=hashlib.sha256((root/p).read_bytes()).hexdigest()==h) for p,h in before.items()]
(here/'protected_check.json').write_text(json.dumps(checks,ensure_ascii=False,indent=2),encoding='utf-8');assert all(c['unchanged'] for c in checks)
(here/'encode_clips.py').write_bytes((root/'Docs/Production/P05_FIX05_R2_Verification/encode_clips.py').read_bytes())
