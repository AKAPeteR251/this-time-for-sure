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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class P02FastSetup
{
    private const string MainPath = "Assets/_Game/Scenes/Main.unity";
    private const string PlayFlag = "ThisTime.P02FastCheck";
    private static string Reports { get { return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "Docs/Production/P02_FAST_Verification"); } }
    private static string Request { get { return Path.Combine(Reports, "P02.request"); } }
    private static bool testing;
    private static EditorWindow gameView;
    private static double startedAt;

    static P02FastSetup()
    {
        EditorApplication.update += Poll;
        EditorApplication.playModeStateChanged += OnPlayChanged;
        Application.logMessageReceived += LogError;
    }

    [MenuItem("Tools/这次一定出/程序/P02-FAST/接入 Main 六卡配置")]
    public static void Integrate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("请先停止 Play。");
        Scene scene = SceneManager.GetSceneByPath(MainPath);
        if (scene.IsValid() && scene.isLoaded && scene.isDirty) throw new InvalidOperationException("Main 有未保存编辑，请先保存。");
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if (openedHere) scene = EditorSceneManager.OpenScene(MainPath, OpenSceneMode.Additive);
        try
        {
            var target = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<PrototypeGame>(true)).Single();
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(U02ArtSetup.PrefabPath).GetComponent<PrototypeGame>();
            if (source.theme == null || source.useFixedMock || source.portraits.Count != 6)
                throw new InvalidOperationException("U02 配置不是六卡真实内存入口。");
            target.theme = source.theme;
            target.useFixedMock = false;
            target.portraits = source.portraits.Select(item => new PrototypePortrait
                { characterId = item.characterId, portrait = item.portrait, thumbnail = item.thumbnail }).ToList();
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Write("integrated", "Main 原有入口已绑定最新 U02 主题和六卡；不重建场景或写 UI 资产。");
        }
        finally { if (openedHere) EditorSceneManager.CloseScene(scene, true); }
    }

    public static void RunFromCommandLine()
    {
        Integrate();
        byte[] first = File.ReadAllBytes(MainPath);
        Integrate();
        File.WriteAllText(Path.Combine(Reports, "integration_repeat.json"), "{\"sameSceneBytes\":" + first.SequenceEqual(File.ReadAllBytes(MainPath)).ToString().ToLowerInvariant() + "}");
        Verify();
    }

    [MenuItem("Tools/这次一定出/程序/P02-FAST/打开 Main")]
    public static void OpenMain()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(MainPath);
    }

    [MenuItem("Tools/这次一定出/程序/P02-FAST/验证 Main 与高级揭晓")]
    public static void Verify()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("请停止 Play 并保存场景后复验。");
        EditorSceneManager.OpenScene(MainPath);
        SessionState.SetBool(PlayFlag, true);
        Write("starting", "从 Main 启动真实内存复验。");
        EditorApplication.EnterPlaymode();
    }

    [MenuItem("Tools/这次一定出/程序/P02-FAST/Play 中预览 SSR")]
    public static void PreviewSSR() { PreviewInPlay("SSR001"); }
    [MenuItem("Tools/这次一定出/程序/P02-FAST/Play 中预览 UR")]
    public static void PreviewUR() { PreviewInPlay("UR001"); }
    private static void PreviewInPlay(string id)
    {
        if (EditorApplication.isPlaying) Call(UnityEngine.Object.FindObjectOfType<PrototypeGame>(), "Preview", id);
    }

    private static void Poll()
    {
        if (testing && EditorApplication.isPlaying && gameView != null) { gameView.Repaint(); EditorApplication.QueuePlayerLoopUpdate(); }
        if (testing && EditorApplication.timeSinceStartup - startedAt > 120)
        { testing = false; Complete("FAIL: Main 运行复验超时，请查看 Editor 日志。"); }
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
        string request = File.ReadAllText(Request).Trim();
        File.Delete(Request);
        try
        {
            if (request == "integrate-verify") RunFromCommandLine();
            else if (request == "verify") Verify();
            else if (request == "open") OpenMain();
            else if (request == "stop") EditorApplication.ExitPlaymode();
        }
        catch (Exception error) { Write("failed", error.ToString()); Debug.LogException(error); }
    }

    private static void OnPlayChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PlayFlag, false)) return;
        SessionState.SetBool(PlayFlag, false);
        testing = true;
        startedAt = EditorApplication.timeSinceStartup;
        Application.runInBackground = true;
        EditorApplication.isPaused = false;
        Resize(1280, 720);
        var ui = UnityEngine.Object.FindObjectOfType<PrototypeGame>();
        if (SceneManager.GetActiveScene().path != MainPath || UnityEngine.Object.FindObjectsOfType<PrototypeGame>().Length != 1)
        { Complete("FAIL: 必须从 Main 唯一入口运行。"); return; }
        Write("running", "Main 最短闭环、三窗口和高级演出检查正在运行。");
        if (Application.isBatchMode) { ui.StartCoroutine(CheckMainFlow(ui)); return; }
        ui.StartCoroutine(Drive(ui.RunArtCheck(result =>
        {
            File.WriteAllText(Path.Combine(Reports, "main_flow_result.txt"), result);
            if (!result.StartsWith("PASS")) { Complete(result); return; }
            ui.StartCoroutine(Drive(CheckPresentation(ui)));
        }, Reports, Resize)));
    }

    // batchmode 验证实际 Main/按钮/内存数据；不使用固定 mock 或截图替代。
    private static IEnumerator CheckMainFlow(PrototypeGame ui)
    {
        var failures = new List<string>();
        var state = Get<IPrototypeUIAdapter>(ui, "game");
        if (ui.useFixedMock || ui.theme.summonBackground == null || ui.portraits.Count != 6) failures.Add("Main六卡真实内存配置");
        if (ui.GetComponentsInChildren<Canvas>().Length != 1 || UnityEngine.Object.FindObjectsOfType<UnityEngine.EventSystems.EventSystem>().Length != 1)
            failures.Add("Canvas/EventSystem唯一");
        if (state.Currency != 30) failures.Add("初始30");
        Get<Button>(ui, "singleButton").onClick.Invoke();
        yield return null;
        Call(ui, "SkipReveal");
        if (state.Currency != 29 || Get<List<PrototypePull>>(ui, "lastResults").Count != 1) failures.Add("单抽29");
        Get<Button>(ui, "summaryTenButton").onClick.Invoke();
        var calculated = Get<List<PrototypePull>>(ui, "lastResults");
        int total = state.TotalPulls;
        yield return null;
        var skip = ui.GetComponentsInChildren<Button>(true).Single(b => b.GetComponentInChildren<Text>() != null && b.GetComponentInChildren<Text>().text == "跳过揭晓");
        skip.onClick.Invoke();
        if (state.Currency != 19 || calculated.Count != 10 || calculated != Get<List<PrototypePull>>(ui, "lastResults") || total != state.TotalPulls)
            failures.Add("十连19/跳过同一结果");
        var seen = new HashSet<string>();
        foreach (var pull in calculated) if (!seen.Add(pull.Card.Id) && pull.IsNew) failures.Add("重复NEW");
        Get<RectTransform>(ui, "summaryCards").GetComponentInChildren<Button>().onClick.Invoke();
        if (Get<string>(ui, "currentScreen") != "Detail") failures.Add("点卡详情");
        Call(ui, "ToggleDetailUI");
        Get<Button>(ui, "restoreUI").onClick.Invoke();
        var chrome = Get<CanvasGroup>(ui, "detailChrome");
        if (chrome.alpha != 1 || !chrome.blocksRaycasts) failures.Add("隐藏恢复");
        Call(ui, "OpenCollection");
        if (state.CollectedCharacters == 0 || Get<string>(ui, "currentScreen") != "Collection") failures.Add("收藏");
        Call(ui, "ShowSummon");
        Get<Button>(ui, "refillButton").onClick.Invoke();
        if (state.Currency != 29 || state.TotalPulls != 11) failures.Add("补充29");
        string result = failures.Count == 0 ? "PASS: Main实际Play真实内存30→29→19→29; 单抽/十连/跳过同一结果/NEW/点卡详情/隐藏恢复/收藏; Canvas与EventSystem唯一。batchmode未截屏。"
            : "FAIL: " + string.Join("; ", failures);
        File.WriteAllText(Path.Combine(Reports, "main_flow_result.txt"), result);
        if (failures.Count != 0) { Complete(result); yield break; }
        yield return Drive(CheckPresentation(ui));
    }
    private static IEnumerator CheckPresentation(PrototypeGame ui)
    {
        var failures = new List<string>();
        IPrototypeUIAdapter state = Get<IPrototypeUIAdapter>(ui, "game");
        int currency = state.Currency, total = state.TotalPulls;
        int[] owned = state.Cards.Select(card => state.CountOf(card.Id)).ToArray();
        Resize(1280, 720);
        yield return null;
        Call(ui, "ShowSummon");
        Call(ui, "Preview", "SSR001");
        FastRevealView view = Get<FastRevealView>(ui, "fastReveal");
        while (Get<bool>(ui, "busy") && view.Progress < 0.23f) yield return null;
        yield return Capture("SSR_premonition");
        if (view.CharacterId != "SSR001" || view.Artwork != ui.theme.summonCharacter) failures.Add("SSR透明人物引用");
        while (Get<bool>(ui, "busy") && view.Progress < 0.72f) yield return null;
        yield return Capture("SSR_revealed");
        while (Get<bool>(ui, "busy")) yield return null;
        if (Get<string>(ui, "currentScreen") != "Summary") failures.Add("SSR自动完成");
        Call(ui, "Preview", "UR001");
        while (Get<bool>(ui, "busy") && view.Progress < 0.20f) yield return null;
        yield return Capture("UR_premonition");
        if (view.CharacterId != "UR001" || view.Artwork != ui.portraits.Single(p => p.characterId == "UR001").portrait)
            failures.Add("UR完整卡图引用");
        if (!ui.GetComponentsInChildren<Image>(true).Single(i => i.name == "RevealEcho").enabled) failures.Add("UR附加星芒");
        // 忙碌时再次请求不会扣资源或替换当前预览结果。
        object calculated = Get<object>(ui, "lastResults");
        Call(ui, "Summon", 10);
        if (Get<object>(ui, "lastResults") != calculated) failures.Add("忙碌重入");
        Button speed = ui.GetComponentsInChildren<Button>(true).Single(b => b.GetComponentInChildren<Text>() != null && b.GetComponentInChildren<Text>().text == "加速");
        float acceleratedAt = Time.realtimeSinceStartup;
        speed.onClick.Invoke();
        while (Get<bool>(ui, "busy") && view.Progress < 0.70f) yield return null;
        yield return Capture("UR_revealed_accelerated");
        while (Get<bool>(ui, "busy")) yield return null;
        float acceleratedSeconds = Time.realtimeSinceStartup - acceleratedAt;
        if (acceleratedSeconds >= ui.urRevealSeconds || Get<Coroutine>(ui, "revealRoutine") != null) failures.Add("加速或协程结束");
        Call(ui, "Preview", "UR001");
        calculated = Get<object>(ui, "lastResults");
        yield return null;
        Button skip = ui.GetComponentsInChildren<Button>(true).Single(b => b.GetComponentInChildren<Text>() != null && b.GetComponentInChildren<Text>().text == "跳过揭晓");
        skip.onClick.Invoke();
        skip.onClick.Invoke();
        if (Get<object>(ui, "lastResults") != calculated || Get<Coroutine>(ui, "revealRoutine") != null || Get<bool>(ui, "busy")) failures.Add("重复跳过固定结果");
        if (ui.GetComponentsInChildren<Image>(true).Where(i => i.name == "RevealStar" || i.name == "RevealEcho").Any(i => i.color.a != 0)) failures.Add("光效清理");
        if (state.Currency != currency || state.TotalPulls != total || !owned.SequenceEqual(state.Cards.Select(card => state.CountOf(card.Id)))) failures.Add("预览不入库");
        File.WriteAllText(Path.Combine(Reports, "presentation_metrics.json"), JsonUtility.ToJson(new Metrics
        { ssrSeconds = ui.ssrRevealSeconds, urSeconds = ui.urRevealSeconds, acceleratedRemainingSeconds = acceleratedSeconds,
            currency = state.Currency, totalPulls = state.TotalPulls, previewCountsUnchanged = owned.SequenceEqual(state.Cards.Select(card => state.CountOf(card.Id))) }, true));
        Call(ui, "ShowSummon");
        Complete(failures.Count == 0 ? "PASS: Main真实内存闭环; SSR透明人物/自动完成; UR双星芒/完整卡图/加速; 忙碌重入/重复跳过/协程与光效清理; 预览不扣费不入库"
            : "FAIL: " + string.Join("; ", failures));
    }

    // 官方 batchmode 不发送 WaitForEndOfFrame；状态验证继续由实际 Play 帧驱动。
    // 批处理仅替换该等待，不伪造截图或结果。
    private static IEnumerator Drive(IEnumerator routine)
    {
        while (routine.MoveNext())
        {
            IEnumerator nested = routine.Current as IEnumerator;
            if (Application.isBatchMode && routine.Current is WaitForEndOfFrame) yield return null;
            else if (nested != null) yield return Drive(nested);
            else yield return routine.Current;
        }
    }
    private static IEnumerator Capture(string name)
    {
        if (Application.isBatchMode) { File.AppendAllText(Path.Combine(Reports, "phase_checks.txt"), name + ": 实际Play状态采样；batchmode未截屏。\n"); yield break; }
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(Reports, "1280x720_" + name + ".png"));
        yield return null;
        yield return null;
    }
    private static T Get<T>(PrototypeGame ui, string name) { return (T)typeof(PrototypeGame).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ui); }
    private static void Call(PrototypeGame ui, string name, params object[] arguments) { typeof(PrototypeGame).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, arguments); }
    private static void Resize(int width, int height)
    {
        typeof(U01Setup).GetMethod("Resize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { width, height });
        gameView = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        gameView.Repaint();
    }
    private static void Complete(string result)
    {
        testing = false;
        Write(result.StartsWith("PASS") ? "passed" : "failed", result);
        File.Copy(Path.Combine(Reports, "runtime_status.json"), Path.Combine(Reports, "final_result.json"), true);
        EditorApplication.delayCall += () => { EditorApplication.ExitPlaymode(); if (Application.isBatchMode) EditorApplication.delayCall += () => EditorApplication.Exit(result.StartsWith("PASS") ? 0 : 1); };
    }
    private static void LogError(string message, string trace, LogType type)
    {
        if (testing && (type == LogType.Exception || type == LogType.Error))
        { Directory.CreateDirectory(Reports); File.AppendAllText(Path.Combine(Reports, "errors.log"), message + "\n" + trace + "\n"); }
    }
    private static void Write(string stage, string result)
    {
        Directory.CreateDirectory(Reports);
        File.WriteAllText(Path.Combine(Reports, "runtime_status.json"), JsonUtility.ToJson(new Status
            { stage = stage, result = result, scene = SceneManager.GetActiveScene().path, unityVersion = Application.unityVersion, atUtc = DateTime.UtcNow.ToString("o") }, true));
        Debug.Log("[P02-FAST] " + result);
    }
    [Serializable] private sealed class Status { public string stage; public string result; public string scene; public string unityVersion; public string atUtc; }
    [Serializable] private sealed class Metrics { public float ssrSeconds; public float urSeconds; public float acceleratedRemainingSeconds; public int currency; public int totalPulls; public bool previewCountsUnchanged; }
}