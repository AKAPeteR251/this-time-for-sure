# U02-ART · 三页真实美术接入交接

日期：2026-10-08。执行：UI agent。状态：**待验收（独立场景真实内存 Play 已通过，待总指挥确认与 P02-FAST 集成）**。

依据：`08_F02_美术接入任务单.md`、P01-ART 唯一六卡映射、A04 manifest、C06 美术检查。沿用 F01/U01；未重写抽卡、收藏、概率或存档。

## 已完成的界面

- 召唤：冰城背景与透明冰法师独立叠放；产品标题“这次一定出”；资源、免费 +10、单抽/十连、收藏、概率说明和高级预览仍为可点击控件。
- 总览：五列两行，读取本次实际随机结果及 IsNew。六条映射各用唯一图片，其余六角色明确显示“立绘待补充”。未照搬参考 PNG 的出货序列。
- 详情：SSR001 使用透明人物，其他角色使用完整卡图；读取角色名、称号、等级和本轮拥有数。支持返回、隐藏和恢复；没有日期字段，省略收录日期。
- 收藏：统一使用六卡缩略图和四档框；未收录不开放详情，不将预览计入收藏。
- 按钮：normal v002 固定底图，十连使用 selected v002 固定金底；Color Tint 提供悬停、按下与禁用，未使用四态 SpriteSwap。
- 装饰 Image 关闭 Raycast Target。一个 Canvas、一个 EventSystem；卡牌背景/按钮才接收射线。
- 独立场景与入口 Prefab 的 `useFixedMock=false`，直接使用原 MemoryPrototypeUIAdapter；没有新业务模型。

## 文件与资产

Unity 工程内路径：

| 文件 | 用途 |
|---|---|
| `Assets/_Game/Scripts/UI/PrototypeGame.cs` | 三页美术布局、通用卡面、六图/缩略图、按钮样式、真实内存 UI 复验 |
| `Assets/_Game/Scripts/UI/PrototypeUITheme.cs` | 背景、人物、按钮、框、面板、图标/星芒引用及每档内窗 |
| `Assets/_Game/UI/Editor/U02ArtSetup.cs` | 清单导入、配置、打开与复验工具；不写 Main |
| `Assets/_Game/UI/Styles/U02ArtTheme.asset` | Editor 生成的本轮主题 |
| `Assets/_Game/UI/Prefabs/UIPrototype.prefab` | 保留原 GUID，绑定六卡、缩略图、新主题，关闭 mock |
| `Assets/_Game/Scenes/UIPrototype.unity` | 独立真实内存运行入口 |
| A04 清单中的 36 个 PNG 的 `.meta` | Editor 生成/更新 Sprite 导入设置，保留已有 GUID |

没有覆盖 PNG 或 ArtSource。36 个 PNG SHA256 全部匹配 A04 清单；Main.unity、PrototypeGacha.cs、EditorBuildSettings.asset 的前后哈希一致。

## 运行与复验

1. Unity 等待脚本导入。首次重新配置可用 `Tools → 这次一定出 → UI → U02-ART → 导入并配置六卡美术`。
2. `Tools → 这次一定出 → UI → U02-ART → 打开真实美术原型`，按 Play。初始 30，可随机单抽和十连；本轮数据停止 Play 后重置。
3. `Tools → 这次一定出 → UI → U02-ART → 验证真实内存闭环与三种窗口`，自动走实际 Button 绑定，结束退出 Play 并恢复此前 GameView 尺寸。
4. 验证菜单应从新 Play 状态开始；如场景有未保存编辑，先保存，再运行。配置工具只保存 UIPrototype，不保存当前 Main。

实际运行于 Unity **2022.3.53f1c1**。最终复验北京时间 **2026-10-08 10:17** 返回：

```text
PASS: 真实内存30→29→19→29; 单抽/十连/跳过固定结果/NEW/详情/隐藏恢复/收藏/SSR与UR预览/六卡图/三种窗口/字体
```

验证包含：初始30、单抽29、十连19、跳过前后同一结果批次且抽数不增加、十张结果、点卡详情、隐藏/恢复、收藏、返回、补充29；SSR 自动结束揭晓、UR 跳过预览均不入库。三尺寸截图复用同一随机十连结果，没有为好看替换成预设出货。

六卡校准图是明确标注的视觉预览，不计入随机抽卡或收藏，不作为随机出货证据。自动检查可用按钮在屏幕内、当前显示文字没有缺字、Canvas/EventSystem 唯一。

证据均在 `Docs/Production/U02_ART_Verification/`：

- [最终 Play 报告](U02_ART_Verification/final_result.json)；[导入 36 项及 GUID](U02_ART_Verification/import_report.json)；[当前运行及 Editor 源码编译](U02_ART_Verification/compile_status.json)，退出码均 0。
- [720p 召唤](U02_ART_Verification/1280x720_summon.png)、[720p 实际随机十连](U02_ART_Verification/1280x720_summary_random.png)、[720p 冰法师详情](U02_ART_Verification/1280x720_detail_SSR001.png)。
- [1080p 六卡绑定校准](U02_ART_Verification/1920x1080_six_card_bindings.png)、[16:10 火女战士详情](U02_ART_Verification/1600x1000_detail_UR001.png)、[16:10 隐藏欣赏](U02_ART_Verification/1600x1000_artwork_SSR001.png)。每个尺寸另有规则、收藏和另外两页截图。
- [PNG 未改动审计](U02_ART_Verification/png_unchanged.json)；`protected_before.json / protected_after.json` 核对 Core、Main、构建配置。

