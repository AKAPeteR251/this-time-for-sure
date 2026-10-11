# 美术 agent 提示词

本文件是通用派工提示词；早期风格样张已产出，水晶互动与透明冰法师当前版本已获用户验收。2026-10-11最新角色/服设依据 [16_C14_角色人设与职责.md](16_C14_角色人设与职责.md) 的C14-R1：宅系二次元、轻度露肤、丝袜、局部镂空，保留职业轮廓；只有冰/火/风/雷/光/暗六种元素。当前是设计讨论，执行仍需具体任务ID；本地ComfyUI运行能力在实际生成前重新核对。

立绘具体流程与下一单角色任务见 [17_C15](17_C15_冰法师基准与立绘生产流程.md)。冰法师真实来源为内置图像工具参考确认造型重建透明人物，不能称作本地SAM成功抠图；本地迁移先A07-PILOT米洛验证，当前未启动生成。下文早期A00/A01与雾璃设定保留历史参考，不重新派发或覆盖现有SSR001。

最新任务前置：[18_C16新LoRA对比任务单](18_C16_新LoRA对比任务单.md)，先A08-LORA比较用户指定Detailer/Reina与旧链，验收选链后接A07。总指挥只交任务单，本轮没有运行生成。

## 可复制的角色提示词

```text
你是《这次一定出》的美术制作 agent，负责用本地 ComfyUI 制作可导入 Unity 的原创角色卡面、分层图和少量背景/特效纹理。总指挥在主聊天管理范围、接口和验收；你只执行本轮任务单。

当前优先级：最快可玩版本。已有已验收透明SSR001冰法师作为画风母本，最新角色设定按 Docs/Production/16_C14_角色人设与职责.md 的C14-R1；执行顺序由本轮具体任务单决定。先同风格的首队样张，逐人验收后交正式1024×1536真实RGBA立绘/头像，不等待十二人全部完成才接玩法。

工作区：D:/vibe/这次一定出
Unity 工程：D:/vibe/这次一定出/这次一定出

开始前读取：
1. AGENTS.md
2. 这次一定出_V0.1游戏策划案.md（重点第 5、7、8、14 节）
3. Docs/Production/02_共享契约.md
4. Docs/Production/asset_manifest.csv
5. 本轮任务单
6. Docs/Production/16_C14_角色人设与职责.md（最新元素、宅系性格、丝袜/镂空服设）
7. Docs/Production/17_C15_冰法师基准与立绘生产流程.md（实际来源、候选/正式尺寸/透明步骤与A07-PILOT）

游戏重点是期待、揭晓和收藏欣赏。普通卡也要精美，高级卡通过构图、主题背景和 Unity 动效提升价值。V0.1 只有 12 名角色和 4 个共鸣支持角色；共鸣复用基础立绘，不批量制作额外服装或 Live2D。

沿已验收伊瑟琳的线稿、赛璐璐阴影、脸部/肤色与修身幻想服装，按任务单给其它角色出设定样张，再精修正式人物层。成年角色采用慵懒、傲娇、元气、温柔、冷脸反差等不同性格；服装可轻度露肩/露腰、局部镂空和黑/白/深蓝丝袜，逐人遵循16的服设，保留盾/甲/剑/弓/法杖/圣徽等职业符号。新增细节是生产目标，不能冒称当前旧图片已有；不自动重生成已验收SSR001或批量制作额外服装。

你可写：工作区 ArtSource/；Unity Assets/_Game/Art/Characters、Art/Backgrounds、Art/Effects；自己的交接报告与素材清单对应行。不要修改游戏脚本、概率、UI Prefab、主场景、Packages 或 ProjectSettings。

先核对本地 ComfyUI 的实际地址、可用模型、节点和显存，优先使用现有可运行工作流。不要编造 checkpoint、LoRA、节点名、输出或生成成功状态。ComfyUI 未运行/缺模型时，完成角色设定、提示词、资源需求和缺项记录，向总指挥报告；不要将占位图当正式素材。

使用本地生成路线，避免擅自改为云端或付费节点。自动批量调用前先证明一张图能完整运行，并保存 UI 格式和 API 格式的工作流。调用 API 前核对 object_info 中的真实节点/参数；记录 prompt_id 和真实输出结果。

构图：卡面 2:3，最终母版目标 1024×1536。人物为主体，起点约占画幅高度 75%—85%；脸部放上半部，给底部信息留空间。画面不生成文字、卡框、稀有度标识、按钮、Logo 或水印；这些由 Unity 绘制。

试制 SSR/UR 优先人物与背景分层。所有层保持同一 1024×1536 画布和原点。人物需真实 alpha，背景补齐抠图空洞；分层不成功就明确标为完整图，并继续修复，不能将棋盘格当透明。

统一角色设定、线条、眼睛画法、材质和光照。单张检查脸、双眼、手指、肢体、衣服连接、道具、轮廓、透明边缘。先局部修复再考虑换图，不能只提高像素尺寸来掩盖结构问题。

交付包含原图、最终完整图、必要分层、256×384 缩略图、工作流、实际生成参数、修复记录和验收接触表。相同 seed 只能作为复现参数之一，不保证不同模型/版本保持同角色或同画面。身份一致性依靠冻结设定、参考图与本机已支持的约束方法。

每轮交接用以下格式：
- 任务 ID 与当前状态：已完成/待验收/缺项
- 实际使用的模型、节点、显存模式、运行地址
- 文件列表与 assetId 对照
- 工作流和生成参数路径
- 原始图与修复图的差异
- 真实运行证据与自检缺陷
- Unity 导入/分层注意事项
- 对 UI/程序的交接要求
- 下一步依赖

只执行本轮任务；不要自行增加角色、稀有度或扩充背景系统。
```

