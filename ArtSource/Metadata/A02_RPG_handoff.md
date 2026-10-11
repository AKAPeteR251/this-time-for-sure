# A02-RPG 职业题材试制交接

日期：2026-10-08。任务 ID：A02-RPG；状态：已完成本轮试制，待用户选型/验收。没有更改正式卡池、属性玩法或 Unity 代码。

## 文件与运行证据

目录：`ArtSource/StyleTests/RPG_Professions/`。

| 探索资源 | 实际选型图 | 职业/元素 | 实际生成 prompt_id |
|---|---|---|---|
| style_rpg_01_ice_mage | 01_ice_mage_v001.png | 冰法师 | ca0437ba-b646-46f1-a5ba-6c7e1cf06cb2 |
| style_rpg_02_light_priest | 02_light_priest_v001.png | 光牧师 | c71e182e-8046-4886-a7f9-2cacf9ec4b1b |
| style_rpg_03_wind_elf_archer | 03_wind_elf_archer_v002.png | 风精灵弓箭手 | fd027850-4d6a-42da-8a62-5ed6bc6ad0e9 |
| style_rpg_04_fire_warrior | 04_fire_warrior_v001.png | 火女战士 | cd17d784-ee47-4c91-bbdf-c429975a40bd |
| style_rpg_05_thunder_miko | 05_thunder_miko_v001.png | 雷巫女 | 361e930d-bdbb-4ecc-926a-3979d3fa6181 |
| style_rpg_06_star_magical_girl | 06_star_magical_girl_v001.png | 星魔法少女 | f464d0b5-242f-4116-9a20-d23a053efdaf |

每张均为 768×1152、2:3 完整不透明图；`*_thumb.png` 为从对应图缩小的 256×384 缩略图。并排图：`RPG_professions_contact_v001.png`。

六职业设定与完整提示词：`A02_RPG_profiles.json`、`ArtSource/Workflows/run_rpg_professions.py`。资产清单与实际尺寸/耗时/哈希：`A02_RPG_asset_manifest.json` / `.csv`。

每张 `_run.json` 保存参数路径、prompt_id、图像 SHA256 与实际自检；对应 `_history.json` 是 ComfyUI 实际返回。工作流 `_api.json` / `_ui.json` 在 `ArtSource/Workflows/`。UI 格式由真实节点 schema 重建，未冒充用户原 PNG 的普通布局。

## 模型与参数

继续使用现有本地 `http://127.0.0.1:8188` 服务；Anima-turbo1.1、原文本编码器/VAE、三枚 LoRA 与原强度、自定义 ClownsharKSampler_Beta、dpmpp_2m/sgm_uniform、8 steps、CFG 1、denoise 1、replay/spectrum 关闭。本轮仅改职业/元素/成人角色描述、seed 与输出前缀，使用已验证的分块解码。

模型实际名称/校验值见 `A01_B_model_hashes.json`，ComfyUI 版本、显存模式与硬件见 `A00_system_stats.json` / 环境记录。峰值显存、节点精确提交版本未取得，不填写推测值。

## 视觉检查与修订

六张的职业符号、配色和服装轮廓形成了可比较方向。年龄为角色设定：24/20/22/25/19/19，少女感通过表情、脸型和装饰表达，本轮魅力造型均为成年人。

精灵弓箭手 v001 拉弦/箭杆交互不成立，保留失败稿和 history，当前对比采用 v002 的持弓姿态。v002 仍有长弓裁切，并非完全实现奔跑动作。该修改是姿态再生成，不宣称局部抠图修补。

其他待精修项：法师手指与杖握持；牧师实际为坐/跪姿、权杖握持不清；战士剑尖裁切及双手；巫女符札的伪文字和指尖；魔法少女星杖握持及伸手透视。符札出现伪文字，所以不能声称六张全部严格无字。每张具体缺陷在实际 `_run.json` 记录。

这些是题材选型稿，不是正式 1024×1536 图。没有 alpha 人物层、背景补洞、游戏内导入或 Play 验证；不得标为已验收正式资产。

## 下一步与文件归属

用户选定喜爱的职业、属性和年龄气质后，挑两张代表角色做正式图，优先修道具/手/服装连接与卡面安全区，再映射现有稳定角色 ID。旧月港/日蚀素材不删除，但不自动继续冻结其题材。

本轮不把六张样张塞进正式 12 人池，不修改游戏属性或增加战斗，不自动建立六名正式角色，不覆盖其他角色负责人的脚本/场景/Prefab。

涉及的 Unity 概念与如何调参见 `A02_RPG_task.md`：职业/属性先作为视觉标签；缩略图辨识与 Sprite 比例/裁切是下一步接入重点；完整图不能冒充真实 alpha 分层。
