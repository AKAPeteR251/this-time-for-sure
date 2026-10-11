# P07-SKY-FEEL 星云/流星与NEW对比

用户授权范围：动态星云改Shader；流星改善贴图感；NEW加速并闪烁；雪花换回第一版写实图作对比。保留P06布局、四档字图、签约/唯一Draw/逐张确认。状态：Main已集成，降低亮度后的最终Shader版本编译及实际Play通过。视觉待用户验收。

## 改动

- `Scripts/Presentation/P04FeelConfig.cs`：程序星空模式、Shader引用、演化速度、透明度、NEW闪烁参数。
- `Art/Effects/F05/Shaders/RevealAtmosphere.shader`：uGUI透明Shader，星云使用随时间演化的三维值噪声、分形叠加与域扭曲；不采样星云图。屏幕呈现仍是2D背景，并非体积ray marching。流星分支用数学函数画光头、软光、逐渐收细/衰减的尾迹，不采样流星图。
- `Scripts/Presentation/RevealSky.cs`：实际实例化星云/流星Material，传入不受Time.timeScale影响的演化时间与屏幕比例；动态轨迹、淡入淡出、恒星/碎屑延续。材料在对象销毁时释放。旧两层图默认禁用，保留用于可切回的对照模式。
- `Scripts/Presentation/RevealGradientBorder.cs`、`FastRevealView.cs`：动态边框0.12→0.36循环/秒，柔和闪烁；文字本身保持稳定，亮点随边框速度加快。
- `Data/Presentation/P05FeelConfig.asset`、`Scenes/Main.unity`：Editor更新配置，雪花引用改回已有 `F04/RevealIdentityR2/ice_snowflake_v002.png`，透明度0.22。不重生成/改写该图。
- `Editor/P07SkyFeelSetup.cs`：独立接入与实际Play/录制入口。只更新明确管理的表现引用，不扩Core或UI框架。

Core、概率、存档、角色模型、PrototypeGame输入、UIAdapter/UITheme均不修改。抽卡演出仍消费已计算的同批结果。

## 对照与运行

上版基线保留：`Docs/Production/P06_REVEAL_R3_Verification/main_r3_layered_reveal.mp4` 与同目录 `1280x720_ssr_wait.png`，对应贴图星云/流星、慢速NEW、简化雪花。

本轮证据在 `P07_SKY_FEEL_Verification/`：`main_p07_shader_sky.mp4` 对应Shader星云/流星、快NEW+闪烁、写实雪花。另有 `1280x720_new_dim.png` / `1280x720_new_bright.png` 两个实际闪烁状态、三窗口角色图及四档稀有度截图。比较来自实际Main Play，不是离线生成的效果图。

打开Main并Play即可查看当前版本。Editor菜单 `Tools/这次一定出/程序/P07-SKY-FEEL/接入素材声音并保存Main` / 验证入口，或报告目录 `SkyFeel.request` 写prepare/verify。不要用旧P06准备入口覆盖当前雪花引用。

## 调参

调 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`：

| 参数 | 本轮值 | 如何比较 |
|---|---|---|
| proceduralRevealSky | true | false切回旧星云/流星图，星空底图与恒星仍保留 |
| nebulaEvolution | 0.09 | 噪声形态随时间变化的速度，减小更慢 |
| proceduralNebulaAlpha | 0.44 | 程序星云叠加透明度，降低更深邃 |
| newGradientSpeed | 0.36 | 上版0.12；本版3倍，约2.78秒循环 |
| newBlinkStrength | 0.35 | 设0关闭闪烁；本版最低边框alpha为基准的65%，不熄灭文字 |
| newBlinkFrequency | 1.6 Hz | 明暗周期，可降低减少抢眼感 |
| iceMageBackdrop / iceBackdropAlpha | 写实v002 / 0.22 | 对照简化图F05/RevealR3/SSR001_snowflake_backdrop_v003.png及上版0.32 |

Shader里可调星云坐标尺度5.4、扭曲强度1.1/1.8、颜色与密度阈值。为保留深邃星空，首轮大片雾感已降低，使用较暗的蓝紫/青蓝色和更细层次。流星沿局部确定性轨迹变化，间隔7秒、划过1.5秒仍由原参数控制，不消费游戏抽卡随机数。

涉及Unity概念：Material实例、Shader属性、程序噪声、UV与屏幕宽高比、透明混合、uGUI stencil/clip兼容、非缩放时间；NEW仍用已有透明边框alpha配合彩色网格，不需要重做素材。

限制：这是一版可调的2D程序效果，需要用户比较视觉；不声称达到体积云/真实天体模拟。未做GPU帧耗专项基准，录制帧率也不是游戏性能基准。现有中文声音与水晶前段保持；VITS需求仍延期。

## 最终实测

2026-10-10 18:39（北京时间）最终 `final_result.json` 为PASS，现有Unity 2022.3.53f1c1图形Editor运行Main；测试结束已退出Play，配置保留。

- `checks.json` 验证实际启用且支持程序Shader、绘制后无Shader编译错误、第一版写实雪花Sprite、NEW速度0.36、噪声演化时间增加超过0.30、闪烁周期覆盖亮/暗、流星出现、恒星闪烁。
- 首/末张等待、动画点击只补完不穿透、真实单抽与十抽唯一Draw、同批跳过、三窗口与四档标识仍通过。资源30→29→19、TotalPulls=11、CommittedSummonRequests=2；固定开发批次不扣费或重算收藏。
- 输入来自实际GraphicRaycaster命中及ExecuteEvents模拟down/up/click，Esc走现有HandleEscape；不宣称物理OS键鼠测试。
- 最终视频 `main_p07_shader_sky.mp4`：11.72秒、1280×720、258帧、采集平均22.03帧/秒；同场AudioListener的48kHz双声道声音，峰值0.2667、削波0。帧/时间戳/PCM源与 `clip_manifest.json` 一并保留，没有后期生成声音。
- `compile_status.json` 两目标exitCode=0；`protected_check.json` 七项Core/UIAdapter/UITheme/UIPrototype/Main GUID/构建配置哈希不变；15项原素材SHA256保持一致。Shader新文件由Unity产生meta，旧生成素材均未改写。
