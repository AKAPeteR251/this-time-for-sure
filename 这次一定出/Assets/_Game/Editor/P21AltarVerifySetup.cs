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
public static class P21AltarVerifySetup
{
    private const string ScenePath="Assets/_Game/Scenes/Main.unity", ConfigPath="Assets/_Game/Data/Presentation/P05FeelConfig.asset", Flag="ThisTime.AltarVerifyVerify";
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P21_ALTAR_Verification");}}
    private static string Request {get{return Path.Combine(Reports,"AltarVerify.request");}}
    private static bool testing; private static double started; private static EditorWindow gameView;
    private static readonly List<string> checks=new List<string>();
    static P21AltarVerifySetup(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=PlayChanged;}
    public static void Prepare(){P21AltarSetup.Prepare();}
    private static bool Ready()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||Enumerable.Range(0,SceneManager.sceneCount).Any(i=>SceneManager.GetSceneAt(i).isDirty))
        {Write("blocked","请先停止Play并保存当前编辑，避免覆盖未完成工作。");return false;}return true;
    }
    [MenuItem("Tools/这次一定出/程序/P21-ALTAR/验证Main输入音画并录制片段")]
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
        var fixedBatch=new List<PrototypePull>{Pull(state,"SSR001",true),Pull(state,"R002",true)};
        var recorder=UnityEngine.Object.FindObjectOfType<AudioListener>().gameObject.AddComponent<P21AltarRecorder>();recorder.Begin("main_p21_altar");
        ui.PlayBatchForDebug(fixedBatch);yield return null;
        while(ui.FlowStage==SummonFlowStage.Ceremony&&Get<ContractCeremonyView>(ui,"ceremonyView").Progress<.84f)yield return null;
        Require(ui.CrystalView.TransitionAlpha>0&&ui.CrystalView.TransitionAlpha<1,"前奏与室内舞台交叉淡化");yield return Capture("1280x720_altar_transition");
        yield return WaitStage(ui,SummonFlowStage.CrystalCharging,3);
        var stage=ui.CrystalView.Stage3D;
        Require(stage.Indoor&&stage.Rendering&&stage.EnvironmentMeshCount==127,"F08实际127网格在Main镜头绘制");
        Require(ui.CrystalView.Root.Find("CrystalLivingSky")==null,"水晶阶段移除星空，不遮盖室内环境");
        var rig=stage.StageCamera.transform.parent;var indoor=rig.GetComponent<AltarInteriorStage>();
        Require(indoor.FlameCount==4,"四火盆真实循环火焰");
        Require(stage.StageCamera.transform.localPosition.magnitude<15,"相机在封闭房间内");
        var bad=rig.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Where(m=>m==null||m.shader==null||!m.shader.isSupported).ToArray();Require(bad.Length==0,"所有运行材质Shader受支持");
        yield return Capture("1280x720_altar_idle");
        var outer=rig.Find("RingOuter");var middle=rig.Find("RingMiddle");var inner=rig.Find("RingInner");
        var o=outer.localRotation;var m=middle.localRotation;var n=inner.localRotation;var body=stage.BodyPosition;var shard=stage.FirstFloatingPosition;
        yield return new WaitForSecondsRealtime(3);
        Require(Quaternion.Angle(o,outer.localRotation)>5&&Quaternion.Angle(m,middle.localRotation)>5&&Quaternion.Angle(n,inner.localRotation)>5,"室内三环独立欧拉方向持续运动");
        Require(Vector3.Distance(body,stage.BodyPosition)>.01f&&Vector3.Distance(shard,stage.FirstFloatingPosition)>.05f,"水晶及碎片停手仍运动");
        CrystalPress(ui);yield return new WaitForSecondsRealtime(.7f);CrystalPress(ui);float kept=ui.CrystalView.Energy;
        yield return new WaitForSecondsRealtime(1);Require(kept>0&&ui.CrystalView.Energy==kept&&ui.CrystalView.ShakeAmplitude==0,"慢点停手保能收震");yield return Capture("1280x720_altar_charge_hold");
        int before=ui.CrystalView.AcceptedHits;
        while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}
        Require(ui.CrystalView.AcceptedHits>before+3,"短震期间连续接受高频点击");
        yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,8);Require(!stage.Rendering,"破裂后室内镜头灯光火焰关闭");yield return Capture("1280x720_ssr_wait");
        yield return new WaitForSecondsRealtime(.5f);Require(ui.RevealIndex==0,"首张等待新确认");Blank(ui);yield return WaitStage(ui,SummonFlowStage.Revealing,2);yield return null;yield return null;Blank(ui);
        yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);Require(ui.RevealIndex==1,"动画点击仅补完当前张");yield return new WaitForSecondsRealtime(.4f);Require(ui.RevealIndex==1,"末张仍等待确认");Blank(ui);yield return WaitStage(ui,SummonFlowStage.Summary,2);
        recorder.End();
        foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1600,1000)})
        {
            Resize(size.x,size.y);yield return null;yield return null;
            ui.PlayBatchForDebug(fixedBatch);ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
            Require(ui.CrystalView.Stage3D.Rendering,"多窗口室内相机运行");yield return Capture(size.x+"x"+size.y+"_altar");CheckBounds(ui);ScanClean(ui);
            ui.HandleEscape();yield return null;Require(ui.FlowStage==SummonFlowStage.Summary&&!stage.Rendering&&ui.CrystalView.WhiteAlpha==0,"蓄能Esc清室内与白幕");
        }
        Resize(1280,720);yield return null;yield return null;
        ui.PlayBatchForDebug(fixedBatch);yield return null;
        while(ui.FlowStage==SummonFlowStage.Ceremony&&Get<ContractCeremonyView>(ui,"ceremonyView").Progress<.84f)yield return null;
        var same=ui.PresentationBatch.ToArray();ui.SkipPresentation();yield return null;Require(ui.FlowStage==SummonFlowStage.Summary&&!stage.Rendering&&ui.PresentationBatch.SequenceEqual(same),"转场跳过清场且同批");
        ui.RequestSummon(1);yield return null;yield return Handwrite(ui,state);Require(state.Currency==29&&ui.CommittedSummonRequests==1,"手写松笔唯一Draw与扣费");ui.SkipPresentation();yield return null;yield return null;
        ui.RequestSummon(10);ui.SkipSigning();yield return WaitStage(ui,SummonFlowStage.Ceremony,2);Require(state.Currency==19&&ui.CommittedSummonRequests==2,"跳过补签唯一十连Draw");
        var batch=ui.PresentationBatch.ToArray();ui.SkipPresentation();yield return null;Require(ui.PresentationBatch.SequenceEqual(batch)&&!stage.Rendering,"真实随机同批跳过清场");
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckReport{checks=checks.ToArray(),audioCues=ui.GetComponent<SummonAudio>().Evidence.ToArray(),currency=state.Currency,totalPulls=state.TotalPulls,commits=ui.CommittedSummonRequests},true));
        Complete("PASS: Main地下圣殿127网格/四火焰/三环运动/自然转场/慢点高频停手/三窗口/签约唯一Draw/逐张等待/同批跳过清场；实际带声录制。视觉待用户验收。");
    }
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
        testing=false;foreach(var recorder in UnityEngine.Object.FindObjectsOfType<P21AltarRecorder>())recorder.End();
        Write(result.StartsWith("PASS")?"passed":"failed",result);File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);
        EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();
    }
    private static void Write(string stage,string result)
    {Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,scene=SceneManager.GetActiveScene().path,unityVersion=Application.unityVersion,atUtc=DateTime.UtcNow.ToString("o")},true));Debug.Log("[P21-ALTAR] "+result);}
    [Serializable]private sealed class SkyObservation{public float seed;public RevealSky.MeteorFlight[] flights;}
    [Serializable]private sealed class Report{public string stage,result,scene,unityVersion,atUtc;}
    [Serializable]private sealed class CheckReport{public string[] checks;public SummonAudio.CueEvidence[] audioCues;public int currency,totalPulls,commits;}
}

// Editor-only evidence capture: records this Unity listener's mix, never system microphone/other apps.
public sealed class P21AltarRecorder : MonoBehaviour
{
    private bool active; private string folder;private readonly object gate=new object();
    private readonly List<float> samples=new List<float>();private readonly List<double> frames=new List<double>();
    private System.Diagnostics.Stopwatch watch;private double firstAudio=-1;private int rate,channels;private Coroutine routine;
    public void Begin(string name)
    {
        End();folder=Path.Combine(P21AltarVerifySetup.Reports,name+"_capture");Directory.CreateDirectory(folder);
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
