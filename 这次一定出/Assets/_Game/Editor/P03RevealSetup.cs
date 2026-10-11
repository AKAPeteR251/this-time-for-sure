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
public static class P03RevealSetup
{
    private const string Flag = "ThisTime.P03RevealCheck";
    private static string Reports { get { return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "Docs/Production/P03_REVEAL_Verification"); } }
    private static string Request { get { return Path.Combine(Reports, "P03.request"); } }
    private static bool testing;
    private static EditorWindow gameView;
    private static double started;
    private static readonly List<string> checks = new List<string>();
    static P03RevealSetup()
    { EditorApplication.update += Poll; EditorApplication.playModeStateChanged += PlayChanged; }
    [MenuItem("Tools/这次一定出/程序/P03-REVEAL/验证契约与逐张确认")]
    public static void Verify()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
        { Write("blocked", "请停止 Play 并保存当前编辑后复验。"); return; }
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");
        SessionState.SetBool(Flag, true); Write("starting", "从 Main 请求契约及逐张真实 Play 检查。");
        EditorApplication.EnterPlaymode();
    }
    private static void Poll()
    {
        if (testing && EditorApplication.isPlaying) { if (gameView != null) gameView.Repaint(); EditorApplication.QueuePlayerLoopUpdate(); }
        if (testing && EditorApplication.timeSinceStartup - started > 90) Complete("FAIL: P03 Play 复验超时。");
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
        string command = File.ReadAllText(Request).Trim(); File.Delete(Request);
        if (command == "verify") Verify();
        else if (command == "status") Write("observed", "play=" + EditorApplication.isPlaying + "; scene=" + SceneManager.GetActiveScene().path);
        else if (command == "stop") EditorApplication.ExitPlaymode();
    }
    private static void PlayChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Flag, false)) return;
        SessionState.SetBool(Flag, false); testing = true; started = EditorApplication.timeSinceStartup; checks.Clear();
        Application.runInBackground = true; EditorApplication.isPaused = false;
        typeof(U01Setup).GetMethod("Resize", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { 1280, 720 });
        gameView = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        Write("running", "实际 Play 检查签名、补签、一次 Draw、十张确认、阶段点击与跳过。");
        var ui = UnityEngine.Object.FindObjectOfType<PrototypeGame>();
        ui.StartCoroutine(Guard(Check(ui)));
    }
    private static IEnumerator Check(PrototypeGame ui)
    {
        var state = Get<IPrototypeUIAdapter>(ui, "game");
        Require(!ui.useFixedMock && state.Currency == 30 && SceneManager.GetActiveScene().path.EndsWith("Main.unity"), "Main真实内存初始30");
        ui.RequestSummon(10);
        Require(ui.FlowStage == SummonFlowStage.Signing && state.Currency == 30 && state.TotalPulls == 0, "签署前不抽取不扣费");
        Require(!Get<RectTransform>(ui,"header").gameObject.activeSelf && !Get<RectTransform>(ui,"sidebar").gameObject.activeSelf, "签署隐藏导航资源");
        ui.HandleEscape(); Require(state.Currency == 30 && ui.FlowStage == SummonFlowStage.Idle, "签署Esc返回不扣费");
        ui.playerDisplayName = "星旅者 Luna";
        ui.RequestSummon(1); ui.SkipSigning();
        Require(ui.FlowStage == SummonFlowStage.AutoSigning && ui.AutoSignatureText.text == ui.playerDisplayName && state.Currency == 30, "补签读取配置昵称且不立即Draw");
        ui.SkipSigning(); ui.CompleteHandwrittenSignature(); ui.CancelSigning();
        Require(state.Currency == 30 && ui.CommittedSummonRequests == 0, "补签显露途中返回仍不扣费");
        ui.RequestSummon(1);
        yield return null; yield return null;
        Canvas.ForceUpdateCanvases();
        var pointer = new PointerEventData(EventSystem.current) { pointerId = -1, button = PointerEventData.InputButton.Left };
        Vector3[] corners = new Vector3[4]; ui.SignaturePad.rectTransform.GetWorldCorners(corners);
        pointer.position = Vector2.Lerp(corners[0],corners[2],0.30f);
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer,hits);
        Require(hits.Count > 0 && hits[0].gameObject == ui.SignaturePad.gameObject, "签名区实际GraphicRaycaster命中");
        ExecuteEvents.Execute(ui.SignaturePad.gameObject,pointer,ExecuteEvents.pointerDownHandler);
        pointer.position = Vector2.Lerp(corners[0],corners[2],0.55f);
        ExecuteEvents.Execute(ui.SignaturePad.gameObject,pointer,ExecuteEvents.dragHandler);
        Require(ui.SignaturePad.Points.Count >= 2 && state.Currency == 30, "真实指针事件绘制可见轨迹，松笔前不扣费");
        yield return Capture("handwritten_signature");
        ExecuteEvents.Execute(ui.SignaturePad.gameObject,pointer,ExecuteEvents.pointerUpHandler);
        ui.CompleteHandwrittenSignature(); ui.RequestSummon(10);
        Require(ui.FlowStage == SummonFlowStage.Ceremony && state.Currency == 29 && state.TotalPulls == 1 && ui.CommittedSummonRequests == 1, "松笔提交一次，重复提交与忙碌请求不再Draw");
        ui.SkipPresentation(); Require(ui.FlowStage == SummonFlowStage.Summary && ui.PresentationBatch.Count == 1, "仪式跳过到单抽同批总览");
        ui.RequestSummon(10); ui.SkipSigning();
        Require(ui.AutoSignatureText.font != null && ui.AutoSignatureText.font.fontNames.Any(n => n.Contains("Xingkai") || n.Contains("行楷") || n.Contains("KaiTi")), "自动补签实际手写风格字体");
        foreach (char letter in ui.playerDisplayName.Where(ch => !char.IsWhiteSpace(ch))) Require(ui.AutoSignatureText.font.HasCharacter(letter), "昵称字形:" + letter);
        yield return new WaitForSecondsRealtime(ui.autoSignatureSeconds * 0.5f);
        yield return Capture("auto_signature_name");
        yield return WaitStage(ui,SummonFlowStage.Ceremony,2);
        Require(state.Currency == 19 && state.TotalPulls == 11 && ui.CommittedSummonRequests == 2, "补签无需确认自动统一Draw十张");
        yield return new WaitForSecondsRealtime(ui.ceremonySeconds * 0.45f);
        var pages=Get<Dictionary<string,GameObject>>(ui,"screens");
        Button ceremonySkip=pages["Reveal"].GetComponentsInChildren<Button>().Single(b=>b.name=="跳过");
        Canvas.ForceUpdateCanvases();
        var skipPointer=new PointerEventData(EventSystem.current){position=ceremonySkip.GetComponent<RectTransform>().TransformPoint(ceremonySkip.GetComponent<RectTransform>().rect.center)};
        var skipHits=new List<RaycastResult>(); EventSystem.current.RaycastAll(skipPointer,skipHits);
        Require(ceremonySkip.gameObject.activeInHierarchy && skipHits.Count>0 && skipHits[0].gameObject==ceremonySkip.gameObject,"仪式跳过按钮可见并在最上层接收射线");
        yield return Capture("contract_ceremony");
        var realBatch = ui.PresentationBatch.ToArray();
        var owned = state.Cards.Select(c => state.CountOf(c.Id)).ToArray();
        ui.AdvancePresentation(); yield return WaitStage(ui,SummonFlowStage.Revealing,2);
        for(int i=0;i<10;i++)
        {
            Require(ui.RevealIndex == i && object.ReferenceEquals(ui.CurrentPull,realBatch[i]), "真实批次原顺序:"+i);
            ui.AdvancePresentation(); ui.AdvancePresentation();
            yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);
            Require(ui.RevealIndex == i, "动画输入只完成当前张:"+i);
            yield return null; yield return null;
            if(i == 0 || i == 9) yield return new WaitForSecondsRealtime(0.12f);
            Require(ui.FlowStage == SummonFlowStage.AwaitingConfirm && ui.RevealIndex == i, "无人确认不自动推进:"+i);
            Button confirm=Get<Button>(ui,"confirmCardButton");
            Require(confirm.GetComponentInChildren<Text>().text == (i == 9 ? "查看结果" : "下一张"), "确认按钮:"+i);
            if(i==9) yield return Capture("last_card_awaiting_confirmation");
            confirm.onClick.Invoke();
            yield return WaitStage(ui,i==9?SummonFlowStage.Summary:SummonFlowStage.Revealing,2);
        }
        Require(ui.PresentationBatch.SequenceEqual(realBatch) && owned.SequenceEqual(state.Cards.Select(c=>state.CountOf(c.Id))) && state.Currency==19 && ui.CommittedSummonRequests==2, "逐张与总览不改结果NEW收藏资源");
        var fixture=new List<PrototypePull>();
        string[] ids={"R001","R001","SR001","SSR001","UR001","R002","SR002","R003","SSR001","UR001"};
        for(int i=0;i<ids.Length;i++) fixture.Add(new PrototypePull{Card=state.Cards.Single(c=>c.Id==ids[i]),IsNew=i==0||i==3||i==4});
        ui.PlayBatchForDebug(fixture); ui.SkipPresentation();
        Require(ui.FlowStage==SummonFlowStage.Summary && ui.PresentationBatch.SequenceEqual(fixture),"指定普通重复SR/SSR/UR批次：仪式跳过保留全批");
        ui.PlayBatchForDebug(fixture); ui.AdvancePresentation(); yield return WaitStage(ui,SummonFlowStage.Revealing,2);
        ui.SkipPresentation(); Require(ui.FlowStage==SummonFlowStage.Summary && ui.PresentationBatch.SequenceEqual(fixture),"单卡动画跳过保留全批");
        ui.PlayBatchForDebug(fixture); ui.AdvancePresentation(); yield return WaitStage(ui,SummonFlowStage.Revealing,2);
        for(int i=0;i<fixture.Count;i++)
        {
            ui.AdvancePresentation(); yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);
            Require(ui.CurrentPull.Card==fixture[i].Card && ui.CurrentPull.IsNew==fixture[i].IsNew,"指定四档与重复卡NEW:"+i);
            yield return null; yield return null;
            ui.AdvancePresentation(); yield return WaitStage(ui,i==fixture.Count-1?SummonFlowStage.Summary:SummonFlowStage.Revealing,2);
        }
        ui.PlayBatchForDebug(fixture); ui.AdvancePresentation(); yield return WaitStage(ui,SummonFlowStage.Revealing,2);
        ui.AdvancePresentation(); yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);
        yield return null; ui.HandleEscape();
        Require(ui.FlowStage==SummonFlowStage.Summary && ui.PresentationBatch.SequenceEqual(fixture),"停留Esc到同批总览");
        ui.PlayBatchForDebug(new List<PrototypePull>{fixture[4]});
        ui.AdvancePresentation(); yield return WaitStage(ui,SummonFlowStage.Revealing,2);
        yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,ui.urRevealSeconds+2);
        yield return new WaitForSecondsRealtime(0.15f);
        Require(ui.FlowStage==SummonFlowStage.AwaitingConfirm && ui.RevealIndex==0,"单抽UR自然动画结束仍等最后确认");
        ui.AdvancePresentation(); yield return WaitStage(ui,SummonFlowStage.Summary,2);
        Require(state.Currency==19 && state.TotalPulls==11 && ui.CommittedSummonRequests==2 && owned.SequenceEqual(state.Cards.Select(c=>state.CountOf(c.Id))),"开发指定批次不抽取不入库");
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckList{checks=checks.ToArray(),nickname=ui.playerDisplayName,fontNames=ui.AutoSignatureText.font.fontNames,finalCurrency=state.Currency,totalPulls=state.TotalPulls,committedRequests=ui.CommittedSummonRequests},true));
        Complete("PASS: 手写松笔/用户名手写补签/取消不扣费/一次Draw; 真实十连原序逐张等待及最后确认; 当前张快速完成/同帧输入不连翻; 仪式动画停留三处跳过与Esc; 指定四档重复NEW及收藏不变");
    }
    private static IEnumerator WaitStage(PrototypeGame ui,SummonFlowStage target,float seconds)
    {
        float deadline=Time.realtimeSinceStartup+seconds;
        while(ui.FlowStage!=target && Time.realtimeSinceStartup<deadline) yield return null;
        if(ui.FlowStage!=target) throw new InvalidOperationException("等待阶段失败："+target+"，实际"+ui.FlowStage);
    }
    private static IEnumerator Capture(string name)
    {
        if(Application.isBatchMode) yield break;
        yield return null; yield return null;
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(Reports,name+".png"));
        yield return null; yield return null;
    }
    private static IEnumerator Guard(IEnumerator routine)
    {
        while(true)
        {
            bool moved=false; Exception problem=null;
            try{moved=routine.MoveNext();}catch(Exception error){problem=error;}
            if(problem!=null){Complete("FAIL: "+problem);yield break;}
            if(!moved)yield break;
            IEnumerator nested=routine.Current as IEnumerator;
            if(nested!=null) { yield return Guard(nested); if(!testing) yield break; } else yield return routine.Current;
        }
    }
    private static void Require(bool condition,string label)
    { if(!condition)throw new InvalidOperationException(label); checks.Add(label); }
    private static T Get<T>(PrototypeGame ui,string name){return (T)typeof(PrototypeGame).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);}
    private static void Complete(string result)
    {
        testing=false;Write(result.StartsWith("PASS")?"passed":"failed",result);
        File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);
        EditorApplication.delayCall+=()=>{EditorApplication.ExitPlaymode();if(Application.isBatchMode)EditorApplication.delayCall+=()=>EditorApplication.Exit(result.StartsWith("PASS")?0:1);};
    }
    private static void Write(string stage,string result)
    { Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,scene=SceneManager.GetActiveScene().path,unityVersion=Application.unityVersion,atUtc=DateTime.UtcNow.ToString("o")},true));Debug.Log("[P03-REVEAL] "+result); }
    [Serializable]private sealed class Report{public string stage,result,scene,unityVersion,atUtc;}
    [Serializable]private sealed class CheckList{public string[] checks,fontNames;public string nickname;public int finalCurrency,totalPulls,committedRequests;}
}



