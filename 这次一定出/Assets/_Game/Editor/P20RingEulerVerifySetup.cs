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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class P20RingEulerVerifySetup
{
    private const string ScenePath="Assets/_Game/Scenes/Main.unity", ConfigPath="Assets/_Game/Data/Presentation/P05FeelConfig.asset", Flag="ThisTime.RingEulerVerify";
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P20_RING_EULER_Verification");}}
    private static string Request {get{return Path.Combine(Reports,"RingEuler.request");}}
    private static bool testing; private static double started; private static EditorWindow gameView;
    private static readonly List<string> checks=new List<string>();
    static P20RingEulerVerifySetup(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=PlayChanged;}
    [MenuItem("Tools/这次一定出/程序/P20-RING-EULER-VERIFY/接入素材声音并保存Main")]
    public static void Prepare()
    {
        if(!Ready())return;
        AssetDatabase.Refresh();
        const string art="Assets/_Game/Art/Effects/F05/RevealR3/";
        foreach(string path in Directory.GetFiles(art,"*.png"))
        {
            var t=AssetImporter.GetAtPath(path.Replace('\\','/')) as TextureImporter;
            t.textureType=path.Contains("nebula")||path.Contains("starfield")?TextureImporterType.Default:TextureImporterType.Sprite;
            t.spriteImportMode=SpriteImportMode.Single;t.spritePivot=new Vector2(.5f,.5f);t.alphaIsTransparency=!path.Contains("starfield");
            t.wrapMode=path.Contains("nebula")?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
            t.mipmapEnabled=false;t.maxTextureSize=path.Contains("starfield")?4096:2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.filterMode=FilterMode.Bilinear;t.SaveAndReimport();
        }
        const string burstArt="Assets/_Game/Art/Effects/F06/RevealBurst/";
        foreach(string path in Directory.GetFiles(burstArt,"*.png"))ImportSprite(path.Replace('\\','/'));
        ImportStage();
        var config=AssetDatabase.LoadAssetAtPath<P04FeelConfig>(ConfigPath);
        config.crystalStageModel=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Effects/F07/CrystalStage/F07_CrystalStage_v001.fbx");
        config.crystalTransitionSeconds=1.25f;config.crystalCameraDistance=18f;config.crystalBodyScale=2f;config.crystalStarDrift=.0003f;
        config.crystalOuterSpeed=8;config.crystalInnerSpeed=-12;config.crystalFloorSpeed=7;config.crystalOrbitSpeed=7;config.crystalBodySpeed=4;config.crystalFloatAmplitude=.16f;
        config.revealBurstShards=new[]{Sprite(burstArt+"reveal_burst_shard_A_v001.png"),Sprite(burstArt+"reveal_burst_shard_B_v001.png"),Sprite(burstArt+"reveal_burst_shard_C_v001.png")};
        config.revealBurstStreak=Sprite(burstArt+"reveal_burst_streak_v001.png");
        config.revealStarfield=AssetDatabase.LoadAssetAtPath<Texture2D>(art+"reveal_starfield_base_v003.png");
        config.nebulaFar=AssetDatabase.LoadAssetAtPath<Texture2D>(art+"reveal_nebula_far_v003.png");config.nebulaNear=AssetDatabase.LoadAssetAtPath<Texture2D>(art+"reveal_nebula_near_v003.png");
        config.starSoft=Sprite(art+"star_soft_v003.png");config.starCross=Sprite(art+"star_cross_v003.png");config.meteorTrail=Sprite(art+"meteor_trail_v003.png");config.revealShard=Sprite(art+"reveal_ice_shard_v003.png");
        config.rarityR=Sprite(art+"rarity_R_v003.png");config.raritySR=Sprite(art+"rarity_SR_v003.png");config.raritySSR=Sprite(art+"rarity_SSR_v003.png");config.rarityUR=Sprite(art+"rarity_UR_v003.png");
        config.rarityAccent=Sprite(art+"rarity_accent_v003.png");config.newText=Sprite(art+"new_text_v003.png");config.newBorder=Sprite(art+"new_border_mask_v003.png");config.iceMageBackdrop=Sprite(art+"SSR001_snowflake_backdrop_v003.png");
        config.iceMageBackdrop=Sprite("Assets/_Game/Art/Effects/F04/RevealIdentityR2/ice_snowflake_v002.png");
        config.revealAtmosphereShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader");
        if(config.revealAtmosphereShader==null||ShaderUtil.ShaderHasError(config.revealAtmosphereShader))throw new InvalidOperationException("P07 Shader导入失败");
        config.revealArrivalShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Art/Effects/F05/Shaders/RevealArrival.shader");if(config.revealArrivalShader==null||ShaderUtil.ShaderHasError(config.revealArrivalShader))throw new InvalidOperationException("双段炸裂Shader导入错误");config.silhouetteBurstEnd=.24f;config.characterBurstStart=.46f;config.characterBurstSpan=.34f;config.silhouetteBurstRadius=.62f;config.characterBurstRadius=.88f;config.revealLightGap=.075f;config.silhouetteBurstStart=.08f;config.revealShardTravelFraction=.26f;config.revealLightSoftness=.14f;config.revealLightScatter=.8f;config.revealBurstStrength=.85f;config.proceduralRevealSky=true;config.nebulaEvolution=.06f;config.proceduralNebulaAlpha=.40f;config.nebulaThreshold=.50f;config.nebulaScale=2.8f;config.nebulaDrift=.0022f;config.nebulaBillow=.025f;config.nebulaComposition=69.7511f;config.nebulaDistortion=.035f;config.nebulaFilaments=.55f;config.nebulaWarp=.035f;config.nebulaGrain=.30f;config.meteorSeconds=3.2f;config.meteorFadeSeconds=2.2f;config.meteorOpacity=.28f;
        config.newGradientSpeed=.36f;config.newBlinkStrength=.35f;config.newBlinkFrequency=1.6f;
        config.iceBackdropAlpha=.22f;config.faceTarget=new Vector2(.55f,.68f);EditorUtility.SetDirty(config);
        var scene=EditorSceneManager.OpenScene(ScenePath);var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();
        if(ui==null||ui.portraits.Count!=6||ui.theme==null)throw new InvalidOperationException("Main既有主题/六卡入口缺失，停止接入。");
        ui.feelConfig=config;ui.signatureFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/_Game/UI/Resources/Fonts/LXGWWenKai-Regular.ttf");
        ui.useFixedMock=false;ui.autoSignatureSeconds=.4f;ui.ceremonySeconds=6.8f;
        ui.normalRevealSeconds=2.8f;ui.srRevealSeconds=3.3f;ui.ssrRevealSeconds=4f;ui.urRevealSeconds=4.6f;

        EditorUtility.SetDirty(ui);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Write("ready","Main接入角色小/大双段炸裂，保留已验收P15星云。");
    }
    private static void ImportStage()
    {
        const string path="Assets/_Game/Art/Effects/F07/CrystalStage/F07_CrystalStage_v001.fbx";
        var importer=AssetImporter.GetAtPath(path) as ModelImporter;
        importer.importAnimation=false;importer.isReadable=false;importer.importCameras=false;importer.importLights=false;
        importer.globalScale=1;importer.useFileScale=true;importer.SaveAndReimport();
        string folder="Assets/_Game/Art/Effects/F07/CrystalStage/Materials";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
        string json=Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"ArtSource/Effects/F07/CrystalLayers/Textures/material_parameters_v001.json");
        var values=JsonUtility.FromJson<MaterialList>(File.ReadAllText(json));
        foreach(var entry in values.materials)
        {
            string asset=folder+"/"+entry.material+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(asset);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,asset);}
            mat.SetColor("_BaseColor",new Color(entry.base_color_linear_rgba[0],entry.base_color_linear_rgba[1],entry.base_color_linear_rgba[2],1).gamma);
            mat.SetFloat("_Metallic",entry.material.Contains("Crystal")?.12f:entry.metallic);mat.SetFloat("_Smoothness",entry.urp_smoothness_start);mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor",new Color(entry.emission_linear_rgb[0],entry.emission_linear_rgb[1],entry.emission_linear_rgb[2])*Mathf.Min(1.2f,entry.emission_strength_reference*(entry.material.Contains("Crystal")?1.5f:.4f)));
            EditorUtility.SetDirty(mat);importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),entry.material),mat);
        }
        importer.SaveAndReimport();
        // MaterialDescription import may refresh remapped material values: tune after the last FBX import.
        foreach(var entry in values.materials)
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(folder+"/"+entry.material+".mat");
            mat.SetFloat("_Metallic",entry.material.Contains("Crystal")?.12f:entry.metallic);
            mat.SetFloat("_Smoothness",entry.urp_smoothness_start);mat.EnableKeyword("_EMISSION");
            mat.SetColor("_BaseColor",new Color(entry.base_color_linear_rgba[0],entry.base_color_linear_rgba[1],entry.base_color_linear_rgba[2],1).gamma);
            mat.SetColor("_EmissionColor",new Color(entry.emission_linear_rgb[0],entry.emission_linear_rgb[1],entry.emission_linear_rgb[2])*Mathf.Min(1.2f,entry.emission_strength_reference*(entry.material.Contains("Crystal")?1.5f:.4f)));
            EditorUtility.SetDirty(mat);
        }
        AssetDatabase.SaveAssets();
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(prefab.GetComponentsInChildren<MeshRenderer>(true).Length!=24)throw new InvalidOperationException("F07预期24独立网格");
        var nodes=prefab.GetComponentsInChildren<Transform>(true);
        foreach(string name in new[]{"RingOuter","RingInner","RingFloor","CrystalBody"})if(!nodes.Any(x=>x.name==name))throw new InvalidOperationException("F07缺节点:"+name);
        File.WriteAllLines(Path.Combine(Reports,"unity_model_nodes.txt"),nodes.Select(x=>x.name+" parent="+(x.parent==null?"":x.parent.name)+" local="+x.localPosition+" rotation="+x.localEulerAngles+" scale="+x.localScale));
    }
    [Serializable]private sealed class MaterialList{public MaterialEntry[] materials;}
    [Serializable]private sealed class MaterialEntry{public string material;public float[] base_color_linear_rgba,emission_linear_rgb;public float metallic,urp_smoothness_start,emission_strength_reference;}
    public static void Boot(){Prepare();EditorApplication.delayCall+=Verify;}
    private static void ImportSprite(string path)
    {
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new FileNotFoundException(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;
        importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }
    private static Sprite Sprite(string path){var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null)throw new FileNotFoundException(path);return sprite;}
    private static AudioClip Clip(string name){var clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/P04/"+name+".wav");if(clip==null)throw new FileNotFoundException(name);return clip;}
    private static bool Ready()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||Enumerable.Range(0,SceneManager.sceneCount).Any(i=>SceneManager.GetSceneAt(i).isDirty))
        {Write("blocked","请先停止Play并保存当前编辑，避免覆盖未完成工作。");return false;}return true;
    }
    [MenuItem("Tools/这次一定出/程序/P20-RING-EULER-VERIFY/验证Main输入音画并录制片段")]
    public static void Verify()
    {
        if(!Ready())return;EditorSceneManager.OpenScene(ScenePath);SessionState.SetBool(Flag,true);Write("starting","请求Main实际Play、内部混音记录及实时画面片段。");EditorApplication.EnterPlaymode();
    }
    private static void Poll()
    {
        if(testing&&EditorApplication.isPlaying){if(gameView!=null)gameView.Repaint();EditorApplication.QueuePlayerLoopUpdate();}
        if(testing&&EditorApplication.timeSinceStartup-started>240)Complete("FAIL: P04 Main Play超时。");
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(Request))return;
        string command=File.ReadAllText(Request).Trim();File.Delete(Request);
        try{if(command=="prepare")Prepare();else if(command=="verify")Verify();else if(command=="status")Write("observed","play="+EditorApplication.isPlaying+"; dirty="+SceneManager.GetActiveScene().isDirty);}
        catch(Exception error){Write("failed",error.ToString());}
    }
    private static void PlayChanged(PlayModeStateChange change)
    {
        if(change!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
        SessionState.SetBool(Flag,false);testing=true;started=EditorApplication.timeSinceStartup;checks.Clear();
        Application.runInBackground=true;EditorApplication.isPaused=false;
        Resize(1280,720);gameView=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        Write("running","Main实际Play：SSR慢点/高频/停手录制、真实签约单抽十连、四阶段清理与三尺寸。");
        var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();ui.StartCoroutine(Guard(Check(ui)));
    }
    private static IEnumerator Check(PrototypeGame ui)
    {
        yield return null;yield return null;
        var state=Get<IPrototypeUIAdapter>(ui,"game");var reveal=Get<FastRevealView>(ui,"fastReveal");
        Require(ui.GetComponentsInChildren<RectTransform>(true).All(t=>t.name!="NavigationRail"&&t.name!="TabRule"),"侧栏与顶部切换删除");
        Require(ui.feelConfig.rarityR!=null&&ui.feelConfig.raritySR!=null&&ui.feelConfig.raritySSR!=null&&ui.feelConfig.rarityUR!=null&&ui.feelConfig.newBorder!=null&&ui.feelConfig.revealStarfield!=null,"Main R3四档与分层背景引用");
        var page=Page(ui,"Reveal");Require(page.GetComponentsInChildren<Button>(true).All(b=>b.name=="AdvanceSurface"||b.name=="跳过"),"没有下一张/查看结果按钮");
        Require(page.GetComponentsInChildren<Text>(true).All(t=>t.name!="CharacterTitle"),"揭晓没有称号节点");
        yield return Capture("1280x720_summon_without_rail");
        var fixedBatch=new List<PrototypePull>{Pull(state,"SSR001",true),Pull(state,"SSR001",false),Pull(state,"R002",true)};
        var recorder=UnityEngine.Object.FindObjectOfType<AudioListener>().gameObject.AddComponent<P20RingEulerVerifyRecorder>();recorder.Begin("main_p20_ring_euler");
        ui.PlayBatchForDebug(fixedBatch);yield return null;
        while(ui.FlowStage==SummonFlowStage.Ceremony&&Get<ContractCeremonyView>(ui,"ceremonyView").Progress<.84f)yield return null;
        Require(ui.CrystalView.Uses3D&&ui.CrystalView.TransitionAlpha>0&&ui.CrystalView.TransitionAlpha<1,"前奏与3D舞台真实交叉淡化");yield return Capture("1280x720_ritual_transition");
        yield return WaitStage(ui,SummonFlowStage.CrystalCharging,3);
        var stage=ui.CrystalView.Stage3D;Require(stage.Rendering&&stage.MeshCount==24,"F07真实24网格通过独立镜头绘制");
        yield return Capture("1280x720_crystal_idle");
        var rotation=stage.OuterRotation;var position=stage.FirstFloatingPosition;var body=stage.BodyPosition;var starPosition=stage.FirstStarPosition;
        yield return new WaitForSecondsRealtime(2.0f);Require(Quaternion.Angle(rotation,stage.OuterRotation)>2&&Vector3.Distance(position,stage.FirstFloatingPosition)>.05f&&Vector3.Distance(body,stage.BodyPosition)>.01f,"停手时巨环/碎片/水晶持续运动");
        Require(Vector2.Distance(starPosition,stage.FirstStarPosition)>.0002f,"水晶舞台星点真实缓慢漂移");CrystalPress(ui);yield return new WaitForSecondsRealtime(.7f);CrystalPress(ui);float kept=ui.CrystalView.Energy;
        yield return new WaitForSecondsRealtime(1.0f);Require(ui.CrystalView.Energy==kept&&ui.CrystalView.ShakeAmplitude==0,"停手保留能量收震且舞台不冻结");yield return Capture("1280x720_crystal_charge_hold");
        var rig=stage.StageCamera.transform.parent;
        var outer=rig.Find("RingOuter");var middle=rig.Find("RingMiddle");var inner=rig.Find("RingInner");
        var samples=new System.Collections.Generic.List<RingSample>();
        Quaternion oa=outer.localRotation,ma=middle.localRotation,ia=inner.localRotation;
        for(int i=0;i<=12;i++)
        {
            samples.Add(new RingSample{age=stage.Age,outer=outer.localEulerAngles,middle=middle.localEulerAngles,inner=inner.localEulerAngles});
            if(i%3==0)yield return Capture("1280x720_three_axes_"+i+"s");
            yield return new WaitForSecondsRealtime(1);
        }
        Require(Quaternion.Angle(oa,outer.localRotation)>15&&Quaternion.Angle(ma,middle.localRotation)>15&&Quaternion.Angle(ia,inner.localRotation)>15,"三个立环独立欧拉旋转实际变化");
        Require(ui.feelConfig.crystalOuterEulerSpeed!=ui.feelConfig.crystalMiddleEulerSpeed&&ui.feelConfig.crystalOuterEulerSpeed!=ui.feelConfig.crystalInnerEulerSpeed,"三个方向参数不同");
        File.WriteAllText(Path.Combine(Reports,"ring_samples.json"),JsonUtility.ToJson(new RingSamples{samples=samples.ToArray()},true));
        ui.SkipPresentation();yield return null;Require(!stage.Rendering&&ui.FlowStage==SummonFlowStage.Summary,"跳过清理三环舞台");recorder.End();
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckReport{checks=checks.ToArray(),audioCues=ui.GetComponent<SummonAudio>().Evidence.ToArray(),currency=state.Currency,totalPulls=state.TotalPulls,commits=ui.CommittedSummonRequests},true));
        Complete("PASS: Main自然转场与三个立环不同欧拉方向旋转、底环保留、舞台清理；实际带声录制。");
    }
    [Serializable]private sealed class RingSamples{public RingSample[] samples;}
    [Serializable]private sealed class RingSample{public float age;public Vector3 outer,middle,inner;}
    private static void Blank(PrototypeGame ui)
    {
        var obj=Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(b=>b.name=="AdvanceSurface").gameObject;
        var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=new Vector2(Screen.width*.12f,Screen.height*.80f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits[0].gameObject==obj,"空白区域真实射线命中");
        ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerClickHandler);
    }
    private static void CrystalPress(PrototypeGame ui)
    {
        var obj=Page(ui,"Reveal").GetComponentsInChildren<Button>().Single(x=>x.name=="AdvanceSurface").gameObject;
        var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=new Vector2(Screen.width*.5f,Screen.height*.5f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits[0].gameObject==obj,"水晶输入首命中");
        ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(obj,e,ExecuteEvents.pointerClickHandler);
    }
    private static PrototypePull Pull(IPrototypeUIAdapter state,string id,bool fresh){return new PrototypePull{Card=state.Cards.Single(c=>c.Id==id),IsNew=fresh};}
    private static IEnumerator Handwrite(PrototypeGame ui,IPrototypeUIAdapter state)
    {
        Canvas.ForceUpdateCanvases();var pad=ui.SignaturePad;var pointer=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left};
        for(int i=0;i<25;i++)
        {
            float u=i/24f;Vector2 local=pad.rectTransform.rect.min+Vector2.Scale(new Vector2(.15f+u*.65f,.5f+Mathf.Sin(u*15)*.2f),pad.rectTransform.rect.size);
            pointer.position=pad.rectTransform.TransformPoint(local);
            if(i==0){var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Require(hits.Count>0&&hits[0].gameObject==pad.gameObject,"手写区真实首命中");ExecuteEvents.Execute(pad.gameObject,pointer,ExecuteEvents.pointerDownHandler);}
            else ExecuteEvents.Execute(pad.gameObject,pointer,ExecuteEvents.dragHandler);
            yield return null;
        }
        Require(state.Currency==30&&pad.Points.Count>20,"可见轨迹松笔前不扣费");ExecuteEvents.Execute(pad.gameObject,pointer,ExecuteEvents.pointerUpHandler);
    }
    private static void Click(Button button)
    {
        Canvas.ForceUpdateCanvases();var rect=button.GetComponent<RectTransform>();var pointer=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=rect.TransformPoint(rect.rect.center)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
        Require(button.interactable&&hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"按钮实际首命中:"+button.name+"; hit="+(hits.Count==0?"无":hits[0].gameObject.name));
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
    }
    private static void CheckBounds(PrototypeGame ui)
    {
        foreach(var button in ui.GetComponentsInChildren<Button>()){if(!button.isActiveAndEnabled)continue;var corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);Require(corners[0].x>=-1&&corners[0].y>=-1&&corners[2].x<=Screen.width+1&&corners[2].y<=Screen.height+1,"安全区:"+Screen.width+"x"+Screen.height+":"+button.name);}
    }
    private static void ScanClean(PrototypeGame ui)
    {foreach(var text in ui.GetComponentsInChildren<Text>()){foreach(string bad in new[]{"尚未接入","本次运行","UI MOCK","占位","校准","SSR001","UR001","这次一定出","预览"})Require(!text.text.Contains(bad),"纯净文案:"+text.name+":"+bad);}}
    private static GameObject Page(PrototypeGame ui,string name){return Get<Dictionary<string,GameObject>>(ui,"screens")[name];}
    private static T Get<T>(PrototypeGame ui,string name){return (T)typeof(PrototypeGame).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);}
    private static void Resize(int width,int height){typeof(U01Setup).GetMethod("Resize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{width,height});}
    private static IEnumerator WaitStage(PrototypeGame ui,SummonFlowStage target,float seconds)
    {float end=Time.realtimeSinceStartup+seconds;while(ui.FlowStage!=target&&Time.realtimeSinceStartup<end)yield return null;if(ui.FlowStage!=target)throw new InvalidOperationException("阶段等待:"+target+" 实际:"+ui.FlowStage);}
    private static IEnumerator Capture(string name)
    {yield return null;yield return null;Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Reports,name+".png"));yield return null;yield return null;}
    private static IEnumerator Guard(IEnumerator routine)
    {while(true){bool moved=false;Exception failure=null;try{moved=routine.MoveNext();}catch(Exception e){failure=e;}if(failure!=null){Complete("FAIL: "+failure);yield break;}if(!moved)yield break;var nested=routine.Current as IEnumerator;if(nested!=null){yield return Guard(nested);if(!testing)yield break;}else yield return routine.Current;}}
    private static void Require(bool good,string label){if(!good)throw new InvalidOperationException(label);checks.Add(label);}
    private static void Complete(string result)
    {
        testing=false;foreach(var recorder in UnityEngine.Object.FindObjectsOfType<P20RingEulerVerifyRecorder>())recorder.End();
        Write(result.StartsWith("PASS")?"passed":"failed",result);File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);
        EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();
    }
    private static void Write(string stage,string result)
    {Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,scene=SceneManager.GetActiveScene().path,unityVersion=Application.unityVersion,atUtc=DateTime.UtcNow.ToString("o")},true));Debug.Log("[P20-RING-EULER-VERIFY] "+result);}
    [Serializable]private sealed class SkyObservation{public float seed;public RevealSky.MeteorFlight[] flights;}
    [Serializable]private sealed class Report{public string stage,result,scene,unityVersion,atUtc;}
    [Serializable]private sealed class CheckReport{public string[] checks;public SummonAudio.CueEvidence[] audioCues;public int currency,totalPulls,commits;}
}

