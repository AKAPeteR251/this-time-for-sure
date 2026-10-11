# F07-ART-CRYSTAL-LAYERS v001

2026-10-10。当前：3D素材已制作、FBX已实际导出和回导检查；待用户视觉评审及P19 Unity实际导入/运行验收。本轮采用任务单最新3D方案，六张旧PNG不是必需项。未修改Unity Assets、Main、代码或Shader。

## 文件与来源

- `F07_CrystalStage_v001.fbx`：正式静态导入入口，26节点、24独立网格、31,312三角面。仅EMPTY/MESH，无相机、灯光、动作、UI、用户名或背景星点。
- `Source/F07_CrystalStage_v001.blend`：另存可编辑母版，`F07_RUNTIME`为交付对象，`SOURCE_REFERENCE_NotForExport`隐藏保留原裂纹曲线、晶壳碎裂关键帧与旧舞台参考，`F07_REVIEW_Only`为参考相机/灯光。
- `Source/build_crystal_stage_v001.py`：实际拆分、UV、导出与回导制作脚本。
- `Source/export_checks_v001.json`：实际节点、材质槽、各碎片完整状态局部位置、包围盒、UV、相机/灯光、回导证据及FBX/blend SHA256。
- `Source/build_v001.log`：实际Blender执行记录。
- `Textures/material_parameters_v001.json`：所有实际纯色/金属/发光材质参数。未使用图像贴图，不凑Base Color/Normal/Metallic/Emission空图。
- `Review/F07_stage_blender_v001.png`：1280×720完整Blender舞台预览，底环完整入镜。
- `Review/F07_node_hierarchy_v001.png`：从实际导出及回导清单绘制的节点层级核对图，不冒充Outliner界面截图。

来源：优先读取既有 `ArtSource/Scenes/SummonStage/v004_click/A05_ClickCrystal_v004.blend`，实查12块晶壳后从第62帧完整状态拆出；原母版未修改。沿用原金色巨环、冰蓝晶体、金晶形状，新增冰晶B的宽窄不对称轮廓。所有交付几何来自项目原创母版或本轮脚本变形，未从合成截图抠模型、未截取其他游戏素材。实际使用Blender 2.93.4 / EEVEE，不涉及模型/LoRA/CFG/采样器。

## 节点、中心与轴向

根`CrystalStage`，尺度(1,1,1)，无动画。各网格已将原旋转/缩放烘入顶点，运行节点不含负尺度或非均匀尺度。

| 节点 | 内容/旋转中心 | 动画消费者 |
|---|---|---|
| RingOuter | 完整外巨环三轨/刻度/8棱饰，中心原世界(0,1.05,4.4) | 独立慢旋 |
| RingMiddle | 独立中环三轨/刻度/8棱饰，中心(0,0,4.4) | 可与外环同向不同速，不能误当重复内环 |
| RingInner | 完整内环三轨/刻度/8棱饰，中心(0,-0.8,4.4) | 反向/不同速 |
| RingFloor | 五条完整底环与24刻度，中心(0,0,-0.045) | 水平慢旋/衔接前奏 |
| CrystalBody | 中央水晶组，中心(0,-0.1,4.4) | 浮动、倾斜、自转、蓄能震动 |
| CrystalBody/Shard_00…11 | 12个各有4顶点/4三角面的封闭四面体独立晶壳；各自重心作pivot，局部位置拼回完整水晶 | 程序可逐片访问；完整状态transform见JSON |
| FloatingIce_A_00、A_02 | 原细长六边双锥冰晶，各自重心pivot | 独立轨道/自转 |
| FloatingIce_B_01、B_03 | 宽、不对称冰晶，同风格不同轮廓，各自重心pivot | 独立轨道/起伏 |
| FloatingGold_00…03 | 四个金色棱块独立网格 | 少量穿插的不同轨道 |

