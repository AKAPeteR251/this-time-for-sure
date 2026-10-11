# A08-LORA 四组实测交付 v001

状态：实际出图与对照完成，待视觉验收。C15/C16 前置；本批不替换任何正式角色、不改 Unity。2026-10-11。

## 实际结果

同一已验收冰法师原稿整画布合成中灰底，1024×1536。四组使用 seed 71008031、KSampler euler/simple、12 steps、CFG 1、denoise 0.3；编码/解码 tile512 overlap64。旧三枚 Flatline 0.6/0.6、chen-bin 0.6/0.6、bakav3 0.8/0.8 不变；新增顺序 Detailer→Reina→关闭 replay/spectrum 的 patch，新 CLIP 强度均0。

|组|Detailer/Reina model|实际耗时（秒）|prompt_id|
|---|---|---|---|
|B0|0/0|254.62|1ed47dab-e949-4b77-92d0-699e6ddd9eab|
|D1|0.30/0|227.21|74363450-2c7f-4416-ba13-2927b706939a|
|R1|0/0.30|236.44|eb51b021-32b0-4e63-9aa2-3dcefb641d21|
|DR1|0.30/0.30|209.10|f1f2c685-d4d5-41ee-a334-e9841ddf2eb1|

服务由本任务隐藏启动，开始前服务不可达且没有现有进程；启动后队列为空。ComfyUI/Python/Torch/显卡实测见 runtime/system_stats.json；节点注册与输入范围见 required_node_schemas.json。原图与测试输入 SHA256、正负提示词见 input/input_sources.json，模型实际哈希/文件头见 runtime/model_checks.json。

两枚新 LoRA 每枚840张量、280组 low-rank 矩阵，280/280 均能按本机 comfy/lora.py 的规则映射到当前 Anima Turbo 的对应二维权重且形状一致。真实推理日志未发现 lora key not loaded / size mismatch。四组有成功 history、不同输出哈希；这支持当前条件下可运行，不等同于美术质量通过。

## 视觉评审

- B0：轮廓、姿态和主配色大体保持，但微笑改为较冷静表情、菱形耳坠改形，原雪晶改成更圆润造型；相比源图，部分细线与材质被简化。它不是原稿的无损精修。
- D1：与B0总体接近，局部线条/褶皱略有变化，未观察到足以决定换链的清晰增益；也未恢复源图表情与雪晶。
- R1：脸部、衣纹和手部有小幅变化，灰背景出现少量白点；没有证明“增加细节且保持画面”。
- DR1：变化更大，仍无明确质感提升；脸部和持杖区域偏离B0更多。大色块轮廓保持，但不标为身份保持通过。
- 全组仍为RGB测试图，不是透明人物；原帽尖和鞋部近画布边缘，这一轮按任务保留原画布，不据此声称符合新人物5%—8%余量。

QA/full_comparison.png 交原图与四组整图；face_hair_stockings_staff_comparison.png 为脸/发丝/丝袜/雪晶/持杖手224×224原尺寸裁切；另有独立局部PNG、256与64头像对照。裁切坐标与差异指标在 comparison_parameters.json。像素差异只证明输出改变，不能量化“更美”或身份正确。

原建议：暂沿旧链试制新人物，已验收冰法师继续保留。用户看完首轮后于本聊天明确改为“用新lora 0.6强度 试制米洛”；下一任务A07按两枚新LoRA各0.60 model、clip0执行，并保留旧三枚基线。这是用户选定的试制条件，不是两枚0.60已经通过冰法师对照或正式生产验收。首轮不额外补冰法师强度网格。

## 文件与复现

outputs 下4张原始生成PNG包含API prompt与UI workflow信息。Workflows/A08_LORA_{组}_api.json / _ui.json 和 Metadata/A08_LORA_{组}_run.json / _history.json 均为新文件；副本也放在runtime随包。UI JSON验证节点与链接结构，未在浏览器手工导入；真实执行使用API图。

包 A08_LORA_Comparison_v001.zip，包含输入、生成原图、QA、真实运行日志、模型/节点记录、执行源和检查值。checks.json记实际验证，SHA256SUMS.txt提供校验。服务器保持在线，不清队列、不中断其它任务。原冰法师与Unity文件均未修改。

下一步A07米洛：按C14-R1人设和用户新强度，先候选，再正式尺寸生成与SAM透明处理；不得把本批RGB测试图直接当成角色层。

涉及Unity概念：本批无Unity修改；RGBA角色层与RGB测试图不同，头像焦点须按新人物单独确定。调参：公共条件先固定，新LoRA仅model强度变化，seed不是风格保证；本轮0.30观察不能自动推广到0.60或其它角色。
