using System;
using System.Collections;
using System.IO;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class P20RingEulerSetup
{
    private const string Config="Assets/_Game/Data/Presentation/P05FeelConfig.asset";
    private static string Folder {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P20_RING_EULER_Verification");}}
    static P20RingEulerSetup(){EditorApplication.update+=Poll;}
    private static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        string path=Path.Combine(Folder,"P20.request");if(!File.Exists(path))return;
        string cmd=File.ReadAllText(path).Trim();File.Delete(path);
        if(cmd=="prepare")Prepare();else if(cmd=="verify")P20RingEulerVerifySetup.Verify();
    }
    [MenuItem("Tools/这次一定出/程序/P20-RING-EULER/保存三环独立旋转")]
    public static void Prepare()
    {
        Directory.CreateDirectory(Folder);
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorSceneManager.GetActiveScene().isDirty){File.WriteAllText(Path.Combine(Folder,"status.txt"),"blocked: Play or dirty scene");return;}
        var cfg=AssetDatabase.LoadAssetAtPath<P04FeelConfig>(Config);
        cfg.crystalOuterEulerSpeed=new Vector3(9,3,0);cfg.crystalMiddleEulerSpeed=new Vector3(0,-11,4);cfg.crystalInnerEulerSpeed=new Vector3(-4,0,13);
        EditorUtility.SetDirty(cfg);AssetDatabase.SaveAssets();
        File.WriteAllText(Path.Combine(Folder,"status.txt"),"ready "+DateTime.UtcNow.ToString("o")+" Outer(9,3,0) Middle(0,-11,4) Inner(-4,0,13)");
    }
}
