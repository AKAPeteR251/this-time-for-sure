# C16：新LoRA核对与A08-LORA对比任务单

日期：2026-10-11。用户提供anima_context_detailer_base10.safetensors与reina_v1_epoch14.safetensors，并确认本聊天只补美术agent执行任务单，沿用总指挥计划/验收分工。

本轮已完成文件与头部元数据核对、测试设计；没有加载模型推理、启动ComfyUI、生成对比图或派发执行agent。A08-LORA状态为待派工，效果未知；这两枚LoRA暂不替换正式生产配置。

**后续C17状态更新：** 美术agent已交 `ArtSource/Metadata/A08_LORA_交接.md` 与四组实际测试，记录当前Turbo下加载/出图成功，同时明确首轮没有证明身份保持或无损增细节；上述“待派工”是本文件编制时状态。当前正式十一人人物由内置image_gen产出，见 [19_C17](19_C17_角色素材映射与Git初始化.md)，不是这些LoRA测试图。总指挥本轮未重跑ComfyUI，不把执行报告等同于独立性能复验或美术质量验收。

## 1. 已核对的文件

共同目录：E:/ComfyUI-aki-v3/ComfyUI-aki-v3/ComfyUI/models/loras/anima/。

| 文件 | 本轮实测 | 文件头信息 | 本轮结论 |
|---|---|---|---|
| anima_context_detailer_base10.safetensors | 91,851,400字节；840个张量条目 | networks.lora_anima；rank/alpha 32；训练基底记录anima_baseV10；1024×1024；没有发现文本编码器权重 | 文件存在，需验证对当前anima_turboV11的实际加载与视觉效果 |
| reina_v1_epoch14.safetensors | 91,851,184字节；840个张量条目 | lycoris.kohya；algo=lora；model_family=anima；preset=anima_full；rank/alpha 32；没有发现文本编码器权重 | 文件存在，需验证实际加载与风格/细节效果 |

SHA256：

```text
detailer: aaaca6107e97bf98d03482464583a0d89fbf14acec8e01685682f11a02915d22
reina:    70ba5c932d5c8b0f9681381e5ae8d2b7545ae249a826e58dc3d930ec18dff6cc
```

“Detailer可在不损失画面情况下增加细节”“Reina为混合NAI5微调”来自用户提供的用途/来源说明。文件头不含Reina的完整训练来源或触发词；本轮不把这两项说明写成已测结论，不猜测额外触发词。

新增两枚LoRA均只测试strength_model，strength_clip=0。原三枚LoRA及其model/clip强度继续作为基线。文件结构属于Anima不等于在Turbo版本下已验证兼容；执行时核对LoraLoader实际加载日志，未匹配权重不能算成功加载。

本轮GET 127.0.0.1:8188/system_stats与/queue均连接被拒绝。记录只表明核对时服务不可达；执行agent先检查实际服务，不把端口状态当永久缺项，也不由总指挥代为启动。

## 2. 测试目标与统一输入

本轮只判断新LoRA是否值得进入 [C15立绘链](17_C15_冰法师基准与立绘生产流程.md)，不生产新正式角色、不抠图、不改已验收冰法师。先比较同一源图的细节与身份保持，再由A07-PILOT验证新人物生产。

固定输入使用 [已验收冰法师透明原稿](../../ArtSource/UIAssets/Originals/ice_mage_cutout_v001.png)。执行agent为图生图生成一份1024×1536中灰底测试副本：保留整画布、姿态和人物，不裁切、不拉伸，不改变原稿；所有测试共用此一副本，并记录输入SHA256。透明原稿本身不被覆盖。

使用 [A01_B_SSR_master_v002_api.json](../../ArtSource/Workflows/A01_B_SSR_master_v002_api.json) / [UI图](../../ArtSource/Workflows/A01_B_SSR_master_v002_ui.json) 另存A08新工作流，替换旧人物LoadImage和提示词。该链的历史运行证据见17；不直接运行旧角色脚本。

固定条件：

