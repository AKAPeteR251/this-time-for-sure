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
public static class P17RevealBurstTuneSetup
{
    private const string ScenePath="Assets/_Game/Scenes/Main.unity", ConfigPath="Assets/_Game/Data/Presentation/P05FeelConfig.asset", Flag="ThisTime.RevealBurstTuneVerify";
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P17_REVEAL_BURST_TUNE_Verification");}}
    private static string Request {get{return Path.Combine(Reports,"RevealBurstTune.request");}}
    private static bool testing; private static double started; private static EditorWindow gameView;
    private static readonly List<string> checks=new List<string>();
    static P17RevealBurstTuneSetup(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=PlayChanged;}
    [MenuItem("Tools/这次一定出/程序/P17-REVEAL-BURST-TUNE/接入素材声音并保存Main")]
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
        var config=AssetDatabase.LoadAssetAtPath<P04FeelConfig>(ConfigPath);
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
        config.revealArrivalShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Art/Effects/F05/Shaders/RevealArrival.shader");if(config.revealArrivalShader==null||ShaderUtil.ShaderHasError(config.revealArrivalShader))throw new InvalidOperationException("双段炸裂Shader导入错误");config.silhouetteBurstEnd=.24f;config.characterBurstStart=.46f;config.characterBurstSpan=.34f;config.silhouetteBurstRadius=.62f;config.characterBurstRadius=.88f;config.revealLightGap=.075f;config.revealBurstStrength=.85f;config.proceduralRevealSky=true;config.nebulaEvolution=.06f;config.proceduralNebulaAlpha=.40f;config.nebulaThreshold=.50f;config.nebulaScale=2.8f;config.nebulaDrift=.0022f;config.nebulaBillow=.025f;config.nebulaComposition=69.7511f;config.nebulaDistortion=.035f;config.nebulaFilaments=.55f;config.nebulaWarp=.035f;config.nebulaGrain=.30f;config.meteorSeconds=3.2f;config.meteorFadeSeconds=2.2f;config.meteorOpacity=.28f;
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
    [MenuItem("Tools/这次一定出/程序/P17-REVEAL-BURST-TUNE/验证Main输入音画并录制片段")]
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
        var recorder=UnityEngine.Object.FindObjectOfType<AudioListener>().gameObject.AddComponent<P17RevealBurstTuneRecorder>();recorder.Begin("main_p17_burst_tune");
        ui.PlayBatchForDebug(fixedBatch);yield return null;ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
        while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}
        yield return WaitStage(ui,SummonFlowStage.Revealing,3);
        while(reveal.Progress<.12f)yield return null;Require(reveal.NameAlpha==0&&reveal.RarityAlpha==0,"剪影阶段无姓名或稀有度");Require(reveal.SmallBurstPulse>.1f&&reveal.BigBurstPulse==0,"黑影阶段仅小炸裂");yield return Capture("1280x720_black_silhouette");
        while(reveal.Progress<.34f)yield return null;
        Require(reveal.SmallBurstPulse==0&&reveal.BigBurstPulse==0&&reveal.NameAlpha==0,"双段之间保持纯黑影停留");yield return Capture("1280x720_silhouette_dwell");
        Require(ui.feelConfig.revealBurstShards.Length==3&&ui.feelConfig.revealBurstStreak!=null,"F06三种碎片与拖尾实际绑定");
        while(reveal.Progress<.56f)yield return null;Require(reveal.BigBurstPulse>.1f,"转正式角色时大炸裂");yield return Capture("1280x720_large_character_burst");
        while(reveal.Progress<.74f)yield return null;
        Require(reveal.NameAlpha>.9f&&reveal.RarityAlpha==0,"角色名字先于稀有度");yield return Capture("1280x720_name_before_rarity");
        yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,4);Require(reveal.RarityAlpha==1,"稀有度随后完整浮现");Require(reveal.BurstAlpha==0,"等待确认无残留炸裂");Require(Page(ui,"Reveal").GetComponentsInChildren<Text>().All(t=>t.text!="冰城守望者"&&!System.Text.RegularExpressions.Regex.IsMatch(t.text,@"^\d+/\d+$")),"仅名称，无称号或张数");yield return Capture("1280x720_ssr_wait");
        var sky=page.GetComponentInChildren<RevealSky>();var border=page.GetComponentInChildren<RevealGradientBorder>();
        Require(sky.UsesProcedural&&ui.feelConfig.revealAtmosphereShader.isSupported,"实际使用受支持的粉尘星云Shader");Require(Mathf.Abs(ui.feelConfig.nebulaDrift-.0022f)<.0001f&&ui.feelConfig.nebulaBillow>.02f,"实际Main慢漂移与局部起伏参数");
        var nebula=Page(ui,"Reveal").GetComponentsInChildren<RawImage>().Single(x=>x.name=="ProceduralNebula");
        Require(nebula.material.GetTexture("_NebulaFarTex")==ui.feelConfig.nebulaFar&&nebula.material.GetTexture("_NebulaNearTex")==ui.feelConfig.nebulaNear,"实际Shader采样P09蓝紫粉尘素材");Require(Mathf.Abs(sky.CloudSeed-69.7511f)<.001f,"采用用户参考的P09构图");Require(Mathf.Abs(nebula.material.GetFloat("_Distortion")-.035f)<.001f,"实际Shader使用局部扭曲参数");
        Require(ui.feelConfig.iceMageBackdrop.name=="ice_snowflake_v002","第一版写实雪花引用");
        float stableSeed=sky.CloudSeed;
        float drift=nebula.material.GetFloat("_DriftTime");Vector2 cloud=sky.CloudOffset;float star=sky.FirstStarAlpha,phase=border.Phase,meteorPeak=0,nebulaTime=sky.NebulaTime,pulseMin=1,pulseMax=0;
        Require(Mathf.Abs(border.speed-.36f)<.001f,"NEW流动3倍速度");
        int capturedFlights=0;float starMin=1,starMax=0;
        for(float t=0;t<24.0f;t+=Time.unscaledDeltaTime)
        {
            starMin=Mathf.Min(starMin,sky.FirstStarAlpha);starMax=Mathf.Max(starMax,sky.FirstStarAlpha);
            meteorPeak=Mathf.Max(meteorPeak,sky.MeteorAlpha);pulseMin=Mathf.Min(pulseMin,border.Pulse);pulseMax=Mathf.Max(pulseMax,border.Pulse);
            var seen=sky.MeteorFlights;
            if(seen.Length>capturedFlights&&sky.Elapsed-seen[seen.Length-1].at>.9f&&sky.MeteorAlpha>.05f)
            {capturedFlights=seen.Length;yield return Capture("1280x720_meteor_"+capturedFlights);}
            yield return null;
        }
        Require(starMax-starMin>.01f,"等待中恒星闪烁");Require(Mathf.Abs(border.Phase-phase)>.001f,"NEW边框动态渐变");Require(meteorPeak>.05f,"实际流星出现并移动");
        Require(sky.NebulaTime-nebulaTime>.30f,"星云3D噪声时间持续演化");Require(Mathf.Abs(sky.CloudSeed-stableSeed)<.001f,"等待期间星云构图种子稳定");Require(nebula.material.GetFloat("_DriftTime")-drift>.035f,"实际横向漂移与纹理演化同时运行");
        var flights=sky.MeteorFlights;Require(flights.Length>=3,"观察到至少3条随机流星轨迹");
        Require(flights.Select(f=>f.start).Distinct().Count()==flights.Length,"多处流星起点");
        Require(flights.All(f=>f.peak<=.31f&&f.seconds>=2.8f&&f.fade>=2.2f),"低亮度慢飞行与2.2秒淡出");
        Require(Mathf.Abs(nebula.material.GetFloat("_CloudSeed")-sky.CloudSeed)<.001f,"Shader实际使用随机云团分布种子");
        File.WriteAllText(Path.Combine(Reports,"sky_observation.json"),JsonUtility.ToJson(new SkyObservation{seed=sky.CloudSeed,flights=flights},true));