窗口控制能力：常规终端及 Node 因 Windows 沙箱初始化 `helper_unknown_error: setup refresh had errors` 无法启动。项目文件读写和编译使用自动审批通过的本地命令；Editor 通过项目内已有请求轮询完成导入和 Play。未使用自制鼠标/键盘控制协议，没有向外部识图网关发送素材。

**物理鼠标点击、悬停和 Esc 键尚未实测。** 自动测试执行真实 Button.onClick 与页面处理函数，可以证明绑定与状态链，不能替代物理输入验收；总指挥/用户试玩时补这项。不将旧 U01 截图当作新美术证据。

## 卡窗与按钮调参

Sprite 导入：Single、FullRect、Bilinear、Clamp、无 Mipmap、无压缩、maxSize=2048；RGBA 开启 Alpha Is Transparency。卡框 Simple，不做九宫格；人物/完整卡图在内窗中按 2:3 适配，未拉伸。

内窗字段为左下原点 `(x, y, width, height)`，对应本轮实际截图：

| 主题字段 | 外画布 | 当前内窗 |
|---|---|---|
| windowR | 528×736 | (0.12, 0.14, 0.76, 0.74) |
| windowSR | 528×736 | (0.12, 0.14, 0.76, 0.74) |
| windowSSR | 528×832 | (0.15, 0.18, 0.70, 0.65) |
| windowUR | 527×832 | (0.15, 0.18, 0.70, 0.65) |

四档分开引用与调参；SSR/UR 当前归一化值相同，但各自按实际画布计算，未套用 R/SR 遮罩。卡图再通过独立 Slot + AspectRatioFitter 放入内窗；脸位于上半部，名称位于底部条，徽标放顶部左右侧。UR 使用完整卡图，不冒充透明分层人物。

按钮导入 Border=(110,70,110,70)；实际使用时从同一 PNG 创建运行时 Sprite，normal 可见主体 Rect=(20,31,792,210)、selected Rect=(20,35,792,210)，左下原点；有效九宫格 Border=(90,40,90,40)。这会去除透明留白，不覆盖原图，也不切换四态。截图已检查文字居中、边角和光晕；物理反馈仍待试玩。

信息面板 Border=(65,45,65,45)，用于顶部资源条；Tooltip 面板已导入并保留引用，本轮规则弹层继续使用纯色可读底，不纵向拉长横条面板。星芒已导入并放主题引用，动画交给 P02-FAST。

## 交给 P02-FAST 的 Main 配置

机器可读配置：[main_binding_config.json](U02_ART_Verification/main_binding_config.json)，从 Editor 实际保存的 Prefab 引用生成，不猜 GUID。

- 复用 Main 原有唯一 PrototypeGame。Theme 绑定 `Assets/_Game/UI/Styles/U02ArtTheme.asset`，`Use Fixed Mock=false`。
- 只更新 Theme/Portraits/Use Fixed Mock 三组字段，不新增第二个入口对象。可从 UIPrototype Prefab 复制这三组，或调用已交付的 Editor 方法 `U02ArtSetup.Configure(mainUI, theme)`，由程序按任务授权保存 Main。
- 六条绑定目录均为 `Assets/_Game/Art/Characters/UIMockRoles/`：

| ID | 完整卡图 | 缩略图 |
|---|---|---|
| R001 | 06_star_magical_girl_v001.png | 06_star_magical_girl_v001_thumb.png |
| SR001 | 02_light_priest_v001.png | 02_light_priest_v001_thumb.png |
| SR002 | 03_wind_elf_archer_v002.png | 03_wind_elf_archer_v002_thumb.png |
| SR003 | 05_thunder_miko_v001.png | 05_thunder_miko_v001_thumb.png |
| SSR001 | 01_ice_mage_v001.png | 01_ice_mage_v001_thumb.png |
| UR001 | 04_fire_warrior_v001.png | 04_fire_warrior_v001_thumb.png |

角色文字和等级继续读取 P01-ART 数据。其余 R002/R003/R004/R005/SR004/SSR002 留作占位。旧 Characters/SSR001、Characters/UR001 图不绑定。

演出部分交接：本轮保留 `PrototypeGame.BuildReveal / Reveal / SkipReveal` 既有逻辑及 0.9/1.8/2.6 秒，不加星芒演出或新抽卡判定。U02 本轮 UI 修改已结束；P02-FAST 可按任务单接管这些方法，使用 Theme.sparkleBlue/Gold 和已有结果调 SSR/UR 节奏。跳过仍应停止协程后展示同一 lastResults；不能调用 Draw。

任务板建议：将 U02-ART 行改为“待验收；独立真实内存 Play PASS，三种窗口新截图及 Main 六卡配置已交付，物理鼠标/Esc 待补”；P02-FAST 等总指挥确认后派工。本轮未修改共享任务板。

## 学习点与范围

- Sprite 引用和 GUID 连接稳定角色 ID 与图片；完整图用于详情，缩略图用于卡格。
- RectTransform Slot 保存布局区域，AspectRatioFitter 只调整内部图片；直接给指定锚点的对象加 Fitter 会接管锚点，已在资源图标问题中修正。
- Image Sliced + Border 保留按钮边角；Color Tint 给同一底图反馈。
- CanvasScaler 参考 1920×1080，Match=0.5；主题资产调颜色/内窗，Portraits 调图片，Inspector 的 Reveal Seconds 调时长。
- 当前仍为内存原型，无保底/赠礼/共鸣/存档/时间/完整分层视差。已有素材的小图标光晕、道具裁切和手部缺陷保留原稿，正式稿修复属于后续美术任务。

下一步：总指挥确认独立入口与物理输入 → P02-FAST 将本配置接到 Main，调最小高级揭晓并重新验证主场景。
