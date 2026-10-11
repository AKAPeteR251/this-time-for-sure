# F09-CONTRACT v003 旧化圆环水晶蜡印

用户修订：蜡印更旧；星门/五角星浮雕改为圆环与水晶。已用内置imagegen对v002印面实际编辑，保存新原稿；酒红蜡增加磨痕、浅细裂纹、边缘小缺口与陈旧质感，保持完整印面。中央为同色蜡质切面水晶、外围双圆环，未加蓝色宝石或金属附件。原稿与精确提示词分别见`Source/Raw/wax_aged_crystal_generated_v003.png`及`Source/wax_edit_provenance_v003.json`，未公开参数不猜测。

## 文件与对齐

- `Textures/contract_wax_seal_red_v003.png`：1024×1024真实RGBA独立印面，中心pivot(0.5,0.5)，四边alpha0，超过约10%透明留白。
- `Textures/contract_wax_shadow_v003.png`：1024×1024柔接触影，延用同中心独立层。
- `Textures/contract_parchment_blank_v003.png`、`contract_english_lettering_v003.png`：2048×1536RGBA，延用v002纸张和Allura连笔字。
- `Preview/contract_complete_preview_v003.png`：完整排版；`contract_1280x720_readability_v003.jpg`：720p缩小评审。
- `Source/F09_ContractParchmentWax_v003.ora`：7层母版，旧化蜡印已替换对应独立层；`contract_english_editable_v003.svg`保留可编辑连笔英文。

印面原始生成稿1254方图，技术适配为800方图置于1024画布中心，保留完整边缘；不把重采样像素数当作原生细节。预览依旧使用360×360矩形(1520,1130)—(1880,1490)、中心(1700,1310)。接触影、签名区域和字层对齐规则与v002一致。签名区域(205,922)—(1475,1275)无字，无玩家名字/签名。

字体仍为实际Allura Regular；`Fonts`含字体与原许可，来源https://github.com/google/fonts/tree/main/ofl/allura。精确文案及字号记录`Source/layout_v003.json`；纸与英文逐像素沿用v002，旧v001/v002全部保留。

## 检查与接入

实际执行`check_contract_v003.py`：尺寸、RGBA、四边透明、蜡印安全边、纸核心不透明、准确英文、签名空白与7层ORA结构通过。证据`Source/delivery_checks_v003.json`，SHA256见`Source/delivery_hashes_v003.json`。已查看完整契约和缩小预览；状态待视觉/Unity验收，未改Main或运行资产。

程序可只替换独立蜡印引用；Tint用白色保留酒红，盖印接触时单独显示shadow，随后跟随纸根升空。纸与英文4:3等比，动态用户名及成立金线继续在Unity。涉及Image透明混合、同中心pivot、Tint与分层盖印；主要调印面大小/亮度、接触影透明度和落印动画。图集/视频/新签署流程均未制作。母版原生蜡材质包含自阴影，外部接触影独立控制。
