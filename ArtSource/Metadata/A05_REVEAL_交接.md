# A05-REVEAL 交接

日期：2026-10-08。状态：**独立素材已交付，待 P04/Unity 接入验收；UR 正式透明人物层仍为缺项。**

已读取09/10/11及完成后的 U03_CLEAN_交接.md。没有修改 Unity 代码、场景、Prefab、字体或抽卡数据，没有制作十连视频。动态图、用户名、契约文案与字幕均未烘焙。内层素材没有外层游戏名，也没有烘焙任何文字。

## 实际文件

Unity 目录前缀：`这次一定出/Assets/_Game/Art/Effects/F03/`。

| 文件 | 实际尺寸 | 用途 |
|---|---|---|
| `contract_paper_v001.png` | 1024×800 RGBA | 无文字象牙白纸，外围透明 |
| `contract_border_v001.png` | 1024×800 RGBA | 独立黄铜边纹，中心和签名区透明 |
| `star_seal_v001.png` | 512×512 RGBA | 线描八芒星印，盖印/契约装饰 |
| `star_seal_anima_v001.png` | 512×512 RGBA | 本地 Anima 生成、SAM 抠出的厚重金色星印备选 |
| `ring_giant_v001.png` | 1024×1024 RGBA | 天穹巨环，中心留空 |
| `ring_outer_v001.png` | 1024×1024 RGBA | 独立外环 |
| `ring_inner_v001.png` | 1024×1024 RGBA | 独立几何内环，中心透明 |
| `shockwave_v001.png` | 1024×1024 RGBA | 无场景的环形冲击与短射线 |
| `light_pillar_v001.png` | 512×1024 RGBA | 独立金白光柱纹理 |

火战士完整卡：`这次一定出/Assets/_Game/Art/Characters/UR001/RPG/UR001_fire_card_base_v001.png`，1024×1536。母稿复制保留在 `ArtSource/Characters/UR001/RPG/UR001_fire_source_v001.png`，768×1152；原职业稿没有修改。完整卡仅重采样，不声称新增细节或补齐原裁切。

透明候选：`ArtSource/Characters/UR001/RPG/UR001_fire_character_candidate_v002.png`，真实 RGBA、1024×1536。这是同一原图的 SAM 抠图与重采样，不是重新生成姿势。**细发丝、暗色衣服/披风边缘还有缺损和背景残留，不作为正式人物层、不默认绑定到 Main。** v001 三个候选、v002 修正、掩码、蓝底预览和参数全部保留。原图右侧头发/披风、剑尖和下部靴子的裁切没有重画补齐，仍是缺项。P04 可继续使用完整图/Mask，不阻塞仪式或逐张流程。

原始来源、PNG 尺寸/alpha/哈希、矢量源和脚本：`ArtSource/Effects/F03/`。生成母图在 `LocalGenerationSmoke/`，六个 SVG 在 `VectorSource/`，检查与预览在 `QA/`。详见 `A05_effects_manifest.json`、`A05_anima_seal_metadata.json`、`A05_checks.json` 和角色 `A05_character_metadata.json`。

## 本地能力与真实参数

启动前 ComfyUI 8188 拒绝连接；本轮从已安装路径启动隐藏服务，实际查询 `/system_stats`、`/object_info`，记录在 A05_local_capabilities.json。版本0.37.0，RTX3060 Laptop、6GiB，torch2.9.1+cu130。RMBG/BiRefNet 节点虽注册，但常用模型目录没有权重；没有下载或声称成功运行它们。

本地星印实际成功出图，prompt_id `24fa16c4-8ef4-4f51-8865-ea69c308c1b6`。沿用用户 API 工作流：Anima `anima_turboV11.safetensors`、编码器 `anima_baseV10_txt.safetensors`、VAE `qwenImage_qwenImageVAE.safetensors`；Flatline0.6、chen-bin0.6、bakav3-000012 0.8；512×512、seed71008051、dpmpp_2m/sgm_uniform、8steps、CFG1、eta0.5、replay/spectrum关闭。完整实际字段以 `LocalGenerationSmoke/api.json` 和 `history.json` 为准。服务报告运行508.82秒，包含加载与本轮显存竞争；不是稳定性能基准。

