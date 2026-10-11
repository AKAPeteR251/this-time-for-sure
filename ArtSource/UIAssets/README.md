# A04-UIASSET：UI 素材交接

状态：素材制作与本地 PNG 检查完成，待 Unity 导入及运行验收。

## 交付内容

新增 24 张独立素材：1 张无人物无 UI 的冰城背景、1 张透明冰法师立绘、4 种稀有度卡框、4 种按钮状态、2 张信息面板、10 张图标与装饰、2 张星芒特效。另外复制了已认可的六职业稿及六张缩略图供原型使用，共 36 张最终素材。

| 素材 | Unity 美术目录 | 使用版本 |
|---|---|---|
| 场景背景 | `Assets/_Game/Art/Backgrounds/` | `UI_FrozenCastle_v001.png`，1920×1080 |
| 透明冰法师 | `Assets/_Game/Art/Characters/UIIceMage/` | `UIIceMage_character_v001.png`，1024×1536 |
| 六职业卡面与缩略图 | `Assets/_Game/Art/Characters/UIMockRoles/` | 精灵采用 v002，其余 v001 |
| R / SR / SSR / UR 卡框 | `Assets/_Game/Art/Effects/UIAssets/` | `frame_*_v002.png` |
| 按钮四状态 | 同上 | `button_*_v002.png`，统一 832×336 |
| 信息与提示面板 | 同上 | `panel_*_v001.png` |
| 导航、资源、元素与装饰 | 同上 | `icon_*`、`element_*`、`deco_star` 的 v001 |
| 蓝色与金色星芒 | 同上 | `fx_sparkle_*_v002.png`，512×512 |

`asset_manifest.json/csv` 列出每个最终路径、实际尺寸、SHA256、来源与检查结果，开发按此清单取图。初次切分的卡框、按钮、星芒 v001 仅保留制作记录，使用上表版本。不得自动选最早文件或全文件夹加载。

原稿在 `Originals/`，生成提示词在 `Prompts/`，原始工具路径在 `generation_sources.json`。使用内置 image_gen.imagegen；接口不公开实际模型版本、seed、CFG 或采样参数，不将其猜测为 image2.5 或 ComfyUI 输出。

## Unity 导入与 UI 使用

PNG 导入为 Sprite (2D and UI)，Sprite Mode Single；不用再次从图集中切分。透明素材启用 Alpha Is Transparency，Filter Mode Bilinear，Wrap Mode Clamp。建议纹理压缩先关闭，背景最大尺寸至少 2048；冰法师保持整画布，不紧裁人物层。由 Unity Editor 自动创建 .meta，不手写 GUID。

装饰与背景 Image 关闭 Raycast Target。文字、稀有度字样、NEW、资源数量和按钮标题由 TMP 单独显示。六职业稿是原型美术，角色 ID 和稀有度仍读取正式数据；不要把职业图片名当角色 ID，也不要把六种职业硬编码为正式卡池。

按钮使用 Image Type Sliced，四种状态建议先保持固定高度，只做横向拉伸。Sprite Editor Border 起点可设左/右 90、上/下 70，再检查实际边角与光晕；这些是调参起点，尚未在 Editor 验证。信息面板边框起点为左/右 70、上/下 50，过度纵向拉伸会拉长内部装饰线，需要实际预览。Button Sprite Swap 分别绑定 normal、selected/highlighted、pressed、disabled；如高亮不需换底，直接使用 Color Tint。

卡框用 Simple Image 与 Preserve Aspect。卡框生成几何并非四张精确相同内窗，需分别校准立绘遮罩和边框位置；立绘保持 2:3，不能以卡框 PNG 画布比例拉伸角色。结果页仍为五列两行，详情页可放大透明立绘，背景独立使用。大卡框不作九宫格拉伸。

导航图标建议显示 32—48 像素，元素图标 24—40 像素。当前是带装饰的较细图标，小尺寸下先减弱 glow 或换用 UI 简化图形。星芒可用于 UI 淡入、缩放与旋转，也可作为粒子纹理；图本身没有动画。共鸣可复用边框和星芒，不新增人物服装。

## 实际验证与限制

- 实测 23 张新素材含真实透明像素；不是图片中画出来的棋盘格。四张卡框中心 alpha 均为 0。
- 本地 QA 将立绘叠到背景并将所有素材叠到棋盘格：人物轮廓、卡框内窗与按钮均可分离；无整块烘焙背景。预览在 `QA/`。
- 背景原稿 1672×941，导入稿重采样到 1920×1080，原稿完整保留；不声称提高了原始细节。
- 按实际边界修正卡框切分，避免下排顶部宝石被均分网格裁掉；按钮状态统一画布。导出脚本为机械复制、网格裁切、画布与尺寸转换，不重画内容。
- 图标边缘仍有很淡的生成光晕，冰元素底部存在小色点；正式 UI 可在实际大小下评估是否需要进一步精修。
- 冰法师是整人物透明层，头发、身体和衣服没有拆成可独立动画的部件；可以整体视差，不能当作 Live2D。
- 六职业卡面为 768×1152 风格稿，缩略图 256×384，仍有此前交接单记录的道具与裁切缺陷，不冒充十二角色正式母版。
- 尚未在 Unity Editor 创建 Sprite Border、Prefab、页面或实际播放验证，不标记为已验收。

## 涉及的 Unity 概念与如何调参

Sprite 是 UI 图片引用；Preserve Aspect 保持立绘比例；Mask 控制卡面窗口；九宫格 Border 固定按钮边角；Canvas Scaler 适配分辨率；Button Sprite Swap 展示交互状态。先调卡面窗口与按钮高度，再调 Border 和图标尺寸，最后调星芒透明度与动画时长。检查 1920×1080、1280×720 和 16:10 后由 UI 开发验收。

运行导出与 QA：使用本机已有 Pillow 的 Python 执行 `export_a04.py` 和 `preview_a04.py`。导出可重复运行，相同内容会复用；不同内容不覆盖旧资产，需升版本。

下一步依赖：UI 按素材清单接入三页，与程序正式数据绑定并运行最短抽卡闭环；其他职业透明立绘、十二角色正式卡面与精细人物分层属于后续批次。
