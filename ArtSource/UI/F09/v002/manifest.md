# F09-CONTRACT v002 连笔艺术字修订

按用户“手写字体换成连笔艺术字”要求，品牌、正文和签名提示全部由Caveat改为Allura Regular连笔花体；标题继续Allura。完整v001保留，未改Unity代码或场景。本版待视觉/集成验收。

实际字号：品牌74px、标题154px、正文100px、签名提示64px。用实际字体排版，没有重新生成英文字形；深褐墨迹与轻微透明度变化保持。确切文本和签名空白仍沿用F09清单。

运行素材：`Textures/contract_parchment_blank_v002.png`、`contract_english_lettering_v002.png`（均2048×1536 RGBA）；`contract_wax_seal_red_v002.png`、`contract_wax_shadow_v002.png`（均1024×1024 RGBA）。纸、蜡印、接触影像素与v001一致，字层重新绘制。四边透明，纸核心不透明，蜡印约10%安全留白。

预览：`Preview/contract_complete_preview_v002.png`、`contract_1280x720_readability_v002.jpg`、`layout_safe_areas_v002.png`。签名区域(205,922)—(1475,1275)字层alpha全0；盖印矩形和中心保持360×360、中心(1700,1310)。纸和字同画布原点，动态用户名不烘焙。

可编辑母版：`Source/F09_ContractParchmentWax_v002.ora`保留纸面、折痕、污渍、角饰、英文、接触影、蜡印7层；字形编辑源`Source/contract_english_editable_v002.svg`嵌入字体。`Source/layout_v002.json`记录文本、字号与准确基线，`assemble_contract_v002.py`可重建。源图复用本批Raw中保留的v001生成原稿；未新增图像生成，原imagegen提示词/来源随包保留。

实际字体`Fonts/Allura/Allura-Regular.ttf`与原OFL许可随包；来源https://github.com/google/fonts/tree/main/ofl/allura。Caveat与许可仅为技术安全区示意文字/历史来源保留，运行契约英文均为Allura。中文动态用户名仍使用项目既有中文字体回退。

已运行`check_contract_v002.py`：准确英文与SVG逐字一致；四张尺寸、RGBA与透明边界通过；签名区无字；7层ORA结构通过。实际查看720p縮小预览，连笔正文仍可辨认。验证证据`Source/delivery_checks_v002.json`，文件哈希`Source/delivery_hashes_v002.json`；未声称Unity已接入。

程序接入只需换新英文层（或整套版本引用）；纸/英文同Rect和比例，使用中性Tint保留褐墨和酒红。接触影在盖印贴纸时显示，完整合成图仅评审；不要把已盖印预览当未签署页。涉及Unity：Image独立字层、等比布局、Tint、动态签名与字体回退。调参先锁字体显示比例，后看1280×720与16:10可读性；本轮无需更改手写流程、唯一Draw或跳过行为。
