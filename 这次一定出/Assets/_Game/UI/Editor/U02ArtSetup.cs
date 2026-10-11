using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// U02-ART 仅管理 UIPrototype、现有 UI 入口 Prefab、新主题及清单中的导入设置。
[InitializeOnLoad]
public static class U02ArtSetup
{
    public const string ThemePath = "Assets/_Game/UI/Styles/U02ArtTheme.asset";
    public const string PrefabPath = "Assets/_Game/UI/Prefabs/UIPrototype.prefab";
    public const string ScenePath = "Assets/_Game/Scenes/UIPrototype.unity";
    private const string Flag = "ThisTime.U02ArtCheck";
    private static string Root { get { return Directory.GetParent(Application.dataPath).Parent.FullName; } }
    private static string ReportRoot { get { return Path.Combine(Root, SessionState.GetBool("ThisTime.U02Visual", false) ? "Docs/Production/U02_VIS_Verification" : "Docs/Production/U02_ART_Verification"); } }
    private static string Request { get { return Path.Combine(Root, "Docs/Production/U02_ART_Verification/U02.request"); } }
    private static EditorWindow gameView;
    private static bool testing;

    public static readonly string[] Ids = { "R001", "SR001", "SR002", "SR003", "SSR001", "UR001" };
    private static readonly string[] Files = { "06_star_magical_girl_v001", "02_light_priest_v001",
        "03_wind_elf_archer_v002", "05_thunder_miko_v001", "01_ice_mage_v001", "04_fire_warrior_v001" };
    private const string Characters = "Assets/_Game/Art/Characters/UIMockRoles/";
    private const string Effects = "Assets/_Game/Art/Effects/UIAssets/";

    static U02ArtSetup()
    {
        EditorApplication.update += Poll;
        EditorApplication.playModeStateChanged += PlayChanged;
        Application.logMessageReceived += LogErrors;
    }

