# A01-R01 新方向样张交接

任务：A01-R01；状态：待验收、等待用户判断魅力尺度与造型方向。沿用用户工作流真实生成，未改 Unity 文件。

- 完整图：`ArtSource/StyleTests/SSR001_dynamic_v001.png`，768×1152、2:3、不透明 PNG。
- 资源 ID：`style_ssr001_dynamic_r01`，已写入素材清单。
- 实际运行：prompt_id `6480f330-da16-410f-9c89-b0f4dda25b9d`，45.23 秒。
- API/UI 工作流：`ArtSource/Workflows/A01_R01_dynamic_api.json` / `A01_R01_dynamic_ui.json`。
- 参数、SHA256 与自检：`ArtSource/Metadata/A01_R01_dynamic_run.json`；服务原始证据：`A01_R01_dynamic_history.json`。
- 差异与执行脚本：`A01_R01_changes.json`、`ArtSource/Workflows/run_rare_redesign.py`。
- 模型/节点/显存：见 A00 环境和真实 system_stats；沿用原三枚 LoRA 强度和自定义采样配置，没有更换模型。峰值显存未取得。

新图保留成年银灰发、青蓝眼和月港主题，改为露肩开衩礼装、抬腿转身、飘动裙摆；增加月牙发饰，星图仪实现为镂空金属环。角色与服装原创。

自检：相比旧图，身体轮廓与动作更明确，月港夜色、星图仪和角色符号已经呈现。抬腿支撑关系、手部持握、服装连接需要在正式母版中复查和修正。当前没有真实 alpha、补洞背景、正式缩略图或 Unity 集成验收，不能称作最终游戏资产。

下一步：用户确认方向后制作 SSR 正式图，保持稀有角色的视觉魅力，再用 UR 验证更高档构图与专属主题。普通角色仍用更素的服装建立反差，不批量扩充换装。

涉及的 Unity 概念与如何调参见 `A01_R01_direction.md`；使用完整图时保持比例，UI 底部信息区不能遮住手和道具。分层人物/背景需同画布，未取得分层前不要接入独立人物视差。
