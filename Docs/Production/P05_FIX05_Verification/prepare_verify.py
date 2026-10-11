from pathlib import Path
root=Path(r'D:/vibe/这次一定出');p=root/'这次一定出/Assets/_Game/Editor/P05CrystalSetup.cs';s=p.read_text(encoding='utf-8-sig').replace('P05CrystalSetup','P05Fix05Setup').replace('P05PlayRecorder','P05Fix05Recorder').replace('P05_CRYSTAL_Verification','P05_FIX05_Verification').replace('P05.request','Fix05.request').replace('ThisTime.P05CrystalVerify','ThisTime.Fix05Verify').replace('P05-CRYSTAL','P05-FIX05')
a=s.index('    private static readonly List<string> inputs');b=s.index('    private static void CrystalPress',a)
s=s[:a]+'''    private static IEnumerator Check(PrototypeGame ui)
    {
        yield return null;yield return null;
        var state=Get<IPrototypeUIAdapter>(ui,"game");var reveal=Get<FastRevealView>(ui,"fastReveal");
        Require(ui.GetComponentsInChildren<RectTransform>(true).All(t=>t.name!="NavigationRail"&&t.name!="TabRule"),"侧栏与顶部切换删除");
        yield return Capture("1280x720_summon_without_rail");
        var fixedBatch=new List<PrototypePull>{Pull(state,"SSR001",true),Pull(state,"SSR001",false),Pull(state,"R002",true)};
        var recorder=UnityEngine.Object.FindObjectOfType<AudioListener>().gameObject.AddComponent<P05Fix05Recorder>();recorder.Begin("main_reveal_layout");
        ui.PlayBatchForDebug(fixedBatch);yield return null;ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
        while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}
        yield return WaitStage(ui,SummonFlowStage.Revealing,3);
        yield return new WaitForSecondsRealtime(.3f);Require(reveal.NameAlpha==0&&reveal.RarityAlpha==0,"剪影阶段无姓名或稀有度");yield return Capture("1280x720_black_silhouette");
        while(reveal.Progress<.50f)yield return null;
        Require(reveal.NameAlpha>.9f&&reveal.RarityAlpha==0,"角色名字先于稀有度");yield return Capture("1280x720_name_before_rarity");
        yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);Require(reveal.RarityAlpha==1,"稀有度随后完整浮现");yield return Capture("1280x720_ssr_wait");
        yield return new WaitForSecondsRealtime(.4f);Require(ui.RevealIndex==0,"首张等待新确认");Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,SummonFlowStage.Revealing,2);
        yield return null;yield return null;Click(Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(x=>x.name=="AdvanceSurface"));
        yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);Require(ui.RevealIndex==1&&reveal.NameAlpha==1&&reveal.RarityAlpha==1,"动画点击补齐当前张与全部信息");
        yield return new WaitForSecondsRealtime(.3f);Require(ui.RevealIndex==1,"不穿透下一张");Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,3);
        yield return new WaitForSecondsRealtime(.5f);Require(ui.RevealIndex==2,"末张仍等待");yield return Capture("1280x720_missing_portrait_wait");Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,SummonFlowStage.Summary,2);
        yield return new WaitForSecondsRealtime(.25f);recorder.End();
        foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1600,1000)})
        {
            Resize(size.x,size.y);yield return null;yield return null;ui.PlayBatchForDebug(new List<PrototypePull>{fixedBatch[0]});ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
            while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,5);
            yield return Capture(size.x+"x"+size.y+"_ssr_wait");CheckBounds(ui);ScanClean(ui);ui.HandleEscape();yield return null;yield return null;
            Require(ui.FlowStage==SummonFlowStage.Summary&&ui.CrystalView.WhiteAlpha==0,"等待Esc回同批清白幕");yield return Capture(size.x+"x"+size.y+"_single_summary");
        }
        Resize(1280,720);yield return null;yield return null;ui.RequestSummon(1);yield return null;yield return Handwrite(ui,state);
        Require(state.Currency==29&&ui.CommittedSummonRequests==1,"手写统一Draw保持");ui.SkipPresentation();yield return null;yield return null;
        ui.RequestSummon(10);ui.SkipSigning();yield return WaitStage(ui,SummonFlowStage.Ceremony,2);Require(state.Currency==19&&ui.CommittedSummonRequests==2,"补签统一十连Draw保持");
        var batch=ui.PresentationBatch.ToArray();ui.SkipPresentation();yield return null;yield return null;Require(ui.PresentationBatch.SequenceEqual(batch),"演出跳过同批");yield return Capture("1280x720_ten_summary_without_rail");
        Click(Page(ui,"Summary").GetComponentsInChildren<Button>().Single(x=>x.name.StartsWith("查看图鉴")));Require(Get<string>(ui,"currentScreen")=="Collection","总览保留图鉴入口");ui.HandleEscape();Require(Get<string>(ui,"currentScreen")=="Summon","图鉴返回召唤保持");
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckReport{checks=checks.ToArray(),audioCues=ui.GetComponent<SummonAudio>().Evidence.ToArray(),currency=state.Currency,totalPulls=state.TotalPulls,commits=ui.CommittedSummonRequests},true));
        Complete("PASS: Main新揭晓顺序、首末张新确认、动画仅补完当前张、三窗口、删除导航与保留图鉴入口、真实签约Draw与同批跳过；实际带声片段。");
    }
''' +s[b:]
(root/'这次一定出/Assets/_Game/Editor/P05Fix05Setup.cs').write_text(s,encoding='utf-8')
# Build standalone runtime/editor response files without changing previous evidence.
out=root/'Docs/Production/P05_FIX05_Verification';assets=root/'这次一定出/Assets/_Game'
for kind in ['runtime','editor']:
 old=(root/f'Docs/Production/P05_CRYSTAL_Verification/{kind}.rsp').read_text(encoding='utf-8-sig').splitlines()
 lines=[l for l in old if not (l.startswith('"') and l.endswith('.cs"')) and not l.startswith('/out:') and not ('/reference:' in l and 'P05_CRYSTAL_Verification' in l)]
 lines.append('/out:"'+str(out/f'{kind}.dll')+'"')
 if kind=='editor':lines.append('/reference:"'+str(out/'runtime.dll')+'"')
 folders=[assets/'Scripts'] if kind=='runtime' else [assets/'Editor',assets/'UI/Editor']
 lines+=['"'+str(p)+'"' for f in folders for p in f.rglob('*.cs')]
 (out/f'{kind}.rsp').write_text('\n'.join(lines)+'\n',encoding='utf-8')
import shutil
shutil.copy2(root/'Docs/Production/P05_CRYSTAL_Verification/encode_clips.py',out/'encode_clips.py')
