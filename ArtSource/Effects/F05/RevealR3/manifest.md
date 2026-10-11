# F05-ART-R3 揭晓素材交付

任务依据：`Docs/Production/F05_ART_R3_揭晓场景素材清单.md`。
状态：15 项素材已生成、规格与透明通道检查通过；程序导入、动态表现和用户视觉验收待验收。

## 文件与规格

以下均位于本目录根层。星空底图完全不透明；其余14项均为带真实透明通道的RGBA PNG。预览/质检图不能作为整张揭晓背景接入。

| ID | 文件 | 导出尺寸 | 透明/用途 |
|---|---|---|---|
| BG01 | reveal_starfield_base_v003.png | 3840×2160 | 不透明，静态深蓝星空底层 |
| BG02 | reveal_nebula_far_v003.png | 2048×1024 | 透明，远层蓝紫星云，横向循环 |
| BG03 | reveal_nebula_near_v003.png | 2048×1024 | 透明，近层蓝青星云，横向循环 |
| FX01 | star_soft_v003.png | 256×256 | 透明，白色圆形恒星柔光 |
| FX02 | star_cross_v003.png | 256×256 | 透明，白色四芒星，可复用于NEW亮点 |
| FX03 | meteor_trail_v003.png | 1024×256 | 透明，单颗流星，亮头向右、尾迹向左 |
| CH01 | SSR001_snowflake_backdrop_v003.png | 2048×2048 | 透明，六臂蓝青大块面雪花 |
| UI01 | rarity_R_v003.png | 1024×512 | 透明，蓝色斜体衬线R |
| UI02 | rarity_SR_v003.png | 1024×512 | 透明，紫色斜体衬线SR |
| UI03 | rarity_SSR_v003.png | 1024×512 | 透明，金色斜体衬线SSR |
| UI04 | rarity_UR_v003.png | 1024×512 | 透明，青/紫/粉/浅金UR |
| UI05 | rarity_accent_v003.png | 1024×128 | 透明，白色细线和中央四芒星 |
| UI06 | new_text_v003.png | 512×192 | 透明，白色NEW文字，无框 |
| UI07 | new_border_mask_v003.png | 768×256 | 透明，白色开放角框遮罩，无文字/填底 |
| FX04 | reveal_ice_shard_v003.png | 256×512 | 透明，单片蓝青尖形冰屑 |

额外文件：`reveal_layout_preview_v003.png`（1920×1080实际分层拼合），`readability_proof_v003.png`（120px稀有度、80/140px NEW），`nebula_repeat_proof_v003.png`（每层连续两块循环图，中央为接缝），`validation.json`（尺寸/alpha/基线/接缝/哈希）。

## 生成工具、模型与原稿

- 工具：本会话内置 `image_gen.imagegen`，每个独立素材分别生成；透明项明确请求 `transparent_background=true`。
- 模型：工具未向本会话返回具体模型名，**不推测模型/种子/采样器**。
- 参考：`ArtSource/Effects/F04/RevealConceptR3/reveal_concept_v003.png`，只作为图形/字形风格参考。CH01、四档字图和NEW相关项使用此参考。
- 所有逐项提示词、ID、生成路径见 `generation_jobs.json`；未改动生成原稿，完整复制保留在 `raw_generated/`。
- 无PSD。可复现导出工程为 `export_assets.ps1`（Windows System.Drawing）；只读检查为 `check_assets.py`（Python/Pillow，不修改图片）。
- 生成原始尺寸与交付尺寸不同，见validation.json的native_size/size。4K星空及2K雪花是规范尺寸的放大导出，**不是原生4K/2K生成**。
- 导出处理：高质量尺寸转换、独立元素安全边距、字图统一256px字高及y360基线、灰白遮罩规范、清除低alpha杂点；两层星云降低alpha、上下柔边、左右128px对称融合，循环边界RGBA完全一致。
- 字图是AI生成的位图字形，没有额外字体文件；四档自然字宽不同，不能强制拉成相同字宽。预览中文仅示意，程序继续动态排版角色名字。

## 人物复用与拼合

角色没有重画或重新抠图。预览直接加载项目现有 `ArtSource/UIAssets/Originals/ice_mage_cutout_v001.png`，其sha256记录在validation.json。程序继续使用现有SSR001透明Sprite引用。

预览由本批独立PNG和原始透明角色通过脚本实际拼合，并非再生成一张相似的概念图。人物完整显示于画面中右侧；右侧约20%留作奖励区域；文字放左侧低对比区。NEW预览里的青/紫/粉/金边框是静态着色示意，真正交付UI07保持白色遮罩，流动和沿边亮点由程序控制。

## 检查结果

`validation.json`状态PASS：15项尺寸匹配；BG01 alpha恒为255；其余项有真实透明像素和有效内容。独立元素四边alpha归零，星云上下alpha归零、左右为循环内容。两层星云左右RGBA最大差为0。四档核心字高/底线检查一致（抗锯齿允许3像素误差）。

已查看拼合、小尺寸字图和循环质检图；角色未被雪花遮盖，字图/NEW读数清楚。阴影/软光在透明区域衰减，不包含棋盘格或整块黑白底。尚未进行Unity运行检查，不能将这些素材检查替代程序的动态及Main验收。

## 程序接入建议

1. 只导入表内15项到新的F05目录，由Unity正常产生meta；不要覆盖v002或改旧GUID。raw_generated和预览/证明图不用导入。
2. 星云使用Repeat，建议RawImage的uvRect或材质UV驱动横向滚动；远/近采用不同慢速，先将额外不透明度设为0.45/0.25再调。星云纹理已预降alpha，默认叠加保持克制。若使用Sprite图集，避免循环纹理被紧缩打包裁边。
3. 星空底图使用Clamp和等比Envelope。其他独立元素Clamp、Alpha Is Transparency、Bilinear；字图不要有损压缩。单元素画布含留白，按Rect尺寸等比显示，不把透明边距当有效字高。
4. 四档共用1024×512、内容字高256、字面顶y104/底y360（图像上原点）。实际Sprite Pivot建议均为(0.5,0.5)。选择真实稀有度字图，不按最高档替代每张标识。UR首版淡显即可，若做颜色流动应使用字alpha保持轮廓。
5. UI05独立调色。UI06保持稳定浅金/白文字；UI07按alpha做青→紫→粉→金流动，FX02沿边移动。只有真实IsNew显示NEW。不要把静态预览着色当已完成流动动画。
6. CH01只用于冰法师背后，建议不透明度0.25—0.4；碎屑以FX04复用，缩放/旋转/淡出；流星随机间隔、右向亮头，整体旋转产生斜线，不在底图中重复播放。

涉及Unity概念：Sprite纹理alpha、Wrap Mode Repeat/Clamp、RawImage UV滚动、图层排序和等比适配。调参入口：星云速度/alpha、恒星数量/闪烁周期、流星间隔、雪花alpha与缩放、NEW边框渐变速度。均只作用表现，不改变概率、Draw或收藏。
