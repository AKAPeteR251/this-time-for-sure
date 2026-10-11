using System;
using System.IO;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class P26ContractBurnSetup
{
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P26_CONTRACT_BURN_Verification");}}
    static P26ContractBurnSetup(){EditorApplication.update+=Poll;}
    private static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        string path=Path.Combine(Reports,"P26.request");if(!File.Exists(path))return;
        string command=File.ReadAllText(path).Trim();File.Delete(path);
        try{if(command=="prepare")Prepare();else if(command=="verify")P26ContractBurnVerify.Verify();}
        catch(Exception e){File.WriteAllText(Path.Combine(Reports,"setup_status.txt"),e.ToString());Debug.LogException(e);}
    }
    [MenuItem("Tools/这次一定出/程序/P26-CONTRACT-BURN/保存契约燃烧配置")]
    public static void Prepare()
    {
        Directory.CreateDirectory(Reports);
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Play或未保存编辑，暂不覆盖配置");
        AssetDatabase.Refresh();var cfg=AssetDatabase.LoadAssetAtPath<P04FeelConfig>("Assets/_Game/Data/Presentation/P05FeelConfig.asset");
        cfg.contractBurnShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Art/Effects/F04/Crystal/Effects/ContractBurn.shader");
        if(cfg.contractBurnShader==null||ShaderUtil.ShaderHasError(cfg.contractBurnShader))throw new InvalidOperationException("契约燃烧Shader导入失败");
        EditorUtility.SetDirty(cfg);AssetDatabase.SaveAssets();File.WriteAllText(Path.Combine(Reports,"setup_status.txt"),"ready "+DateTime.UtcNow.ToString("o"));
    }
}
