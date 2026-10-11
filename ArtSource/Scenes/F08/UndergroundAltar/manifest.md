# F08-ALTAR 地下圣殿祭坛 v001

日期2026-10-10。依据`Docs/Production/F08_地下圣殿祭坛_美术素材清单.md`与已给出的地下圣殿参考图。状态：实际模型/贴图/预览已交付，FBX回导检查通过；待视觉评审及后续独立Unity集成任务验收。只制作环境，不修改Main、Shader、F07原件或抽卡流程。

## 模型、单位和模块

正式入口`Models/F08_UndergroundAltar_v001.fbx`；可编辑母版`Source/F08_UndergroundAltar_v001.blend`。127独立Mesh、128节点（含根），84,888三角面。不是参考图平面，实际有完整24段厚墙、闭合0.5米厚穹顶、祭坛、模块地板、石柱和火盆。

根`F08_UndergroundAltar`位于(0,0,0)、scale1；地面高度0。1 Blender单位=1米。室内半径15m、墙高10m；穹顶内顶13.2m，厚0.5m；圆形祭坛三层半径8/7.65/7.3m，高度0.18/0.36/0.54m。相对30m直径使用浅圆穹顶；保留宽而浅的台阶，不新增水晶底座。

| 节点 | 拆分内容 |
|---|---|
| Altar_Base、Altar_Steps_Lower/Upper | 独立台面与两层台阶，中心pivot |
| Altar_Inlay_Metal_*、Altar_Inlay_MetalTick_* | 普通古金几何镶边与刻度 |
| Altar_Inlay_Emission_* | 三条独立发光仪式轨，程序可分别调亮度 |
| Floor_Module_00…23 | 扇区石砖模块、统一世界对齐的平铺UV，中心区域由祭坛封住 |
| Wall_Module_00…23 | 全周连续厚墙，包括完整后墙；底部pivot |
| Wall_Alcove_Back_*、Wall_Alcove_Arch_* | 六组暗凹龛及独立厚石拱框；凹龛背后仍有完整墙 |
| Pillar_Module_00…07、Pillar_GoldTrim_* | 八套厚柱与独立古金圈，底部pivot，可复用 |
| Ceiling_Dome、Ceiling_Ribs、Ceiling_GoldInlay_* | 闭合穹顶、同心石梁、金色细边分别控制 |
| Brazier_Base_00…03、Brazier_Bowl_00…03 | 两侧主火盆与后侧两小火盆，石座与空心金属盆分别控制，底部/盆底pivot |
| Wall_Emblem_00…05 | 一种几何双环挂饰复用，无字无Logo |

网格无未应用修改器/几何节点。旋转与缩放应用入顶点，各运行节点尺度为1；无负尺度。墙、地板、柱、祭坛、穹顶有几何厚度。法线重算，提供可平铺UV0。没有非重叠UV1，若烘焙静态灯光，由Unity模型导入勾选Generate Lightmap UVs；本批材质不烘焙灯光。

导出：Forward=-Z、Up=Y、global_scale1、apply unit scale、FBX_SCALE_UNITS，space transform开启，bake space transform关闭；仅EMPTY/MESH、无动作。实际另开空场景回导FBX：名称/层级一致、各Mesh顶点数与材质槽一致、有UV，所有scale1；包围盒角点最大误差约0.00000344m。证据在`Source/model_checks_v001.json`及`Source/build_v001.log`。没有宣称Unity导入实测。

## F07复用与空间

母版`F08_REFERENCE_Only`含F07的未修改机构，仅根沿Z上移0.585m，底环落到祭坛台面附近。F07几何、材质、比例不改；环境FBX明确不导出这些参考物体，运行时使用原F07 FBX，避免双份水晶/环。预览包含F07只是对照，远景水晶原尺寸保留，另交近景。

石柱中心半径12.9m、最宽半径0.83m；中心立环以原4.35m半径级别为参照，墙距15m，柱内边距约12.07m。环正常倾斜范围的空间余量充足；全角度实际动画/视锥和台面相交仍需P19/后续集成验证，不把静态摆位当作Unity动画验收。

## 贴图和材质（全部实际交付）

石材与金属各4张2048 PNG：

- `stone_basecolor_2048_v001.png`、`antique_gold_basecolor_2048_v001.png`：sRGB颜色，无阴影/光源；石材深蓝灰，古金低饱和暖色。
- `*_normal_2048_v001.png`：切线空间OpenGL +Y法线；Unity按Normal Map导入。不是彩色灯光图。
- `*_metallic_2048_v001.png`：单通道灰度，linear；石材0、古金约0.86。
- `*_roughness_2048_v001.png`：单通道灰度，linear；石材约0.76、金约0.38，微弱纹理起伏。URP Smoothness=1-Roughness；未打包到未知Mask格式。

