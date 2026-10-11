# U02-VIS：字体和概念图布局修正

任务依据：用户在 U02-ART 交付后反馈“字体很难看，而且画面也没有还原概念图”，附结果页参考图。依赖 U02-ART 六卡、背景、主题和真实内存 UI 入口；依赖已具备。

范围：修改 UI 显示和独立验证入口。保留共享角色 ID、抽卡计算与收藏数据；不写 Main、Core 和构建配置。状态：待验收。

## 本批修改

- `这次一定出/Assets/_Game/Scripts/UI/PrototypeGame.cs`：标题使用系统 STZhongsong（华文中宋），正文 SimSun（宋体），关键文字固定字号；顶部召唤/图鉴导航、左侧栏和紧凑资源栏；结果页五列两行大卡窗、人物等比裁切、元素图标、明亮稀有度与 NEW 角标、底部左右按钮；召唤页放大冰法师至半身主视觉，移除大片遮罩和主页面调试按钮。
- `这次一定出/Assets/_Game/Scripts/UI/PrototypeUITheme.cs`、`UI/Styles/U02ArtTheme.asset`：增加现有导航和元素图标引用。角色大图与小图标的 `.meta` 经 Editor 启用 mipmap / Trilinear，用于缩小显示抗锯齿，PNG、GUID 不变。
- `这次一定出/Assets/_Game/UI/Editor/U02ArtSetup.cs`：增加 `verify-visual` 请求，视觉复验输出到独立 `U02_VIS_Verification` 目录，不覆盖 U02-ART 原验收证据。

## 运行与复验

Unity 打开 `Assets/_Game/Scenes/UIPrototype.unity`，进入 Play。顶部或侧栏进入召唤/图鉴；单抽/十连进入演出，跳过进入既定结果，点卡打开详情，隐藏/恢复，再返回。

自动复验：停止 Play 并保存编辑，在工作区 `Docs/Production/U02_ART_Verification/U02.request` 写入 `verify-visual`。Editor 会打开独立场景，运行真实内存闭环，截图后退出 Play。

截图目录：`Docs/Production/U02_VIS_Verification`，包含 1920×1080、1280×720、1600×1000 的召唤、规则、真实随机总览、SSR/UR 详情、隐藏界面、收藏、六卡绑定以及参考十卡布局。

`concept_layout_preview` 是有明确标题的视觉校准，顺序与参考图一致；仅构建显示列表，不调用抽卡，不改变扣费、收藏或真实结果。`summary_random` 才是实际随机十连。

## 验证和限制

编译结果见 `U02_VIS_Verification/compile_status.json`，两个程序集退出码均为 0。最终 Play 结果见 `final_result.json`；2026-10-08 11:08:37（北京时间）通过真实内存 30→29→19→29、单抽/十连、跳过固定结果、NEW、详情、隐藏恢复、收藏、SSR/UR 预览、六卡、三种窗口与中文覆盖，另检查可见稀有度角标的实际文字宽度。运行时字体和显示信息见 `visual_runtime.txt`。`png_unchanged.json` 核对清单中的 36 张 PNG，全部哈希不变。

截图 `1280x720_concept_final.png` 为最终视觉校准预览。按钮使用不透明深蓝/浅金底层，避免在线性颜色空间下金色底图透出而偏褐；大图缩小过滤解决了最初截图的碎斑锯齿。排查期间的临时 Shader/Material 已删除，界面继续使用 Unity 默认 UI 材质。

现有六张职业稿与概念图的人物构图和精细度并非完全一致，当前以已有素材实现布局，不声称逐像素一致。十二角色中其余六张继续使用明确的立绘待补充占位。系统字体采用运行时加载，没有把 Windows 字体文件复制或分发到工程；正式发行需补有授权的字体资产及 TMP 字体。

主场景集成仍交给程序/总指挥；本批只验证 UIPrototype。自动复验走已绑定按钮事件，不替代尚未实测的人工鼠标和 Escape 验收。

## Unity 概念和调参

- CanvasScaler 用 1920×1080 参考尺寸与窗口锚点缩放。Summary 的 ResultCards 区域控制卡格占屏比例，ConceptCard 控制卡面、名称条和角标。
- 在 `U02ArtTheme.asset` Inspector 中调 `summaryPortraitCrop`，当前 `(0.175, 0.50, 0.65, 0.50)` 选取头肩近景。RawImage 的 uvRect 选取这块区域，AspectRatioFitter 的 FitInParent 保持裁切区域比例；详情仍显示原图。UV 裁切不改变原 PNG/GUID，也不依赖实时遮罩。
- Label 的字号与 Title 的标题字体分开；标题、卡名和按钮关闭自动缩小。BuildArtSummon 中 IceMage 的区域决定主视觉缩放与构图。
- 图鉴使用紧凑卡名字号与角标字号，总览保留大字；SSR 三个字母单独留足宽度，避免换行后截成 SS。
- UI 装饰不拦截射线，按钮负责请求；演出仍消费已计算结果。预览只显示数据，不入库。
- 大图缩小很多时，mipmap / Trilinear 减少细线闪烁与碎斑；卡图小比例变化由 UV 区域和卡窗锚点共同决定。深色按钮背景应使用不透明色，避免亮金底层参与混色。
