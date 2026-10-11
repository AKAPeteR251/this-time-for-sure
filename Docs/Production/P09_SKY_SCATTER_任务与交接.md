# P09-SKY-SCATTER 全屏星云与流星分布

用户授权：星云全屏不规则随机分布，去线状排列；流星更淡、起点多样、更慢、消失更慢。保留粉尘感、已认可的写实雪花/NEW及所有抽卡输入。本轮只改背景表现与调参。状态：Main已集成，最终编译和实际Play检查通过，约33秒实录已交；视觉待用户验收。

## 实现

星云保留既有粉尘素材采样，改为三个不同旋转角/缩放/偏移的层，配合全屏不规则噪声密度遮罩。每次角色舞台启用选择独立云团种子；同批揭晓中保持稳定并缓慢演化，避免点击换张时突然切云。两层横向带状RawImage仍禁用，不叠加白色筋线。

流星在六个屏幕区域轮换，区域内部随机位置；左右向、飞行距离、大小、亮度、间隔与时长有小幅变化。纯随机曾在一次观察里集中于同一高度，最终采用分区轮换避免连续扎堆。所有视觉随机使用RevealSky自己的System.Random，不共享Core抽卡随机流。

默认亮度0.28，个体峰值0.224—0.308（原先约0.65）；飞行约3.2秒±12%，之后继续轻微漂移并用2.2秒SmoothStep慢淡出，入场0.7秒柔显。通常一颗消退后再随机等待，避免突然同屏刷出很多流星。现有Shader光头/尾迹仍沿用。NEW速度/闪烁、写实雪花、角色/名字/稀有度不改。

## 改动文件

Unity工程相对路径：

- `Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader`：新增_CloudSeed；三方向粉尘混合、不规则密度遮罩。流星绘制函数保持。
- `Assets/_Game/Scripts/Presentation/RevealSky.cs`：云团种子、六区域随机流星调度、沿轨迹调整朝向、慢飞行/慢淡出。只读MeteorFlights/CloudSeed为Editor证据。
- `Assets/_Game/Scripts/Presentation/P04FeelConfig.cs`：新增meteorOpacity/meteorFadeSeconds，沿用meteorSeconds/meteorInterval。
- `Assets/_Game/Editor/P09SkyScatterSetup.cs`：独立导入/实际Play入口，24秒首张观察，自动保存多处流星截图和轨迹JSON。
- `Assets/_Game/Data/Presentation/P05FeelConfig.asset`、`Assets/_Game/Scenes/Main.unity`：由Editor保存参数，Main GUID保持。

素材原图、Core/概率/存档/角色模型、PrototypeGame输入、UIAdapter/UITheme不改。流程仍是签署/一次Draw/演出/每张及末张确认，同批跳过。

## 调参和运行

直接Main Play即可；Editor菜单 `Tools/这次一定出/程序/P09-SKY-SCATTER/接入素材声音并保存Main` / 验证入口，或报告目录 `SkyScatter.request` 写prepare/verify。旧入口可能覆盖本轮参数，优先本轮工具。

调 `P05FeelConfig.asset`：

| 参数 | 默认值 | 调整 |
|---|---|---|
| meteorOpacity | 0.28 | 降低更淡，个体另有0.8—1.1倍变化 |
| meteorSeconds | 3.2秒 | 增大飞行更慢，路径不加长 |
| meteorFadeSeconds | 2.2秒 | 增大消失更慢，尾迹不会立刻切断 |
| meteorInterval | 7秒 | 控制飞行/淡出后的间隔基准，另有轻微随机偏差 |
| proceduralNebulaAlpha | 0.40 | 星云总体明暗 |
| nebulaEvolution / Warp / Grain | 0.09 / 0.035 / 0.30 | 缓慢变化、局部扰动和粉尘细节 |

Shader旋转/缩放与遮罩控制分布；CloudSeed每次舞台启用变化，若需要重复某一构图可在Editor实验入口固定该种子，不进入玩家流程。流星区域定义在TickMeteor的3列×2行，各列内部随机；它不会只从边角或上边同一处出现。

涉及Unity概念：按屏幕比例计算轨迹旋转角，RectTransform锚点位置、非缩放时间调度、SmoothStep透明渐变、Shader多层采样与独立随机流。单张等待24秒只发生在Editor验证，游戏本身持续等待新确认。

## 证据与限制

`P09_SKY_SCATTER_Verification/` 保存实际Main片段、三窗口/四档字图截图、多起点流星截图、`sky_observation.json`（云团种子、每颗起止点/出现时间/飞行秒数/淡出秒数/峰值/大小）、checks/编译/保护文件报告。历史P07/P08片段不覆盖。

视觉效果仍待用户看实录验收。此背景仍是带素材质感的2D Shader，不是体积模拟；主界面不展示调试随机种子。没有新增角色、声音或GPU性能基准。

## 最终实测

2026-10-10 19:09（北京时间）`final_result.json` PASS，现有Unity 2022.3.53f1c1图形Editor实际Main Play。结束后已退出Play，配置保存。

- `sky_observation.json` 云团种子69.7511。四条流星起点依次(.241,.283)、(.712,.865)、(.457,.222)、(.177,.780)，高低交替；实际飞行2.89—3.42秒，淡出均2.2秒，峰值0.255—0.304。坐标为屏幕归一化锚点；实际方向按宽高比计算。
- 24秒首张停留中云团时间演化、恒星与NEW继续运行；至少三条随机轨迹、实际Shader无编译错误、素材绑定/种子使用通过。三窗口及四档标识通过，流星截图1—4可核对不同起点。
- 动画输入只补完当前张、每张/末张等待、签署松笔/跳过补签、同批演出跳过通过。真实单抽/十抽资源30→29→19、TotalPulls=11、CommittedSummonRequests=2。合成输入通过实际GraphicRaycaster+ExecuteEvents，Esc为HandleEscape；不宣称OS物理键鼠验证。
- `main_p09_scattered_sky.mp4` 为最终实际Main画面与同场AudioListener声音：32.683秒、1280×720、734帧、采集平均22.46帧/秒，48kHz双声道、音频削波0。原帧/时间戳/PCM和clip_manifest.json保留，没有后期替换声音。
- `compile_status.json` runtime/editor均exitCode=0；七项Core/UIAdapter/UITheme/UIPrototype/Main GUID/构建配置哈希均未变；15项源素材哈希不变。原始图与旧证据不覆盖。
