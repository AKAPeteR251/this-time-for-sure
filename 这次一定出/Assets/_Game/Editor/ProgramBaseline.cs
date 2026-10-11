using System;
using System.IO;
using System.Linq;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// P00 只管理程序目录和自己的实验入口，不覆盖 Main/UI 场景。
public static class ProgramBaseline
{
    private const string LabPath = "Assets/_Game/Scenes/PresentationLab.unity";
    private static readonly string[] Folders =
    {
        "Assets/_Game/Scripts/Core", "Assets/_Game/Scripts/Infrastructure",
        "Assets/_Game/Scripts/Presentation", "Assets/_Game/Data/Characters",
        "Assets/_Game/Data/Pools", "Assets/_Game/Data/Presentation",
        "Assets/_Game/Tests/EditMode", "Assets/_Game/Tests/PlayMode",
        "Assets/_Game/Prefabs/Presentation", "Assets/_Game/Scenes"
    };

    [Serializable]
    private sealed class BaselineReport
    {
        public string atUtc;
        public string unityVersion;
        public string editorPath;
        public bool windowsBuildSupported;
        public bool labHasOnePrototype;
        public bool repeatedSetupPreservedLab;
        public bool repeatedSetupPreservedBuildScenes;
        public string[] packages;
        public string[] missingFolders;
        public string validationScope;
    }

    [MenuItem("Tools/这次一定出/程序/初始化并校验开发基线")]
    public static void InitializeAndCheck()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Initialize();
        byte[] labBefore = File.ReadAllBytes(LabPath);
        string[] buildsBefore = EditorBuildSettings.scenes.Select(s => s.path + ":" + s.enabled).ToArray();
        Initialize();

        // 加载为附加场景，检查完成即关闭；不切换用户正在编辑的场景。
        Scene lab = SceneManager.GetSceneByPath(LabPath);
        bool openedHere = !lab.IsValid() || !lab.isLoaded;
        if (openedHere) lab = EditorSceneManager.OpenScene(LabPath, OpenSceneMode.Additive);
        bool onePrototype = lab.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<PrototypeGame>(true)).Count() == 1;
        if (openedHere) EditorSceneManager.CloseScene(lab, true);

        var report = new BaselineReport
        {
            atUtc = DateTime.UtcNow.ToString("o"),
            unityVersion = Application.unityVersion,
            editorPath = EditorApplication.applicationPath,
            windowsBuildSupported = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64),
            labHasOnePrototype = onePrototype,
            repeatedSetupPreservedLab = labBefore.SequenceEqual(File.ReadAllBytes(LabPath)),
            repeatedSetupPreservedBuildScenes = buildsBefore.SequenceEqual(EditorBuildSettings.scenes.Select(s => s.path + ":" + s.enabled)),
            packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()
                .Where(p => p.name == "com.unity.ugui" || p.name == "com.unity.textmeshpro" || p.name == "com.unity.render-pipelines.universal")
                .Select(p => p.name + "@" + p.version).ToArray(),
            missingFolders = Folders.Where(path => !AssetDatabase.IsValidFolder(path)).ToArray(),
            validationScope = "Editor import and repeated initialization only; Play Mode is a separate check."
        };
        string reportRoot = Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName, "Docs", "Production", "Verification");
        Directory.CreateDirectory(reportRoot);
        File.WriteAllText(Path.Combine(reportRoot, "P00_editor_status.json"), JsonUtility.ToJson(report, true));
        Debug.Log("[P00] 开发基线校验已记录：" + reportRoot);
    }

    private static void Initialize()
    {
        foreach (string folder in Folders) EnsureFolder(folder);
        if (!File.Exists(LabPath))
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene lab = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var root = new GameObject("ProgramPrototypeLab");
            SceneManager.MoveGameObjectToScene(root, lab);
            root.AddComponent<PrototypeGame>();
            EditorSceneManager.SaveScene(lab, LabPath);
            EditorSceneManager.CloseScene(lab, true);
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        }
        AssetDatabase.SaveAssets();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = path.Substring(0, path.LastIndexOf('/'));
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    [MenuItem("Tools/这次一定出/程序/打开程序实验场景")]
    public static void OpenLab()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Initialize();
        EditorSceneManager.OpenScene(LabPath);
    }
}