Require(pulseMax-pulseMin>.8f,"NEW柔和闪烁实际持续变化");
        Require(!ShaderUtil.ShaderHasError(ui.feelConfig.revealAtmosphereShader)&&!ShaderUtil.ShaderHasError(ui.feelConfig.revealArrivalShader),"星云和双段炸裂Shader实际绘制无错误");
        yield return Capture("1280x720_ssr_dynamic_wait");
        while(border.Pulse>.08f)yield return null;yield return Capture("1280x720_new_dim");
        while(border.Pulse<.92f)yield return null;yield return Capture("1280x720_new_bright");Require(ui.RevealIndex==0,"首张等待新确认");Blank(ui);yield return WaitStage(ui,SummonFlowStage.Revealing,2);
        yield return null;yield return null;Blank(ui);
        yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,2);Require(ui.RevealIndex==1&&reveal.NameAlpha==1&&reveal.RarityAlpha==1,"动画点击补齐当前张与全部信息");
        Require(reveal.BurstAlpha==0&&page.GetComponentsInChildren<Image>(true).Single(x=>x.name=="ArrivalFrontFlash").color.a==0,"动画点击立即清两段爆发和前景闪光");
        yield return new WaitForSecondsRealtime(.3f);Require(ui.RevealIndex==1,"不穿透下一张");
        Require(Page(ui,"Reveal").GetComponentsInChildren<Text>().All(t=>t.text!="NEW"),"非NEW角色不显示标签");Require(Mathf.Abs(sky.CloudSeed-stableSeed)<.001f&&sky.NebulaTime>nebulaTime,"翻张星云种子与累计演化连续");Blank(ui);yield return WaitStage(ui,SummonFlowStage.Revealing,2);yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,7);
        yield return new WaitForSecondsRealtime(.5f);Require(ui.RevealIndex==2,"末张仍等待");yield return Capture("1280x720_missing_portrait_wait");Blank(ui);yield return WaitStage(ui,SummonFlowStage.Summary,2);
        yield return new WaitForSecondsRealtime(.25f);recorder.End();
        // Developer-only future phase probes, outside the real-time recording.
        ui.PlayBatchForDebug(new List<PrototypePull>{fixedBatch[0]});ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
        while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,7);
        foreach(float age in new[]{90f,180f})
        {
            typeof(RevealSky).GetField("skyAge",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(sky,age);
            yield return null;yield return Capture("1280x720_nebula_"+age+"s_phase_probe");
        }
        Require(Mathf.Abs(sky.CloudSeed-stableSeed)<.001f,"重新进入揭晓仍保留同片星云种子");ui.SkipPresentation();yield return null;
        foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1600,1000)})
        {
            Resize(size.x,size.y);yield return null;yield return null;ui.PlayBatchForDebug(new List<PrototypePull>{fixedBatch[0]});ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
            while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,7);
            yield return Capture(size.x+"x"+size.y+"_ssr_wait");CheckBounds(ui);ScanClean(ui);ui.HandleEscape();yield return null;yield return null;
            Require(ui.FlowStage==SummonFlowStage.Summary&&ui.CrystalView.WhiteAlpha==0,"等待Esc回同批清白幕");yield return Capture(size.x+"x"+size.y+"_single_summary");
        }
        Resize(1280,720);yield return null;yield return null;
        foreach(CardRarity tier in Enum.GetValues(typeof(CardRarity)))
        {
            ui.PlayBatchForDebug(new List<PrototypePull>{new PrototypePull{Card=state.Cards.First(c=>c.Rarity==tier),IsNew=true}});yield return null;ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
            while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}
            yield return WaitStage(ui,SummonFlowStage.AwaitingConfirm,7);
            var icon=Page(ui,"Reveal").GetComponentsInChildren<Image>().Single(x=>x.name=="SsrCrest");
            Sprite expected=tier==CardRarity.R?ui.feelConfig.rarityR:tier==CardRarity.SR?ui.feelConfig.raritySR:tier==CardRarity.SSR?ui.feelConfig.raritySSR:ui.feelConfig.rarityUR;
            Require(icon.sprite==expected,"本张真实稀有度字图:"+tier);yield return Capture("1280x720_rarity_"+tier);ui.HandleEscape();yield return null;
        }
        foreach(float cutPhase in new[]{.12f,.56f})
        {
            ui.PlayBatchForDebug(new List<PrototypePull>{fixedBatch[0]});yield return null;ui.AdvancePresentation();yield return WaitStage(ui,SummonFlowStage.CrystalCharging,2);
            while(ui.CrystalView.Energy<1){CrystalPress(ui);yield return new WaitForSecondsRealtime(.07f);}yield return WaitStage(ui,SummonFlowStage.Revealing,3);
            while(reveal.Progress<cutPhase)yield return null;Require(reveal.BurstAlpha>.1f,"跳过前爆发正在播放:"+cutPhase);
            if(cutPhase<.3f)ui.HandleEscape();else ui.SkipPresentation();yield return null;
            Require(ui.FlowStage==SummonFlowStage.Summary&&reveal.BurstAlpha==0&&page.GetComponentsInChildren<Image>(true).Single(x=>x.name=="ArrivalFrontFlash").color.a==0,"小/大爆发中途跳过清理并同批总览:"+cutPhase);
        }
        ui.RequestSummon(1);yield return null;yield return Handwrite(ui,state);
        Require(state.Currency==29&&ui.CommittedSummonRequests==1,"手写统一Draw保持");ui.SkipPresentation();yield return null;yield return null;
        ui.RequestSummon(10);ui.SkipSigning();yield return WaitStage(ui,SummonFlowStage.Ceremony,2);Require(state.Currency==19&&ui.CommittedSummonRequests==2,"补签统一十连Draw保持");
        var batch=ui.PresentationBatch.ToArray();ui.SkipPresentation();yield return null;yield return null;Require(ui.PresentationBatch.SequenceEqual(batch),"演出跳过同批");yield return Capture("1280x720_ten_summary_without_rail");
        Click(Page(ui,"Summary").GetComponentsInChildren<Button>().Single(x=>x.name.StartsWith("查看图鉴")));Require(Get<string>(ui,"currentScreen")=="Collection","总览保留图鉴入口");ui.HandleEscape();Require(Get<string>(ui,"currentScreen")=="Summon","图鉴返回召唤保持");
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckReport{checks=checks.ToArray(),audioCues=ui.GetComponent<SummonAudio>().Evidence.ToArray(),currency=state.Currency,totalPulls=state.TotalPulls,commits=ui.CommittedSummonRequests},true));
        Complete("PASS: Main仅名称、无按钮/张数、空白新点击确认与末张等待、P16黑影小炸裂/转角色大炸裂与随机淡流星、已认可NEW/写实雪花、四档字图、三窗口、真实Draw和同批跳过；实际带声片段。");
    }
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
        testing=false;foreach(var recorder in UnityEngine.Object.FindObjectsOfType<P17RevealBurstTuneRecorder>())recorder.End();
        Write(result.StartsWith("PASS")?"passed":"failed",result);File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);
        EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();
    }
    private static void Write(string stage,string result)
    {Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,scene=SceneManager.GetActiveScene().path,unityVersion=Application.unityVersion,atUtc=DateTime.UtcNow.ToString("o")},true));Debug.Log("[P17-REVEAL-BURST-TUNE] "+result);}
    [Serializable]private sealed class SkyObservation{public float seed;public RevealSky.MeteorFlight[] flights;}
    [Serializable]private sealed class Report{public string stage,result,scene,unityVersion,atUtc;}
    [Serializable]private sealed class CheckReport{public string[] checks;public SummonAudio.CueEvidence[] audioCues;public int currency,totalPulls,commits;}
}

// Editor-only evidence capture: records this Unity listener's mix, never system microphone/other apps.
public sealed class P17RevealBurstTuneRecorder : MonoBehaviour
{
    private bool active; private string folder;private readonly object gate=new object();
    private readonly List<float> samples=new List<float>();private readonly List<double> frames=new List<double>();
    private System.Diagnostics.Stopwatch watch;private double firstAudio=-1;private int rate,channels;private Coroutine routine;
    public void Begin(string name)
    {
        End();folder=Path.Combine(P17RevealBurstTuneSetup.Reports,name+"_capture");Directory.CreateDirectory(folder);
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