    [MenuItem("Tools/这次一定出/UI/U02-ART/导入并配置六卡美术")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) { Write("blocked", "请停止 Play 后配置。"); return; }
        Scene current = SceneManager.GetActiveScene();
        if (current.path == ScenePath && current.isDirty) { Write("blocked", "UIPrototype 有未保存编辑，请先保存。"); return; }
        Directory.CreateDirectory(ReportRoot);
        AssetDatabase.Refresh();
        string source = File.ReadAllText(Path.Combine(Root, "ArtSource/UIAssets/asset_manifest.json"));
        Manifest manifest = JsonUtility.FromJson<Manifest>("{\"entries\":" + source + "}");
        var imported = new List<ImportEntry>();
        foreach (Asset entry in manifest.entries)
        {
            int start = entry.path.IndexOf("Assets/_Game/Art/", StringComparison.Ordinal);
            if (start < 0) continue;
            string path = entry.path.Substring(start).Replace('\\', '/');
            if (Hash(Path.Combine(Root, entry.path)) != entry.sha256) throw new InvalidOperationException("素材哈希不一致:" + path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = entry.mode == "RGBA";
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = (path.StartsWith(Characters) && !path.EndsWith("_thumb.png")) || path.Contains("icon_") || path.Contains("element_");
            if (importer.mipmapEnabled) importer.filterMode = FilterMode.Trilinear;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            if (path.Contains("button_")) importer.spriteBorder = new Vector4(110, 70, 110, 70);
            else if (path.Contains("panel_")) importer.spriteBorder = new Vector4(65, 45, 65, 45);
            else importer.spriteBorder = Vector4.zero;
            importer.SaveAndReimport();
            imported.Add(new ImportEntry { assetId = entry.assetId, path = path, guid = AssetDatabase.AssetPathToGUID(path),
                border = importer.spriteBorder.ToString(), sha256 = entry.sha256 });
        }
        if (imported.Count != 36) throw new InvalidOperationException("预期清单36项，实际:" + imported.Count);
        Directory.CreateDirectory("Assets/_Game/UI/Styles");
        var theme = AssetDatabase.LoadAssetAtPath<PrototypeUITheme>(ThemePath);
        if (theme == null) { theme = ScriptableObject.CreateInstance<PrototypeUITheme>(); AssetDatabase.CreateAsset(theme, ThemePath); }
        theme.background = new Color(0.02f, 0.045f, 0.09f);
        theme.panel = new Color(0.03f, 0.055f, 0.105f);
        theme.text = new Color(1, 0.95f, 0.83f);
        theme.primary = new Color(0.87f, 0.71f, 0.43f);
        theme.muted = new Color(0.71f, 0.78f, 0.85f);
        theme.summonBackground = Load("Assets/_Game/Art/Backgrounds/UI_FrozenCastle_v001.png");
        theme.summonCharacter = Load("Assets/_Game/Art/Characters/UIIceMage/UIIceMage_character_v001.png");
        theme.buttonNormal = Load(Effects + "button_normal_v002.png");
        theme.buttonPrimary = Load(Effects + "button_selected_v002.png");
        theme.informationPanel = Load(Effects + "panel_information_v001.png");
        theme.tooltipPanel = Load(Effects + "panel_tooltip_v001.png");
        theme.frameR = Load(Effects + "frame_R_v002.png");
        theme.frameSR = Load(Effects + "frame_SR_v002.png");
        theme.frameSSR = Load(Effects + "frame_SSR_v002.png");
        theme.frameUR = Load(Effects + "frame_UR_v002.png");
        theme.currencyIcon = Load(Effects + "icon_currency_v001.png");
        ConfigureIcons(theme);
        theme.sparkleBlue = Load(Effects + "fx_sparkle_blue_v002.png");
        theme.sparkleGold = Load(Effects + "fx_sparkle_gold_v002.png");
        EditorUtility.SetDirty(theme);
        if (!File.Exists(PrefabPath) || !File.Exists(ScenePath)) U01Setup.Create();
        GameObject prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try { Configure(prefab.GetComponent<PrototypeGame>(), theme); PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        var ui = scene.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<PrototypeGame>(true)).Single();
        Configure(ui, theme);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        if (openedHere) EditorSceneManager.CloseScene(scene, true);
        AssetDatabase.SaveAssets();
        File.WriteAllText(Path.Combine(ReportRoot, "import_report.json"), JsonUtility.ToJson(new ImportReport
        { unityVersion = Application.unityVersion, assets = imported.ToArray(), useFixedMock = false,
            theme = ThemePath, prefab = PrefabPath, scene = ScenePath }, true));
        Write("ready", "36项 Sprite 导入完成，UIPrototype 与 Prefab 已绑定六卡及 U02 主题，真实内存入口。");
    }

    public static void Configure(PrototypeGame ui, PrototypeUITheme theme)
    {
        ui.theme = theme;
        ui.useFixedMock = false;
        ui.portraits = new List<PrototypePortrait>();
        for (int i = 0; i < Ids.Length; i++) ui.portraits.Add(new PrototypePortrait
        { characterId = Ids[i], portrait = Load(Characters + Files[i] + ".png"),
            thumbnail = Load(Characters + Files[i] + "_thumb.png") });
        EditorUtility.SetDirty(ui);
    }

    private static Sprite Load(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new InvalidOperationException("Sprite 缺失:" + path);
        return sprite;
    }

    private static void ConfigureIcons(PrototypeUITheme theme)
    {
        theme.summonIcon = Load(Effects + "icon_summon_v001.png");
        theme.collectionIcon = Load(Effects + "icon_collection_v001.png");
        theme.elementIce = Load(Effects + "element_ice_v001.png");
        theme.elementLight = Load(Effects + "element_light_v001.png");
        theme.elementWind = Load(Effects + "element_wind_v001.png");
        theme.elementFire = Load(Effects + "element_fire_v001.png");
        theme.elementThunder = Load(Effects + "element_thunder_v001.png");
    }

