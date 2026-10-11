using System;
using System.IO;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PrototypeSetup
{
    private const string ScenePath = "Assets/_Game/Scenes/Main.unity";
    private const string SmokeFlag = "ThisTime.PrototypeQuickCheck";
    private static string WorkRoot { get { return Directory.GetParent(Application.dataPath).Parent.FullName; } }
    private static string ReportRoot { get { return Path.Combine(WorkRoot, "Docs", "Production"); } }
    private static string RequestPath { get { return Path.Combine(ReportRoot, "Prototype.request"); } }

    [Serializable]
    private sealed class RunReport
    {
        public string stage;
        public string result;
        public string scene;
        public string screenshot;
        public string at;
    }

    static PrototypeSetup()
    {
        EditorApplication.delayCall += AutoSetup;
        EditorApplication.update += CheckRequest;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void AutoSetup()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !File.Exists(ScenePath)) CreateMain();
    }

    [MenuItem("Tools/这次一定出/创建原型主场景")]
    public static void CreateMain()
    {
        if (!File.Exists(ScenePath))
        {
            Directory.CreateDirectory("Assets/_Game/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var root = new GameObject("PlayablePrototype");
            SceneManager.MoveGameObjectToScene(root, scene);
            root.AddComponent<PrototypeGame>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.Refresh();
        }
        // 保留集成负责人配置的其他场景；重复初始化不追加第二份入口。
        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!buildScenes.Exists(item => item.path == ScenePath))
            buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
        AssetDatabase.SaveAssets();
        WriteReport("ready", "主场景已创建；打开后按 Play。", "");
    }

    [MenuItem("Tools/这次一定出/打开可玩原型")]
    public static void OpenMain()
    {
        CreateMain();
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Tools/这次一定出/验证最短可玩流程")]
    public static void QuickCheck()
    {
        CreateMain();
        if (SceneManager.GetActiveScene().isDirty)
        {
            WriteReport("ready", "请先保存当前场景，再运行最短流程验证。", "");
            return;
        }
        EditorSceneManager.OpenScene(ScenePath);
        SessionState.SetBool(SmokeFlag, true);
        EditorApplication.EnterPlaymode();
    }

    // 通过项目内请求文件使用已有 Editor，避免第二个 Editor 抢工程锁。
    private static void CheckRequest()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.ExitPlaymode();
            return;
        }
        string request = File.ReadAllText(RequestPath).Trim();
        File.Delete(RequestPath);
        if (request == "smoke") QuickCheck();
        else if (request == "setup") CreateMain();
        else if (request == "p00") ProgramBaseline.InitializeAndCheck();
    }

    private static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(SmokeFlag, false)) return;
        SessionState.SetBool(SmokeFlag, false);
        PrototypeGame prototype = UnityEngine.Object.FindObjectOfType<PrototypeGame>();
        string screenshot = Path.Combine(ReportRoot, "F01_runtime.png");
        prototype.StartCoroutine(prototype.RunQuickCheck(result =>
        {
            WriteReport(result.StartsWith("PASS") ? "passed" : "failed", result, screenshot);
            EditorApplication.delayCall += () => EditorApplication.ExitPlaymode();
        }, screenshot));
    }

    private static void WriteReport(string stage, string result, string screenshot)
    {
        Directory.CreateDirectory(ReportRoot);
        File.WriteAllText(Path.Combine(ReportRoot, "F01_runtime_status.json"), JsonUtility.ToJson(new RunReport
        {
            stage = stage, result = result, scene = ScenePath, screenshot = screenshot,
            at = DateTime.UtcNow.ToString("o")
        }, true));
        Debug.Log("[这次一定出原型] " + result);
    }
}