| 项目 | 本轮固定值 |
|---|---|
| 模型/编码器/VAE | anima_turboV11 / anima_baseV10_txt / qwenImage_qwenImageVAE，沿既有文件 |
| 原LoRA链 | Flatline 0.6/0.6 → chen-bin_v4.0 0.6/0.6 → bakav3-000012 0.8/0.8，model/clip强度不变 |
| 图像输入 | 同一1024×1536冰法师中灰底副本，LoadImage→ImageScale等比/crop disabled→VAEEncodeTiled |
| 图生图采样 | KSampler，euler / simple，12 steps，CFG 1，denoise 0.3，seed 71008031，batch_size 1 |
| 编码/解码 | VAEEncodeTiled / VAEDecodeTiled；tile 512、overlap 64 |
| 模型patch | AnimaLayerReplayPatcher的replay/spectrum关闭 |
| 提示词 | 下文同一份正负提示词，不为某一LoRA单独追加质量词或触发词 |
| 运行顺序 | 串行，先基线再单项再组合；不与SAM或其它GPU生成并行 |

此次选图生图是为了比较同一脸、服装和姿态的保持度。对比图是新生成的测试RGB图，不称为已验收人物层；同seed只固定随机条件，不保证加入LoRA后构图完全相同。

## 3. 首轮四张，必要时再补强度

新增链路放在原三枚LoRA之后、AnimaLayerReplayPatcher之前：**原链 → Detailer → Reina → patch → sampler**。所有组保持同一连接顺序，新LoRA的CLIP强度均为0；表中0表示停用该新增LoRA。

| 组ID | Detailer model强度 | Reina model强度 | 目的 |
|---|---:|---:|---|
| B0 | 0 | 0 | 旧生产链基线，观察本次重绘本身的变化 |
| D1 | 0.30 | 0 | Detailer独立效果：细节、噪点和画面保持 |
| R1 | 0 | 0.30 | Reina独立效果：脸/线稿/色块与原画风的距离 |
| DR1 | 0.30 | 0.30 | 两者叠加是否更好，是否产生风格冲突 |

0.30是本轮低强度探测起点，没有声称是模型作者推荐值或已验证最优值。首轮四张足以判明显方向；不先穷举完整网格。

仅在首轮结果支持继续时，每个分支最多补一张：

- Detailer增益太弱且未损脸/线条，可补D2：0.60/0。
- Reina变化太弱且画风/身份保持良好，可补R2：0/0.60。
- 若选定的单项强度与DR1不同，补DR2：两个已选强度组合；如果Reina明显与旧链冲突，可改用这一张预算比较“旧三枚停用＋Reina”，保持其它条件与R1相同，明确这是替换链实验。

总量目标四张，按证据最多七张。出现噪点、五官漂移、衣物结构损坏，优先降低或停用相应LoRA；不因它宣称增加细节就继续加大权重。改变CFG/采样器/denoise等公共条件时，必须重新生成同条件基线，不跨条件比较优劣。

## 4. 固定测试提示词

正向：

```text
One adult 24-year-old ice sorceress, preserve the source character's exact face, icy blue eyes, long silver-white hair, huge white-and-blue pointed witch hat with snowflake decorations, white and ice-blue off-shoulder fitted slit gown, dark thigh-high stockings and black ankle boots. Preserve the source's seated floating bent-knee pose, complete snowflake staff in one hand and other palm extended, all clothing shapes and the same full-body composition. Clean fine anime linework, precise cel shading with restrained soft gradients, clean skin colors, readable eyes, carefully rendered hair strands, stocking material, fabric folds and crystal facets. Plain mid-gray background, no new scenery or additional accessories. No text, UI, card frame, logo, watermark or separate floating particles.
```

负向：

```text
blurry, muddy colors, excessive microtexture, gritty skin, oversharpening, noise, photorealistic, 3D render, extra character, changed face, changed hairstyle, changed costume, extra limbs, malformed hands, extra fingers, broken wrist, distorted staff, cropped head, cropped feet, cropped weapon, face hidden, dense effects, background scenery, text, logo, watermark, card frame, checkerboard
```

同一提示词同时约束所有组。新LoRA没有从文件元数据取得明确触发词；本轮不猜触发词。后续若取得作者的明确要求，再作为单独记录的复测条件。

## 5. 怎么验收“增加细节且保持画面”

对照原始冰法师、B0及D1/R1/DR1，检查：

