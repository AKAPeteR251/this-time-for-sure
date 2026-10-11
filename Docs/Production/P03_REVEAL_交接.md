# P03-REVEAL 交接

任务：P03-REVEAL。依据：09_F03、10_F03 与最新 11_C09。状态：已实现、Main 实际 Play 检查通过，交付待负责人验收；未标为“已验收”。

## 已实现的流程

召唤按钮 → Signing（不抽取、不扣费）→ 手写松笔或跳过补签 → 唯一 CommitContract → 一次内存 Draw/扣费/入库 → Ceremony → 原序逐张 Revealing → AwaitingConfirm → 最后一次确认后 Summary。

- 手写记录一笔归一化指针轨迹，松笔完成；无需识字或二次确认。空白签名区保留透明网格以接收真实 GraphicRaycaster 命中。
- 签署页只有返回、跳过。跳过读取 `playerDisplayName`，以手写字体横向显露约 0.4 秒，随后自动提交。没有快速签署或补签后确认按钮。返回/Esc 在签署及补签阶段取消，无消耗。
- `pendingSummonCount` 在 Draw 前清零，忙碌状态阻止重复请求；手写与补签共用提交点。签名不参与概率。
- 十连一次计算，仪式只播一次，逐张使用该批 `PrototypePull` 和既定 `IsNew`，没有重新抽取、排序或重写收藏。
- 动画时点击/Space/Enter 只快速完成当前阶段；同帧重复输入被忽略，确认至少隔一帧。完成后留在大图，每张均等待新的确认；第十张/单抽最后一张按钮为“查看结果”。没有自动等待超时或长按翻页。
- 演出跳过/Esc 在仪式、单卡动画、停留三个阶段均清理演出并进入同批全部结果总览。签署页的跳过只补签，不能直接进入总览。
- Signing/Reveal 隐藏常驻导航和资源；总览恢复。新增契约、字幕以及既有顶栏标题使用《星界回响》。神圣宣告收尾准确为“星门，为您而开！”。

## 改动文件及边界

Unity 工程根目录为 `D:/vibe/这次一定出/这次一定出`。

| 文件（工程根目录下） | 改动 |
|---|---|
| Assets/_Game/Scripts/UI/PrototypeGame.cs | 约定范围的用户名/签名字段、签署页与指针入口、唯一提交点、揭晓状态、阶段输入、沉浸显示；顶栏两处内层名称同步；旧复验增加签约前置 |
| Assets/_Game/Scripts/Presentation/ContractSignaturePad.cs + .meta | 一笔轨迹、uGUI 网格绘制、真实指针输入；meta 由 Unity 导入生成 |
| Assets/_Game/Scripts/Presentation/ContractCeremonyView.cs + .meta | 现有星芒、纸张、签名金色重描、缩放/旋转/光柱与定稿字幕的简单仪式；meta 由 Unity 生成 |
| Assets/_Game/Scripts/Presentation/FastRevealView.cs | 大图区域扩大，显示既定 NEW，动画结束清理星芒并保留人物 |
| Assets/_Game/Editor/P03RevealSetup.cs + .meta | 在 Main 的实际 Play 复验菜单与证据导出；meta 由 Unity 生成 |

没有改 Core、UIAdapter、UITheme、Main、UIPrototype、UI Prefab、U02ArtTheme 或 BuildSettings。`P03_REVEAL_Verification/protected_before.json` 与 `protected_after.json` 核对上述 9 项 SHA256 全部一致。没有移动/重命名素材。旧美术/UI/快速检查适配了补签前置，其旧通过报告不代表已经验收本轮逐张确认；本轮用独立 P03 验证。

## 给 U03/P04 的 UI 接口

所有正式 UI 入口都位于现有 `ThisTime.Game.PrototypeGame`，继续引用现有 `PrototypePull` / `PrototypeCard`，不另建模型。

| 入口/状态 | 用法 |
|---|---|
| RequestSummon(int count) | 1 或 10；只进入签署，不立即 Draw |
| CompleteHandwrittenSignature() | 有轨迹且处于 Signing 时统一提交；`SignaturePad.StrokeAccepted` 已绑定，无须另建确认按钮 |
| SkipSigning() | Signing → AutoSigning → 自动提交；只绑定签署页“跳过” |
| CancelSigning() | Signing/AutoSigning 返回；清除未提交签名与请求 |
| AdvancePresentation() | 仪式/单卡动画快速结束当前阶段；AwaitingConfirm 才确认当前张 |
| SkipPresentation() | 演出 → 同批总览；如果从 Signing 调用则转 SkipSigning；AutoSigning 重复调用忽略 |
| HandleEscape() | 按当前阶段取消签署、跳过演出，或沿用详情/图鉴/概率页返回 |
| FlowStage / FlowStageChanged | 状态：Idle、Signing、AutoSigning、Commit、Ceremony、Revealing、AwaitingConfirm、Summary；事件监听可刷新按钮/页面 |
| RevealIndex / CurrentPull | 0 起当前索引与既定结果；仪式索引 -1。总览不应通过它重算结果 |
| PresentationBatch | 同批只读列表外观；元素仍是现有模型，消费者不得修改 Card/IsNew |
| SignaturePad / AutoSignatureText | 当前运行生成的轨迹区/自动姓名 Text；用于 UI 展示和对接，不参与 Draw |
| CommittedSummonRequests | 本次运行正式提交次数，供开发复验；不在游戏页面显示 |
| PlayBatchForDebug(List<PrototypePull>) | Editor/Development Build 专用，复制指定批次，不 Draw/扣费/入库；正式 UI 不暴露此入口 |