本地抠图使用已安装 `segment_anything` 和 `models/sams/sam_vit_b_01ec64.pth`，CUDA，保留权重 SHA256、坐标点、分区 box、候选分数与时间。人物首轮10.578秒；补选膝甲、双腿、躯干与头发，合并掩码并移除分离的碎点。分数不是美术验收，最终按目视保留缺项。星印使用三正点和对象 box，掩码0.45像素抗锯齿，原母图保留。

其余八层是原创可编辑几何/解析光纹，本地 Pillow 双倍采样后导出；没有使用云生成或将这些矢量层伪报为 Anima 输出。颜色/环半径/分段/辉光/光柱公式全部在 `build_a05_layers.py`。旋转、穿越、巨印重量感、人物揭晓和停留由 P04 在 Unity 控制，这些素材本身不是动画成片。

## U03 安全区与接入

页面坐标左下原点，参考1920×1080，CanvasScaler匹配0.5，以下均消费 U03：

- 纸边：x0.205—0.795、y0.085—0.91。纸和边纹使用同一 RectTransform/画布，不分别紧裁。纸内标题/正文由 UI 现有节点绘制。
- 签名：纸内x0.10—0.90、y0.16—0.41；PNG 顶部原点对应x102—922、y472—672，边纹该区 alpha 全0，纸底不含文字或线状干扰。
- 人物：x0.27—0.73、y0.15—0.93，等比，身份与进度在底部；默认光在人物后。
- 跳过：x0.86—0.97、y0.90—0.97；确认：x0.80—0.96、y0.035—0.115。素材没有内置按钮；接入时用遮罩/层级保护这两区，装饰关闭 Raycast Target。

Sprite 导入为 Single、Alpha Is Transparency、Bilinear、Clamp；建议先关闭压缩，环纹最大尺寸1024，卡图最大尺寸2048。边纹与纸可用 Simple/Image，避免拉伸到其他比例后签名区漂移；不是已验证的9-slice成品。环/印原点均在画布中心，独立旋转，保持比例。光柱可放于背后纵向伸展；冲击快速扩张后迅速淡出。SVG 源便于调整边纹，PNG 附加柔光参数在脚本中，SVG本体以几何为主。

复用而不重生成：`Art/Characters/UIIceMage/UIIceMage_character_v001.png` 和 `Art/Effects/UIAssets/fx_sparkle_blue_v002.png`、`fx_sparkle_gold_v002.png`。不把候选透明火战士当可摆手脚的分层人物；没有制作 Live2D。

分镜接入起点：0.45—1.15秒巨印落下+shockwave；1.15—2.50秒纸升空+giant；2.50—4.20秒outer/inner反向转动+pillar；5—6.2秒大冲击，6.2秒后强光退到人物后。约5—7秒只是09/11的起点，实际音画时长由 P04 调整。所有等级均可用庄严开场，R/SR短揭晓、SSR/UR更强冲击，角色出现后退出脸部和操作区。

## 检查与下一步

八层 PNG 的哈希、真实透明像素、环中心 alpha0 和边纹签名区 alpha0 已实测。金色星印备选已在暗蓝底目视确认独立轮廓；角色 v002 未通过最终透明层目视检查。没有 Unity Sprite 导入、Main Play 或三窗口演出验收，不标记为已验收。QA 是素材检查，不能替代运行证据。

P04 接入上述9层与现有冰法师/星芒，保留 U03 RevealIdentity、RevealArtworkSlot/UnknownSilhouette、RisingContract/SignatureName，检验固定按钮和角色脸部不被遮挡；完整卡路线先行。正式火战士透明层需后续精修轮廓/补齐原裁切后再另升版本。任务板建议记为“素材待验收；UR透明层缺项”，由总指挥更新，本任务不修改任务板。

涉及 Unity 概念：Image/Sprite、透明混合、RectTransform中心旋转、CanvasGroup淡出、Mask裁切、粒子纹理与Raycast Target。如何调参：先锁U03纸和操作区，再调环直径/转速、巨印缩放回落、冲击扩张速度和光柱透明度；角色出现后迅速减小辉光，最后才调镜头震动和旁白同步。不要改变抽卡结果或让特效覆盖输入。
