using System;
using System.IO;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class P23ContractSetup
{
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P23_CONTRACT_Verification");}}
    private const string Art="Assets/_Game/UI/Sprites/F09/ContractParchmentWax_v003/";
    static P23ContractSetup(){EditorApplication.update+=Poll;}
    private static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        string path=Path.Combine(Reports,"P23.request");if(!File.Exists(path))return;
        string command=File.ReadAllText(path).Trim();File.Delete(path);
        try{if(command=="prepare")Prepare();else if(command=="verify")P23ContractVerify.Verify();}
        catch(Exception e){Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"setup_status.txt"),e.ToString());Debug.LogException(e);}
    }
    [MenuItem("Tools/这次一定出/程序/P23-CONTRACT/保存F09契约引用")]
    public static void Prepare()
    {
        Directory.CreateDirectory(Reports);
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Play或未保存编辑，暂不覆盖配置");
        AssetDatabase.Refresh();
        foreach(string file in new[]{"contract_parchment_blank_v003.png","contract_english_lettering_v003.png","contract_wax_seal_red_v003.png","contract_wax_shadow_v003.png"})
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Art+file);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
        }
        var cfg=AssetDatabase.LoadAssetAtPath<P04FeelConfig>("Assets/_Game/Data/Presentation/P05FeelConfig.asset");
        cfg.contractPaper=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"contract_parchment_blank_v003.png");
        cfg.contractEnglishLettering=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"contract_english_lettering_v003.png");
        cfg.starSeal=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"contract_wax_seal_red_v003.png");
        cfg.contractWaxShadow=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"contract_wax_shadow_v003.png");
        cfg.contractAspect=4f/3f;
        cfg.contractSignatureRect=new Vector4(205f/2048,1-1275f/1536,1475f/2048,1-922f/1536);
        cfg.contractWaxRect=new Vector4(1520f/2048,1-1490f/1536,1880f/2048,1-1130f/1536);
        if(!ContractPaperLayout.Available(cfg)||cfg.starSeal==null||cfg.contractWaxShadow==null)throw new InvalidOperationException("F09素材引用缺失");
        EditorUtility.SetDirty(cfg);AssetDatabase.SaveAssets();File.WriteAllText(Path.Combine(Reports,"setup_status.txt"),"ready "+DateTime.UtcNow.ToString("o"));
    }
}
