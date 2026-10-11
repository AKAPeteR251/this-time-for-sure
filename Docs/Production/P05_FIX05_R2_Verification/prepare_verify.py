from pathlib import Path
import shutil
root=Path(r'D:/vibe/这次一定出');assets=root/'这次一定出/Assets/_Game';out=root/'Docs/Production/P05_FIX05_R2_Verification'
s=(assets/'Editor/P05Fix05Setup.cs').read_text(encoding='utf-8-sig').replace('P05Fix05Setup','P05Fix05R2Setup').replace('P05Fix05Recorder','P05Fix05R2Recorder').replace('P05_FIX05_Verification','P05_FIX05_R2_Verification').replace('Fix05.request','Fix05R2.request').replace('ThisTime.Fix05Verify','ThisTime.Fix05R2Verify').replace('P05-FIX05','P05-FIX05-R2')
insert='''        const string identityArt="Assets/_Game/Art/Effects/F04/RevealIdentityR2/";
        foreach(string p in Directory.GetFiles(identityArt,"*.png"))ImportSprite(p.Replace('\\\\','/'));
        config.revealSsrBadge=Sprite(identityArt+"ssr_crest_v002.png");config.revealNewPlaque=Sprite(identityArt+"new_plaque_v002.png");config.iceMageBackdrop=Sprite(identityArt+"ice_snowflake_v002.png");
        config.iceBackdropAlpha=.22f;config.faceTarget=new Vector2(.55f,.68f);EditorUtility.SetDirty(config);
'''
s=s.replace('        var scene=EditorSceneManager.OpenScene(ScenePath);',insert+'        var scene=EditorSceneManager.OpenScene(ScenePath);',1)
s=s.replace('Click(Get<Button>(ui,"confirmCardButton"))','Blank(ui)')
s=s.replace('Click(Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(x=>x.name=="AdvanceSurface"))','Blank(ui)')
s=s.replace('        yield return Capture("1280x720_summon_without_rail");','''        Require(ui.feelConfig.revealSsrBadge!=null&&ui.feelConfig.revealNewPlaque!=null&&ui.feelConfig.iceMageBackdrop!=null,"Main三张生成参考实际引用");
        var page=Page(ui,"Reveal");Require(page.GetComponentsInChildren<Button>(true).All(b=>b.name=="AdvanceSurface"||b.name=="跳过"),"没有下一张/查看结果按钮");
        Require(page.GetComponentsInChildren<Text>(true).All(t=>t.name!="CharacterTitle"),"揭晓没有称号节点");
        yield return Capture("1280x720_summon_without_rail");''')
s=s.replace('recorder.Begin("main_reveal_layout")','recorder.Begin("main_names_only_reveal")')
s=s.replace('yield return Capture("1280x720_ssr_wait");','''Require(Page(ui,"Reveal").GetComponentsInChildren<Text>().All(t=>t.text!="冰城守望者"&&!System.Text.RegularExpressions.Regex.IsMatch(t.text,@"^\\d+/\\d+$")),"仅名称，无称号或张数");yield return Capture("1280x720_ssr_wait");''')
s=s.replace('yield return new WaitForSecondsRealtime(.3f);Require(ui.RevealIndex==1,"不穿透下一张");','''yield return new WaitForSecondsRealtime(.3f);Require(ui.RevealIndex==1,"不穿透下一张");
        Require(Page(ui,"Reveal").GetComponentsInChildren<Text>().All(t=>t.text!="NEW"),"非NEW角色不显示标签");''')
s=s.replace('        Complete("PASS: Main新揭晓顺序、首末张新确认、动画仅补完当前张、三窗口、删除导航与保留图鉴入口、真实签约Draw与同批跳过；实际带声片段。");','        Complete("PASS: Main仅名称、无按钮/张数、空白新点击确认与末张等待、生成SSR/NEW/雪花引用、三窗口、真实Draw和同批跳过；实际带声片段。");')
s=s.replace('    private static void CrystalPress(PrototypeGame ui)','''    private static void Blank(PrototypeGame ui)
    {
        var obj=Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(b=>b.name=="AdvanceSurface").gameObject;
        var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=new Vector2(Screen.width*.12f,Screen.height*.80f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits[0].gameObject==obj,"空白区域真实射线命中");
        ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerClickHandler);
    }
    private static void CrystalPress(PrototypeGame ui)''')
(assets/'Editor/P05Fix05R2Setup.cs').write_text(s,encoding='utf-8')
for kind in ['runtime','editor']:
 old=(root/f'Docs/Production/P05_CRYSTAL_Verification/{kind}.rsp').read_text(encoding='utf-8-sig').splitlines()
 lines=[l for l in old if not (l.startswith('"') and l.endswith('.cs"')) and not l.startswith('/out:') and not ('/reference:' in l and 'P05_CRYSTAL_Verification' in l)]
 lines.append('/out:"'+str(out/f'{kind}.dll')+'"')
 if kind=='editor':lines.append('/reference:"'+str(out/'runtime.dll')+'"')
 folders=[assets/'Scripts'] if kind=='runtime' else [assets/'Editor',assets/'UI/Editor']
 lines+=['"'+str(p)+'"' for f in folders for p in f.rglob('*.cs')]
 (out/f'{kind}.rsp').write_text('\n'.join(lines)+'\n',encoding='utf-8')
shutil.copy2(root/'Docs/Production/P05_CRYSTAL_Verification/encode_clips.py',out/'encode_clips.py')
