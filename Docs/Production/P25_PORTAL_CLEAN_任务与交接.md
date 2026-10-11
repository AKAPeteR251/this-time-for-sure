# P25-PORTAL-CLEAN — 删除2D聚集与背景跳色

状态：Main 集成与实际 Play 通过，视觉待验收。

授权：用户指出2D菱形水晶聚集无合适素材，要求删除并修复背景颜色突然变化。本次仅修改签约/演出背景视觉与聚集层，保留2D法阵、P24运镜、3D水晶及浮动碎片、签名金线和所有抽卡输入流程。

原因：CelestialLinesGraphic 同时画签名金线与16个菱形及拖线；CeremonyDark 另外重绘一张蓝色乘染背景，其颜色与 Signing 的暗遮罩不一致，后半段整层淡出还会改变背景明暗。

实施：删除菱形及聚集线绘制；统一签署与Reveal页面的背景遮罩，放在法阵淡出层外，沿用共同背景图片，等3D圆形开口覆盖。

## 改动与运行

- `Scripts/Presentation/CelestialLinesGraphic.cs`：删除16个菱形及向内聚集/拖线网格，仅保留签名成立时的金色笔迹带。
- `ContractCeremonyView.cs`：删除重绘并乘染背景的 CeremonyDark；节点改为 AscendingSignatureGoldRibbon。
- `ContractPaperLayout.cs`：共享 BackgroundShade 颜色。
- `Scripts/UI/PrototypeGame.cs`：Signing 与 Reveal 使用相同遮罩，Reveal 遮罩独立于 CeremonyStage 的 CanvasGroup，转场淡出不再让共同背景变亮。
- `Editor/P25PortalCleanVerify.cs`：独立实际验证与音画录制入口，不改原P24证据。

打开 Main 正常召唤并签约即可查看。3D水晶和真实浮动碎片保留；无需新美术素材，未改 Main 场景文件、概率、存档或抽卡批次。

## 实际证据

2026-10-11 09:57（北京时间），Unity 2022.3.53f1c1 图形 Editor Main Play 通过。`P25_PORTAL_CLEAN_Verification/`：

- `final_result.json` / `checks.json`：同一背景遮罩、无重复染色图片、遮罩不参与法阵淡出、法阵阶段无可见菱形聚集网格；真实相机推进、三窗口、转场输入、连击、同批跳过与唯一Draw。
- `1280x720_signing_background.png` / `1280x720_ceremony_background.png` / `background_continuity.json`：实际签署与落印前演出截图，背景矩形(20,160)—(210,540)的RGB逐像素一致，最大差值0。核对避开纸张与演出闪光，盖印的短暂闪光仍按原设计保留。
- `1280x720_portal_middle.png`：已查看，原红圈菱形和拖线消失；另外两窗口截图可比对。
- `camera_samples.json`：连续相机位置、FOV及2D法阵比例。
- `main_p25_portal_clean.mp4` / `clip_manifest.json`：实际Main帧时间戳画面与同次AudioListener混音，无重建音轨。

验证采用Editor驱动既有入口与EventSystem指针事件，不冒充人工键鼠验收。首次验证工具编译遇到 CanvasRenderer.GetMesh 接口签名差异，已改为当前Unity的无参数版本；修正后实际运行通过。

## 涉及的Unity概念与调参

Graphic 网格按层删除；Image 颜色乘染与纯色遮罩不同；CanvasGroup 的层级决定淡出的范围。本次遮罩集中在 `ContractPaperLayout.BackgroundShade`，改此值会同时影响签署与法阵；P24运镜参数继续使用 `P05FeelConfig.asset`，祭坛光火仍是原待办。
