# P24-PORTAL-CAMERA — 签约法阵推进至3D祭坛

状态：已集成 Main，实际 Play 通过，视觉待验收。

授权：用户要求签约后2D法阵通过运镜过渡至3D场景，本次明确修改 Main 的必要演出接口、2D法阵视图、3D相机与可重复配置工具。不改 Core、结果、签署流程、星云和角色揭示，不重做祭坛光火。

方案：2D法阵放大并轻微转正，中心开口先显露3D祭坛；3D镜头同时由偏侧广角向水晶推进，最后平稳落到既有连击镜头。两侧共享时序，输入在转场完成后进入水晶蓄能。

## 实现与改动

- `ContractCeremonyView.PortalTransition(progress)`：法阵的视觉根放大到 3.1 倍，过程中轻微倾斜并回正；Begin/Finish 复位，签名纸与蜡印时间线不改。
- `CrystalChargeView.Transition / CeremonyFade`：同一时序驱动3D镜头、中心开口、后半段2D层淡出；3D阶段不再整体淡入。
- `CrystalStage3D`：真实隔离相机由原广角位置加偏侧偏上偏移推进到原 `altarCameraClose`，视野59°到46°，2°倾斜平稳回正。水晶本体保持既定比例，进入的放大来自相机距离，不再同步缩放模型制造进场。
- `CrystalPortal.shader`：RawImage 上的圆形柔边开口，按窗口宽高比保持圆形，由中心扩展至全屏；材质实例随运行对象销毁。不需要新美术。
- `PrototypeGame.Reveal`：只增加上述演出接口，共用最后1.8秒；转场完成仍进入原水晶连击阶段。
- `P04FeelConfig` 与 `P05FeelConfig.asset`：增加镜头调参、保存Shader引用。
- `P24PortalSetup / P24PortalVerify`：Editor 可重复配置、实际Main验证和本会话音画录制。

Main 沿用同一配置引用，不改场景文件，不增加相机框架。Shader运行在已有uGUI渲染合成上，3D仍是现有真实FBX、URP相机和 RenderTexture。

## 实际验证

2026-10-11 09:49（北京时间），Unity 2022.3.53f1c1 图形 Editor Main 实际 Play 通过：

- 真实签署页跳过补签、单次Draw、资源30→29、1次提交。
- 转场连续采样记录位置、FOV、进度和法阵比例；相机真实推进超过2米，终点与原连击镜头一致；法阵结束复位。
- 运镜期 HitCrystal 入口不提前蓄能，完成后 EventSystem 真实射线与指针事件点击水晶可蓄能。
- 水晶阶段 Esc 同批总览，运镜中演出跳过清场，点击补完前奏直接落在相机终点，不重复计算。
- 1280×720、1920×1080、1600×1000 截图与按钮安全区。

证据：`P24_PORTAL_CAMERA_Verification/final_result.json`、`checks.json`、`camera_samples.json`；三个窗口的转场截图；`main_p24_portal_camera.mp4` 为实际帧时间戳画面与同次 Unity AudioListener 混音。验证由 Editor 驱动既有入口及 EventSystem，未声称人工键鼠验收。

## 运行与调参

打开 Main，召唤→签署或跳过补签→红蜡印升空→法阵中心穿越→3D水晶。

在 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`：

- `crystalTransitionSeconds`：当前1.8秒，使用前奏最后这段时间，不延长整个前奏。
- `crystalPortalScale`：法阵最后倍率，当前3.1。
- `crystalPortalEntryOffset`：3D起点相对原广角位置的偏移，当前(-0.7,0.6,-0.5)。
- `crystalPortalRoll`：起点镜头侧倾，当前2°，终点0°。
- `altarCameraWide / Close`、`altarFocusWide / Close`、`altarWideFov / CloseFov`：原有相机起终点与视野。
- `crystalPortalCamera`：关闭可对比原淡入方式；`crystalPortalShader` 保持现有引用。

重新导入配置菜单：`Tools/这次一定出/程序/P24-PORTAL-CAMERA/保存法阵运镜配置`；同菜单下「验证Main输入音画并录制片段」为开发验证入口。

Unity概念：2D变换模拟前景推进，RenderTexture柔边遮罩形成门洞，真实相机位置/FOV同步插值。两者使用相同时间，但2D法阵没有真实厚度，并非可从背面观看的3D门户；当前第一版视觉待用户验收。祭坛光火保持既有版本与待办状态。
