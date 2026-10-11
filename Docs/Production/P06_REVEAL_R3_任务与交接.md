# P06-REVEAL-R3 分层星空揭晓

执行范围：按用户提供的 F05/RevealR3/manifest.md 接入15项素材，Main使用深邃星空、双层慢移星云、闪烁恒星、偶发流星、简化雪花、R/SR/SSR/UR字图和NEW动态渐变边框。保留已有签约、唯一Draw、空白点击和每张/末张等待。只改表现/UI消费和明确管理的Main配置；不改Core、概率、存档或角色模型。

任务状态：已集成 Main，编译与实际 Play 检查通过；待用户视觉验收。素材规格检查不替代运行验收，本单不将新视觉标为已验收。

## 实际交付

- 15项PNG原样复制至 `这次一定出/Assets/_Game/Art/Effects/F05/RevealR3/`，Unity正常生成meta。没有导入拼合预览、质检图或raw_generated；没有覆盖F04旧素材。
- 3840×2160星空底图按比例裁切适配；两层透明星云使用Repeat和RawImage UV缓慢反向漂移；28颗独立恒星错峰闪烁，流星用单张尾迹移动/淡入淡出。角色等待确认时仍持续运行。
- SSR001使用本批简化大雪花，透明度0.32，已有立绘与原1.9倍近景缩回保留。碎裂后的向外碎屑使用本批独立冰屑，而非静态背景烘焙。
- R蓝/SR紫/SSR金/UR炫彩使用本张真实稀有度对应Sprite。统一画布/基线，不拉伸字宽；独立线条星形按稀有度调色。UR当前显示已生成的炫彩渐变字图并淡显。
- NEW文字稳定浅金色，白色角框遮罩通过uGUI纹理alpha与动态顶点渐变着色，青→紫→粉→金流动，另有四芒亮点沿边移动。修正了首轮自定义Graphic的纹理绑定缺失，最终版本继承Image，实际截图中边框可见。非NEW禁用文字、边框和亮点。
- 保持仅名称、无称号/张数/下一张按钮；右侧奖励区留白。角色动画中的点击只补齐当前张，下一次空白点击才进入下一张，末张等待后进入总览。演出跳过/Esc仍回同批；签署跳过仍只自动补签后继续。

## 改动与接口

Unity工程相对路径：

- `Scripts/Presentation/P04FeelConfig.cs`：新增分层纹理、四档字图、NEW图及动态参数。
- `Scripts/Presentation/RevealSky.cs`：星空比例适配、双层UV、恒星、流星、冰屑；只在所在角色舞台活跃时Update。使用局部确定性分布，不消费抽卡随机数。
- `Scripts/Presentation/RevealGradientBorder.cs`：基于Image的alpha遮罩与动态彩色网格，不新建大型渲染框架。
- `Scripts/Presentation/FastRevealView.cs`：消费15项资源、替换旧徽章/NEW框、控制各层淡显与新鲜确认。
- `Editor/P06RevealR3Setup.cs`：独立、可重复的15项导入与Main配置接入、Play验证/录制入口。不会重绑现有角色或音频。
- `Data/Presentation/P05FeelConfig.asset`、`Scenes/Main.unity`：由Editor保存实际引用。Main GUID未变。

沿用 `FastRevealView.Begin/Animate/Finish/Transition/TickIdle` 与 PrototypeGame 现有输入/批次接口。新增只读 `RevealSky.CloudOffset/FirstStarAlpha/MeteorAlpha` 和 `RevealGradientBorder.Phase` 供Editor观察。未改PrototypeGame本轮代码、Core、概率、存档、模型、UIAdapter或UITheme。旧R2图引用保留以兼容旧配置，但Main使用新的四档/NEW字段，不再显示旧宝石徽章。

## 实测证据

报告目录：`Docs/Production/P06_REVEAL_R3_Verification/`。

