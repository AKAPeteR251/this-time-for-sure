# P02-FAST：Main 美术集成与最小揭晓交接

日期：2026-10-08。执行：程序 agent。状态：**待验收（Main 实际 Play 功能复验通过；新增演出视觉与物理输入待补）**。

依据：`08_F02_美术接入任务单.md`、U02-ART 配置交接及 U02-VIS 最新修订。用户已直接授权执行 P02-FAST。沿用内存数据，不引入完整概率规则、存档、音频或新的卡牌模型。

## 本轮改动

- `这次一定出/Assets/_Game/Scenes/Main.unity`：由 Unity Editor 保存。复用既有 PlayablePrototype 上的唯一 PrototypeGame，只更新 theme、portraits、useFixedMock；最新 U02ArtTheme 与六卡/缩略图引用来自实际 UIPrototype Prefab。useFixedMock=false，保留原有 0.9/1.8/2.6 秒和 Main GUID，没有重建主场景。
- `Assets/_Game/Scripts/Presentation/FastRevealView.cs` 与 Editor 生成的 `.meta`：简单结果展示视图。预兆 → 剪影 → 完整卡面与身份 → 光效减弱；SSR 透明冰法师 + 金色星芒，UR 完整火女战士卡图 + 更强双星芒（反向旋转）、更暗预兆与较长节奏。星芒位于人物后方。R/SR 使用较短节奏与蓝色星芒，无图角色保留文字。
- `Assets/_Game/Scripts/UI/PrototypeGame.cs`：按 U02 明确交接仅改揭晓字段及 BuildReveal、Reveal、SkipReveal。保留最新字体、三页布局、图鉴、隐藏/恢复与适配器。结果先由既有 Draw 计算，演出只消费 lastResults；加速沿用 4 倍速度，跳过停止协程、清理星芒并展示同一批结果。重复跳过在空闲时直接返回。
- `Assets/_Game/Editor/P02FastSetup.cs` 与 Editor 生成的 `.meta`：可重跑 Main 配置、打开入口、Play 复验和 Editor 高级卡预览菜单。配置仅消费已保存 U02 引用，不重新导入或重写 UI 资产。
- `Docs/Production/P02_FAST_Verification/`：独立编译/运行/变更核对证据及首次尝试日志，不覆盖 U02 的旧报告。

## 如何打开与调参

1. 从 Unity Hub 正常打开工程 `D:/vibe/这次一定出/这次一定出`，等待导入完成。
2. 菜单 `Tools → 这次一定出 → 程序 → P02-FAST → 打开 Main`，按 Play，使用真实内存抽卡。
3. 需要重新同步六卡时先停止 Play、保存编辑，再用同组菜单的“接入 Main 六卡配置”。工具拒绝覆盖有未保存修改的 Main；已有入口必须唯一。
4. Play 中可用同组“Play 中预览 SSR / UR”，体验高级揭晓，不消耗资源或入库。主界面不增加调试按钮。
5. 自动复验菜单为“验证 Main 与高级揭晓”。普通 Editor 会执行 Main 的实际按钮流程、三种窗口截图和高级揭晓；输出在本轮目录。截图路径只有文件真实产生才算证据。

Inspector 中选 Main 的 PlayablePrototype，Normal/Ssr/Ur Reveal Seconds 调揭晓时长，当前分别 0.9/1.8/2.6 秒。FastRevealView.Animate 中调星芒透明度、转速、0.44–0.68 的角色出现区间和 0.70 后的收束。完整图保持 Preserve Aspect；SSR001 单独消费 theme.summonCharacter，UR001 不伪装成透明分层。主题与六卡引用继续在 U02 的资产/Prefab 中维护。

## 实际验证

最终运行于 Unity **2022.3.53f1c1**，北京时间 **2026-10-08 11:31:44**，从 `Assets/_Game/Scenes/Main.unity` 实际进入 Play。使用 Unity 官方 batchmode，运行实际 MonoBehaviour、Coroutine、Button.onClick 和 MemoryPrototypeUIAdapter；不是纯 C# 模拟、固定 mock 或截图推断。

[最终报告](P02_FAST_Verification/final_result.json)：

```text
PASS: Main真实内存闭环; SSR透明人物/自动完成; UR双星芒/完整卡图/加速; 忙碌重入/重复跳过/协程与光效清理; 预览不扣费不入库
```

