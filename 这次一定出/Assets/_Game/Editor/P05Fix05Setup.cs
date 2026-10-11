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
public static class P05Fix05Setup
{
    private const string ScenePath="Assets/_Game/Scenes/Main.unity", ConfigPath="Assets/_Game/Data/Presentation/P05FeelConfig.asset", Flag="ThisTime.Fix05Verify";
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P05_FIX05_Verification");}}
    private static string Request {get{return Path.Combine(Reports,"Fix05.request");}}
    private static bool testing; private static double started; private static EditorWindow gameView;
    private static readonly List<string> checks=new List<string>();
    static P05Fix05Setup(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=PlayChanged;}
    [MenuItem("Tools/这次一定出/程序/P05-FIX05/接入素材声音并保存Main")]
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

        const string crystal="Assets/_Game/Art/Effects/F04/Crystal/";
        foreach(string p in Directory.GetFiles(crystal,"*.png",SearchOption.AllDirectories))ImportSprite(p.Replace('\\','/'));
        config.crystalInteraction=true;config.crystalBackground=Sprite(crystal+"background_giant_rings_720p_v004.png");
        config.crystalIntact=Sprite(crystal+"States/00_intact_v004.png");config.crystalCracks=Sprite(crystal+"States/01_first_cracks_v004.png");config.crystalDeep=Sprite(crystal+"States/02_deep_cracks_v004.png");config.crystalLeak=Sprite(crystal+"Effects/light_leak_v004.png");
        config.crystalRupture=Enumerable.Range(0,24).Select(i=>Sprite(crystal+"Rupture/crystal_"+i.ToString("D3")+".png")).ToArray();
        config.voiceFirst=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/F04/voice_first.wav");config.voiceSecond=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/F04/voice_second.wav");config.voiceFinal=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/F04/voice_final.wav");
        config.finalChargeSeconds=1.60f;EditorUtility.SetDirty(config);
        var scene=EditorSceneManager.OpenScene(ScenePath);var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();
        if(ui==null||ui.portraits.Count!=6||ui.theme==null)throw new InvalidOperationException("Main既有主题/六卡入口缺失，停止接入。");
        ui.feelConfig=config;ui.signatureFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Game/UI/Resources/Fonts/LXGWWenKai-Regular.ttf");
        ui.useFixedMock=false;ui.autoSignatureSeconds=.4f;ui.ceremonySeconds=6.8f;
        ui.normalRevealSeconds=.75f;ui.srRevealSeconds=1.1f;ui.ssrRevealSeconds=2.2f;ui.urRevealSeconds=3.2f;
        ui.portraits.Single(p=>p.characterId=="UR001").portrait=Sprite(ur);
        EditorUtility.SetDirty(ui);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Write("ready","Main经Editor保存：v004水晶/24帧、透明冰法师、P05连续蓄能、分段中文旁白；U03字体与六卡保持。");
    }
    public static void Boot(){Prepare();EditorApplication.delayCall+=Verify;}
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
    [MenuItem("Tools/这次一定出/程序/P05-FIX05/验证Main输入音画并录制片段")]
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
        Write("running","Main实际Play：SSR慢点/高频/停手录制、真实签约单抽十连、四阶段清理与三尺寸。");
        var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();ui.StartCoroutine(Guard(Check(ui)));
    }
    private static IEnumerator Check(PrototypeGame ui)
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
        yield return new WaitForSecondsRealtime(.3f);Require(ui.RevealIndex==1,"不穿透下一张");Click(Get<Button>(ui,"confirmCardButton"));yield return WaitStage(ui,SummonFlowStage.Revealing,2);yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,3);
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
    private static void CrystalPress(PrototypeGame ui)
    {
        var obj=Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(x=>x.name=="AdvanceSurface").gameObject;
        var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits[0].gameObject==obj,"水晶输入首命中");
        ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerClickHandler);
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
    {yield return null;yield return null;Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Reports,name+".png"));yield return null;yield return null;}
    private static IEnumerator Guard(IEnumerator routine)
    {while(true){bool moved=false;Exception failure=null;try{moved=routine.MoveNext();}catch(Exception e){failure=e;}if(failure!=null){Complete("FAIL: "+failure);yield break;}if(!moved)yield break;var nested=routine.Current as IEnumerator;if(nested!=null){yield return Guard(nested);if(!testing)yield break;}else yield return routine.Current;}}
    private static void Require(bool good,string label){if(!good)throw new InvalidOperationException(label);checks.Add(label);}
    private static void Complete(string result)
    {
        testing=false;foreach(var recorder in UnityEngine.Object.FindObjectsOfType<P05Fix05Recorder>())recorder.End();
        Write(result.StartsWith("PASS")?"passed":"failed",result);File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);
        EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();
    }
    private static void Write(string stage,string result)
    {Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,scene=SceneManager.GetActiveScene().path,unityVersion=Application.unityVersion,atUtc=DateTime.UtcNow.ToString("o")},true));Debug.Log("[P05-FIX05] "+result);}
    [Serializable]private sealed class Report{public string stage,result,scene,unityVersion,atUtc;}
    [Serializable]private sealed class CheckReport{public string[] checks;public SummonAudio.CueEvidence[] audioCues;public int currency,totalPulls,commits;}
}

// Editor-only evidence capture: records this Unity listener's mix, never system microphone/other apps.
public sealed class P05Fix05Recorder : MonoBehaviour
{
    private bool active; private string folder;private readonly object gate=new object();
    private readonly List<float> samples=new List<float>();private readonly List<double> frames=new List<double>();
    private System.Diagnostics.Stopwatch watch;private double firstAudio=-1;private int rate,channels;private Coroutine routine;
    public void Begin(string name)
    {
        End();folder=Path.Combine(P05Fix05Setup.Reports,name+"_capture");Directory.CreateDirectory(folder);
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
