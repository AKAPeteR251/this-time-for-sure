# A01-A 美术交接：SSR001 风格选型

任务 ID：A01-A（衔接 F02）；状态：旧方向归档。用户随后要求 SSR/UR 增强魅力、动态与识别特征，当前执行入口改为 `A01_R01_direction.md`。以下选型稿保留为历史比较，不作为已选正式方向。A00 执行链已实测成功，尚未接入 Unity。

## 交付

以用户 PNG 中提取的 API 工作流为基础制作。原图、原始 prompt 保留；没有改写用户原始工作流、模型或素材。普通 UI 格式由真实节点 schema 重建，未冒充原图保存的布局。

| 资源 ID | 本轮候选 | 实际尺寸 | 自检 |
|---|---|---|---|
| style_ssr001_a | `ArtSource/StyleTests/SSR001_style_a.png` | 768×1152 | 清晰平涂、脸部可读；背景过亮，星图仪像容器 |
| style_ssr001_b | `ArtSource/StyleTests/SSR001_style_b_v002.png` | 768×1152 | 线条和五官有所变化，单手持物；仍偏平涂，没有充分实现柔绘 |
| style_ssr001_c | `ArtSource/StyleTests/SSR001_style_c.png` | 768×1152 | 星图主题、港口和暗色天幕更突出；服装蓝白比例漂移，道具偏灯具 |

并排对比：`ArtSource/StyleTests/SSR001_contact_sheet.png`；首版原强度对比：`SSR001_contact_sheet_v001.png`。B/C 第二版试验保留独立 `_v002` 后缀，不覆盖首版原图。

实际配置、prompt_id、耗时和图像 SHA256 均见 `ArtSource/Metadata/A00_smoke_run.json`、`A01_a_run.json`、`A01_b_run.json`、`A01_c_run.json`、`A01_b_v002_run.json`、`A01_c_v002_run.json`，各自 history 为服务真实返回。

工作流在 `ArtSource/Workflows/`：`user_00348_prompt.json` 为原 API；各批次 `_api.json` 为实际提交图，`_ui.json` 可在 ComfyUI 打开。首版差异在 `A01_v001_diff.json`；第二版差异在对应 `_changes.json`。

## 结果与限制

- 原工作流 LoRA 的画风约束很强；首版三张都偏硬边平涂。第二版在同一图结构中调低三枚 LoRA 强度并强调画法，保留采样配置，未宣称完全得到三种独立成熟画风。
- 角色主体共用同一描述和 seed，但衣服、双手和背景不完全一致。当前只适合判断方向，不能当成严格同姿态画法实验。
- 当前输出为完整不透明图，没有人物 alpha、补洞背景或前景层；不具备分层视差交付。
- 768×1152 是选型尺寸；尚未生产正式 1024×1536 母版、正式卡面缩略图或 UR。
- 没有修改 Unity 脚本、Prefab、主场景、概率、包或项目设置，没有进行实际页面与 Play 验收。
- 星图仪结构、月港夜色、服装冻结、手部细节需在选型后的正式稿修正。

## 下一步依赖与接入

用户选择 A/B/C 或具体保留的特征后，进入 A01-B：先制作一张可接入 SSR 完整图，再验证 UR001 一致性。精细分层后移，先接入完整图。程序/UI 负责人使用 `PrototypeGame` 的角色图片映射接入 SSR001；正式资产归属 `Assets/_Game/Art/Characters/SSR001/`，当前候选留在 ArtSource，避免误用为正式素材。

相关 Unity 概念、调参说明见 `ArtSource/StyleTests/A01_art_brief.md`。运行步骤和环境缺项见 `A00_environment.md`。