`F08_DeepBlueStone`/`F08_AntiqueGold`使用上述实际贴图，平铺Repeat。`F08_NicheShadowStone`为简单深色石材参数BaseColor linear(.007,.012,.025)、Metallic0、Roughness.7。`F08_AltarInlayEmission`为独立线网格，Base/Emission linear(1,.5,.13)、Metallic.2、Roughness.4、参考Emission strength1.5，Unity可直接控制。

`altar_emission_mask_2048_v001.png`：2048单通道灰度，纯黑底/白线，原创同心轨、刻度、几何菱形，无辉光。可作为祭坛额外平面/投影的发光遮罩，UV(0.5,0.5)为中心；它不是Mesh上三条简单发光轨的逐像素UV烘焙。程序按祭坛平面独立叠加或只复用现有发光网格，避免重复纹样过亮。

`flame_amber_flipbook_4x4_2048_v001.png`：真实RGBA，16帧，每帧512，4×4图集，从左上到右下按行顺序；参考12fps，循环1.333秒。解析周期相位i×2π/16，虚拟第16帧回第0帧；不能把第15帧误作重复首帧。每格四边alpha为0，均有部分透明像素；独立柔化琥珀火焰，浅黄芯，不含盆/背景。Unity用Texture Sheet Animation 4×4，确认粒子系统UV起点/行方向；若倒序按视觉预览纠正，不能默认Blender图像坐标与粒子索引一致。Wrap Clamp，关不必要的Mipmap或检查小火盆距离的图集串色；使用粒子材质控制亮度/软粒子。最终动画不是FBX灯光驱动。

`floor_mist_1024_v001.png`：1024 RGBA中性灰白软雾，alpha最高约.45；`dust_soft_512_v001.png`：512 RGBA软尘点。边缘alpha0，无固定环境光，供粒子少量贴地铺开/漂移。预览没有强雾遮中心，运行中雾的运动由Unity控制。

母版已打包图像，独立贴图另放`Textures`；FBX材质槽/路径不保证URP自动等效，程序需用外部贴图重建URP Lit。BaseColor sRGB，其余数据图取消sRGB；Normal Map另按导入类型处理。没有AO图，未用通道不凑图。

## 预览与复现

- `Preview/front_gameview.png`：1280×720，封闭室内正面，能见后墙、浅穹顶、祭坛及冷暖对比。
- `Preview/crystal_closeup.png`：同分辨率，靠近原尺寸水晶的互动构图，外圈可自然出框。
- `Preview/side_structure.png`：诊断剖视图，仅渲染时隐藏近侧墙和顶盖以查看结构；交付母版/FBX墙顶全部恢复完整，不能把此图当完整游戏画面。
- `Preview/flame_loop_preview.gif`：16帧图集循环参考，深色底仅预览。

EEVEE48samples、Filmic/Medium High Contrast、曝光+.35、AO/SSR、Bloom参考.045。正面相机(0,-13.7,7.2)，看向(0,0,5.6)，17mm；近景(0,-9,5.3)，看向(0,0,4.8)，26mm；侧视(24,-18,20)，看向(0,0,4)，26mm。灯光仅母版`REF_*`：四盏琥珀点光600W、晶体冷色面光1500W、房间冷色面光1800W等。单位/强度不能直接照搬Unity；参考位置颜色可从母版读取。

## 来源、检查、限制与接入

参考图仅指导色彩/空间方向，没有把它贴在墙面或拷贝其中新水晶。原创模块通过Blender脚本建模，技术PBR数据图与循环特效由确定性脚本制作，seed8081010；没有使用AI图片生成、外部下载或不明素材。源脚本为`Source/build_textures_v001.py`和`Source/build_altar_v001.py`；模型构建读取项目F07母版作参照，交付母版/FBX不依赖其路径。原参考与F07保持原样。

贴图实际检查见`Source/texture_checks_v001.json`：12张文件，尺寸正确；16火焰格真实透明；雾/尘埃四边alpha0；逐贴图SHA256保留。全部交付文件哈希见`Source/delivery_hashes_v001.json`。几何回导与三预览实测，不代表视觉认可、材质Unity等效或Main验收。

后续独立集成任务由程序导入FBX、让Editor产生.meta，先验证材质和模块，再替换水晶室内环境。模型不含相机/灯光/F07，运行应单独放回F07。涉及Unity概念：FBX层级/Pivot、URP Lit与粗糙度转换、Lightmap UV、Texture Sheet Animation、软粒子和相机/uGUI合成。调参顺序：先锁房间/F07同一尺度与镜头，后调冷暖灯光/Emission，再调火焰大小、12fps循环和低雾密度；动画流程/唯一Draw/跳过/逐张确认不在本轮修改。

限制：是可集成的首版模块环境，未做布幡、火星、烟气或额外光束；未进入Unity验证三窗口和环运动碰撞，未做水晶最终破裂新动画；UV1待程序按需生成。任务状态待验收。