Blender单位米，1单位=1m。原舞台底环最大直径约12.6m，外环直径约8.7m；Unity可在根上统一正值等比缩放，不单独拉伸环。FBX导出Forward=-Z、Up=Y、global_scale=1、单位转换开启、`FBX_SCALE_UNITS`、space transform开启、bake space transform关闭、bake animation关闭。Unity通常使用Y为上；请以实际导入验证轴向，不能凭Blender原坐标把旋转轴写死。底环应绕舞台竖直轴转，立环应绕其法线转。JSON的位置为Blender坐标；Unity以导入transform为准。

环几何完整360°，不是旧相机可见部分。中心及法阵未与其他环合并。裂纹曲线/漏光/碎裂旧关键帧仅保留隐藏源参考，不在静态FBX中强制播放；本轮未新增第3项光束或第4项爆裂动画。不要把十二块晶壳可访问误认为新爆裂效果已完成。

## 材质与相机灯光

所有Mesh有UV、有效材质槽，已重算法线。纯色材质无图像通道，故不存在贴图色彩空间或MR打包布局；JSON颜色为scene-linear RGB，不是截图sRGB。材质名`F07_`前缀与FBX槽一一对应。

P19使用URP Lit opaque重新映射Base Color、Metallic、Smoothness（以1-Roughness为起点）及Emission RGB×Strength；Blender节点和EEVEE Bloom不保证在FBX/Unity自动等效。参考光强与Unity灯光单位不直接等同，需实际画面对照调节。发光过亮时先降低Emission与Bloom，不用覆盖灰白切面。

参考相机位置(3,-22,10)，看向(0,0,3.5)，28mm透视、36mm传感器。完整旋转欧拉角在JSON。三盏AREA灯：GoldKey位置(-4,-5,9)、1700W、RGB(1,.68,.28)、size7m；BlueRim位置(4,2,8)、2300W、RGB(.25,.48,1)、size6m；FrontSoft位置(0,-8,6)、1100W、RGB(.7,.8,1)、size5m。64samples，参考Bloom intensity .025。预览深色世界不是模型底板，FBX没有星空/星点；程序复用现有F05背景。

## 实际检查与限制

已经实际执行导出FBX，另建空Blender场景回导。26节点名称与父子关系逐个一致；24网格均有UV，顶点数/材质槽匹配，回导节点scale均为1；世界包围盒角点最大误差约0.00000143m。十二个晶壳确实是独立网格，并非仅旧记录声称。FBX和母版哈希见JSON。预览目视完整巨环、底环、十二壳完整水晶以及外围晶体。

缺项：未实际进入Unity验证材质/轴向/镜头/运动；无原生Blender Outliner界面截图，提供实际节点数据与层级核对图替代，明确没有将其标为界面截图。以上不宣称已验收Main效果。

## P19导入及学习点

导入FBX后由Unity Editor生成.meta，不手填GUID；先在独立测试视图核对26节点与材质再接Main，不依赖Unity直接转换blend。保留根和CrystalBody空节点、十二片局部rest transforms，关闭Import Animation；绑定素材时不要重置所有晶壳localPosition为零。导入推荐正常单位尺度；法线优先Import，检查正反面；不需要骨骼或蒙皮。

涉及Unity概念：FBX静态层级、MeshRenderer材质槽、Transform局部/世界空间、父级统一缩放、URP Lit与Emission、相机/RenderTexture和uGUI合成。调参先锁舞台root尺度与镜头，再设环不同方向/速度、外围椭圆轨道/起伏、水晶轻微倾斜与点击振动。运动用连续时间，点击不重置轨道。守住既有签约/唯一Draw/跳过清理/逐张确认，保留P18角色阶段。

复跑：Blender `--background --factory-startup --python Source/build_crystal_stage_v001.py`；脚本需要旧v004母版原项目路径。交付blend/FBX自身不依赖该路径或外部贴图；仅重建脚本有来源依赖。再次修改先升版本。
