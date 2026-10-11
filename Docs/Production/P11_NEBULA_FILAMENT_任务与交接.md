# P11-NEBULA-FILAMENT

状态：Main已接入，实际运行检查通过，待用户视觉验收。

用户反馈：P10粉尘可保留，但像烟雾，缺少星云脉络；分布常偏一个角落；变化像闪现而非漂移。本任务只优化Main揭晓星云，保持雪花/NEW/流星与抽卡契约。

方案：稳定宏观结构沿横向漂移，内部蓝紫细丝慢变；约束云团分布覆盖不同横向区域，但保持不同大小与不规则边界；每张重新启用不换种子，单独累计星云时间，柔和淡入。保留纯Shader粉尘，不加白色筋纹。新增细丝强度调参。

## 实现与文件

- Unity工程 `Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader`：宏观噪声不再随时间切换深度；低频形变与三个不同位置/形状的柔和覆盖区建立布局，覆盖区随横向坐标运动并在屏幕外衔接。内部多层噪声生成不规则、断续的细丝与暗隙，沿用P10蓝紫色和细粉尘。没有白色轮廓线，流星分支保持原样。
- `Assets/_Game/Scripts/Presentation/RevealSky.cs`：星云累计时间与逐页流星时间分开；OnEnable不再重置星云种子或累计时间，翻张/重新进入页面保留同片星云。每次进入揭晓以1.6秒柔和淡入。星星和流星保留既有逐页调度。
- `Assets/_Game/Scripts/Presentation/P04FeelConfig.cs`、`Data/Presentation/P05FeelConfig.asset`：新增nebulaFilaments，并调整现有星云参数。
- `Assets/_Game/Editor/P11NebulaFilamentSetup.cs`：独立准备/验证入口、实际Main录制及连续性检查。

全部资产路径相对 `D:/vibe/这次一定出/这次一定出`，Main使用既有配置，不新建角色数据、抽卡模型或UI接口。Core、概率、存档未扩展，原始素材不覆盖。

## 运行与调参

Unity停止Play且无未保存场景编辑后，菜单 `Tools/这次一定出/程序/P11-NEBULA-FILAMENT/接入素材声音并保存Main`；随后同级验证菜单，或直接Main Play。自动验证结束退出Play，报告在 `Docs/Production/P11_NEBULA_FILAMENT_Verification/`。请求入口NebulaFilament.request支持prepare/verify/status，最终结果以final_result.json时间为准。

选择 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`：

| 参数 | 值 | 调整作用 |
|---|---:|---|
| nebulaDrift | 0.007 | 横向漂移速度；增大移动更明显，负值反向 |
| nebulaEvolution | 0.055 | 仅内部细丝/细颗粒变化，大结构不再被时间噪声重建 |
| nebulaFilaments | 0.55 | 细丝和暗隙占比；0偏粉尘，越高脉络越明显 |
| proceduralNebulaAlpha | 0.42 | 总体亮度 |
| nebulaThreshold | 0.50 | 调高降低宏观云团亮度，覆盖区保留少量粉尘底层 |
| nebulaScale | 2.8 | 云内纹理尺度；不直接改变覆盖区位置 |
| nebulaWarp / nebulaGrain | 0.035 / 0.45 | 内部缓慢形变幅度 / 细粉尘强度 |

覆盖区的位置、大小、屏幕外衔接和1.6秒淡入在Shader/RevealSky中调节，避免随机噪声单独决定全屏云团位置。粒子感依旧为程序噪声；底层星空、星星、雪花、NEW继续使用既有素材。视觉随机流与抽卡随机流程分开。

涉及Unity概念：噪声域形变、分形密度脉络、柔和覆盖遮罩、屏幕外连续衔接、材质参数、非缩放时间，以及MonoBehaviour.OnEnable对表现连续性的影响。此效果是2D Shader，不是体积模拟。

## 验证范围

实际Main Play复核纯星云材质无贴图输入、漂移与内部演化同时推进、等待及翻张种子/累计时间稳定；保留实际三窗口/四档/签约Draw与演出跳过流程。输入使用实际GraphicRaycaster+ExecuteEvents，Esc调用HandleEscape，不宣称OS物理键鼠测试。视觉仍待用户验收，日语VITS女声继续按用户要求后续实现。

另有 `1280x720_nebula_90s_phase_probe.png` / `1280x720_nebula_180s_phase_probe.png`：开发验证入口把星云累计时间切至90/180秒，由实际Main绘制，用于检查未来漂移相位是否留下一侧过大空白。这两张是加速相位检查，并非声称实际等待三分钟；它们在实时片段录制结束后单独生成，不混入正常速度的视频。

## 最终实测

2026-10-10 19:48:22（北京时间），Unity 2022.3.53f1c1图形Editor实际Main Play最终PASS。实测片段对应打散细丝、降低线条感和屏幕外衔接修正后的最终版本。

- `main_p11_filament_nebula.mp4`：32.369秒，1280×720，728帧，采集平均22.49帧/秒（不是GPU性能基准）；同场AudioListener录音48kHz双声道，削波0。原始帧、时间戳、PCM与clip_manifest.json保留。
- 首张24秒正常停留前后截图保留，星云横向漂移且内部慢变。90/180秒相位截图目视核对，左右均有蓝紫细丝粉尘，保留不规则深色留白，没有一侧完全空掉。
- `checks.json` / `final_result.json`：等待、翻张、重新进入揭晓仍保留同一构图种子；累计星云时间不因翻张归零；Shader实际绘制无错误。既有确认/跳过、三窗口/四档、签署/补签唯一Draw检查通过，真实资源30→29→19、抽数11、提交2次。
- runtime/editor编译exitCode均0；七项保护文件哈希未变，15项原始素材哈希未变且.meta存在。运行结束退出Play，配置已保存。

外观是否达到星云脉络的目标仍待用户验收；本次不把运行PASS标为视觉已验收。
