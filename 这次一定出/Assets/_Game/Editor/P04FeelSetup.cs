using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class P04FeelSetup
{
    private const string ScenePath="Assets/_Game/Scenes/Main.unity", ConfigPath="Assets/_Game/Data/Presentation/P04FeelConfig.asset", Flag="ThisTime.P04FeelVerify";
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P04_FEEL_Verification");}}
    private static string Request {get{return Path.Combine(Reports,"P04.request");}}
    private static bool testing; private static double started; private static EditorWindow gameView;
    private static readonly List<string> checks=new List<string>();
    static P04FeelSetup(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=PlayChanged;}
    [MenuItem("Tools/这次一定出/程序/P04-FEEL/接入素材声音并保存Main")]
    public static void Prepare()
    {
        if(!Ready())return;
        AssetDatabase.Refresh();
        const string art="Assets/_Game/Art/Effects/F03/";
        string[] layers={"contract_paper_v001","contract_border_v001","star_seal_anima_v001","ring_giant_v001","ring_outer_v001","ring_inner_v001","shockwave_v001","light_pillar_v001"};
        foreach(string name in layers)ImportSprite(art+name+".png");
        const string ur="Assets/_Game/Art/Characters/UR001/RPG/UR001_fire_card_base_v001.png";ImportSprite(ur);
        foreach(string path in Directory.GetFiles("Assets/_Game/Audio/P04","*.wav"))
        {
            AudioImporter importer=AssetImporter.GetAtPath(path.Replace('\\','/')) as AudioImporter;
            var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;
            settings.preloadAudioData=true;importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
        }
        var config=AssetDatabase.LoadAssetAtPath<P04FeelConfig>(ConfigPath);
        if(config==null){config=ScriptableObject.CreateInstance<P04FeelConfig>();AssetDatabase.CreateAsset(config,ConfigPath);}
        config.contractPaper=Sprite(art+layers[0]+".png");config.contractBorder=Sprite(art+layers[1]+".png");config.starSeal=Sprite(art+layers[2]+".png");
        config.giantRing=Sprite(art+layers[3]+".png");config.outerRing=Sprite(art+layers[4]+".png");config.innerRing=Sprite(art+layers[5]+".png");
        config.shockwave=Sprite(art+layers[6]+".png");config.lightPillar=Sprite(art+layers[7]+".png");
        config.narration=Clip("sacred_narration");config.ceremonyBed=Clip("ceremony_bed");config.sealSound=Clip("seal_impact");config.gateSound=Clip("gate_burst");
        config.rReveal=Clip("r_reveal");config.srReveal=Clip("sr_reveal");config.ssrCharge=Clip("ssr_charge");config.ssrReveal=Clip("ssr_reveal");
        config.urCharge=Clip("ur_charge");config.urReveal=Clip("ur_reveal");config.confirm=Clip("confirm");config.skip=Clip("skip");EditorUtility.SetDirty(config);
        var scene=EditorSceneManager.OpenScene(ScenePath);var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();
        if(ui==null||ui.portraits.Count!=6||ui.theme==null)throw new InvalidOperationException("Main既有主题/六卡入口缺失，停止接入。");
        ui.feelConfig=config;ui.signatureFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Game/UI/Resources/Fonts/LXGWWenKai-Regular.ttf");
        ui.useFixedMock=false;ui.autoSignatureSeconds=.4f;ui.ceremonySeconds=6.8f;
        ui.normalRevealSeconds=.75f;ui.srRevealSeconds=1.1f;ui.ssrRevealSeconds=2.2f;ui.urRevealSeconds=3.2f;
        ui.portraits.Single(p=>p.characterId=="UR001").portrait=Sprite(ur);
        EditorUtility.SetDirty(ui);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Write("ready","Main经Editor保存：U03内置字体/纯净UI、P03状态流程、A05八层/UR完整卡、中文旁白及合成样音。");
    }
    private static void ImportSprite(string path)
    {
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new FileNotFoundException(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;
        importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
    private static Sprite Sprite(string path){var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null)throw new FileNotFoundException(path);return sprite;}
    private static AudioClip Clip(string name){var clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/P04/"+name+".wav");if(clip==null)throw new FileNotFoundException(name);return clip;}
    private static bool Ready()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||Enumerable.Range(0,SceneManager.sceneCount).Any(i=>SceneManager.GetSceneAt(i).isDirty))
        {Write("blocked","请先停止Play并保存当前编辑，避免覆盖未完成工作。");return false;}return true;
    }
    [MenuItem("Tools/这次一定出/程序/P04-FEEL/验证Main输入音画并录制片段")]
    public static void Verify()
    {
        if(!Ready())return;EditorSceneManager.OpenScene(ScenePath);SessionState.SetBool(Flag,true);Write("starting","请求Main实际Play、内部混音记录及实时画面片段。");EditorApplication.EnterPlaymode();
    }
    private static void Poll()
    {
        if(testing&&EditorApplication.isPlaying){if(gameView!=null)gameView.Repaint();EditorApplication.QueuePlayerLoopUpdate();}
        if(testing&&EditorApplication.timeSinceStartup-started>240)Complete("FAIL: P04 Main Play超时。");
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(Request))return;
        string command=File.ReadAllText(Request).Trim();File.Delete(Request);
        try{if(command=="prepare")Prepare();else if(command=="verify")Verify();else if(command=="status")Write("observed","play="+EditorApplication.isPlaying+"; dirty="+SceneManager.GetActiveScene().isDirty);}
        catch(Exception error){Write("failed",error.ToString());}
    }
    private static void PlayChanged(PlayModeStateChange change)
    {
        if(change!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
        SessionState.SetBool(Flag,false);testing=true;started=EditorApplication.timeSinceStartup;checks.Clear();
        Application.runInBackground=true;EditorApplication.isPaused=false;
        Resize(1280,720);gameView=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        Write("running","Main实际Play：手写单抽、补签十连、四档原序、阶段输入、声音清理与三尺寸。");
        var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();ui.StartCoroutine(Guard(Check(ui)));
    }
    private static IEnumerator Check(PrototypeGame ui)
    {
        yield return null;yield return null;
        var state=Get<IPrototypeUIAdapter>(ui,"game");var sound=ui.GetComponent<SummonAudio>();
        Require(!ui.useFixedMock&&state.Currency==30&&ui.feelConfig!=null&&ui.signatureFont!=null,"Main真实内存/配置/内置字体");
        Require(UnityEngine.Object.FindObjectsOfType<AudioListener>().Length==1,"唯一AudioListener");
        Require(ui.feelConfig.narration!=null&&Mathf.Abs(ui.feelConfig.narration.length-6.8f)<.05f,"实际中文旁白6.8秒音轨引用");
        ScanClean(ui);
        Click(Get<Button>(ui,"singleButton"));Require(state.Currency==30&&ui.FlowStage==SummonFlowStage.Signing,"召唤先签署不扣费");ui.HandleEscape();Require(state.Currency==30,"签署Esc取消无扣费");
        ui.playerDisplayName="星旅者 Luna";
        var recorder=UnityEngine.Object.FindObjectOfType<AudioListener>().gameObject.AddComponent<P04PlayRecorder>();
        recorder.Begin("main_single_handwritten");
        Click(Get<Button>(ui,"singleButton"));yield return new WaitForSecondsRealtime(.2f);
        yield return Handwrite(ui,state);Require(state.Currency==29&&ui.CommittedSummonRequests==1,"手写松笔只扣一次");
        yield return Capture("1280x720_signature_gold");
        yield return WaitStage(ui,SummonFlowStage.Revealing,8);
        Require(sound.Evidence.Any(e=>e.cue=="seal_impact")&&sound.Evidence.Any(e=>e.cue=="gate_burst"),"共同仪式盖印和爆开声音实发");
        yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);yield return new WaitForSecondsRealtime(.65f);
        Require(ui.RevealIndex==0&&ui.FlowStage==SummonFlowStage.AwaitingConfirm,"单抽最后一张不自动推进");
        Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,SummonFlowStage.Summary,2);yield return new WaitForSecondsRealtime(.3f);recorder.End();
        ScanClean(ui);yield return Capture("1280x720_single_summary");
        recorder.Begin("main_ten_auto_signed");Click(Get<Button>(ui,"summaryTenButton"));
        Require(state.Currency==29,"十连签署前不扣费");yield return null;yield return null;Click(Page(ui,"Signing").GetComponentsInChildren<Button>().Single(b=>b.name=="跳过"));
        Require(ui.FlowStage==SummonFlowStage.AutoSigning&&ui.AutoSignatureText.text==ui.playerDisplayName,"签署跳过补签配置姓名，未直达总览");
        yield return WaitStage(ui,SummonFlowStage.Ceremony,2);Require(state.Currency==19&&ui.CommittedSummonRequests==2,"补签后自动统一十连Draw");
        var batch=ui.PresentationBatch.ToArray();var owned=state.Cards.Select(c=>state.CountOf(c.Id)).ToArray();
        yield return new WaitForSecondsRealtime(3.0f);yield return Capture("1280x720_giant_rings");
        yield return WaitStage(ui,SummonFlowStage.Revealing,5);
        for(int i=0;i<batch.Length;i++)
        {
            Require(ui.RevealIndex==i&&ReferenceEquals(ui.CurrentPull,batch[i]),"真实十连原序:"+i);
            if(i==2){Click(Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(b=>b.name=="AdvanceSurface"));yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);Require(ui.RevealIndex==i,"动画点击只完成当前张");}
            else yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);
            yield return new WaitForSecondsRealtime(.4f);Require(ui.RevealIndex==i&&ui.FlowStage==SummonFlowStage.AwaitingConfirm,"逐张停留:"+i);
            if(i==9){yield return Capture("1280x720_last_wait");Require(Get<Button>(ui,"confirmCardButton").GetComponentInChildren<Text>().text=="查看结果","第十张明确等待查看结果");}
            Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,i==9?SummonFlowStage.Summary:SummonFlowStage.Revealing,2);
        }
        Require(ui.PresentationBatch.SequenceEqual(batch)&&owned.SequenceEqual(state.Cards.Select(c=>state.CountOf(c.Id)))&&state.Currency==19,"演出保留同批NEW收藏与扣费");
        yield return new WaitForSecondsRealtime(.35f);recorder.End();yield return Capture("1280x720_ten_summary");
        var fixedBatch=new List<PrototypePull>{Pull(state,"R001",false),Pull(state,"SR001",true),Pull(state,"SSR001",true),Pull(state,"UR001",true)};
        recorder.Begin("main_four_tiers_fixed");ui.PlayBatchForDebug(fixedBatch);yield return WaitStage(ui,SummonFlowStage.Revealing,8);
        for(int i=0;i<4;i++)
        {
            yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);Require(ui.CurrentPull.Card==fixedBatch[i].Card,"四档固定演出:"+i);
            yield return Capture("1280x720_"+fixedBatch[i].Card.Rarity+"_wait");yield return new WaitForSecondsRealtime(.75f);
            Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,i==3?SummonFlowStage.Summary:SummonFlowStage.Revealing,2);
        }
        yield return new WaitForSecondsRealtime(.2f);recorder.End();
        foreach(var tier in new[]{CardRarity.R,CardRarity.SR,CardRarity.SSR,CardRarity.UR})Require(sound.Evidence.Any(e=>e.cue=="reveal_"+tier),"四档揭晓声音实发:"+tier);
        // 三处跳过均消费同批，不重新抽取；音源的淡出允许一个很短的尾端。
        for(int phase=0;phase<3;phase++)
        {
            ui.PlayBatchForDebug(fixedBatch);yield return null;yield return null;
            if(phase>0){ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.Revealing,2);}
            if(phase==2){ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);yield return null;}
            if(phase==2)ui.HandleEscape();else Click(Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(b=>b.name=="跳过"));
            Require(ui.FlowStage==SummonFlowStage.Summary&&ui.PresentationBatch.SequenceEqual(fixedBatch),"阶段跳过同批:"+phase);
            yield return new WaitForSecondsRealtime(.18f);Require(!sound.NarrationPlaying&&!sound.ChargePlaying&&!sound.CeremonyBedPlaying,"跳过后旁白蓄力无拖尾:"+phase);
        }
        foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1600,1000)})
        {
            Resize(size.x,size.y);yield return null;yield return null;
            Click(Get<Button>(ui,"summaryTenButton"));yield return Capture(size.x+"x"+size.y+"_signing");ui.CancelSigning();
            ui.PlayBatchForDebug(new List<PrototypePull>{fixedBatch[2],fixedBatch[3]});
            yield return new WaitForSecondsRealtime(2.8f);yield return Capture(size.x+"x"+size.y+"_ceremony");ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.Revealing,2);
            for(int i=0;i<2;i++){yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);yield return Capture(size.x+"x"+size.y+"_"+ui.CurrentPull.Card.Rarity+"_wait");CheckBounds(ui);yield return null;Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,i==1?SummonFlowStage.Summary:SummonFlowStage.Revealing,2);}
            ScanClean(ui);yield return Capture(size.x+"x"+size.y+"_summary");
        }
        Require(state.Currency==19&&state.TotalPulls==11&&ui.CommittedSummonRequests==2&&owned.SequenceEqual(state.Cards.Select(c=>state.CountOf(c.Id))),"调试演出与多窗口不改真实内存");
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckReport{checks=checks.ToArray(),audioCues=sound.Evidence.ToArray(),currency=state.Currency,totalPulls=state.TotalPulls,commits=ui.CommittedSummonRequests},true));
        Resize(1280,720);Complete("PASS: Main手写/补签/唯一Draw，真实十连原序每张及最后确认，四档演出实发声音，三阶段跳过清音，三窗口安全区与纯净文案，三段实际Play画面/内部混音已录制待封装。");
    }
    private static PrototypePull Pull(IPrototypeUIAdapter state,string id,bool fresh){return new PrototypePull{Card=state.Cards.Single(c=>c.Id==id),IsNew=fresh};}
    private static IEnumerator Handwrite(PrototypeGame ui,IPrototypeUIAdapter state)
    {
        Canvas.ForceUpdateCanvases();var pad=ui.SignaturePad;var pointer=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left};
        for(int i=0;i<25;i++)
        {
            float u=i/24f;Vector2 local=pad.rectTransform.rect.min+Vector2.Scale(new Vector2(.15f+u*.65f,.5f+Mathf.Sin(u*15)*.2f),pad.rectTransform.rect.size);
            pointer.position=pad.rectTransform.TransformPoint(local);
            if(i==0){var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Require(hits.Count>0&&hits[0].gameObject==pad.gameObject,"手写区真实首命中");ExecuteEvents.Execute(pad.gameObject,pointer,ExecuteEvents.pointerDownHandler);}
            else ExecuteEvents.Execute(pad.gameObject,pointer,ExecuteEvents.dragHandler);
            yield return null;
        }
        Require(state.Currency==30&&pad.Points.Count>20,"可见轨迹松笔前不扣费");ExecuteEvents.Execute(pad.gameObject,pointer,ExecuteEvents.pointerUpHandler);
    }
    private static void Click(Button button)
    {
        Canvas.ForceUpdateCanvases();var rect=button.GetComponent<RectTransform>();var pointer=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=rect.TransformPoint(rect.rect.center)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Require(button.interactable&&hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"按钮实际首命中:"+button.name+"; hit="+(hits.Count==0?"无":hits[0].gameObject.name));
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
    }
    private static void CheckBounds(PrototypeGame ui)
    {
        foreach(var button in ui.GetComponentsInChildren<Button>()){if(!button.isActiveAndEnabled)continue;var corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);Require(corners[0].x>=-1&&corners[0].y>=-1&&corners[2].x<=Screen.width+1&&corners[2].y<=Screen.height+1,"安全区:"+Screen.width+"x"+Screen.height+":"+button.name);}
    }
    private static void ScanClean(PrototypeGame ui)
    {foreach(var text in ui.GetComponentsInChildren<Text>()){foreach(string bad in new[]{"尚未接入","本次运行","UI MOCK","占位","校准","SSR001","UR001","这次一定出","预览"})Require(!text.text.Contains(bad),"纯净文案:"+text.name+":"+bad);}}
    private static GameObject Page(PrototypeGame ui,string name){return Get<Dictionary<string,GameObject>>(ui,"screens")[name];}
    private static T Get<T>(PrototypeGame ui,string name){return (T)typeof(PrototypeGame).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);}
    private static void Resize(int width,int height){typeof(U01Setup).GetMethod("Resize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{width,height});}
    private static IEnumerator WaitStage(PrototypeGame ui,SummonFlowStage target,float seconds)
    {float end=Time.realtimeSinceStartup+seconds;while(ui.FlowStage!=target&&Time.realtimeSinceStartup<end)yield return null;if(ui.FlowStage!=target)throw new InvalidOperationException("阶段等待:"+target+" 实际:"+ui.FlowStage);}
    private static IEnumerator Capture(string name)
    {yield return null;yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Reports,name+".png"));yield return null;yield return null;}
    private static IEnumerator Guard(IEnumerator routine)
    {while(true){bool moved=false;Exception failure=null;try{moved=routine.MoveNext();}catch(Exception e){failure=e;}if(failure!=null){Complete("FAIL: "+failure);yield break;}if(!moved)yield break;var nested=routine.Current as IEnumerator;if(nested!=null){yield return Guard(nested);if(!testing)yield break;}else yield return routine.Current;}}
    private static void Require(bool good,string label){if(!good)throw new InvalidOperationException(label);checks.Add(label);}
    private static void Complete(string result)
    {
        testing=false;foreach(var recorder in UnityEngine.Object.FindObjectsOfType<P04PlayRecorder>())recorder.End();
        Write(result.StartsWith("PASS")?"passed":"failed",result);File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);
        EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();
    }
    private static void Write(string stage,string result)
    {Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,scene=SceneManager.GetActiveScene().path,unityVersion=Application.unityVersion,atUtc=DateTime.UtcNow.ToString("o")},true));Debug.Log("[P04-FEEL] "+result);}
    [Serializable]private sealed class Report{public string stage,result,scene,unityVersion,atUtc;}
    [Serializable]private sealed class CheckReport{public string[] checks;public SummonAudio.CueEvidence[] audioCues;public int currency,totalPulls,commits;}
}

