using UnityEngine;

namespace ThisTime.Game
{
    [CreateAssetMenu(menuName = "星界回响/召唤演出配置")]
    public sealed class P04FeelConfig : ScriptableObject
    {
        [Header("P05 水晶连击（v004 三图为视觉层，不是三次点击）")]
        public bool crystalInteraction;
        [Header("R2 揭晓视觉参考（可替换Sprite）")]
        public Sprite revealSsrBadge, revealNewPlaque, iceMageBackdrop;
        [Range(0, 1)] public float iceBackdropAlpha = .22f;
        [Header("P06 分层星空与四档揭晓")]
        public Texture2D revealStarfield, nebulaFar, nebulaNear;
        public Sprite starSoft, starCross, meteorTrail, revealShard;
        public Sprite rarityR, raritySR, raritySSR, rarityUR, rarityAccent, newText, newBorder;
        public float nebulaFarSpeed=.006f, nebulaNearSpeed=-.010f;
        [Range(0,1)] public float nebulaFarAlpha=.45f, nebulaNearAlpha=.25f;
        [Range(8,64)] public int revealStarCount=28;
        public float meteorInterval=7f, meteorSeconds=1.5f, newGradientSpeed=.12f;
        [Header("P07 程序星云/流星与NEW闪烁")]
        public Shader revealAtmosphereShader;
        public bool proceduralRevealSky;
        public float nebulaEvolution=.09f;
        [Range(0,1)] public float proceduralNebulaAlpha=.44f;
        [Range(0,1)] public float newBlinkStrength=.35f;
        public float newBlinkFrequency=1.6f;
        [Header("P08 粉尘星云（Shader扰动现有素材）")]
        [Range(0,.1f)] public float nebulaWarp=.035f;
        [Range(0,1)] public float nebulaGrain=.3f;
        [Header("P09 流星淡显与随机分布")]
        [Range(0,1)] public float meteorOpacity=.28f;
        public float meteorFadeSeconds=2.2f;
        [Header("P10 蓝紫粉尘：漂移与形态演化独立")]
        [Range(.3f,.7f)] public float nebulaThreshold=.46f;
        [Range(1f,5f)] public float nebulaScale=2.8f;
        public float nebulaDrift=.004f;
        [Range(0,1)] public float nebulaFilaments=.65f;
        [Range(0,.06f)] public float nebulaBillow=.025f;
        public float nebulaComposition=69.7511f;
        [Range(0,.08f)] public float nebulaDistortion=.035f;
        [Header("P16 角色双段炸裂")]
        public Shader revealArrivalShader;
        public Sprite[] revealBurstShards;
        public Sprite revealBurstStreak;
        public float silhouetteBurstEnd=.24f,characterBurstStart=.46f,characterBurstSpan=.34f;
        public float silhouetteBurstRadius=.62f,characterBurstRadius=.88f;
        [Range(.02f,.2f)] public float revealLightGap=.075f;
        [Range(0,1)] public float revealBurstStrength=.85f;
        [Header("P18 短促碎片与柔和散光")]
        [Range(0,.15f)] public float silhouetteBurstStart=.08f;
        [Range(.12f,.6f)] public float revealShardTravelFraction=.26f;
        [Range(.05f,.25f)] public float revealLightSoftness=.14f;
        [Range(0,1)] public float revealLightScatter=.8f;
        [Header("P19 F07实时3D舞台")]
        public GameObject crystalStageModel;
        public float crystalTransitionSeconds=1.25f,crystalCameraDistance=18f;
        [Header("P24 法阵穿越镜头")]
        public bool crystalPortalCamera=true;
        public Shader crystalPortalShader;
        public float crystalPortalScale=3.1f,crystalPortalRoll=2f;
        public Vector3 crystalPortalEntryOffset=new Vector3(-.7f,.6f,-.5f);
        public float crystalOuterSpeed=8f,crystalInnerSpeed=-12f,crystalFloorSpeed=7f;
        public float crystalOrbitSpeed=7f,crystalBodySpeed=4f,crystalFloatAmplitude=.16f;
        public float crystalBodyScale=2f,crystalStarDrift=.0003f;
        [Header("P20 三个立环独立欧拉速度（度/秒）")]
        public Vector3 crystalOuterEulerSpeed=new Vector3(9,3,0);
        public Vector3 crystalMiddleEulerSpeed=new Vector3(0,-11,4);
        public Vector3 crystalInnerEulerSpeed=new Vector3(-4,0,13);
        [Header("P21 F08地下圣殿（仅水晶阶段）")]
        public GameObject altarEnvironmentModel;
        public Texture2D altarFlame,altarMist,altarDust;
        public Shader altarBillboardShader;
        public Vector3 altarCameraWide=new Vector3(0,7.2f,-13.7f),altarCameraClose=new Vector3(0,6.4f,-11f);
        public Vector3 altarFocusWide=new Vector3(0,5.6f,0),altarFocusClose=new Vector3(0,4.9f,0);
        public float altarWideFov=59f,altarCloseFov=46f,altarStageLift=.585f;
        public float altarWarmLight=5f,altarCoolLight=8f,altarFlameFps=12f,altarMistAlpha=.08f;
        public float altarBloom=.4f,altarExposure=.15f;
        [Header("P22 水晶裂隙片光")]
        public Shader crystalSheetLightShader;
        public float crystalSheetStart=.24f,crystalSheetLength=3.8f,crystalSheetWidth=.9f,crystalSheetIntensity=2.7f;
        public Sprite crystalBackground, crystalIntact, crystalCracks, crystalDeep, crystalLeak;
        public Sprite[] crystalRupture;
        public AudioClip voiceFirst, voiceSecond, voiceFinal;
        public float preludeSeconds = 4.35f;
        public float hitEnergy = .065f, comboWindow = .28f, comboMultiplier = 1.5f;
        public float hitShakeSeconds = .25f, crystalShakePixels = 12f, ruptureFps = 28f;
        public float finalChargeSeconds = 1.5f, whiteSeconds = .22f;
        public float faceZoom = 1.9f;
        public Vector2 faceUv = new Vector2(.5f, .80f), faceTarget = new Vector2(.64f, .68f);
        public float nameCharacterSeconds = .12f;
        [Header("A05 独立图层")]
        public Sprite contractPaper, contractBorder, starSeal, giantRing, outerRing, innerRing, shockwave, lightPillar;
        [Header("F09 写实羊皮纸与蜡印")]
        public Sprite contractEnglishLettering, contractWaxShadow;
        public float contractAspect = 4f/3f;
        public Vector4 contractSignatureRect = new Vector4(.1000977f,.1699219f,.7202148f,.3997396f);
        public Vector4 contractWaxRect = new Vector4(.7421875f,.0299479f,.9179688f,.2643229f);
        public float contractWaxShadowAlpha = .7f;
        [Header("P26 契约燃烧")]
        public Shader contractBurnShader;
        public float contractBurnStart=1.05f,contractBurnSeconds=1.5f,contractBurnEdge=.06f;
        [Header("P27 契约漂入与环境火星")]
        public float contractArriveSeconds=.95f,contractFloatPixels=7f,contractFloatSpeed=.6f;
        public int contractEmberCount=30;
        public float contractEmberAlpha=.5f;
        [Header("仪式关键时刻（秒，参考 6.8 秒）")]
        public float referenceSeconds = 6.8f;
        public float sealImpact = 0.72f;
        public float ringsAppear = 1.35f;
        public float contraction = 4.25f;
        public float gateBurst = 5.85f;
        public float voiceSecondLine = 1.75f;
        public float voiceLastLine = 5f;
        [Header("强度")]
        [Range(0, 1)] public float effectStrength = 0.85f;
        [Range(0, 25)] public float shakePixels = 12f;
        public float giantRingScale = 1.65f;
        public float ringRotation = 110f;
        [Header("实际旁白与原创合成样音")]
        public AudioClip narration, ceremonyBed, sealSound, gateSound;
        public AudioClip rReveal, srReveal, ssrCharge, ssrReveal, urCharge, urReveal, confirm, skip;
        [Range(0, 1)] public float masterVolume = 0.8f;
        [Range(0, 1)] public float voiceVolume = 0.95f;
        [Range(0, 1)] public float bedVolume = 0.32f;
        [Range(0, 1)] public float effectVolume = 0.65f;
        [Range(0.02f, 0.2f)] public float stopFadeSeconds = 0.075f;
    }
}
