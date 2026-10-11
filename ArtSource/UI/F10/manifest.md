# F10-CARD-UI — 角色卡面素材交付 v001

状态：素材已制作，待视觉验收、待 Unity 接入。任务依据：`Docs/Production/F10_角色卡面_美术素材清单.md`。本批只写入 ArtSource/UI/F10；未改 Unity 资产、代码或场景。

## 交付内容

|文件|尺寸|用途|
|---|---|---|
|Textures/card_frame_R/SR/SSR/UR_v001.png（4 张）|1024×1536|冰蓝、紫、金、炫彩薄框；同轮廓、同透明内窗，少量晶体角饰|
|Textures/card_rarity_R/SR/SSR/UR_v001.png（4 张）|512×512|固定稀有度字母与小型半透明徽记，外围透明|
|Textures/card_new_border_mask_v001.png|512×192|无文字灰阶 RGBA 边框遮罩，供实时渐变/闪烁|
|Textures/card_new_fill_v001.png|512×192|无文字深靛色半透明内底，与遮罩同画布|
|Textures/card_name_shade_v001.png|1024×256|通用深靛渐隐名字底，顶部和四边透明|
|Textures/card_name_ornament_v001.png|1024×256|弱线条与小晶体，名字文字区留空|
|Preview/card_R/SR/SSR/UR_full_v001.png|1024×1536|四档完整评审图，均使用原雷巫女作校准|
|Preview/card_R/SR/SSR/UR_no_new_v001.png|1024×1536|移除 NEW 的完整评审图|
|Preview/cards_small_readability_v001.png|1280×850|四档分别以 300×450、180×270 实际像素排列|
|Preview/layout_safe_areas_v001.png|1024×1536|排版与安全区示意，仅评审使用|
|Source/card_R/SR/SSR/UR_layers_v001.ora|1024×1536|四份真实 OpenRaster 分层母版，每份 10 层|
|layout.json|—|最终像素布局、坐标原点、统一 pivot 与内窗|
|checks.json / SHA256SUMS.txt|—|真实检查结果及文件校验值|

包：`F10_CharacterCardUI_v001.zip`。所有运行图片是正常直通 alpha 的 RGBA PNG；没有棋盘格、角色、背景、名字或 NEW 文字。只有固定四档字母在各自徽记中烘焙。NEW 与 UR 只交静态基础素材，动画由 Unity 后续实现。

## 排版与调整

左上为原点，矩形为（左、上、右、下），单位像素；具体值以 layout.json 为准。

|层/区域|最终矩形|
|---|---|
|框|（0,0,1024,1536）|
|透明内窗|（48,64,976,1472）|
|NEW 两层|（72,64,392,184）|
|稀有度|（768,56,984,272）|
|名字底与装饰|（48,1250,976,1472）|
|元素图标|（72,1336,224,1488）|
|名字文字安全区|（264,1352,920,1472）|
|脸部建议安全区|（170,280,854,900）|

内窗较清单建议略收窄，以保留细线框与角饰；四档严格相同。原角色画只等比裁切填入内窗，不拉伸、不重绘。元素图标依清单独立叠放，因此允许图标下部跨过内窗至框内留白区。名字预览用 96px 霞鹜文楷，2—6 字可在 656px 安全宽度内排版；长名字应由后续动态文字适配。NEW 预览用 66px Cinzel，文字居中，运行时独立显示。

ORA 中原角色画、名字底、名字装饰、复用图标、名字文字、卡框、徽记、NEW 内底、NEW 遮罩和 NEW 文字各为单独图层。角色、图标、名字、NEW 字样均标为 preview only，不应作为通用运行素材导入。

## 来源、参数和许可

- UI 图形为本批原生绘制的路径、细线、晶体分面与渐变；实际在本机 Python/Pillow 执行，采用 2 倍抗锯齿后导出。未调用图片模型重新生成角色，也未使用参考图中的重绘角色。
- 风格依据：`ArtSource/UI/CardConcept/card_sr_thunder_reference_v001.png`，只参考层级与细边框方向。
- 校准角色：`ArtSource/StyleTests/RPG_Professions/05_thunder_miko_v001.png`，原文件保持不变；副本在 Source/PreviewInputs。四档同一人物仅用来比较 UI，不修改角色的实际稀有度。
- 雷图标：复用 `这次一定出/Assets/_Game/Art/Effects/UIAssets/element_thunder_v001.png`。其余元素继续使用现有图标，本批不替换。
- 固定稀有度字母与 NEW 预览字体：Cinzel，可变字体 weight=700。实际字体 `Source/Fonts/Cinzel/Cinzel[wght].ttf`；来源 [Google Fonts 官方 Cinzel 目录](https://github.com/google/fonts/tree/main/ofl/cinzel)，随包保留原 `OFL.txt`（SIL Open Font License 1.1）。
- 中文名字预览字体：LXGWWenKai-Regular / 霞鹜文楷，直接复用工程已有字体与许可，随包保留 `Source/Fonts/LXGWWenKai/`。来源 [霞鹜文楷项目](https://github.com/lxgw/LxgwWenKai)，许可见该目录 OFL 文件。
- 颜色、字重、抗锯齿和制作参数保存在 Source/design_parameters_v001.json；完整可重复生成源在 Source/build_card_ui_v001.py。Source/Vector 的 SVG 只是内外框基础轨道，不包含完整晶体角饰；完整图层在 ORA，全部几何参数在生成源。
- Source/Raw 保留首次导出稿。最终 Textures 另修正了抗锯齿缩采样造成的内窗边缘残留 alpha，原始初稿未覆盖。现有参考与人物原稿未覆盖。

## 实际验证与限制

真实逐项检查见 checks.json：12 张尺寸/通道/四周透明、透明像素无隐藏 RGB、四档完全一致 alpha 轮廓、内窗 alpha=0、NEW 灰阶与画布对齐、名字装饰不侵入文字区域、渐隐上缘、四份 ORA 的 10 层结构、逐层重新合成与交付预览逐像素一致、校准人物及元素副本与原稿 SHA256 相同。交付 ZIP 另通过 CRC 完整性检查。

已人工查看 180×270 与 300×450 对照，稀有度字母、NEW 和名字可辨；去 NEW 版本单独交付。未在外部绘图软件实际打开 ORA，已通过文件结构与重新合成验证。未导入 Unity，未验证 Main 游戏运行；状态不能标为已验收。

## 后续接入与调参

涉及 Unity 概念：Sprite 透明叠层、统一中心 pivot、2:3 等比显示、独立 TMP 动态文本、遮罩 UV 渐变。主框不做九宫格拉伸。将 Textures 导入为 Sprite，徽记/NEW/名字各用独立引用，按 layout.json 换算 RectTransform；PNG 的四周透明空间需保留。

动态 NEW 显示跟随真实 NEW 状态；姓名跟随角色数据。UR 炫彩与 NEW 流动需后续 Shader 接入，不能把本包静态预览当成已实现动画。调节时优先改变原生尺寸上的文字字号、标签位置、名字底透明度、遮罩渐变速度和强度，不扩大角饰侵入内窗。具体 Unity 修改由另立程序任务承担。
