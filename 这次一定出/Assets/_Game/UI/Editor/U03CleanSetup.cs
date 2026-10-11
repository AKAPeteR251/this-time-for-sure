using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class U03CleanSetup
{
    private const string Flag = "ThisTime.U03Clean";
    private const string ScenePath = "Assets/_Game/Scenes/UIPrototype.unity";
    private const string PrefabPath = "Assets/_Game/UI/Prefabs/UIPrototype.prefab";
    private const string FontPath = "Assets/_Game/UI/Resources/Fonts/LXGWWenKai-Regular.ttf";
    private static string Reports { get { return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "Docs/Production/U03_CLEAN_Verification"); } }
    private static bool testing;
    private static double started;
    private static EditorWindow gameView;
    private static readonly List<string> checks = new List<string>();
    private static readonly List<string> visiblePages = new List<string>();
    static U03CleanSetup()
    { EditorApplication.update += Poll; EditorApplication.playModeStateChanged += PlayChanged; }

    [MenuItem("Tools/这次一定出/UI/U03-CLEAN/准备内置签名字体")]
    public static void Prepare()
    {
        AssetDatabase.Refresh();
        var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null) throw new InvalidOperationException("签名字体未导入");
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var ui = prefab.GetComponent<PrototypeGame>();
            ui.signatureFont = font; ui.useFixedMock = false;
            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        AssetDatabase.SaveAssets(); Write("ready", "内置字体已绑定独立 UI Prefab，未修改 Main。");
    }

    [MenuItem("Tools/这次一定出/UI/U03-CLEAN/验证签约输入与三种窗口")]
    public static void Verify()
    {
        if(!EditorApplication.isPlayingOrWillChangePlaymode && SceneManager.GetActiveScene().path == ScenePath && SceneManager.GetActiveScene().isDirty)
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        if (EditorApplication.isPlayingOrWillChangePlaymode || Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
        { Write("blocked", "play=" + EditorApplication.isPlaying + "; changing=" + EditorApplication.isPlayingOrWillChangePlaymode + "; scenes=" + string.Join(" | ",Enumerable.Range(0,SceneManager.sceneCount).Select(i=>SceneManager.GetSceneAt(i).path + ":dirty=" + SceneManager.GetSceneAt(i).isDirty))); return; }
        Prepare(); EditorSceneManager.OpenScene(ScenePath);
        SessionState.SetBool(Flag, true); Write("starting", "独立 UI 场景进入 Play，验证 EventSystem 输入。");
        EditorApplication.EnterPlaymode();
    }
    private static void Poll()
    {
        if (testing && EditorApplication.isPlaying)
        { if (gameView != null) gameView.Repaint(); EditorApplication.QueuePlayerLoopUpdate(); }
        if (testing && EditorApplication.timeSinceStartup - started > 180) Complete("FAIL: 验证超时");
        string request = Path.Combine(Reports, "U03.request");
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(request)) return;
        string command = File.ReadAllText(request).Trim(); File.Delete(request);
        try
        {
            if (command == "prepare") Prepare();
            else if (command == "verify") Verify();
            else if (command == "stop" && SceneManager.GetActiveScene().path == ScenePath) EditorApplication.ExitPlaymode();
            else if (command == "status") Write("observed", "play=" + EditorApplication.isPlaying + "; scene=" + SceneManager.GetActiveScene().path + "; dirty=" + SceneManager.GetActiveScene().isDirty);
        }
        catch (Exception error) { Write("failed", error.ToString()); }
    }
    private static void PlayChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Flag, false)) return;
        SessionState.SetBool(Flag, false); testing = true; started = EditorApplication.timeSinceStartup;
        checks.Clear(); visiblePages.Clear(); Application.runInBackground = true;
        Resize(1280, 720);
        gameView = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        Write("running", "正在验证实际签名射线/指针事件、跳过/逐张确认及页面文案。");
        UnityEngine.Object.FindObjectOfType<PrototypeGame>().StartCoroutine(Guard(Check(UnityEngine.Object.FindObjectOfType<PrototypeGame>())));
    }
    private static IEnumerator Check(PrototypeGame ui)
    {
        var data = Get<IPrototypeUIAdapter>(ui, "game");
        Require(!ui.useFixedMock && data.Currency == 30, "独立场景真实内存初始30");
        Require(AssetDatabase.GetAssetPath(ui.AutoSignatureText.font) == FontPath, "签名使用项目内置字体");
        string[] names = { "星旅者 Luna", "Alice", "小满·Echo 07", "召唤者" };
        foreach (string name in names)
        {
            Font signature = ui.AutoSignatureText.font; signature.RequestCharactersInTexture(name, 58);
            foreach (char letter in name.Where(c => !char.IsWhiteSpace(c))) Require(signature.HasCharacter(letter), "昵称字形 " + name + ":" + letter);
            ui.playerDisplayName = name; ui.RequestSummon(1); ui.SkipSigning();
            Require(ui.FlowStage == SummonFlowStage.AutoSigning && ui.AutoSignatureText.text == name, "补签读取昵称 " + name);
            ui.CancelSigning(); Require(data.Currency == 30, "补签中取消不扣费 " + name);
        }
        ui.playerDisplayName = " "; ui.RequestSummon(1); ui.SkipSigning();
        Require(ui.AutoSignatureText.text == "召唤者", "空白配置昵称回落召唤者"); ui.CancelSigning();
        ui.playerDisplayName = "星旅者 Luna";
        int[,] sizes = { {1280,720}, {1920,1080}, {1600,1000} };
        var fixture = new List<PrototypePull>();
        string[] ids = { "SSR001", "UR001", "SR002", "SR001", "R001", "SR003", "SR002", "R002", "R001", "SSR001" };
        for (int i = 0; i < ids.Length; i++) fixture.Add(new PrototypePull { Card = data.Cards.Single(c => c.Id == ids[i]), IsNew = i < 2 });
        for (int size = 0; size < 3; size++)
        {
            Resize(sizes[size,0], sizes[size,1]); yield return null; yield return null;
            string prefix = sizes[size,0] + "x" + sizes[size,1] + "_";
            Call(ui, "ShowSummon"); yield return Capture(ui, prefix + "summon");
            Click(ButtonNamed(ui, "概率说明  ›")); yield return Capture(ui, prefix + "rules");
            Click(ButtonNamed(ui, "知道了 · Esc"));
            Click(Get<Button>(ui, "singleButton"));
            int before = data.Currency, requests = ui.CommittedSummonRequests;
            Require(ui.FlowStage == SummonFlowStage.Signing, prefix + "召唤按钮进入签署");
            yield return Capture(ui, prefix + "signing");
            var pointer = Pointer(ui.SignaturePad.rectTransform, new Vector2(0.16f,0.35f));
            Require(Hit(pointer) == ui.SignaturePad.gameObject, prefix + "签名区GraphicRaycaster命中");
            ExecuteEvents.Execute(ui.SignaturePad.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            for (int j = 0; j < 20; j++)
            {
                pointer.position = Position(ui.SignaturePad.rectTransform, new Vector2(0.16f + j * 0.033f, 0.5f + Mathf.Sin(j * 0.85f) * 0.23f));
                ExecuteEvents.Execute(ui.SignaturePad.gameObject, pointer, ExecuteEvents.dragHandler);
            }
            Require(data.Currency == before && ui.SignaturePad.Points.Count > 10, prefix + "拖动墨迹，未松笔不扣费");
            yield return Capture(ui, prefix + "handwritten");
            if (size == 0)
            {
                ExecuteEvents.Execute(ui.SignaturePad.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                Require(ui.FlowStage == SummonFlowStage.Ceremony && data.Currency == before - 1 && ui.CommittedSummonRequests == requests + 1, "真实松笔只提交单抽一次");
                yield return null; yield return null;
                Click(ButtonNamed(ui, "跳过")); Require(ui.FlowStage == SummonFlowStage.Summary, "仪式按钮跳过直达同批单卡总览");
            }
            else { Click(ButtonNamed(ui, "返回")); Require(data.Currency == before, prefix + "签署返回不扣费"); }
            Call(ui, "ShowSummon");
            yield return null; yield return null;
            Click(size == 0 ? Get<Button>(ui,"tenButton") : Get<Button>(ui,"singleButton"));
            yield return null; yield return null;
            before = data.Currency; requests = ui.CommittedSummonRequests;
            Click(ButtonNamed(ui, "跳过"));
            Require(ui.FlowStage == SummonFlowStage.AutoSigning && ui.AutoSignatureText.text == ui.playerDisplayName && data.Currency == before, prefix + "签署跳过只开始昵称显露");
            yield return new WaitForSecondsRealtime(ui.autoSignatureSeconds * 0.75f);
            yield return Capture(ui, prefix + "auto_signature");
            yield return WaitStage(ui, SummonFlowStage.Ceremony, 2);
            Require(ui.CommittedSummonRequests == requests + 1 && data.Currency == before - (size == 0 ? 10 : 1), prefix + "补签后无确认直接一次提交");
            Require(!Get<Text>(ui,"revealCount").gameObject.activeSelf && !Get<RectTransform>(ui,"header").gameObject.activeSelf, prefix + "仪式隐藏进度导航资源");
            yield return Capture(ui, prefix + "ceremony_signature");
            if (size == 0)
            {
                var batch = ui.PresentationBatch.ToArray(); var owned = data.Cards.Select(c => data.CountOf(c.Id)).ToArray();
                Click(AdvanceButton(ui));
                yield return WaitStage(ui, SummonFlowStage.Revealing, 2);
                for (int i = 0; i < 10; i++)
                {
                    Require(ui.RevealIndex == i && ReferenceEquals(ui.CurrentPull, batch[i]), "十连原序 " + (i+1));
                    Click(AdvanceButton(ui)); yield return WaitStage(ui, SummonFlowStage.AwaitingConfirm, 4);
                    yield return null; yield return null;
                    Require(ui.RevealIndex == i, "动画点击只完成当前张 " + (i+1));
                    Button confirm = Get<Button>(ui,"confirmCardButton");
                    Require(confirm.GetComponentInChildren<Text>().text == (i == 9 ? "查看结果" : "下一张"), "逐张确认标签 " + (i+1));
                    if (i == 0 || i == 9) yield return Capture(ui, prefix + (i == 0 ? "first_card" : "last_card"));
                    Require(ui.FlowStage == SummonFlowStage.AwaitingConfirm, "每张等待输入 " + (i+1));
                    Click(confirm); yield return WaitStage(ui, i == 9 ? SummonFlowStage.Summary : SummonFlowStage.Revealing, 2);
                }
                Require(ui.PresentationBatch.SequenceEqual(batch) && owned.SequenceEqual(data.Cards.Select(c => data.CountOf(c.Id))), "逐张与总览保持原批次NEW收藏");
                yield return Capture(ui, prefix + "real_summary");
            }
            else { Click(ButtonNamed(ui,"跳过")); Require(ui.FlowStage == SummonFlowStage.Summary, prefix + "仪式跳过总览"); }
            ui.PlayBatchForDebug(fixture); yield return null; yield return null;
            if(size == 0) Click(ButtonNamed(ui,"跳过"));
            else
            {
                Click(AdvanceButton(ui)); yield return WaitStage(ui,SummonFlowStage.Revealing,2);
                for(int i=0;i<fixture.Count;i++)
                {
                    Click(AdvanceButton(ui)); yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);
                    yield return null; yield return null;
                    if(i==0||i==fixture.Count-1)yield return Capture(ui,prefix+(i==0?"first_card":"last_card"));
                    Click(Get<Button>(ui,"confirmCardButton"));
                    yield return WaitStage(ui,i==fixture.Count-1?SummonFlowStage.Summary:SummonFlowStage.Revealing,2);
                }
            }
            yield return Capture(ui, prefix + "summary");
            var cards = Get<RectTransform>(ui,"summaryCards"); Click(cards.GetComponentsInChildren<Button>().First());
            yield return Capture(ui, prefix + "detail");
            Click(ButtonNamed(ui,"隐藏界面 · 欣赏")); yield return Capture(ui,prefix + "artwork");
            Click(ButtonNamed(ui,"显示界面 · Esc")); Click(ButtonNamed(ui,"返回"));
            yield return null; yield return null;
            Click(ButtonNamed(ui,"查看图鉴  ›")); yield return Capture(ui, prefix + "collection");
            Click(ButtonNamed(ui,"返回召唤"));
        }
        // 四档的等待状态及两处跳过均消费同一已确定批次，不抽取。
        foreach (string id in new[] { "R001", "SR001", "SSR001", "UR001", "R002" })
        {
            ui.PlayBatchForDebug(new List<PrototypePull>{ fixture.FirstOrDefault(p=>p.Card.Id==id) ?? new PrototypePull{Card=data.Cards.Single(c=>c.Id==id)} });
            yield return null; yield return null;
            Click(AdvanceButton(ui)); yield return WaitStage(ui,SummonFlowStage.Revealing,2);
            Click(AdvanceButton(ui)); yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);
            yield return null; yield return null;
            Require(Get<Text>(ui,"revealCount").text == "1/1" && Get<Button>(ui,"confirmCardButton").GetComponentInChildren<Text>().text == "查看结果", "单卡最后确认 " + id);
            yield return Capture(ui,"1600x1000_reveal_" + id);
            Click(ButtonNamed(ui,"跳过")); Require(ui.FlowStage==SummonFlowStage.Summary,"等待跳过原批次 " + id);
        }
        var retainedCurrency = data.Currency;
        ui.PlayBatchForDebug(fixture); yield return null; yield return null;
        Click(AdvanceButton(ui)); yield return WaitStage(ui,SummonFlowStage.Revealing,2);
        Click(ButtonNamed(ui,"跳过"));
        Require(ui.FlowStage==SummonFlowStage.Summary && ui.PresentationBatch.SequenceEqual(fixture) && data.Currency==retainedCurrency,"单张动画中的跳过也直达同批总览");
        while (data.Currency > 0)
        {
            ui.RequestSummon(data.Currency >= 10 ? 10 : 1); ui.SkipSigning();
            yield return WaitStage(ui,SummonFlowStage.Ceremony,2); ui.SkipPresentation();
        }
        Call(ui,"ShowSummon");
        Require(!Get<Button>(ui,"singleButton").interactable && !Get<Button>(ui,"tenButton").interactable, "资源0单抽十连禁用");
        yield return Capture(ui,"1600x1000_insufficient");
        Click(Get<Button>(ui,"refillButton")); Require(data.Currency==10 && Get<Button>(ui,"tenButton").interactable,"+10补充后召唤按钮恢复");
        // 无美术主题也不出现旧开发说明；仅修改 Play 内对象。
        var alternate = new GameObject("FallbackCheck"); ui.transform.Find("PrototypeCanvas").gameObject.SetActive(false);
        var fallback = alternate.AddComponent<PrototypeGame>(); yield return null;
        yield return Capture(fallback,"1600x1000_fallback_summon");
        Call(fallback,"OpenRules"); yield return Capture(fallback,"1600x1000_fallback_rules"); Call(fallback,"CloseRules");
        fallback.PlayBatchForDebug(fixture); fallback.SkipPresentation(); yield return Capture(fallback,"1600x1000_fallback_summary");
        UnityEngine.Object.Destroy(alternate); ui.transform.Find("PrototypeCanvas").gameObject.SetActive(true);
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new Evidence{checks=checks.ToArray(),names=names,font=AssetDatabase.GetAssetPath(ui.AutoSignatureText.font),fontNames=ui.AutoSignatureText.font.fontNames,pages=visiblePages.ToArray(),currency=data.Currency,totalPulls=data.TotalPulls},true));
        Complete("PASS: 内置字体/中英混合昵称/手写射线及指针输入/自动签署/逐张等待和最后确认/同批跳过/三种窗口/纯净文案/资源不足与补充");
    }
    private static Button AdvanceButton(PrototypeGame ui)
    { return Get<Dictionary<string,GameObject>>(ui,"screens")["Reveal"].transform.Find("AdvanceSurface").GetComponent<Button>(); }
    private static Button ButtonNamed(PrototypeGame ui,string label)
    { return ui.GetComponentsInChildren<Button>().Single(b=>b.name==label && b.gameObject.activeInHierarchy); }
    private static Vector2 Position(RectTransform rect,Vector2 normalized)
    { return rect.TransformPoint(new Vector2(Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,normalized.x),Mathf.Lerp(rect.rect.yMin,rect.rect.yMax,normalized.y))); }
    private static PointerEventData Pointer(RectTransform rect,Vector2 normalized)
    { Canvas.ForceUpdateCanvases(); return new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=Position(rect,normalized)}; }
    private static GameObject Hit(PointerEventData pointer)
    { var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);return hits.Count==0?null:hits[0].gameObject; }
    private static void Click(Button button)
    {
        Require(button!=null && button.IsInteractable() && button.gameObject.activeInHierarchy,"输入按钮可用 " + (button==null?"null":button.name));
        var pointer=Pointer(button.GetComponent<RectTransform>(),new Vector2(.5f,.5f));
        var hit=Hit(pointer); Require(hit!=null && hit.GetComponentInParent<Button>()==button,"按钮射线命中 " + button.name + "; 实际=" + (hit==null?"无":hit.name));
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
    }
    private static IEnumerator Capture(PrototypeGame ui,string name)
    {
        yield return null; yield return null; Canvas.ForceUpdateCanvases();
        foreach(Text text in ui.GetComponentsInChildren<Text>())
        {
            if(!text.enabled || !text.gameObject.activeInHierarchy) continue;
            string clean=Regex.Replace(text.text,@"[^\p{L}\p{N}]","");
            Require(!clean.Contains("这次一定出") && !Regex.IsMatch(text.text,"尚未接入|本轮|本次运行|UI MOCK|占位|校准|测试|原型|待补充|高级预览|快速签署|确认签署|签名不影响概率"),"页面文案 " + name + ":" + text.name);
            if(name.EndsWith("detail"))Require(!Regex.IsMatch(text.text,@"\b(R|SR|SSR|UR)\d{3}\b"),"详情无内部ID");
        }
        foreach(Button button in ui.GetComponentsInChildren<Button>())
        {
            if(!button.gameObject.activeInHierarchy || button.name=="AdvanceSurface") continue;
            Vector3[] corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);
            Require(corners.All(p=>p.x>=-1 && p.x<=Screen.width+1 && p.y>=-1 && p.y<=Screen.height+1),"按钮屏内 " + name + ":" + button.name);
        }
        visiblePages.Add(name+" "+string.Join(" | ",ui.GetComponentsInChildren<Text>().Where(t=>t.enabled&&t.gameObject.activeInHierarchy).Select(t=>t.text.Replace("\n"," / "))));
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Reports,name+".png"));yield return null;
    }
    private static IEnumerator WaitStage(PrototypeGame ui,SummonFlowStage target,float timeout)
    { float end=Time.realtimeSinceStartup+timeout;while(ui.FlowStage!=target&&Time.realtimeSinceStartup<end)yield return null;Require(ui.FlowStage==target,"到达阶段 " + target); }
    private static IEnumerator Guard(IEnumerator routine)
    {
        while(true)
        {
            bool moved=false;Exception problem=null;
            try{moved=routine.MoveNext();}catch(Exception error){problem=error;}
            if(problem!=null){Complete("FAIL: "+problem);yield break;}
            if(!moved)yield break;
            IEnumerator nested=routine.Current as IEnumerator;
            if(nested!=null){yield return Guard(nested);if(!testing)yield break;}else yield return routine.Current;
        }
    }
    private static T Get<T>(PrototypeGame ui,string field){return (T)typeof(PrototypeGame).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);}
    private static void Call(PrototypeGame ui,string method){typeof(PrototypeGame).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,null);}
    private static void Require(bool pass,string message){if(!pass)throw new InvalidOperationException(message);checks.Add(message);}
    private static void Resize(int width,int height){typeof(U01Setup).GetMethod("Resize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{width,height});}
    private static void Complete(string result)
    {
        testing=false;Write(result.StartsWith("PASS")?"passed":"failed",result);
        File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);
        File.WriteAllText(Path.Combine(Reports,"partial_checks.txt"),string.Join("\n",checks));
        EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();
    }
    private static void Write(string stage,string result)
    { Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,scene=SceneManager.GetActiveScene().path,unityVersion=Application.unityVersion,atUtc=DateTime.UtcNow.ToString("o")},true));Debug.Log("[U03-CLEAN] "+result); }
    [Serializable]private sealed class Report{public string stage,result,scene,unityVersion,atUtc;}
    [Serializable]private sealed class Evidence{public string[] checks,names,fontNames,pages;public string font;public int currency,totalPulls;}
}
