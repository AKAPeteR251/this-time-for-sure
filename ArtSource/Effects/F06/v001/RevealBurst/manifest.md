# F06-ART-REVEAL-BURST v001

2026-10-10交付。依据 `Docs/Production/F06_ART_REVEAL_BURST_可选素材清单.md`。实际PNG已生成、文件与透明检查通过；视觉及Unity集成状态为待验收。只补可选美术，不修改P16、Main、Shader、代码或场景。

| 文件 | 尺寸 | 方向/设计 |
|---|---|---|
| reveal_burst_shard_A_v001.png | 512×1024 | 尖端朝上，尖长非规则薄片，立体折面 |
| reveal_burst_shard_B_v001.png | 512×1024 | 尖端朝上，宽破碎菱片，非对称锯齿边 |
| reveal_burst_shard_C_v001.png | 512×1024 | 主尖端朝上，弯折细长轮廓、侧向分叉 |
| reveal_burst_streak_v001.png | 256×1024 | 亮端朝上，尾部朝下渐细、渐隐，独立单束 |

三碎片为灰白/浅蓝中性明暗面，供程序按稀有度调色。没有文字、人物、Logo、光尾烘焙或整圈构图。预览深蓝底仅用于评审，四个PNG均为真实RGBA，非棋盘格/硬矩形背景。光尾不与碎片合并。

## 来源、母版与复跑

原创程序化几何与材质，由本机Blender 2.93.4、EEVEE、64samples实际渲染。未截取任何现成游戏素材，未使用生成模型/LoRA/VAE；这些模型参数、seed、采样器、CFG不适用。未取得清单提到的两张参考截图，本批按文字所述方向、层次与轮廓原创设计。

`Source/F06_RevealBurst_v001.blend`保留四个独立对象、切面材料、柔化光尾节点与正交相机。`Source/build_reveal_burst_v001.py`为制作源，`Source/parameters_v001.json`记录实际参数，`Source/render_v001.log`记录渲染。`Source/Raw/`保留未经处理的原始渲染稿，最终PNG未经过抠图、扩边或图像修补。

使用Blender：`--background --factory-startup --python Source/build_reveal_burst_v001.py`可重建母版及四PNG。当前v001交付后修订应升版本，不覆盖原稿。使用本机Python/Pillow/numpy运行`Source/check_reveal_burst_v001.py`可重复检查与生成预览。

## 实际检查

四文件尺寸逐项匹配；全部RGBA；四边每像素alpha为0；非透明像素包围盒到各边均至少约8%；有抗锯齿/柔化的部分透明像素。逐文件SHA256、包围盒和透明像素记录在`Source/alpha_checks_v001.json`。目视拼图确认三碎片形状明显不同且存在明暗切面，光尾为独立柔化单束。

检查不替代实际Unity运行验收；本轮不声明P16已使用新版素材。正式.meta由Editor产生，未手填GUID。

## Unity概念与调参

程序导入时建议Sprite(2D and UI)、Alpha Is Transparency、Wrap Clamp、Bilinear、中心pivot(0.5,0.5)、Full Rect，保持原始画布；装饰Image关闭Raycast Target。尖端方向为图片上方，径向运动角度由程序统一旋转；纹理不提供整圈位置。

涉及Image色乘、Sprite导入、alpha混合与独立Transform。A/C与B混排避免外圈全是细线；用程序调宽高、随机旋转、径向位移、淡出。光尾单独调长度、透明度及亮度，放在碎片后方；其源图最大alpha约为柔光中间值，增强可在程序中调整，避免误认硬边矩形。不要把预览JPG导入运行层。中性色乘会保留切面明暗，但不能把深面直接乘成完全黑色。

限制：碎片是静态单元素，运动和双段爆发时间由P16控制；不含新内圈打光，不扩展角色/稀有度徽记。本批未改任何Unity资产。
