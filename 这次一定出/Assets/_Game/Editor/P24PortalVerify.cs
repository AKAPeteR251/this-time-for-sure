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
public static class P24PortalVerify
{
    private const string ScenePath="Assets/_Game/Scenes/Main.unity", ConfigPath="Assets/_Game/Data/Presentation/P05FeelConfig.asset", Flag="ThisTime.PortalVerifyVerify";
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P24_PORTAL_CAMERA_Verification");}}
    private static string Request {get{return Path.Combine(Reports,"PortalVerify.request");}}
    private static bool testing; private static double started; private static EditorWindow gameView;
    private static readonly List<string> checks=new List<string>();
    static P24PortalVerify(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=PlayChanged;}
    public static void Prepare(){P24PortalSetup.Prepare();}
    private static bool Ready()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||Enumerable.Range(0,SceneManager.sceneCount).Any(i=>SceneManager.GetSceneAt(i).isDirty))
        {Write("blocked","请先停止Play并保存当前编辑，避免覆盖未完成工作。");return false;}return true;
    }
    [MenuItem("Tools/这次一定出/程序/P24-PORTAL-CAMERA/验证Main输入音画并录制片段")]
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
        Write("running","Main实际Play：法阵穿越、相机推进、三尺寸、输入和跳过清场。");
        var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();ui.StartCoroutine(Guard(Check(ui)));
    }
    private static IEnumerator Check(PrototypeGame ui)
    {
        yield return null;yield return null;
        var state=Get<IPrototypeUIAdapter>(ui,"game");var cfg=ui.feelConfig;
        Require(cfg.crystalPortalShader!=null&&cfg.crystalPortalCamera,"运镜与中心开口Shader配置");
        var recorder=UnityEngine.Object.FindObjectOfType<AudioListener>().gameObject.AddComponent<P24PortalRecorder>();recorder.Begin("main_p24_portal_camera");
        ui.RequestSummon(1);yield return null;Click(Page(ui,"Signing").GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>().text=="跳过"));
        yield return WaitStage(ui,SummonFlowStage.Ceremony,2);
        Require(state.Currency==29&&ui.CommittedSummonRequests==1,"补签后唯一Draw与扣费");
        var stage=ui.CrystalView.Stage3D;var ceremony=Get<ContractCeremonyView>(ui,"ceremonyView");
        var samples=new List<PortalSample>();bool first=false,middle=false,last=false;
        while(ui.FlowStage==SummonFlowStage.Ceremony)
        {
            float blend=Mathf.InverseLerp(cfg.preludeSeconds-cfg.crystalTransitionSeconds,cfg.preludeSeconds,ceremony.Progress*cfg.preludeSeconds);
            if(blend>0)
            {
                samples.Add(new PortalSample{progress=blend,entry=stage.Entry,position=stage.StageCamera.transform.localPosition,fov=stage.StageCamera.fieldOfView,ringScale=Page(ui,"Reveal").transform.Find("CeremonyStage/RitualWorld").localScale.x});
                ui.HitCrystal();Require(ui.CrystalView.Energy==0&&ui.CrystalView.AcceptedHits==0,"运镜期间点击不提前蓄能");
                if(!first&&blend>.12f){first=true;yield return Capture("1280x720_portal_start");}
                if(!middle&&blend>.48f){middle=true;yield return Capture("1280x720_portal_middle");}
                if(!last&&blend>.79f){last=true;yield return Capture("1280x720_portal_end");}
            }
            yield return null;
        }
        Require(first&&middle&&last&&samples.Count>8,"连续实际镜头采样");
        Require(Vector3.Distance(samples[0].position,samples[samples.Count-1].position)>2,"真实3D相机连续推进");
        Require(samples[samples.Count-1].ringScale>2.8f,"2D法阵放大到画面外");
        Require(ui.FlowStage==SummonFlowStage.CrystalCharging&&Vector3.Distance(stage.StageCamera.transform.localPosition,cfg.altarCameraClose)<.001f,"平稳落到原有水晶镜头");
        Require(Page(ui,"Reveal").transform.Find("CeremonyStage/RitualWorld").localScale==Vector3.one,"2D法阵完成后复位");
        yield return new WaitForSecondsRealtime(.6f);yield return Capture("1280x720_altar_ready");
        CrystalPress(ui);yield return new WaitForSecondsRealtime(.6f);Require(ui.CrystalView.AcceptedHits==1&&ui.CrystalView.Energy>0,"运镜完成可点击水晶");
        var same=ui.PresentationBatch.ToArray();ui.HandleEscape();yield return null;
        Require(ui.FlowStage==SummonFlowStage.Summary&&!stage.Rendering&&ui.CrystalView.WhiteAlpha==0&&ui.PresentationBatch.SequenceEqual(same),"充能Esc清场同批");
        recorder.End();File.WriteAllText(Path.Combine(Reports,"camera_samples.json"),JsonUtility.ToJson(new PortalSamples{samples=samples.ToArray()},true));
        var fixedBatch=new List<PrototypePull>{Pull(state,"SSR001",true)};
        foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1600,1000)})
        {
            Resize(size.x,size.y);yield return null;yield return null;ui.PlayBatchForDebug(fixedBatch);yield return null;
            while(ui.FlowStage==SummonFlowStage.Ceremony&&ceremony.Progress<(cfg.preludeSeconds-cfg.crystalTransitionSeconds*.5f)/cfg.preludeSeconds)yield return null;
            yield return Capture(size.x+"x"+size.y+"_portal_middle");CheckBounds(ui);
            ui.SkipPresentation();yield return null;
            Require(ui.FlowStage==SummonFlowStage.Summary&&!stage.Rendering&&ui.CrystalView.WhiteAlpha==0,"多窗口运镜跳过清场");
        }
        Resize(1280,720);yield return null;yield return null;
        ui.PlayBatchForDebug(fixedBatch);yield return null;ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
        Require(Vector3.Distance(stage.StageCamera.transform.localPosition,cfg.altarCameraClose)<.001f,"点击补完前奏相机直接落终点");ui.SkipPresentation();yield return null;
        Require(state.Currency==29&&ui.CommittedSummonRequests==1,"运镜补完与跳过不重新计算");
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckReport{checks=checks.ToArray(),audioCues=ui.GetComponent<SummonAudio>().Evidence.ToArray(),currency=state.Currency,totalPulls=state.TotalPulls,commits=ui.CommittedSummonRequests},true));
        Complete("PASS: Main法阵中心穿越/真实3D相机连续推进/终点复位/转场不蓄能/完成可连击/三窗口/跳过与点击补完清场；带声片段，视觉待验收。");
    }
    [Serializable]private sealed class PortalSample{public float progress,entry,fov,ringScale;public Vector3 position;}
    [Serializable]private sealed class PortalSamples{public PortalSample[] samples;}
    [Serializable]private sealed class LightObservation{public float age,energy,strength;public int sheets;}
    [Serializable]private sealed class LightObservations{public LightObservation[] samples;}
    private static void Blank(PrototypeGame ui)
    {
        var obj=Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(b=>b.name=="AdvanceSurface").gameObject;
        var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=new Vector2(Screen.width*.12f,Screen.height*.80f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits[0].gameObject==obj,"空白区域真实射线命中");
        ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerClickHandler);
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
        testing=false;foreach(var recorder in UnityEngine.Object.FindObjectsOfType<P24PortalRecorder>())recorder.End();
        Write(result.StartsWith("PASS")?"passed":"failed",result);File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);
        EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();
    }
    private static void Write(string stage,string result)
    {Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,scene=SceneManager.GetActiveScene().path,unityVersion=Application.unityVersion,atUtc=DateTime.UtcNow.ToString("o")},true));Debug.Log("[P24-PORTAL-CAMERA] "+result);}
    [Serializable]private sealed class SkyObservation{public float seed;public RevealSky.MeteorFlight[] flights;}
    [Serializable]private sealed class Report{public string stage,result,scene,unityVersion,atUtc;}
    [Serializable]private sealed class CheckReport{public string[] checks;public SummonAudio.CueEvidence[] audioCues;public int currency,totalPulls,commits;}
}

// Editor-only evidence capture: records this Unity listener's mix, never system microphone/other apps.
public sealed class P24PortalRecorder : MonoBehaviour
{
    private bool active; private string folder;private readonly object gate=new object();
    private readonly List<float> samples=new List<float>();private readonly List<double> frames=new List<double>();
    private System.Diagnostics.Stopwatch watch;private double firstAudio=-1;private int rate,channels;private Coroutine routine;
    public void Begin(string name)
    {
        End();folder=Path.Combine(P24PortalVerify.Reports,name+"_capture");Directory.CreateDirectory(folder);
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
