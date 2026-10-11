using System;
using System.IO;
using System.Reflection;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class U01Setup
{
    private const string ScenePath = "Assets/_Game/Scenes/UIPrototype.unity";
    private const string PrefabPath = "Assets/_Game/UI/Prefabs/UIPrototype.prefab";
    private const string ThemePath = "Assets/_Game/UI/Styles/PrototypeUITheme.asset";
    private const string Flag = "ThisTime.U01Check";
    private static readonly string Reports = Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,
        "Docs", "Production", "U01_Verification");
    private static string Request { get { return Path.Combine(Reports, "U01.request"); } }
    private static int previousSize = -1;
    private static EditorWindow gameView;
    private static bool testing;

    static U01Setup()
    {
        EditorApplication.update += ReadRequest;
        EditorApplication.update += RepaintTest;
        EditorApplication.playModeStateChanged += PlayChanged;
        Application.logMessageReceived += RecordError;
    }

    [MenuItem("Tools/这次一定出/UI/创建独立UI原型")]
    public static void Create()
    {
        Directory.CreateDirectory("Assets/_Game/UI/Prefabs");
        Directory.CreateDirectory("Assets/_Game/UI/Styles");
        AssetDatabase.Refresh();
        PrototypeUITheme theme = AssetDatabase.LoadAssetAtPath<PrototypeUITheme>(ThemePath);
        if (theme == null)
        {
            theme = ScriptableObject.CreateInstance<PrototypeUITheme>();
            AssetDatabase.CreateAsset(theme, ThemePath);
        }
        if (!File.Exists(PrefabPath))
        {
            var root = new GameObject("U01_UIPrototype");
            PrototypeGame ui = root.AddComponent<PrototypeGame>();
            ui.theme = theme;
            ui.useFixedMock = true;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }
        if (!File.Exists(ScenePath))
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
        }
        AssetDatabase.SaveAssets();
        Write("ready", "独立 UI 场景与 Prefab 已生成；不覆盖已有资产。");
    }

    [MenuItem("Tools/这次一定出/UI/打开独立UI原型")]
    public static void Open()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Create();
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Tools/这次一定出/UI/验证交互与三种窗口")]
    public static void Verify()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
        {
            Write("blocked", "请先停止 Play 并保存当前场景，再复验。");
            return;
        }
        Create();
        EditorSceneManager.OpenScene(ScenePath);
        SessionState.SetBool(Flag, true);
        Write("starting", "已请求进入 Play；等待实际运行复验。");
        EditorApplication.EnterPlaymode();
    }

    private static void ReadRequest()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
        string command = File.ReadAllText(Request).Trim();
        File.Delete(Request);
        if (command == "verify") Verify();
        else if (command == "create") Create();
        else if (command == "status") Write("observed", "isPlaying=" + EditorApplication.isPlaying
            + "; willChange=" + EditorApplication.isPlayingOrWillChangePlaymode
            + "; scene=" + SceneManager.GetActiveScene().path);
    }

    private static void PlayChanged(PlayModeStateChange state)
    {
        if (SessionState.GetBool(Flag, false)) Write("transition", state.ToString());
        if (state == PlayModeStateChange.EnteredEditMode && gameView != null && previousSize >= 0)
        {
            gameView.GetType().GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic).SetValue(gameView, previousSize);
            previousSize = -1;
        }
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Flag, false)) return;
        SessionState.SetBool(Flag, false);
        testing = true;
        Application.runInBackground = true;
        Directory.CreateDirectory(Reports);
        PrototypeGame ui = UnityEngine.Object.FindObjectOfType<PrototypeGame>();
        Write("running", "独立 UI 场景已启动；正在执行按钮流程与截图。");
        ui.StartCoroutine(ui.RunUICheck(result =>
        {
            testing = false;
            Write(result.StartsWith("PASS") ? "passed" : "failed", result);
            EditorApplication.delayCall += () => EditorApplication.ExitPlaymode();
        }, Reports, Resize));
    }

    private static void RepaintTest()
    {
        if (testing && EditorApplication.isPlaying && gameView != null) gameView.Repaint();
    }

    private static void RecordError(string message, string trace, LogType type)
    {
        if (type != LogType.Exception && type != LogType.Error) return;
        Directory.CreateDirectory(Reports);
        File.AppendAllText(Path.Combine(Reports, "editor_errors.log"), message + "\n" + trace + "\n");
    }

    private static void Resize(int width, int height)
    {
        // 仅由显式复验调用；通过 Editor 的 GameView 固定尺寸实际渲染。
        Assembly editor = typeof(Editor).Assembly;
        Type viewType = editor.GetType("UnityEditor.GameView");
        gameView = EditorWindow.GetWindow(viewType);
        PropertyInfo selected = viewType.GetProperty("selectedSizeIndex", BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic);
        if (previousSize < 0) previousSize = (int)selected.GetValue(gameView);
        Type sizesType = editor.GetType("UnityEditor.GameViewSizes");
        Type singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singleton.GetProperty("instance").GetValue(null);
        object groupType = sizesType.GetProperty("currentGroupType").GetValue(sizes);
        object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { groupType });
        Type groupClass = group.GetType();
        int total = (int)groupClass.GetMethod("GetTotalCount").Invoke(group, null);
        int index = -1;
        for (int i = 0; i < total; i++)
        {
            object size = groupClass.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
            Type type = size.GetType();
            if ((int)type.GetProperty("width").GetValue(size) == width &&
                (int)type.GetProperty("height").GetValue(size) == height) { index = i; break; }
        }
        if (index < 0)
        {
            Type sizeType = editor.GetType("UnityEditor.GameViewSize");
            object fixedType = Enum.Parse(editor.GetType("UnityEditor.GameViewSizeType"), "FixedResolution");
            object custom = Activator.CreateInstance(sizeType, new[] { fixedType, (object)width, height, "U01 " + width + "x" + height });
            groupClass.GetMethod("AddCustomSize").Invoke(group, new[] { custom });
            index = total;
        }
        selected.SetValue(gameView, index);
        gameView.Focus();
    }

    private static void Write(string stage, string result)
    {
        Directory.CreateDirectory(Reports);
        File.WriteAllText(Path.Combine(Reports, "runtime_status.json"), JsonUtility.ToJson(new Report
        { stage = stage, result = result, at = DateTime.UtcNow.ToString("o") }, true));
        Debug.Log("[U01 UI] " + result);
    }

    [Serializable] private sealed class Report { public string stage; public string result; public string at; }
}
