# U03-CLEAN 交接

日期：2026-10-08。状态：**独立 UI 场景实测通过；Main 集成和用户视觉验收待验收**。

依据：09/10/11、UI Agent 提示词、共享契约、P03_REVEAL 交接。仅修改 UI 归属，没有改概率、扣费/收藏计算、存档、Presentation 源文件或 Main。

## 交付与改动文件

Unity 根目录：`D:/vibe/这次一定出/这次一定出`。

| 文件 | 改动 |
|---|---|
| `Assets/_Game/Scripts/UI/PrototypeGame.cs` | 契约签名区、手写字体加载、进度和确认控件、纯净玩家文案、缺立绘显示；消费既有 P03 接口 |
| `Assets/_Game/Scripts/UI/UICharacterSilhouette.cs` 和 meta | 共用中性 uGUI 轮廓，保留真实名字与稀有度 |
| `Assets/_Game/UI/Editor/U03CleanSetup.cs` 和 meta | 独立 UI 准备、真实 Play 输入验收、三尺寸截图 |
| `Assets/_Game/UI/Resources/Fonts/` 及 meta | 原始 TTF、完整 OFL 许可、来源说明 |
| `Assets/_Game/UI/Prefabs/UIPrototype.prefab` | Editor 显式绑定签名字体，保留 GUID、主题和六卡，真实内存入口 |
| `Assets/_Game/Scenes/UIPrototype.unity` | Editor 保存独立 UI 场景，GUID 不变 |
| `Docs/Production/U03_CLEAN_Verification/` | 编译、运行、字体/保护文件哈希、46 张实际截图 |

契约采用暗蓝底、象牙白纸和黄铜细边，品牌/标题分层；中央下部扩大签名区，仅提示“在此签名”。手写松笔消费 `CompleteHandwrittenSignature`；签署跳过消费 `SkipSigning`，以配置昵称约0.4秒横向显露后自动继续。没有快速签署或补签后确认按钮。

演出跳过固定右上；进度左下，仪式阶段隐藏；下一张/查看结果统一右下，动画中隐藏确认按钮。底部暗蓝信息带提高文字对比。使用 `AdvancePresentation` / `SkipPresentation`，不重新计算结果。

两处页头在接手时已由 P03 改为“星界回响”，本轮核对两分支并扫描所有可见文本。契约/结果/演出不含外层名或标点变体；宣告收尾保持“星门，为您而开！”。

## 字体与昵称

