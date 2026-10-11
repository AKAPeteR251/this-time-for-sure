from pathlib import Path
import shutil,json,hashlib
root=Path(r'D:/vibe/这次一定出');proj=root/'这次一定出';out=root/'Docs/Production/P05_CRYSTAL_Verification';out.mkdir(exist_ok=True)
assets=proj/'Assets/_Game';target=assets/'Art/Effects/F04/Crystal';target.mkdir(parents=True,exist_ok=True)
src=root/'ArtSource/Scenes/SummonStage/v004_click'
for sub in ['States','Effects','Rupture']:
 (target/sub).mkdir(exist_ok=True)
 for p in (src/sub).glob('*.png'):shutil.copy2(p,target/sub/p.name)
shutil.copy2(src/'background_giant_rings_720p_v004.png',target/'background_giant_rings_720p_v004.png')
audio=assets/'Audio/F04';audio.mkdir(exist_ok=True)
for i,n in enumerate(['voice_first','voice_second','voice_final']):shutil.copy2(root/f'ArtSource/Audio/P04/voice_fitted_{i}.wav',audio/f'{n}.wav')
protected=[p for p in (assets/'Scripts/Core').glob('*.cs')]+[assets/'Scripts/UI/PrototypeUIAdapter.cs',assets/'Scripts/UI/PrototypeUITheme.cs',assets/'Scenes/UIPrototype.unity',assets/'Scenes/Main.unity.meta',assets/'UI/Prefabs/UIPrototype.prefab',proj/'ProjectSettings/EditorBuildSettings.asset']
(out/'protected_before.json').write_text(json.dumps({str(p.relative_to(root)):hashlib.sha256(p.read_bytes()).hexdigest() for p in protected},indent=2),encoding='utf-8')
s=(assets/'Editor/P04FeelSetup.cs').read_text(encoding='utf-8-sig').replace('P04FeelSetup','P05CrystalSetup').replace('P04PlayRecorder','P05PlayRecorder').replace('P04_FEEL_Verification','P05_CRYSTAL_Verification').replace('P04.request','P05.request').replace('ThisTime.P04FeelVerify','ThisTime.P05CrystalVerify').replace('P04-FEEL','P05-CRYSTAL').replace('P04FeelConfig.asset','P05FeelConfig.asset')
insert='''
        const string crystal="Assets/_Game/Art/Effects/F04/Crystal/";
        foreach(string p in Directory.GetFiles(crystal,"*.png",SearchOption.AllDirectories))ImportSprite(p.Replace('\\\\','/'));
        config.crystalInteraction=true;config.crystalBackground=Sprite(crystal+"background_giant_rings_720p_v004.png");
        config.crystalIntact=Sprite(crystal+"States/00_intact_v004.png");config.crystalCracks=Sprite(crystal+"States/01_first_cracks_v004.png");config.crystalDeep=Sprite(crystal+"States/02_deep_cracks_v004.png");config.crystalLeak=Sprite(crystal+"Effects/light_leak_v004.png");
        config.crystalRupture=Enumerable.Range(0,24).Select(i=>Sprite(crystal+"Rupture/crystal_"+i.ToString("D3")+".png")).ToArray();
        config.voiceFirst=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/F04/voice_first.wav");config.voiceSecond=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/F04/voice_second.wav");config.voiceFinal=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/F04/voice_final.wav");
        config.finalChargeSeconds=1.60f;EditorUtility.SetDirty(config);
'''
s=s.replace('        var scene=EditorSceneManager.OpenScene(ScenePath);',insert+'        var scene=EditorSceneManager.OpenScene(ScenePath);',1)
s=s.replace('    private static void ImportSprite(string path)','    public static void Boot(){Prepare();EditorApplication.delayCall+=Verify;}\n    private static void ImportSprite(string path)')
a=s.index('    private static IEnumerator Check(PrototypeGame ui)');b=s.index('    private static PrototypePull Pull',a)
s=s[:a]+'''    private static readonly List<string> inputs=new List<string>();
    private static IEnumerator Check(PrototypeGame ui)
    {
        yield return null;yield return null;
        var state=Get<IPrototypeUIAdapter>(ui,"game");var sound=ui.GetComponent<SummonAudio>();
        Require(state.Currency==30&&ui.feelConfig.crystalInteraction,"Main内存、P05配置及透明冰法师");
        var recorder=UnityEngine.Object.FindObjectOfType<AudioListener>().gameObject.AddComponent<P05PlayRecorder>();
        var ssr=Pull(state,"SSR001",true);
        foreach(bool fast in new[]{false,true})
        {
            recorder.Begin(fast?"main_crystal_high_frequency":"main_crystal_slow_pause");
            ui.PlayBatchForDebug(new List<PrototypePull>{ssr,Pull(state,"SSR001",false)});
            yield return WaitStage(ui,SummonFlowStage.CrystalCharging,6);
            int k=0;
            while(ui.CrystalView.Energy<1)
            {
                if(!fast&&k==5)
                {
                    float saved=ui.CrystalView.Energy;yield return new WaitForSecondsRealtime(1.1f);
                    Require(ui.CrystalView.Energy==saved&&ui.CrystalView.ShakeAmplitude<.01f,"停手能量保留、震动收束");
                    yield return Capture("1280x720_crystal_pause");
                }
                CrystalPress(ui);k++;
                inputs.Add((fast?"high":"slow")+" hit="+k+" energy="+ui.CrystalView.Energy.ToString("F4")+" at="+Time.realtimeSinceStartup.ToString("F3"));
                yield return new WaitForSecondsRealtime(fast?.075f:.38f);
            }
            Require(k>3,"非固定三次:"+k);
            inputs.Add((fast?"high":"slow")+" thresholdHits="+k);
            yield return WaitStage(ui,SummonFlowStage.CrystalBreaking,2);
            int n=ui.CrystalView.AcceptedHits;CrystalPress(ui);ui.AdvancePresentation();Require(ui.CrystalView.AcceptedHits==n,"终结输入不再蓄能、不穿透");
            yield return WaitStage(ui,SummonFlowStage.Revealing,3);
            yield return new WaitForSecondsRealtime(.32f);yield return Capture(fast?"1280x720_face_closeup":"1280x720_face_slow");
            yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);
            yield return new WaitForSecondsRealtime(.45f);Require(ui.RevealIndex==0,"首张等待新确认");yield return Capture("1280x720_full_portrait");
            Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,SummonFlowStage.Revealing,2);
            yield return new WaitForSecondsRealtime(.25f);Click(Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(x=>x.name=="AdvanceSurface"));
            yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);Require(ui.RevealIndex==1,"动画点击仅补完当前张");
            yield return new WaitForSecondsRealtime(.35f);Require(ui.FlowStage==SummonFlowStage.AwaitingConfirm,"末张仍等待");
            Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,SummonFlowStage.Summary,2);yield return new WaitForSecondsRealtime(.25f);recorder.End();
        }
        // 真实随机单抽和十连；指定SSR不修改随机模型或收藏。
        Click(Get<Button>(ui,"summarySingleButton"));yield return null;yield return Handwrite(ui,state);
        Require(state.Currency==29&&ui.CommittedSummonRequests==1,"手写唯一Draw");ui.SkipPresentation();yield return null;yield return null;
        Click(Get<Button>(ui,"summaryTenButton"));yield return null;yield return null;
        Click(Page(ui,"Signing").GetComponentsInChildren<Button>().Single(x=>x.name=="跳过"));
        Require(ui.FlowStage==SummonFlowStage.AutoSigning,"签署跳过补签");yield return WaitStage(ui,SummonFlowStage.Ceremony,2);
        Require(state.Currency==19&&ui.CommittedSummonRequests==2,"补签后统一十连Draw");
        var batch=ui.PresentationBatch.ToArray();var owned=state.Cards.Select(c=>state.CountOf(c.Id)).ToArray();
        ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
        while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}
        yield return WaitStage(ui,SummonFlowStage.Revealing,3);
        for(int i=0;i<10;i++)
        {
            Require(ReferenceEquals(ui.CurrentPull,batch[i]),"真实随机原序:"+i);yield return null;yield return null;ui.AdvancePresentation();
            yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);yield return new WaitForSecondsRealtime(.12f);
            Require(ui.RevealIndex==i,"每张等确认:"+i);Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,i==9?SummonFlowStage.Summary:SummonFlowStage.Revealing,2);
        }
        Require(owned.SequenceEqual(state.Cards.Select(c=>state.CountOf(c.Id)))&&ui.PresentationBatch.SequenceEqual(batch),"原批NEW收藏不变");
        recorder.Begin("main_crystal_skip_cleanup");
        for(int phase=0;phase<4;phase++)
        {
            ui.PlayBatchForDebug(new List<PrototypePull>{ssr});yield return null;yield return null;
            if(phase>0){ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);}
            if(phase>1){while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}yield return WaitStage(ui,SummonFlowStage.CrystalBreaking,2);}
            if(phase==3)yield return WaitStage(ui,SummonFlowStage.Revealing,3);
            inputs.Add("skip stage="+ui.FlowStage+" at="+Time.realtimeSinceStartup.ToString("F3"));
            if(phase%2==0)ui.HandleEscape();else Click(Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(x=>x.name=="跳过"));
            yield return new WaitForSecondsRealtime(.2f);
            Require(ui.FlowStage==SummonFlowStage.Summary&&ui.CrystalView.WhiteAlpha==0&&!sound.NarrationPlaying&&!sound.ChargePlaying&&!sound.CeremonyBedPlaying,"阶段跳过清音清白幕:"+phase);
        }
        recorder.End();
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1600,1000)})
        {
            Resize(size.x,size.y);yield return null;yield return null;
            ui.PlayBatchForDebug(new List<PrototypePull>{ssr});yield return new WaitForSecondsRealtime(.8f);yield return Capture(size.x+"x"+size.y+"_prelude");
            ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
            for(int i=0;i<8;i++){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}yield return Capture(size.x+"x"+size.y+"_crystal");
            while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}
            yield return WaitStage(ui,SummonFlowStage.Revealing,3);yield return new WaitForSecondsRealtime(.3f);yield return Capture(size.x+"x"+size.y+"_face");
            yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);yield return Capture(size.x+"x"+size.y+"_portrait");CheckBounds(ui);ScanClean(ui);ui.SkipPresentation();yield return null;yield return null;
        }
        Require(state.Currency==19&&state.TotalPulls==11&&ui.CommittedSummonRequests==2,"固定样例不Draw不改真实数据");
        Require(sound.Evidence.Any(x=>x.cue=="voice_first")&&sound.Evidence.Any(x=>x.cue=="voice_second")&&sound.Evidence.Any(x=>x.cue=="voice_final"),"分段实际旁白触发");
        File.WriteAllText(Path.Combine(Reports,"input_records.txt"),string.Join("\\n",inputs));
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckReport{checks=checks.ToArray(),audioCues=sound.Evidence.ToArray(),currency=state.Currency,totalPulls=state.TotalPulls,commits=ui.CommittedSummonRequests},true));
        Resize(1280,720);Complete("PASS: Main水晶慢点/高频/停手、终结与角色新确认、四阶段跳过清音白幕、真实单抽十连唯一Draw、三窗口、实际带声录制。");
    }
    private static void CrystalPress(PrototypeGame ui)
    {
        var obj=Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(x=>x.name=="AdvanceSurface").gameObject;
        var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits[0].gameObject==obj,"水晶输入首命中");
        ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerClickHandler);
    }
''' +s[b:]
# Existing Handwrite assumes resource30, correct test before first real Draw.
(assets/'Editor/P05CrystalSetup.cs').write_text(s,encoding='utf-8')
shutil.copy2(root/'Docs/Production/P04_FEEL_Verification/encode_clips.py',out/'encode_clips.py')
for kind in ['runtime','editor']:
 old=(root/f'Docs/Production/P04_FEEL_Verification/{kind}.rsp').read_text(encoding='utf-8-sig').splitlines()
 lines=[l for l in old if not (l.startswith('"') and l.endswith('.cs"')) and not l.startswith('/out:') and not ('/reference:' in l and 'P04_FEEL_Verification' in l)]
 lines.append('/out:"'+str(out/f'{kind}.dll')+'"')
 if kind=='editor':lines.append('/reference:"'+str(out/'runtime.dll')+'"')
 folders=[assets/'Scripts'] if kind=='runtime' else [assets/'Editor',assets/'UI/Editor']
 lines+=['"'+str(p)+'"' for f in folders for p in f.rglob('*.cs')]
 (out/f'{kind}.rsp').write_text('\n'.join(lines),encoding='utf-8')
print('P05 assets copied; editor and compile inputs prepared')
