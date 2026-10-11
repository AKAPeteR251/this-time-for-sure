# A01-B 美术交接：SSR 母版与 UR 两卡试制

任务 ID：A01-B；状态：旧题材备选归档。用户认可魅力/动态方向，但随后要求试制传统 RPG 职业，最新任务见 `A02_RPG_task.md`。本轮旧图仍保留，不作为已冻结的新角色名单；未进行 Unity 导入/页面/Play 实测。

## 本轮文件

| 资源 ID | Unity 美术目录内的文件 | 尺寸 | 状态 |
|---|---|---|---|
| char_ssr001_card | `Assets/_Game/Art/Characters/SSR001/SSR001_card_base_v002.png` | 1024×1536 | 认可稿低强度图生图细化，待接入 |
| char_ssr001_thumb | `Assets/_Game/Art/Characters/SSR001/SSR001_thumb_v002.png` | 256×384 | 从完整母版派生 |
| char_ur001_card | `Assets/_Game/Art/Characters/UR001/UR001_card_base_v001.png` | 1024×1536 | 同风格 UR 候选，设计待用户确认 |
| char_ur001_thumb | `Assets/_Game/Art/Characters/UR001/UR001_thumb_v001.png` | 256×384 | 从 UR 母版派生 |

Unity 工程根为 `D:/vibe/这次一定出/这次一定出`。原始母版与认可源图在工作区 `ArtSource/Characters/SSR001/`、`UR001/`。并排预览为 `ArtSource/Characters/A01_B_two_card_sheet.png`。

没有覆盖原始生成图、用户工作流或既有 Unity 资产；没有手写 .meta/GUID。没有修改程序、场景、Prefab、Packages、ProjectSettings 或 UI 负责人的文件。

## 实际运行证据

- 环境：现有 `http://127.0.0.1:8188`，ComfyUI 0.37.0、RTX 3060 Laptop 6GB、动态显存；实际启动配置见 `A00_system_stats.json`。峰值显存未取得。
- 模型/编码器/VAE/三枚 LoRA 沿用用户工作流；名称、文件大小和实际 SHA256 在 `A01_B_model_hashes.json`。自定义节点精确提交版本尚未记录。
- SSR v001：`64b546c0-705f-4c6a-8b75-d2c277cb993e`，70.46 秒，出现明显噪点，标为 rejected_visual_quality，只保留失败母版与 history，没有放入 Unity。
- SSR v002：`694b7740-c3c3-4fc1-a382-94a8417f9eca`，121.33 秒。使用认可图输入、Lanczos 预放大、分块 VAE、核心 KSampler Euler/simple 12 步、CFG 1、denoise 0.3；原模型、LoRA 和角色提示词保持。
- UR v001：`5831d7f1-0731-4210-be3f-7a0aeb3c2957`，125.46 秒。1024×1536 原生文生图，用户自定义采样配置保持，解码改为分块；未使用图像参考条件，不能宣称参考图身份约束。
- 工作流均在 `ArtSource/Workflows/`，各批次 `_api.json` 是实际提交图，`_ui.json` 是依据真实节点 schema 重建的 UI 格式。原 PNG 只含 API prompt，不含原普通 UI 布局。
- 参数与图像哈希：`A01_B_SSR_master_v002_run.json`、`A01_B_UR_trial_run.json`；真实服务 history 为同名前缀 `_history.json`。
- 资产复制、尺寸、SHA256 与导入建议：`A01_B_export_manifest.json`。

## 视觉检查与限制

SSR v002 噪点已消除，保留银灰发、青蓝眼、月牙、礼装、抬腿构图、星图仪和夜色。持握及抬腿支撑关系仍需在实际欣赏页细检。

UR 使用黑发金眼、黑金礼装、红披风、大日蚀，面部和色块画法与 SSR 接近。掌中道具生成成小日蚀环，尚未实现原设定的三片碎星核；可由用户确认候选的实际设计后冻结。UR 的身体动作比 SSR 更克制，主要差异来自披风、日蚀和色调，不能声称完成 UR 登场演出。

本轮均为完整不透明图，没有人物 alpha、背景补洞、前景层和分层视差。这些后移，不阻塞最快原型的完整图接入。未生产其他十名角色，不增加共鸣换装。

## 对程序/UI 的交接

1. 等 Unity 刷新 Assets，选择两张完整图：Texture Type = Sprite (2D and UI)，Sprite Mode = Single，Max Size = 2048；首次比对建议关闭压缩，sRGB 保持开启。
2. 在现有 `PrototypeGame.portraits` 中分别以 `SSR001` / `UR001` 绑定对应 Sprite；该接口已从当前脚本确认存在，本轮未改动或写入映射。
3. 卡面 Image 使用 Preserve Aspect，装饰关闭 Raycast Target；通过 SSR/UR 预览入口检查脸、道具、底部信息和实际窗口裁切。
4. 当前是完整图，只能整体移动；不要配置成独立人物层视差。名称、边框、版本纹样和粒子仍由 Unity 叠加。
5. 总指挥集成并实际运行通过后，才将相关任务标为已验收。

## 涉及的 Unity 概念与如何调参

Sprite 是 uGUI Image 引用的美术对象；复制 PNG 进 Assets 不等于已经验证 Sprite 导入与页面绑定。Max Size 小于母版尺寸会让导入图降低分辨率，需实际比较压缩质量。缩略图是母版的缩小副本，不另生成身份不同的图。

图生图 denoise 控制重新绘制程度，数值低并不自动保证稳定；此次用户自定义采样器的低强度细化产生噪点，修正阶段使用核心采样器，模型与 LoRA 保持。复跑必须保留版本，相关脚本会拒绝覆盖已有正式版本。

下一步依赖：用户确认 UR 实际设计、程序/UI 接入两张 Sprite、实际页面与揭晓流程验证；随后再补人物分层与背景，不批量生成剩余角色。