签名字体 **霞鹜文楷 / LXGW WenKai Regular，v1.522**，实际 Unity 家族 `LXGW WenKai`，约24.4MiB，字体未修改。[作者发布页](https://github.com/lxgw/LxgwWenKai/releases/tag/v1.522)，[SIL OFL 1.1 许可](https://github.com/lxgw/LxgwWenKai/blob/main/OFL.txt)。完整版权/许可在字体目录 `OFL.txt`，发包时随游戏附上。哈希在 `font_hashes.json`。

UIPrototype 显式引用 Font；`signatureFont` 留空时从 `Resources/Fonts/LXGWWenKai-Regular` 加载，所以 Main 旧组件也能消费内置字体。契约和仪式用户名使用同一字体，仪式长名字自动适配。正文和标题沿用现有系统宋体配置，没有复制 Windows 字体。

| 配置昵称 | 实际检查 |
|---|---|
| `星旅者 Luna` | 读取一致，三尺寸补签截图完整显示，自动继续仪式 |
| `Alice` | 实际 Text 读取一致、逐字 HasCharacter 通过 |
| `小满·Echo 07` | 中文/英文/中点/数字逐字覆盖通过、读取一致 |
| `召唤者` | 默认名字逐字覆盖通过 |
| 空白字符串 | 回落“召唤者” |

昵称来自唯一字段 `PrototypeGame.playerDisplayName`，不取 Windows 登录名。全部名字在 Play 中检查真实 Font 引用和字形；只有“星旅者 Luna”交三尺寸昵称截图，其他名字交读取/字形证据。没有声称所有 Unicode 昵称已验证。

## 实际输入与验证

最终运行：Unity `2022.3.53f1c1`，`UIPrototype.unity`，2026-10-08 17:48:45（UTC+8）。Runtime / Editor 编译退出码均为0；`final_result.json` 为 PASS。46张截图、1523条记录包含重复文案/边界扫描，并非1523个独立用例。验证后 Play 已退出，UI场景无脏状态。

输入为**实际 Play 中的 EventSystem / GraphicRaycaster / ExecuteEvents 指针事件**；未声称实体鼠标或OS键盘。按钮先检查射线第一命中，再发 down/up/click。手写发 down、20次drag、up，实际生成墨迹。

| 输入流程 | 实测结果 |
|---|---|
| 点击召唤 → 签署 | 不扣费、不 Draw，隐藏导航资源 |
| 手写拖动 → 松笔 | 松笔前不扣费；松笔只提交一次单抽，30→29 |
| 点击签署跳过 | 开始配置昵称显露，不直达总览；无需确认，自动提交十连，29→19 |
| 真实十连逐张 | 原批次原序；动画点击只完成当前张；十张均等待，第十张“查看结果”，确认后总览；NEW/收藏保持 |
| 仪式/动画/等待跳过 | 都进入同一个完整已确定批次，无重新抽取 |
| 签署返回/补签中取消 | 不扣资源，补签取消通过 P03 CancelSigning 接口验证 |
| 总览/详情/隐藏/恢复/图鉴 | 实际按钮指针事件通过，详情无内部ID |
| 资源0 / +10 | 单抽十连禁用；点击+10恢复到10并开放十连，无开发解释弹窗 |
| 无美术分支 | 召唤/规则/总览无旧开发说明，无玩家预览入口 |

真实内存用于单抽/十连/资源闭环。四档、重复和缺立绘对照图使用 `PlayBatchForDebug` 固定批次，不抽取、不改收藏，没有开放玩家调试按钮。最终资源10、总抽数30，全部仅独立Play内存。

截图位置：`D:/vibe/这次一定出/Docs/Production/U03_CLEAN_Verification/`。三个前缀 `1280x720_`、`1920x1080_`、`1600x1000_` 均有 summon/rules/signing/handwritten/auto_signature/ceremony_signature/first_card/last_card/summary/detail/artwork/collection。另有720p真实批次总览、16:10四档和缺立绘等待图、资源不足、无美术分支。

最新图已检查昵称完整、剪影出现、首末张按钮、底部文字与人物分离。早期调试发现缺CanvasRenderer及文字对比问题已修复，验收以最新final_result与截图为准。

## 文案清理对照

| 原内容 | 现在 |
|---|---|
| UI MOCK/固定数据、常驻角色池·本轮收藏 | 常驻角色池 |
| 保底/共鸣/赠礼尚未接入、基础随机·尚未接入 | 删除 |
| 玩家预览SSR/UR入口 | 隐藏，指定批次仅用于Editor验收 |
| 高级预览不消耗/不入收藏、角色预览、校准标题 | 正常结果标题，开发信息留报告 |
| 本轮收藏、本轮获得 | 图鉴·收录数量/角色数 |
| 普通版·本轮获得、本次运行内收藏 | 拥有n张、已收录/未收录 |
| 详情内部ID、共鸣/桌面展示尚未接入 | 删除 |
| 角色立绘占位、立绘待补充 | 共用中性剪影，保留真实角色信息 |
| 概率页原型/存档限制/未接入/预览说明 | 实际R70%/SR24%/SSR5%/UR1%、同档等概率、消耗1/10、补充+10 |

概率文案对照现有 PrototypeGacha.Draw，计算代码未改。旧 adapter.RulesDescription 不再供玩家弹层展示，未改共享接口；规则变化时需同步此UI文案。

## Main 与后续交接

Core、三个Presentation文件及meta、Main共9项哈希全部不变，见protected_before/after.json。Main本轮未保存、未做新的集成验收。

P04在Main的PlayablePrototype / PrototypeGame核对并通过Editor保存：主题继续U02ArtTheme、六卡保持、useFixedMock=false；signatureFont可显式引用上述TTF，留空自动加载；playerDisplayName为配置游戏昵称（默认召唤者）；autoSignatureSeconds=0.4。保留P03分阶段签约/跳过/确认，不加第二个签署确认入口。Main与键盘/音画片段由P04/C08复验。

本轮未交旁白、声音、星环、透明火女战士，也不声称概念图完全还原；这些属于A05/P04和用户视觉验收。

## A05/P04安全区与调参

页面左下原点归一化，参考1920×1080，CanvasScaler匹配0.5：

| 元素 | 区域 |
|---|---|
| 契约纸边 | x0.205—0.795，y0.085—0.91，无烘焙中文 |
| 纸内品牌/标题/正文 | 品牌y0.86—0.95，标题y0.74—0.86；正文x0.06—0.94、y0.49—0.70 |
| 纸内签名 | x0.10—0.90、y0.16—0.41，花纹避开动态笔迹/昵称 |
| 签署返回/跳过 | 左x0.03—0.14 / 右x0.86—0.97，y0.90—0.97 |
| 演出人物 | x0.27—0.73，y0.15—0.93，保持等比 |
| 身份信息/进度 | 身份x0.19—0.77、y0.04—0.145；进度x0.04—0.15、y0.045—0.11 |
| 下一张/查看结果 | x0.80—0.96、y0.035—0.115，右上始终留跳过 |

P04替换演出时保留或同步当前UI适配节点RevealIdentity、RevealArtworkSlot/UnknownSilhouette、RisingContract/SignatureName，不让环印遮挡跳过。

复验菜单：`Tools/这次一定出/UI/U03-CLEAN/验证签约输入与三种窗口`，停止Play后执行。工具只运行独立UI场景；U03.request支持prepare/verify/status/stop，stop仅作用于UIPrototype。

涉及Unity概念：CanvasScaler和锚点适配；RectMask2D横向显露；动态Font字形；MaskableGraphic+CanvasRenderer绘剪影；GraphicRaycaster与指针事件验证可点目标。装饰关闭Raycast Target，输入控件保留。

调参：昵称、签名字体、补签时长在PrototypeGame Inspector；签名线宽为SignaturePad.penWidth；纸/控件锚点在BuildSigning/BuildReveal；色彩沿用theme。更换字体后复验混合昵称与三尺寸，显示参数不改Draw/存档。