| 看什么 | 有效增益 | 需要扣分的变化 |
|---|---|---|
| 脸与身份 | 眼睛清楚、肤色干净，仍是伊瑟琳 | 五官/表情改变、皮肤出现颗粒或噪点 |
| 头发/线稿 | 发丝层次清晰、粗细受控，仍是原画法 | 多余乱线、毛刺、过度锐化、厚涂/写实漂移 |
| 丝袜/服装 | 丝袜材质与褶皱更清楚，镂空/连接结构正确 | 把丝袜变成皮肤/靴筒，新增无依据服装或花纹堆积 |
| 武器/晶体 | 杖身、雪晶切面更清楚且结构一致 | 改变武器形状、握持恶化、装饰遮脸 |
| 整体画面 | 原有轮廓/姿态/配色保持，细节服务角色 | 构图漂移、轮廓裁切、浮游特效或背景被新增 |
| 小尺寸展示 | 缩小后表情/职业仍清楚 | 细节让头像变脏，画面失去大色块层次 |

“无损”是本次用户提出的验收目标，不承诺逐像素不变。LoRA叠加与图生图会产生新输出；结论写成“这些条件下细节更清楚，未观察到明显身份/构图损失”或列出实际损失，不能写成对所有角色都无损。

首轮交四张整图接触表和同位置脸部、发丝、丝袜、雪晶100%裁切；另交头像大小对照。统一裁切框、缩放和排版；对比图只标组ID/参数，文字不写入人物源PNG。

## 6. A08-LORA可复制执行提示词

```text
你执行A08-LORA。读AGENTS、02共享契约、17立绘流程、18新LoRA对比任务单。用户指定加入anima_context_detailer_base10.safetensors及reina_v1_epoch14.safetensors测试，仍由美术agent执行，总指挥只验收/计划。先核对两个真实文件/哈希、ComfyUI服务和LoraLoader匹配日志；它们的文件头没有文本编码器权重，新LoRA strength_clip=0。保留当前Anima Turbo、编码器、VAE及旧三枚LoRA基线。用已验收冰法师原稿整画布生成统一中灰底测试副本，另存旧正式尺寸图生图链的新工作流；不覆盖原图、旧工作流或旧脚本输出。固定输入、正负提示词、1024×1536、seed71008031、KSampler euler/simple 12步CFG1 denoise0.3、tile512 overlap64。新增LoRA顺序为旧链→Detailer→Reina→patch，先串行出B0(0/0)、D1(.30/0)、R1(0/.30)、DR1(.30/.30)四张，新CLIP强度全部0。按18的条件最多补三张，不穷举网格；以脸/线稿/丝袜/服装/法杖/全身轮廓保持与有效细节判断，不把模型用途描述当实测结论。记录实际加载警告、每组强度、prompt_id/history、原PNG、运行时间、SHA256、整图/局部/头像接触表及建议链。没有成功匹配权重或输出，就如实记缺项，不编造成功。服务需要启动时先确认现有进程/队列，遵守Windows隐藏启动约定，不清队列或中断其它任务。只写ArtSource/StyleTests/A08_LORA、ArtSource/Workflows/A08_LORA_*与ArtSource/Metadata/A08_LORA_*；不下载新模型，不改Unity、正式角色、概率/存档，不生成米洛或批量角色。本轮交付状态待验收，选定链由总指挥核对后更新A07-PILOT配置。
```

建议交付（执行agent创建，当前不存在）：

```text
ArtSource/StyleTests/A08_LORA/
  input/ice_mage_gray_v001.png
  outputs/B0.png D1.png R1.png DR1.png
  QA/full_comparison.png
  QA/face_hair_stockings_staff_comparison.png
  QA/avatar_comparison.png
ArtSource/Workflows/A08_LORA_{group}_api.json
ArtSource/Workflows/A08_LORA_{group}_ui.json
ArtSource/Metadata/A08_LORA_{group}_run.json
ArtSource/Metadata/A08_LORA_{group}_history.json
ArtSource/Metadata/A08_LORA_交接.md
```

下一步：A08交实际样张/记录 → 总指挥验收并选择旧链/单项/组合 → 更新17的生产参数 → A07-PILOT米洛。未通过的新LoRA不强制进入生产，也不阻塞继续使用旧链。

涉及Unity概念：本次不改Unity；比较整图与头像是为了保证后续Sprite特写/缩略图可读，测试RGB图不能代替正式透明Sprite。调参重点是LoRA model强度与组合，公共采样参数先固定；CLIP权重按实际张量范围设置，seed只控制随机条件。