// Editor-only evidence capture: records this Unity listener's mix, never system microphone/other apps.
public sealed class P20RingEulerVerifyRecorder : MonoBehaviour
{
    private bool active; private string folder;private readonly object gate=new object();
    private readonly List<float> samples=new List<float>();private readonly List<double> frames=new List<double>();
    private System.Diagnostics.Stopwatch watch;private double firstAudio=-1;private int rate,channels;private Coroutine routine;
    public void Begin(string name)
    {
        End();folder=Path.Combine(P20RingEulerVerifySetup.Reports,name+"_capture");Directory.CreateDirectory(folder);
        lock(gate){samples.Clear();firstAudio=-1;}frames.Clear();rate=AudioSettings.outputSampleRate;watch=System.Diagnostics.Stopwatch.StartNew();active=true;routine=StartCoroutine(Record());
    }
    private IEnumerator Record()
    {
        double next=0;
        while(active)
        {
            yield return new WaitForEndOfFrame();if(!active)yield break;
            double elapsed=watch.Elapsed.TotalSeconds;if(elapsed<next)continue;next=elapsed+1.0/24;
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,frames.Count.ToString("D5")+".jpg"),image.EncodeToJPG(83));Destroy(image);frames.Add(elapsed);
        }
    }
    private void OnAudioFilterRead(float[] data,int count)
    {lock(gate){if(!active)return;if(firstAudio<0)firstAudio=watch.Elapsed.TotalSeconds;channels=count;samples.AddRange(data);}}
    public void End()
    {
        if(!active)return;active=false;if(routine!=null)StopCoroutine(routine);routine=null;
        float[] pcm;lock(gate){pcm=samples.ToArray();}
        double duration=watch.Elapsed.TotalSeconds;
        using(var writer=new BinaryWriter(File.Create(Path.Combine(folder,"unity_listener_mix.wav"))))
        {
            int count=pcm.Length*2;writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);writer.Write((short)1);writer.Write((short)Math.Max(1,channels));writer.Write(rate);writer.Write(rate*Math.Max(1,channels)*2);writer.Write((short)(Math.Max(1,channels)*2));writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count);foreach(float value in pcm)writer.Write((short)(Mathf.Clamp(value,-1,1)*32767));
        }
        File.WriteAllText(Path.Combine(folder,"capture.json"),JsonUtility.ToJson(new CaptureReport{frameTimes=frames.ToArray(),duration=duration,audioStart=firstAudio,sampleRate=rate,channels=channels,samples=pcm.Length,peak=pcm.Length==0?0:pcm.Max(v=>Mathf.Abs(v))},true));
    }
    private void OnDisable(){End();}
    [Serializable]private sealed class CaptureReport{public double[] frameTimes;public double duration,audioStart;public int sampleRate,channels,samples;public float peak;}
}
