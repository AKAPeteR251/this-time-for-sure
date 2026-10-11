# P08-NEBULA-DUST 星云粉尘修正

用户反馈：写实雪花与NEW可保留；星云缺少贴图版粉末感，变化中的白色纹理违和。仅调整星云：去白色筋状纹理，以既有蓝紫/蓝青粉尘素材为质感，由Shader局部扰动、混合与细颗粒演化驱动，避免整层匀速平移。保持P07流星、写实雪花、NEW、布局及抽卡逻辑。状态：Main已接入，最终实际Play与编译通过，约20秒动态观察片段已交；星云视觉待用户验收。

## 这版星云怎么变

P07的白色筋纹来自分形噪声中的ribbon/veins。此分支已删除，不再叠加白色轮廓或电流状亮线。颜色来自F05已有蓝紫/蓝青素材，保留细小粉尘的原始质感。

当前是**粉尘素材 + Shader局部变化**：两层以不同速度与局部扰动混合，低频噪声改变区域密度，细频噪声只轻微调制彩色颗粒。没有声称本版是完全不采样纹理的程序云；与P06的两张RawImage匀速滚动相比，各位置的UV和密度随噪声演化，因此变化不只是整张图移动。

恒星/流星、写实雪花v002透明度0.22、NEW速度0.36及闪烁强度0.35/频率1.6Hz保持。人物/名字/SSR排版不改，角色待确认期间背景持续运行。

## 改动文件与接口

Unity工程相对路径：

- `Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader`：替换星云分支，新增两层粉尘纹理/_Warp/_Grain；流星分支保持。
- `Assets/_Game/Scripts/Presentation/RevealSky.cs`：绑定现有星云纹理与局部扰动/颗粒参数，其余动态和Material释放沿用。
- `Assets/_Game/Scripts/Presentation/P04FeelConfig.cs`：新增nebulaWarp/nebulaGrain。
- `Assets/_Game/Editor/P08NebulaDustSetup.cs`：独立Editor准备、实际Main验证及录制。星云观察停留由4秒延长到12秒，方便看慢速变化；游戏本身仍一直等待用户新确认，不会12秒自动前进。
- `Assets/_Game/Data/Presentation/P05FeelConfig.asset`、`Assets/_Game/Scenes/Main.unity`：Editor保存现有纹理与本版参数，Main GUID不变。

原始素材没有修改/重新生成。Core、概率、存档、角色模型、PrototypeGame输入/Draw、UIAdapter与UITheme未改。所有FastRevealView和批次/跳过接口仍沿用。

## 打开与调参

打开Main并Play即可。Editor菜单 `Tools/这次一定出/程序/P08-NEBULA-DUST/接入素材声音并保存Main` 或验证入口；也可报告目录 `NebulaDust.request` 写prepare/verify。准备入口的名称沿用旧工具格式，实际不重绑音频。

在 `P05FeelConfig.asset` Inspector：

| 参数 | 本轮值 | 作用 |
|---|---|---|
| proceduralNebulaAlpha | 0.40 | 整体星云透明度，减少更深、更暗 |
| nebulaWarp | 0.035 | 局部UV扰动；减少更平静，设0仍有分层漂移/密度变化 |
| nebulaGrain | 0.30 | 细颗粒调制强度，设0保留原图自身粉尘、不增加程序颗粒 |
| nebulaEvolution | 0.09 | 扰动和密度噪声演化速度 |

参与的Unity概念：Shader纹理采样、UV局部扭曲、多层alpha混合、Material属性与unscaled时间。此处的procedural字段名是历史配置接口，本版含素材采样，调色不会生成白色筋线。

## 对照与证据

本轮 `P08_NEBULA_DUST_Verification/` 有实际Main截图、输入报告、编译/保护文件检查和 `main_p08_dust_nebula.mp4`，录制包含首张长停留、NEW/星云持续变化和后续新确认。P07纯噪声版本及P06贴图版本的原片段保留，均不覆盖。

视觉参考：P07 `main_p07_shader_sky.mp4`（白色筋纹，被用户指出违和）；P06 `main_r3_layered_reveal.mp4`（原粉尘纹理），都是历史实录。旧准备工具引用同一Shader文件，不可用它们重建历史Shader版本；对照以保留的视频为准。

限制：保留质感的同时仍要采样现有素材，细颗粒与局部流动的观感待用户比较。没有体积云/ray marching、GPU专项基准或声音扩充；其余角色仍是已有带底卡面/缺图剪影，完整透明首秀为冰法师。

## 最终验证结果

`final_result.json`：2026-10-10 18:52（北京时间）PASS，实际Unity 2022.3.53f1c1图形Editor的Main Play。验证结束已退出Play，表现配置已保存。

- Shader实际绑定原有两张粉尘纹理、演化时间持续增加、实际绘制后无Shader编译错误。观察首张12秒仍未自动前进，恒星/流星与NEW渐变闪烁保持运行。三窗口截图和R/SR/SSR/UR各自字图通过。
- 动画点击补完当前张且不穿透；每张/末张等待新确认；签署松笔与跳过补签、演出跳过保持同批。输入由实际GraphicRaycaster首命中及ExecuteEvents合成pointer事件，Esc走HandleEscape，不声称OS物理键鼠测试。
- 真实单抽/十抽资源30→29→19，TotalPulls=11，CommittedSummonRequests=2；开发固定演出批次不扣费或重抽。
- `main_p08_dust_nebula.mp4`：19.76秒、1280×720、444帧、采集平均22.49帧/秒；同场AudioListener 48kHz双声道，峰值0.2667、削波0。源帧、时间戳、PCM及clip_manifest.json保留；视频声音未重新配制。
- `compile_status.json`：runtime/editor均exitCode=0；`protected_check.json`七项Core/UIAdapter/UITheme/UIPrototype/Main GUID/构建配置均未变；15张素材与ArtSource哈希一致。Shader保留原meta/GUID，未移动旧资产。