- [Main 流程结果](P02_FAST_Verification/main_flow_result.txt)：初始30 → 实际单抽29 → 实际十连19 → 跳过同一结果 → 十张结果 → 点卡详情 → 隐藏/恢复 → 收藏 → 返回 → 补充29；总抽数11。Canvas/EventSystem 均唯一，批内重复角色不会重复标 NEW。
- [演出指标](P02_FAST_Verification/presentation_metrics.json)：SSR 自动完成；UR 使用正确完整卡图且额外星芒启用；实际加速按钮触发后剩余时间约 **0.519 秒**；忙碌时再次召唤不替换结果；重复跳过后协程结束、星芒 alpha 清零。所有高级预览前后资源、总抽数和各角色拥有数一致。
- [阶段采样](P02_FAST_Verification/phase_checks.txt)：SSR/UR 预兆、揭晓的实际 Play 状态采样，明确标记 batchmode 未截屏，不充当视觉图片。
- [重复集成](P02_FAST_Verification/integration_repeat.json)：连续两次配置 Main，场景文件字节相同。
- [变更审计](P02_FAST_Verification/change_audit.json)：UI 非揭晓源码不变；Core、适配器、UI主题源码、U02ArtTheme、UIPrototype 场景/Prefab、构建列表、Main.meta 的前后哈希一致；A04 的36张 PNG仍与 manifest 全部匹配。
- [编译记录](P02_FAST_Verification/compile_status.json)：当前运行与 Editor 源码编译退出码均0。独立编译的少量 CS0649 属于既有 U02 JsonUtility 清单字段，没有新增编译错误。Unity 最终导入及 Play 另由 `Unity_P02_final_play.log` 证明。

本轮自动验证不会写持久化存档；停止 Play 后内存重置。程序验证 DLL 仅留在文档证据目录，不导入 Assets。

## 复验命令

没有其他 Editor 打开此工程时，可用以下命令复验；不要加 `-quit`，工具会在 Play 验证结束后自行退出批处理 Editor：

```powershell
& 'E:/Unity/2022.3.53f1c1/Editor/Unity.exe' -batchmode -projectPath 'D:/vibe/这次一定出/这次一定出' -executeMethod P02FastSetup.RunFromCommandLine -logFile 'D:/vibe/这次一定出/Docs/Production/P02_FAST_Verification/Unity_P02_recheck.log'
```

已有正常 Editor 则用菜单，或向本轮目录的 `P02.request` 写入 `verify`；不要同时启动第二个 Editor。菜单复验从新 Play 状态开始，先保存当前场景。

## 限制与下一步依赖

**本轮没有新的演出截图、三窗口视觉或物理鼠标/Esc 的通过证据。** Batchmode 无 GameView 截屏，最终复验工具明确跳过截图，不使用 U02 图片冒充本轮效果。按钮事件与状态链已实测，手感、脸部遮挡、星芒强度和物理输入还需普通权限 Editor 试玩。因此总任务保持待验收。

常规终端/Node 的 Windows 沙箱初始化故障延续；文件操作和本地编译通过自动审批的命令完成。computer-use 初始化连续失败，无法观察/点击窗口。图形 Editor 启动还出现管理员运行提示；首次复验没有产生帧结果而超时，日志保留。最终选择官方 batchmode 完成实际 Main Play，不接受管理员警告、不修改系统权限。早期 batch 截图尝试失败日志也保留，`final_result.json` 与 `Unity_P02_final_play.log` 才是最终功能依据；不要把累积 `errors.log` 当最终单次结论。

十连仍消费同批最高稀有度的一次短揭晓，然后进入总览，沿用 F01 的快速方式；未制作逐张高级演出、升格、共鸣、完整分层或音效。六张临时职业图及其余六张占位沿用既定映射。

下一步由总指挥/用户从普通权限 Unity 试玩 Main，补新增演出视觉与物理输入，按实际体验调时长/光效，再决定 F03。任务板建议：P02-FAST 改为“待验收；Main 已由 Editor 集成，实际 batchmode Play 闭环及高级演出状态检查通过；新增视觉/物理输入待补”。本轮未改共享任务板。

## Unity 学习点

EditorSceneManager 保存实际场景与 GUID 引用；Sprite 是只读展示资源。Coroutine 按已计算结果控制时序，停止协程不重新抽卡；清理视图避免跳过后的残余光效。Canvas 的层级让星芒位于人物后方，Preserve Aspect 保持完整图比例。batchmode 适合真实逻辑/组件状态复验，不能替代 GameView 截图和人工输入验收。
批处理通过并开始退出后，进程仍停留在关闭阶段；已仅结束本轮明确命令行的批处理进程，释放工程锁，记录见 P02_FAST_Verification/editor_cleanup.json。没有结束用户 Editor。