    [MenuItem("Tools/这次一定出/UI/U02-ART/打开真实美术原型")]
    public static void Open()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Tools/这次一定出/UI/U02-ART/验证真实内存闭环与三种窗口")]
    public static void Verify()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
        { Write("blocked", "请停止 Play 并保存当前编辑后复验。"); return; }
        var theme = AssetDatabase.LoadAssetAtPath<PrototypeUITheme>(ThemePath);
        foreach (string file in Files)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(Characters + file + ".png");
            if (importer.mipmapEnabled && importer.filterMode == FilterMode.Trilinear) continue;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.SaveAndReimport();
        }
        ConfigureIcons(theme);
        foreach (string path in AssetDatabase.FindAssets("t:Texture2D", new[] { Effects.TrimEnd('/') }).Select(AssetDatabase.GUIDToAssetPath))
        {
            if (!path.Contains("icon_") && !path.Contains("element_")) continue;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.mipmapEnabled && importer.filterMode == FilterMode.Trilinear) continue;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.SaveAndReimport();
        }
        EditorUtility.SetDirty(theme);
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(ScenePath);
        SessionState.SetBool(Flag, true);
        Write("starting", "请求真实内存 Play 复验。");
        EditorApplication.EnterPlaymode();
    }

    private static void Poll()
    {
        if (testing && EditorApplication.isPlaying && gameView != null) gameView.Repaint();
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
        string request = File.ReadAllText(Request).Trim();
        File.Delete(Request);
        if (request == "prepare") Prepare();
        else if (request == "verify") { SessionState.SetBool("ThisTime.U02Visual", false); Verify(); }
        else if (request == "verify-visual") { SessionState.SetBool("ThisTime.U02Visual", true); Verify(); }
        else if (request == "open") Open();
        else if (request == "stop") EditorApplication.ExitPlaymode();
        else if (request == "status") Write("observed", "play=" + EditorApplication.isPlaying + "; scene=" + SceneManager.GetActiveScene().path);
    }

    private static void PlayChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Flag, false)) return;
        SessionState.SetBool(Flag, false);
        testing = true;
        Application.runInBackground = true;
        Write("running", "运行真实内存抽卡和三窗口截图。");
        var ui = UnityEngine.Object.FindObjectOfType<PrototypeGame>();
        ui.StartCoroutine(ui.RunArtCheck(result =>
        {
            testing = false;
            Write(result.StartsWith("PASS") ? "passed" : "failed", result);
            File.Copy(Path.Combine(ReportRoot, "runtime_status.json"), Path.Combine(ReportRoot, "final_result.json"), true);
            EditorApplication.delayCall += () => EditorApplication.ExitPlaymode();
        }, ReportRoot, Resize));
    }

    private static void Resize(int width, int height)
    {
        // 复用已实测的 U01 GameView 尺寸管理及退出恢复。
        typeof(U01Setup).GetMethod("Resize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { width, height });
        gameView = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        gameView.Repaint();
    }

    private static string Hash(string path)
    {
        using (SHA256 sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
    }

    private static void LogErrors(string message, string trace, LogType type)
    {
        if ((type == LogType.Error || type == LogType.Exception) && (trace.Contains("PrototypeGame") || trace.Contains("U02ArtSetup")))
        { Directory.CreateDirectory(ReportRoot); File.AppendAllText(Path.Combine(ReportRoot, "errors.log"), message + "\n" + trace + "\n"); }
    }

    private static void Write(string stage, string result)
    {
        Directory.CreateDirectory(ReportRoot);
        File.WriteAllText(Path.Combine(ReportRoot, "runtime_status.json"), JsonUtility.ToJson(new Status
        { stage = stage, result = result, atUtc = DateTime.UtcNow.ToString("o") }, true));
        Debug.Log("[U02-ART] " + result);
    }
    [Serializable] private sealed class Status { public string stage; public string result; public string atUtc; }
    [Serializable] private sealed class Manifest { public Asset[] entries; }
    [Serializable] private sealed class Asset { public string assetId; public string path; public string sha256; public string mode; }
    [Serializable] private sealed class ImportEntry { public string assetId; public string path; public string guid; public string border; public string sha256; }
    [Serializable] private sealed class ImportReport { public string unityVersion; public ImportEntry[] assets; public bool useFixedMock; public string theme; public string prefab; public string scene; }
}
