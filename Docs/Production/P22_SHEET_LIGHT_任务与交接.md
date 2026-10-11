# P22-SHEET-LIGHT

用户要求：火焰/室内打光优化记为待办；恢复水晶破裂前蓄能漏光，并最新明确重做素材为片光光束。以最新指令为准，不恢复旧2D细线贴图。

范围：在现有3D水晶舞台添加程序网格片光与URP柔边Shader，蓄能驱动数量/长度/亮度，短震与终结驱动表现。独立资产与调参入口，不改变F07原件、室内照明/火焰、Main抽卡逻辑、概率/存档及角色揭晓。

状态：实现、编译、Main实际Play检查通过；视觉待用户验收。

## 新素材与接入

新增 `Assets/_Game/Scripts/Presentation/CrystalSheetLight.cs`，在隔离3D舞台创建7组程序扇面网格，每组12段、24三角面。几何由窄裂隙端向外展开，有浅弯曲与不同倾角，使用实际场景深度遮挡。没有输入Collider，不扩大水晶点击范围。

新增 `Assets/_Game/Art/Effects/F04/Crystal/Effects/CrystalSheetLight.shader`：URP加色混合、横向柔边、远端渐隐、微弱流动折纹；颜色跟随同批稀有度。素材原创程序生成，不下载第三方资产，也不覆盖旧 `light_leak_v004.png`。本轮“片光”由网格与Shader组成，无需另交光束PNG。

`CrystalStage3D.cs` 在原StageCamera与水晶节点构建完成后创建片光，Tick在相机姿态更新后同步位置、蓄能、震幅、终结强度。光源位置围绕水晶正面裂隙区域，与水晶摇动一起移动；不是屏幕上的UI线条。整个rig停用时片光清零，再次进入从零开始。

`P04FeelConfig.cs`及 `P05FeelConfig.asset`保存Shader与参数；`P22SheetLightSetup.cs`提供可重复保存入口；`P22SheetLightVerify.cs`为独立实机验证和录制。Core、PrototypeGame、室内打光/火焰代码、已验收星云Shader和F07原始FBX保持字节不变。

## Main实际验证

2026-10-10 23:52:29北京时间，已有Unity图形Editor中Main Play验证通过。输入通过uGUI射线与ExecuteEvents，Esc通过HandleEscape入口；未声称真人硬件输入。固定SSR仅开发演出样例，真实单抽/十连另验证唯一Draw与同批跳过。

证据目录 `Docs/Production/P22_SHEET_LIGHT_Verification`：

- `final_result.json` / `checks.json` / `light_observations.json`：初始无光；中段亮起；高蓄能至少5片；停手保持能量和片光但收震；非水晶空白不蓄能；破裂及跳过清光；三环运动保留；逐张及末张等待、动画点击仅补当前张；手写松笔及跳过補签分别唯一扣费。
- `1280x720_sheet_medium.png`、`1280x720_sheet_high.png`、`1280x720_sheet_hold.png`，另有 `1920x1080_sheet_high.png` 与 `1600x1000_sheet_high.png`。
- `main_p22_sheet_light.mp4`：实际Main片段17.98秒、393帧、平均21.86fps；同次48kHzUnity音轨，无削波。
- `compile_status.json` 与 `protected_check.json`：运行/Editor编译通过，冻结范围不变。`setup_status.txt`为Editor配置保存证据。

## 操作与调参

Main正常召唤并签约后点击水晶，蓄能达到约24%开始漏光；不同光片依次增强，高蓄能更长更亮，停手不清空，终结进一步增强，破裂切白光时关闭。旧2D贴图逻辑不变，但3D路线不再使用它。

`P05FeelConfig.asset` → P22水晶裂隙片光：

| 字段 | 默认值 | 用途 |
|---|---|---|
| crystalSheetStart | 0.24 | 起始蓄能阈值 |
| crystalSheetLength | 3.8 | 世界空间长度系数 |
| crystalSheetWidth | 0.9 | 扇面宽度，增大更宽、更像片光 |
| crystalSheetIntensity | 2.7 | HDR强度，室内已有Bloom响应 |

Editor菜单 `Tools/这次一定出/程序/P22-SHEET-LIGHT/保存水晶片光` 只保存本轮配置，需停止Play并保存编辑；同组验证菜单运行实际Main录制。涉及Unity概念：程序Mesh/UV、MaterialPropertyBlock、柔边加色Shader、场景深度遮挡和GameObject生命周期。

限制：是柔边几何片光首版，未实现体积散射积分、真实折射/投影阴影或水晶面按裂纹破开。室内火焰与打光已按用户要求登记 `视觉待办事项.md`，本轮未采购或导入联网素材。碎裂最终动画升级仍待后续独立任务。
