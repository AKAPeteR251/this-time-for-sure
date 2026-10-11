using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ThisTime.Game;

[InitializeOnLoad]
public static class P29CardUISetup
{
    public static string Reports { get { return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P29_CARD_UI_Verification"); } }
    private const string Folder="Assets/_Game/UI/Sprites/F10/CharacterCardUI_v001/";
    static P29CardUISetup() { EditorApplication.update += Poll; }
    private static void Poll()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string path=Path.Combine(Reports,"P29.request"); if(!File.Exists(path))return;
        string command=File.ReadAllText(path).Trim(); File.Delete(path);
        try { if(command=="prepare") Prepare(); else if(command=="verify") P29CardUIVerify.Verify(); }
        catch(Exception e) { Directory.CreateDirectory(Reports); File.WriteAllText(Path.Combine(Reports,"setup_status.txt"),e.ToString()); Debug.LogException(e); }
    }
    [MenuItem("Tools/这次一定出/程序/P29-CARD-UI/导入F10卡面")]
    public static void Prepare()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorSceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("请停止Play并保存编辑");
        Directory.CreateDirectory(Reports); AssetDatabase.Refresh();
        foreach(string path in Directory.GetFiles(Folder,"*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.wrapMode=TextureWrapMode.Clamp;
            importer.maxTextureSize=2048; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        }
        var theme=AssetDatabase.LoadAssetAtPath<PrototypeUITheme>("Assets/_Game/UI/Styles/U02ArtTheme.asset");
        theme.cardFrameR=Load("card_frame_R");theme.cardFrameSR=Load("card_frame_SR");theme.cardFrameSSR=Load("card_frame_SSR");theme.cardFrameUR=Load("card_frame_UR");
        theme.cardBadgeR=Load("card_rarity_R");theme.cardBadgeSR=Load("card_rarity_SR");theme.cardBadgeSSR=Load("card_rarity_SSR");theme.cardBadgeUR=Load("card_rarity_UR");
        theme.cardNewMask=Load("card_new_border_mask");theme.cardNewFill=Load("card_new_fill");theme.cardNameShade=Load("card_name_shade");theme.cardNameOrnament=Load("card_name_ornament");
        theme.cardNewSpeed=.36f;theme.cardNewBlink=.35f;theme.cardUrSpeed=.12f;
        EditorUtility.SetDirty(theme); AssetDatabase.SaveAssets(); File.WriteAllText(Path.Combine(Reports,"setup_status.txt"),"ready "+DateTime.UtcNow.ToString("o"));
    }
    private static Sprite Load(string name) {var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+name+"_v001.png");if(sprite==null)throw new InvalidOperationException("素材导入失败:"+name);return sprite;}
}