U03 可替换 `BuildSigning` 的纸张/字号/安全区及 `BuildReveal` 的计数/下一张/跳过布局，保留这些绑定和状态意义。`ContractSignaturePad` 由 RectTransform 区域决定范围，`penWidth` 为画布单位，归一化点可由 `CopyStroke` 复用于后续仪式。P04 可替换 CeremonyView 与 FastRevealView 的视觉实现，保持“动画结束后等待、同批跳过”的协程逻辑和提交边界。

`signatureFont` 优先使用 Inspector 指定字体。本机实际安装并使用 `STXingkai`（华文行楷），检验昵称“星旅者 Luna”的每个非空字符均有字形。无账号依赖；唯一昵称源是 `playerDisplayName`。若为空，运行回退“召唤者”。正式分发由 U03 接入可分发手写字体资产；本批未复制系统字体文件或宣称其他机器字体已验证。

## 导入、运行与复验

1. 用 Unity 2022.3.53f1c1 打开上述工程，等待脚本导入；打开 `Assets/_Game/Scenes/Main.unity`，按 Play。
2. 点击单抽/十连，在纸中按住拖动后松开，或点击右上跳过。签署页返回/Esc 取消；签约后观察一次仪式和逐张角色大图。
3. 大图动画中点击只完成当前张，再次点击或点击下一张才进入下一张；最后一张点击查看结果。演出右上跳过/Esc 进入整批总览。
4. 自动复验：停止 Play、保存编辑，然后菜单 `Tools/这次一定出/程序/P03-REVEAL/验证契约与逐张确认`。它打开 Main，以独立运行内存测试并退出 Play；不写账号或存档，不保存修改后的场景。存在脏场景/已在 Play 时会拒绝自动覆盖。

## 实测证据

2026-10-08 17:04（北京时间）在本机现有 Unity Editor、Main 场景、图形 Play、1280×720 完成检查，不是静态截图代替运行。

- `P03_REVEAL_Verification/final_result.json`：stage=passed，Unity 2022.3.53f1c1。
- `checks.json`：76 条检查通过。含签名区 GraphicRaycaster 真实命中、PointerDown/Drag/Up 绘制/松笔提交，配置昵称手写字形，签署/补签取消、唯一 Draw，真实十连原序十次停留及第十次确认，动画点击仅当前张、同帧去重，仪式跳过按钮可见且最上层命中，三个演出阶段跳过和 Esc。
- 真实内存单抽+十连：最终资源 19、累计抽取 11、正式提交 2。开发固定批次覆盖 R/SR/SSR/UR、重复角色与既定 NEW，未改变资源或收藏。单抽 UR 自然动画结束后仍等待最后确认。
- `compile_status.json`：Runtime 与 Editor 编译 exit=0。Editor 日志仅有既有 U02 JSON DTO 的 CS0649 字段警告。
- `handwritten_signature.png`：松笔前可见轨迹；`auto_signature_name.png`：补签横向显露中途（可见昵称前半段）；`contract_ceremony.png`：现有星芒仪式、字幕和固定跳过；`last_card_awaiting_confirmation.png`：10/10 停留及查看结果按钮。截图为实际 Play 输出。
- 输入验证由真实 Play 内 EventSystem 射线、ExecuteEvents 指针事件、实际 Button.onClick 与公开输入入口驱动；没有宣称由人工鼠标/键盘完成。实体按键与手感、多窗口比例的人工体验验收留负责人复核。本批不另扩完整测试体系。

初次实测发现缺 CanvasRenderer，后续射线检查发现空白网格问题，均已修复；上列最终报告为修复后重跑通过。旧失败不当作交付证据。

## 限制与下一步依赖

- 本批只做流程与现有素材的简单仪式。没有真实旁白/音轨，也未宣称豪华分镜、声画同步、Blender 空间段已交付；P04 继续完成正式约 5–7 秒视听。当前简单仪式默认 6.6 秒，点击可结束它。
- U03 继续全游戏旧开发说明、详情内部 ID、未完成入口清理，以及正式契约排版/字体分发/各比例安全区。本批未接管旧总览、详情、图鉴布局，不能宣称全游戏清理已完成。
- 缺少角色素材时显示暗色形体；正式现有六卡绑定仍使用原素材。A05 可接纸张、印纹、巨环、碎光。未做账号、OCR、存档、完整规则或大型框架。
- 请求负责人将任务板 P03 状态更新为“已实现，Main Play 76 项通过，待体验验收”；U03 可消费本接口，P04 等待 U03/A05 明确交接后做正式集成。本交付不自行冻结共享契约或改主场景资产。

## 涉及的 Unity 概念与如何调参

uGUI `MaskableGraphic/CanvasRenderer` 绘制笔迹；`RectMask2D` 限制区域与显露姓名；EventSystem 指针事件记录一笔；协程用 `unscaledDeltaTime` 播演出；阶段状态及 `GetKeyDown` 分离“完成动画”和“确认下一张”；只读批次沿用先计算后表现。

选中 Main 的 `PlayablePrototype` 上 `PrototypeGame`：`playerDisplayName` 调昵称、`signatureFont` 接手写字体；`autoSignatureSeconds` 默认 0.4；`ceremonySeconds` 默认 6.6；R=`normalRevealSeconds`、SR=`srRevealSeconds` 默认 1.1、SSR=`ssrRevealSeconds`、UR=`urRevealSeconds`。等待确认不计入这些动画时长。布局在 BuildSigning/BuildReveal 的锚点中调整，笔宽在 SignaturePad.penWidth 调整；不需要改 Draw 或概率。
