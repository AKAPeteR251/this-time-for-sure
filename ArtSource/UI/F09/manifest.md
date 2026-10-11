# F09-CONTRACT 旧羊皮纸契约与红蜡印 v001

2026-10-11。按`Docs/Production/F09_旧羊皮纸契约与红蜡印_美术素材清单.md`完成实际制作，状态待视觉/Unity集成验收。本批不修改Unity Assets、Main、程序引用或已有签署流程。

## 五张必做素材

| 路径 | 尺寸/通道 | 内容 |
|---|---|---|
| Textures/contract_parchment_blank_v001.png | 2048×1536 RGBA | 正视横版旧羊皮纸，磨损边外透明、核心纸内不透明；轻折痕、淡外围污渍和褪色角饰；无字、无签名、无蜡 |
| Textures/contract_english_lettering_v001.png | 2048×1536 RGBA | 准确英文单独层，和纸同画布；无名字、按钮或假签名 |
| Textures/contract_wax_seal_red_v001.png | 1024×1024 RGBA | 酒红真实蜡印、原创星门/五角星浮雕、自然不规则蜡液边；约11%透明安全边 |
| Textures/contract_wax_shadow_v001.png | 1024×1024 RGBA | 单独柔和接触影，同中心；不是远距离强投影 |
| Preview/contract_complete_preview_v001.png | 2048×1536 RGBA | 合成评审示意；主签名区空白；不作为唯一运行素材 |

各RGBA四边alpha均0，不含桌面/房间或棋盘底。纸与蜡用柔和左上参考光，保留中间调，无Bloom。纸面少量角饰已并入空白纸，不另做金属/发光border。蜡印本体不带独立落影，接触影单层控制。

## 可编辑母版及动态区域

`Source/F09_ContractParchmentWax_v001.ora`为OpenRaster等效分层母版，7层可独立编辑/隐藏：纸面材料、软折痕、外围污渍、褪色角饰、英文、接触影、红蜡印。各图层也保留在`Source/Layers/`，不是只交合并稿。生成的纸纤维与自然纸边磨损是底材自身；额外折痕/污渍/角饰为独立可编辑层。

`Source/contract_english_editable_v001.svg`为可编辑文字矢量源，嵌入实际字体；`Source/layout_v001.json`记录字体大小、准确文本、基线与逐段边界。ORA中的字层为字体栅格稿，改字应编辑SVG/源脚本再生成，不能把栅格字假称为文本对象。

签名安全区域：左上(205,922)、右下(1475,1275)，即画布约x10%—72%、y60%—83%。实查此区英文层alpha全0。用户名、手写墨迹、成立金线均未烘焙。签名提示`Sign your name`位于此区下方y1280，品牌与标题在上部，正文到y830结束。

预览蜡印/接触影使用同一360×360 Sprite矩形(1520,1130)—(1880,1490)，中心(1700,1310)；实体印面更小，位于右下而不压主签名。两张1024源图中心pivot(0.5,0.5)，在Unity同时等比缩放、移动；不能先显示盖好章的评审合成稿给未签字玩家。

`Preview/layout_safe_areas_v001.png`标注正式边界，仅交接用；`Preview/contract_1280x720_readability_v001.jpg`是在1280×720画布上放880×660契约的实际缩小评审稿，不代表Unity运行截图。正文缩小后仍可读，需后续实际窗口验证。

## 准确文字和字体

逐字采用任务单：`Star Echo`、`Covenant of the Stars`、`By this name, I awaken the echoes`、`that slumber among the stars.`、`Let this covenant be sealed,`、`with the stars as witness.`、`Sign your name`。标点、换行由实际字体排版，不使用生成图中的假英文。

标题Allura Regular 154px，品牌Caveat 62px，正文Caveat 82px/weight500，提示Caveat50px。墨迹深褐且有轻微透明度起伏，不改变字形或拼写。

字体来源：[Google Fonts Allura](https://github.com/google/fonts/tree/main/ofl/allura)、[Google Fonts Caveat](https://github.com/google/fonts/tree/main/ofl/caveat)。实际字体及原始OFL许可均随包：`Fonts/Allura/Allura-Regular.ttf`、`Fonts/Allura/OFL.txt`、`Fonts/Caveat/Caveat[wght].ttf`、`Fonts/Caveat/OFL.txt`。本批不购买字体、不修改字体文件；原许可保留。中文动态用户名继续由项目现有LXGWWenKai/中文字体回退处理，不能用本批拉丁字体替换中文回退。

## 实际制作和来源

使用imagegen技能的内置`image_gen.imagegen`：先生成纸和蜡原稿，再将纸稿去除明显折痕/污渍/角饰，取得可分层底材。精确提示词、工具与来源记录见`Source/imagegen_provenance_v001.json`；所有原始稿在`Source/Raw/`。工具未公开实际底层模型版本、seed、steps、CFG，记录为未公开，不猜测。蜡徽记为本轮原创几何星门/五角星，非现实家族纹章或已有游戏Logo。

源纸1448×1086、蜡1254×1254；最终尺寸通过保持比例的技术重采样与透明边距适配。纸放到1920×1440后置于2048×1536画布中心；蜡放800×800后置于1024方图中心，保证完整边缘与留白。导出把alpha≥240规范为255、alpha<3清零，消除生成底材近乎不透明但不是完全不透明的核心；保留原始稿对照。像素母版达到要求尺寸不意味着原生生成了2048细节。

额外折痕/污渍/角饰和柔接触影由可重复源脚本作为新独立图层作者设计，准确文字使用实际字体。`Source/assemble_contract_v001.py`可复跑排版和ORA/SVG；不覆盖已有Unity资产。后续修订请升版本，保留本批原稿。

## 实际检查与Unity接入说明

`Source/check_contract_v001.py`已运行：4张运行图尺寸与真实RGBA通过、四边alpha0、纸核心alpha255、蜡约10%留白、英文源字符串与SVG逐字一致、签名区无字、7层ORA结构完整。逐文件证据见`Source/delivery_checks_v001.json`，全部交付SHA256见`Source/delivery_hashes_v001.json`。已目视合成稿与720p缩小稿；未做OCR、未声称Main已经接入或实机验收。

程序另建接入任务：保持4:3等比，不拉伸写实纸纹；纸/英文同RectTransform尺寸和原点；图层顺序纸→英文→动态签名→接触影→红蜡印。装饰Image关闭Raycast Target；纸/蜡导入Alpha Is Transparency，Clamp/Bilinear、中心pivot，由Editor生成.meta。纸不要九宫格拉伸。英文替换原中文标题/正文时删除旧字以免叠字。

红蜡印入口沿用starSeal，但现有金黄色Tint要改为中性白色；落纸时再显示接触影，未接触时隐藏，升空时两者随同一纸根移动。阴影强度和落印缩放由Unity控制；不能用完整预览代替签署页。保持松笔继续、跳过补签、唯一Draw、同批结果跳过以及中文用户名回退。

涉及Unity概念：Image层级/透明混合、RectTransform同画布对齐、CanvasScaler等比、Tint颜色乘、TMP字体回退与运行手写层。调参先锁纸Rect与签名坐标，再调正文显示比例、蜡印360px参考大小、接触影透明度和盖印落点。当前只交素材，纹理/文字实际窗口可读性与签署/盖印动画待程序验证。
