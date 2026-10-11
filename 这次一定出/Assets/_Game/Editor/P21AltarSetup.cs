using System;
using System.IO;
using System.Linq;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class P21AltarSetup
{
    public const string Art="Assets/_Game/Art/Backgrounds/F08/UndergroundAltar/";
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P21_ALTAR_Verification");}}
    static P21AltarSetup(){EditorApplication.update+=Poll;}
    private static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        string file=Path.Combine(Reports,"P21.request");if(!File.Exists(file))return;
        string cmd=File.ReadAllText(file).Trim();File.Delete(file);
        try{if(cmd=="prepare")Prepare();else if(cmd=="verify")P21AltarVerifySetup.Verify();}
        catch(Exception e){Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"import_status.txt"),e.ToString());Debug.LogException(e);}
    }
    [MenuItem("Tools/这次一定出/程序/P21-ALTAR/导入地下圣殿并接入Main配置")]
    public static void Prepare()
    {
        Directory.CreateDirectory(Reports);
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Play/未保存编辑，停止素材接入");
        AssetDatabase.Refresh();
        foreach(string file in Directory.GetFiles(Art+"Textures","*.png"))
        {
            var t=AssetImporter.GetAtPath(file.Replace('\\','/')) as TextureImporter;
            t.textureType=file.Contains("normal")?TextureImporterType.NormalMap:TextureImporterType.Default;
            t.sRGBTexture=file.Contains("basecolor")||file.Contains("flame");t.alphaIsTransparency=file.Contains("flame")||file.Contains("mist")||file.Contains("dust");
            t.wrapMode=file.Contains("stone")||file.Contains("antique_gold")?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
            t.mipmapEnabled=!t.alphaIsTransparency;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();
        }
        Directory.CreateDirectory(Art+"Materials");
        Pack("stone");Pack("antique_gold");AssetDatabase.Refresh();
        foreach(string prefix in new[]{"stone","antique_gold"})
        {
            var t=AssetImporter.GetAtPath(Art+"Textures/"+prefix+"_metallic_smoothness_2048_p21.png") as TextureImporter;t.sRGBTexture=false;t.wrapMode=TextureWrapMode.Repeat;t.SaveAndReimport();
        }
        var importer=AssetImporter.GetAtPath(Art+"F08_UndergroundAltar_v001.fbx") as ModelImporter;
        importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.globalScale=1;importer.useFileScale=true;
        importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
        foreach(string name in new[]{"F08_DeepBlueStone","F08_AntiqueGold","F08_NicheShadowStone","F08_AltarInlayEmission"})
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(Art+"Materials/"+name+".mat");
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,Art+"Materials/"+name+".mat");}
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),name),mat);
        }
        importer.SaveAndReimport();
        foreach(string name in new[]{"F08_DeepBlueStone","F08_AntiqueGold","F08_NicheShadowStone","F08_AltarInlayEmission"})
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(Art+"Materials/"+name+".mat");mat.shader=Shader.Find("Universal Render Pipeline/Lit");
            if(name=="F08_DeepBlueStone"||name=="F08_AntiqueGold")
            {
                string p=name=="F08_DeepBlueStone"?"stone":"antique_gold";
                mat.SetColor("_BaseColor",Color.white);mat.SetTexture("_BaseMap",Texture(p+"_basecolor_2048_v001.png"));
                mat.SetTexture("_BumpMap",Texture(p+"_normal_2048_v001.png"));mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.5f);
                mat.SetTexture("_MetallicGlossMap",Texture(p+"_metallic_smoothness_2048_p21.png"));mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.SetFloat("_Smoothness",1);
            }
            else if(name.Contains("Niche")){mat.SetColor("_BaseColor",new Color(.007f,.012f,.025f).gamma);mat.SetFloat("_Metallic",0);mat.SetFloat("_Smoothness",.3f);}
            else{mat.SetColor("_BaseColor",new Color(1,.5f,.13f).gamma);mat.SetFloat("_Metallic",.2f);mat.SetFloat("_Smoothness",.6f);mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",new Color(1,.5f,.13f)*.6f);}
            EditorUtility.SetDirty(mat);
        }
        var config=AssetDatabase.LoadAssetAtPath<P04FeelConfig>("Assets/_Game/Data/Presentation/P05FeelConfig.asset");
        config.altarEnvironmentModel=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"F08_UndergroundAltar_v001.fbx");
        config.altarFlame=Texture("flame_amber_flipbook_4x4_2048_v001.png");config.altarMist=Texture("floor_mist_1024_v001.png");config.altarDust=Texture("dust_soft_512_v001.png");
        config.altarBillboardShader=AssetDatabase.LoadAssetAtPath<Shader>(Art+"AltarBillboard.shader");
        if(ShaderUtil.ShaderHasError(config.altarBillboardShader))throw new InvalidOperationException("Altar shader error");
        EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();
        var meshes=config.altarEnvironmentModel.GetComponentsInChildren<MeshRenderer>(true);
        if(meshes.Length!=127)throw new InvalidOperationException("F08预期127网格，实际"+meshes.Length);
        File.WriteAllLines(Path.Combine(Reports,"unity_environment_nodes.txt"),config.altarEnvironmentModel.GetComponentsInChildren<Transform>(true).Select(t=>t.name+" local="+t.localPosition+" rot="+t.localEulerAngles+" scale="+t.localScale));
        File.WriteAllText(Path.Combine(Reports,"import_status.txt"),"ready "+DateTime.UtcNow.ToString("o")+"; F08 127 meshes, four URP materials, 12 source maps, two packed maps; existing Main config updated");
    }
    private static Texture2D Texture(string name){return AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Textures/"+name);}
    private static void Pack(string prefix)
    {
        var metal=new Texture2D(2,2,TextureFormat.RGBA32,false,true);var rough=new Texture2D(2,2,TextureFormat.RGBA32,false,true);
        metal.LoadImage(File.ReadAllBytes(Art+"Textures/"+prefix+"_metallic_2048_v001.png"));rough.LoadImage(File.ReadAllBytes(Art+"Textures/"+prefix+"_roughness_2048_v001.png"));
        var colors=metal.GetPixels32();var r=rough.GetPixels32();for(int i=0;i<colors.Length;i++)colors[i]=new Color32(colors[i].r,0,0,(byte)(255-r[i].r));
        metal.SetPixels32(colors);metal.Apply();File.WriteAllBytes(Art+"Textures/"+prefix+"_metallic_smoothness_2048_p21.png",metal.EncodeToPNG());UnityEngine.Object.DestroyImmediate(metal);UnityEngine.Object.DestroyImmediate(rough);
    }
}