- 最终 `final_result.json`：2026-10-10 18:23（北京时间）PASS。现有图形Unity Editor的Main实际Play，不是重建静态预览。程序工具结束测试后已退出Play，Main配置保留。
- `checks.json`：等待4秒期间两层UV持续变化、恒星亮度变化、NEW渐变相位变化，流星实际出现；首张等待/动画补完不穿透/末张等待通过；四档字图与对应Card.Rarity逐一比对；三尺寸安全区及纯净文案通过。
- 验证输入使用实际GraphicRaycaster首命中及ExecuteEvents down/up/click，Esc使用现有HandleEscape接口；不宣称已做OS物理键鼠测试。
- 开发固定批次只验证表现，不改抽卡结果。另验证真实随机单抽和十抽：资源30→29→19，TotalPulls=11，CommittedSummonRequests=2，演出跳过保持同批。
- `1280x720_ssr_wait.png`、`1280x720_ssr_dynamic_wait.png`、`1920x1080_ssr_wait.png`、`1600x1000_ssr_wait.png`；另有四档R/SR/SSR/UR、黑剪影、姓名先于稀有度、缺立绘、单/十抽总览截图。
- `main_r3_layered_reveal.mp4`：11.257秒，1280×720，254张真实画面、平均22.56帧/秒；同场AudioListener 48kHz双声道声音，峰值0.2692，无削波。包含流星、等待中的漂移/闪烁/边框和新点击过程。原帧时间戳与Unity PCM在capture子目录；`clip_manifest.json`有SHA256与编码信息。
- `compile_status.json`：runtime/editor均exitCode=0，实际Unity导入及Play通过。原有U02未赋值DTO警告未扩展处理。
- `asset_import_check.json`：15项与ArtSource源文件SHA256一致，meta全存在。`protected_check.json`：Core/UIAdapter/UITheme/UIPrototype scene/prefab/Main.meta/构建配置七文件均未变。

## 打开与调参

直接打开 `这次一定出/Assets/_Game/Scenes/Main.unity` 并Play。Editor菜单 `Tools/这次一定出/程序/P06-REVEAL-R3/接入素材声音并保存Main` 只更新本轮明确管理的纹理/配置；名称沿用既有工具风格，实际不会重导音频。验证菜单可生成新证据。也可写报告目录中的 `RevealR3.request` 为 `prepare` 或 `verify`。不要运行旧R2准备入口来重绑旧雪花。

在 `P05FeelConfig.asset` Inspector：

| 参数 | 当前值 | 用途 |
|---|---|---|
| nebulaFarSpeed / nebulaNearSpeed | 0.006 / -0.010 UV/s | 远/近星云方向与速度，负数反向 |
| nebulaFarAlpha / nebulaNearAlpha | 0.45 / 0.25 | 星云额外透明度，降低可让星空更深 |
| revealStarCount | 28 | 星体数量，8—64 |
| meteorInterval / meteorSeconds | 7 / 1.5秒 | 流星间隔与划过时长，首颗在角色舞台约1.8秒出现 |
| iceBackdropAlpha | 0.32 | 雪花强度 |
| newGradientSpeed | 0.12循环/秒 | 边框彩色流动与沿边亮点速度 |

图层顺序：星空→远星云→近星云→闪烁星/流星/碎屑→雪花→角色→名字/稀有度/NEW→页面跳过。NEW框锚点(.075,.595)—(.19,.67)，稀有度槽(.05,.16)—(.32,.40)，人物沿用(.26,.10)—(.81,.96)。布局调FastRevealView锚点；等待期间动态由活跃的RevealSky/RevealGradientBorder组件驱动，不依赖确认按钮或演出协程继续计时。

涉及Unity概念：TextureImporter的Repeat/Clamp、RawImage.uvRect、RectTransform锚点、Image.preserveAspect、Sprite alpha、Canvas网格顶点颜色与UI层级；这些只改变表现，不参与Draw和收藏。

## 限制及下一步

完整透明角色首秀仍是冰法师；其他职业沿用现有带底卡面或缺图剪影，UR截图可看到原卡面底色。本轮只验证四档标识，不重新抠人物或扩角色素材。UR字图当前是静态炫彩，NEW边框才持续流动。星云为纹理平移，不是体积渲染；流星当前沿固定斜向路径周期出现，可后续调轨迹。VITS日语双女性声音仍按用户要求延期；本片沿用已有中文音效/旁白。水晶投射光束/破裂观感修正未混入本批。最终视觉、星云明暗与字图大小待用户验收。