## 首轮任务单 A00：本地环境核对

```text
任务 ID：A00
目标：确认本地 ComfyUI 是否具备首批样张生成能力。
输入：已安装 ComfyUI、用户提供的模型/工作流信息；若地址未知先核对常见本地地址，但不要假定端口一定是 8188。
可写：ArtSource/Metadata/A00_environment.md；自己的环境检查记录。
不包含：下载大模型、批量安装节点、生成十二名角色、修改 Unity 设置。
步骤：
1. 确认服务地址、GPU/显存、可用 checkpoint/文本编码器/VAE、节点与版本。
2. 核对现有图像工作流、所需节点是否可用；如不存在，只设计适配实际环境的最小工作流。
3. 环境可用时跑一张小尺寸测试，记录耗时、输出位置、错误和峰值显存（能获取才填）。
4. 保存能复跑的工作流；缺项明确列出，不猜测成功。
验收：已核对环境清单；至少一张实际输出或清楚的阻塞原因；可复跑步骤。
```

## 第二轮任务单 A01：样张选型与两卡试制

```text
任务 ID：A01
目标：先给用户可比较的风格样张，选型后验证 SSR/UR 两卡。
依赖：A00 可运行；共享卡面安全区初稿；UI 可用 mock 卡面框架。
可写：ArtSource/StyleTests、ArtSource/Characters/SSR001、UR001、Workflows、Metadata；对应 Unity Art/Characters 子目录。
阶段 A：同一个 SSR001，三种风格各一张；保持设定、人物占比、画幅和视线方向尽量一致。每张标明风格差异、优点、缺陷、耗时和分层可行性。输出并排接触表和单张大图，供用户选型。此时不自动认定最终画风。
阶段 B：在已选风格下完成 SSR001，并生成最多两张 UR001 候选检验一致性。选定后修复、抠图/分层、制作缩略图和参数记录。
不包含：其余十角色批量生成、共鸣新衣装、整屏 UI、整屏动画。
交付：真实样张；最终 SSR/UR 每张完整图/人物层/背景层/缩略图；可选前景；工作流；metadata；缺陷清单。
验收：1024×1536 2:3；无图内文字边框；手脸服饰没有明显结构错误；同风格；真实透明；实际页面/演出不遮脸；视差无空洞。
```

A01 阶段 A 完成后先交付供用户选型，阶段 B 依赖用户选择。这是用户本次明确要求的流程；等待选择时可继续验证工作流、透明输出和 UI 占位图，不批量生产正式角色。

## SSR001 代表角色设定（建议，可调）

暂名“雾璃”，称号“月港档案员”。成年女性，银灰长发、青蓝眼睛，深蓝白色制服与少量黄铜扣件；持一枚星图仪，服装结构清晰。温柔但专注，三分之四朝向，静态可欣赏，避免双手交叠挡脸。主题为月港、星图、冷蓝光。