// Editor-only evidence capture: records this Unity listener's mix, never system microphone/other apps.
public sealed class P04PlayRecorder : MonoBehaviour
{
    private bool active; private string folder;private readonly object gate=new object();
    private readonly List<float> samples=new List<float>();private readonly List<double> frames=new List<double>();
    private System.Diagnostics.Stopwatch watch;private double firstAudio=-1;private int rate,channels;private Coroutine routine;
    public void Begin(string name)
    {
        End();folder=Path.Combine(P04FeelSetup.Reports,name+"_capture");Directory.CreateDirectory(folder);
        lock(gate){samples.Clear();firstAudio=-1;}frames.Clear();rate=AudioSettings.outputSampleRate;watch=System.Diagnostics.Stopwatch.StartNew();active=true;routine=StartCoroutine(Record());
    }
    private IEnumerator Record()
    {
        double next=0;
        while(active)
        {
            yield return new WaitForEndOfFrame();if(!active)yield break;
            double elapsed=watch.Elapsed.TotalSeconds;if(elapsed<next)continue;next=elapsed+1.0/24;
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,frames.Count.ToString("D5")+".jpg"),image.EncodeToJPG(83));Destroy(image);frames.Add(elapsed);
        }
    }
    private void OnAudioFilterRead(float[] data,int count)
    {lock(gate){if(!active)return;if(firstAudio<0)firstAudio=watch.Elapsed.TotalSeconds;channels=count;samples.AddRange(data);}}
    public void End()
    {
        if(!active)return;active=false;if(routine!=null)StopCoroutine(routine);routine=null;
        float[] pcm;lock(gate){pcm=samples.ToArray();}
        double duration=watch.Elapsed.TotalSeconds;
        using(var writer=new BinaryWriter(File.Create(Path.Combine(folder,"unity_listener_mix.wav"))))
        {
            int count=pcm.Length*2;writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);writer.Write((short)1);writer.Write((short)Math.Max(1,channels));writer.Write(rate);writer.Write(rate*Math.Max(1,channels)*2);writer.Write((short)(Math.Max(1,channels)*2));writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count);foreach(float value in pcm)writer.Write((short)(Mathf.Clamp(value,-1,1)*32767));
        }
        File.WriteAllText(Path.Combine(folder,"capture.json"),JsonUtility.ToJson(new CaptureReport{frameTimes=frames.ToArray(),duration=duration,audioStart=firstAudio,sampleRate=rate,channels=channels,samples=pcm.Length,peak=pcm.Length==0?0:pcm.Max(v=>Mathf.Abs(v))},true));
    }
    private void OnDisable(){End();}
    [Serializable]private sealed class CaptureReport{public double[] frameTimes;public double duration,audioStart;public int sampleRate,channels,samples;public float peak;}
}
