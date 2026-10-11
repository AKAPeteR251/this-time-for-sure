# P19-CRYSTAL-MOTION

任务：接入F07_CrystalLayers_v001.zip的拆分FBX，Unity实时动画实现巨环/水晶/碎片/群星运动，以及法阵进入水晶的连贯转场。状态：Main集成及程序实测通过，视觉待用户验收。

范围：F07模型/材质、独立舞台表现、CrystalChargeView、PrototypeGame仪式协程最小衔接、P04FeelConfig/Main配置与Editor导入验证。不改变Core、概率、扣费、收藏、角色模型和存档。保留已验收星云与P18角色演出。第3项投射光束和第4项新碎裂效果后续单独处理。

先核对真实FBX层级/材质/轴向与Unity导入，再做运动和Main验证，不以Blender预览代替Unity实测。新增动画由连续时间驱动，不在点击时重置轨道。实际交付带声视频、转场/停手/连击/跳过及三窗口证据，模型来源与调参入口。

## 实现与资源

F07静态FBX原样导入 `Assets/_Game/Art/Effects/F07/CrystalStage/F07_CrystalStage_v001.fbx`；母版与源码仅归档，没有执行美术包脚本。Unity实际导入24个MeshRenderer（四环、12片中央晶壳、8个漂浮晶体），根另有FBX轴向转换，读取真实节点后保留转换，动画节点重新置于统一Y-up空间，晶壳保留局部完整状态位置。关闭FBX动画/相机/灯光导入。Unity产生meta。

JSON实际材质为六种纯色材质、无贴图文件。Editor将FBX材质槽映射到六个URP Lit .mat，并按实际画面调低晶体金属度、提亮冰蓝发光；不假定Blender材质节点或Bloom会自动传入Unity。保留源参数JSON与新材质，模型原稿不改。

`CrystalStage3D` 使用独立实时Camera / RenderTexture，把真实3D网格画面合成到水晶uGUI舞台；不把FBX再渲染成离线视频或PNG。镜头只绘制31层的舞台，运行根远离Main原点，灯光限制同层；退出舞台立即禁用镜头与模型，销毁时释放RenderTexture。RT按当前窗口更新，最大1920×1080。现有星空/已验收星云Shader复用，星点额外做慢漂移和闪烁；不改角色揭晓的星云Shader或P18参数。

外/中/内环与底环按不同速度转动，底环绕Y轴、立环绕Z轴；中央水晶浮动、自转、微倾斜；外围物体沿有深度的轨道公转并独立自转、起伏。点击不重置时间或轨道。停手保留能量、震动自然衰减，其余运动持续。中央晶壳随蓄能略开缝、内部材质按当前稀有度发光，用来保留进度可读性；这是基础状态反馈，不作为第3项投射光束完成。

前奏最后1.25秒保留原法阵，3D舞台交叉渐入；镜头从较远、较低视点推进到水晶中心，水晶同步渐长，进度进入等待后连续运动。原法阵收起时新舞台已全显；动画补完前奏也能立即得到完整舞台。Stage3D.Contains将点击坐标映射到实时Camera视线，与中央晶壳包围盒相交，避免使用旧贴图位置作为输入区。

## 文件与接口

新增 CrystalStage3D.cs、P19CrystalMotionSetup.cs、FBX及六个材质；修改CrystalChargeView.cs、P04FeelConfig.cs、PrototypeGame.cs的前奏衔接、P05FeelConfig.asset与Main绑定。Core/概率/收藏/存档/角色模型不改。

`CrystalChargeView.BeginPreview/Transition` 接前奏交叉渐入，旧 `Begin/Hit/Tick/Terminal/Burst/Clear` 保持功能；`Stage3D` 暴露Rendering、Age、Entry和节点位置供Editor观察。角色阶段仍沿用P18。水晶旧破裂序列与白幕继续保留，3D转旧破裂的终结效果待第4项改进。

## 运行与调参

Main的feelConfig仍指向 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`。打开Main按Play即可；开发接入/验证菜单为 `Tools/这次一定出/程序/P19-CRYSTAL-MOTION/…`。导入工具先验证完整FBX与24网格，再保存Main，不覆盖脏场景；旧菜单保留历史基准，当前使用P19。

- `crystalTransitionSeconds=1.25`：与前奏末尾交叠的秒数。
- `crystalCameraDistance=18`：越小舞台越近；镜头对准约4.1m高度，FOV37。
- `crystalBodyScale=2`：中央水晶单独放大，不缩放其他环。
- `crystalOuterSpeed=8`、`crystalInnerSpeed=-12`、`crystalFloorSpeed=7`：每秒旋转角度，中环为外环0.72倍。
- `crystalOrbitSpeed=7`、`crystalBodySpeed=4`：外围公转和中央自转速度；外围叠加不同自转。
- `crystalFloatAmplitude=0.16`：局部空间起伏幅度；`crystalStarDrift=0.0003`：星点每秒归一化漂移。
- 震动、蓄能、资源和声音继续使用现有字段；材质可在F07/CrystalStage/Materials逐项调色。

涉及Unity概念：FBX层级/轴向与局部Transform、URP材质映射、独立Camera/RenderTexture与uGUI合成、连续时间动画、CanvasGroup交叉淡化、视线和包围盒输入判断、运行对象与GPU资源清理。

## 最终Main实测

2026-10-10 22:13:44（北京时间）最新一轮通过。证据目录 `Docs/Production/P19_CRYSTAL_MOTION_Verification`，final_result.json / checks.json记录255条检查（含重复文案扫描，不是255个独立用例），真实单抽/十连共两次提交。自然前奏与3D舞台实际交叉淡化；停手2秒内环角度、漂浮物位置、水晶位置均变化，星点位置亦真实漂移；慢点、停手保能收震、高频蓄能完成、逐张等待及末张新确认通过。

转场0.6/0.84进度处跳过、3D蓄能Esc、角色两段爆发中跳过均关闭舞台镜头/模型并到原批总览；动画补完当前张不穿透。实际1280×720、1920×1080、1600×1000流程通过；截图包括 `1280x720_ritual_transition.png`、`1280x720_crystal_idle.png`、`1280x720_crystal_charge_hold.png`、两个大窗口 `*_crystal_3d.png`。输入由实际Main的GraphicRaycaster/ExecuteEvents和HandleEscape入口注入，不声称物理键鼠人工验收。

录像 `main_p19_crystal_motion.mp4` 为实际Main Play45.31秒、962帧、1280×720、平均捕获21.23fps；同次Unity AudioListener 48kHz双声道音轨，无削波。`main_p19_crystal_motion_focus.mp4` 为前18秒原速截取，含自然前奏、转场、停手运动和连击。帧/混音原始记录及clip_manifest.json保留，没有离线重建演出或配音。

FBX与原ZIP哈希一致，24个网格实际Unity导入清单见unity_model_nodes.txt；六个URP材质及meta齐全。修正MaterialDescription重导入可能刷新材质参数的顺序，最后一次FBX导入后再应用调色。runtime/editor编译通过，受保护Core/UI和已验收星云Shader未改。后续依赖：用户验收本轮水晶运动和转场。投射光束及新碎裂/白光效果仍为第3、4项。