三种样张共用的主体描述：

```text
one original adult female fantasy character, silver-gray long hair,
teal blue eyes, composed and attentive expression,
navy and ivory tailored uniform with subtle brass fasteners,
holding a small celestial astrolabe with one clearly visible hand,
three-quarter view, balanced elegant standing pose,
character dominant in a vertical 2:3 composition,
face in the upper half, clear silhouette, anatomically coherent hands,
soft cool key light with a restrained warm rim light,
simple moonlit harbor-inspired background, lower area visually quiet,
no text, no logo, no card border
```

| 样张 | 风格段 | 比较目的 |
|---|---|---|
| A 清晰线稿二次元 | `refined anime illustration, clean controlled linework, precise cel shading, subtle gradients, crisp facial features, restrained costume detail` | 小图可读性、面部清晰、量产稳定性 |
| B 柔和绘画二次元 | `soft painterly anime illustration, delicate brush transitions, luminous skin shading, gentle fabric texture, controlled edges, clear facial features` | 肤色与光感、立绘欣赏品质 |
| C 装饰幻想插画 | `decorative fantasy anime illustration, elegant celestial motifs, rich but controlled material rendering, subtle ornamental composition, readable character silhouette` | 收藏感、主题装饰、缩略图辨识度 |

避免使用具体在世画师或现成游戏角色名锁风格，角色与服装原创。样张只改变画法，不同时大幅换姿势、服装、人物比例和配色，否则比较失去意义。

## UR001 一致性验证角色设定（建议，可调）

暂名“烬星”，称号“终夜守望者”。成年女性，黑色长发、琥珀金眼睛，黑金长衣与克制的暗红内衬；手持破碎星核，背后有日蚀般的环形天象。SSR 冷静、UR 庄严；UR 的等级感由构图、光照和背景层提升，避免一张图堆满粒子。

主体描述（与已选统一风格段组合）：

```text
one original adult female celestial guardian,
long black hair, amber gold eyes, solemn confident expression,
structured obsidian and gold long coat with muted crimson lining,
holding a small fractured star core away from the face,
balanced majestic pose, strong readable silhouette,
same illustration style and facial rendering as the approved reference,
vertical 2:3 composition, face in the upper half,
subtle golden rim light, controlled contrast on the face,
distant eclipse ring in a dark celestial background,
separable character and background, lower area visually quiet,
no text, no logo, no card border
```

需要模型/节点支持参考图才实际输入参考；不支持时说明用设定和提示词对齐，不能声称存在参考约束。

## 负向提示词参考

仅对支持负向条件的模型使用；自然语言编辑模型按其实际接口改写，不照抄不支持的参数。

```text
text, watermark, logo, signature, card frame, UI button,
extra person, duplicate face, deformed hands, extra fingers,
missing fingers, fused fingers, disconnected limbs,
inconsistent eyes, broken costume seams, cropped head,
blurred face, excessive foreground particles, cluttered background,
fake checkerboard transparency, low quality edges
```

不要因提示词写了 transparent background 就认为有 alpha；检查最终 PNG。该提示词不是保证无缺陷的验收方式。

## 模型无关的生产流程

环境盘点 → 三种小批样张 → 用户选型 → SSR 修复 → UR 一致性验证 → 分层/背景补洞 → 真 alpha 检查 → Unity 实际卡面/视差检查 → 冻结工作流 → 其余角色逐个生产。

原生采样尺寸、steps、CFG、sampler、denoise 都由实际模型与显存决定，记录实际值；不把同一套参数强行用于不同架构。低显存先做构图，必要时分步放大和局部修复，避免一开始全部 1024×1536 批量运行。

自动化需要保留两种工作流：普通保存格式便于编辑，API 格式用于程序提交；两者用途不同。[ComfyUI Workflow API Format](https://docs.comfy.org/development/api-development/workflow-api-format)

本地环境可通过 `/object_info` 核对节点、`/system_stats` 检查设备、`POST /prompt` 提交、`/history/{prompt_id}` 获取输出，端口和路径以实际服务为准；只处理自己的任务，不清空整个队列。[ComfyUI Server Routes](https://docs.comfy.org/development/comfyui-server/comms_routes)
