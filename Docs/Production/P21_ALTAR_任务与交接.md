# P21-ALTAR

依据：用户选择地下圣殿参考方向，现已提供F08_UndergroundAltar_v001.zip，授权接入水晶舞台。读取共享契约、F08清单/manifest及P19/P20交接后执行。

范围：导入F08环境127个网格和12张贴图，构建URP材质与火焰/薄雾/尘埃；在现有独立3D舞台内实例化，室内相机与灯光取代水晶阶段星空。保留F07原件、P20三个欧拉方向、连击与演出流程、已验收角色星云。不修改Core/概率/存档/角色模型，不实施后续投射漏光和新破裂动画。

状态：接入、编译与Main实际Play验证完成；视觉待用户验收，未标为已验收。

## 实际交付

- 原包安全解压至 `ArtSource/Scenes/F08/UndergroundAltar`，未执行包内建模脚本。FBX及12张原始贴图复制至 `这次一定出/Assets/_Game/Art/Backgrounds/F08/UndergroundAltar`，哈希一致，Editor生成meta。
- `P21AltarSetup.cs` 重建四套URP Lit材质并映射实际FBX材质名。石材/金属BaseColor为sRGB，法线按NormalMap导入；金属图R通道和1-粗糙度A通道打包为两张新增贴图，逐像素验证一致。原始图不覆盖。现有三条发光网格已足够，未重复叠加额外祭坛遮罩。
- `AltarInteriorStage.cs` 实例化127网格环境、4组12fps火焰图集、5组低雾、少量循环尘埃及室内冷暖灯光。火焰为跟随镜头的透明图集，原素材左上逐行顺序转换为Unity UV；灯光微弱波动。`AltarBillboard.shader`负责图集及透明素材绘制。
- `CrystalStage3D.cs` 同一独立镜头/layer31合成房间与F07，原FBX轴向保留。F07动画根统一抬高0.585米，让底环落在祭坛台面上；三个欧拉速度不改。室内使用局部Volume/HDR/ACES及柔和Bloom，空间在隔离舞台10000米位置，未改全局URP/RenderSettings。
- `CrystalChargeView.cs` 仅在室内素材存在时不创建水晶阶段的CrystalLivingSky，角色结果RevealSky继续原样。破裂或跳过沿现有接口关闭整个rig，灯光/火焰/薄雾/相机同步停止。
- `P04FeelConfig.cs`与 `Data/Presentation/P05FeelConfig.asset`新增并保存F08引用、镜头与光照参数。现有Main的feelConfig引用不变。`P21AltarVerifySetup.cs`是本轮独立Editor实机验证/录制入口。

## 实测证据

2026-10-10 23:37:54北京时间，已有图形Unity Editor内Main Play通过80项检查，证据目录 `Docs/Production/P21_ALTAR_Verification`。输入通过实际uGUI射线与ExecuteEvents派发，Esc用游戏HandleEscape入口；不声称真人鼠标键盘测试。固定SSR批次仅开发演出入口，另验证真实随机单抽与十连。

- `final_result.json`、`checks.json`：室内127网格/四火焰、镜头在房间内、支持的运行Shader、自然交叉淡化、三环/碎片/水晶持续运动、慢点高频蓄能、停手保能收震、逐张与末张确认、动画点击仅补齐当前张、转场跳过/蓄能Esc同批清场、手写松笔和跳过补签分别唯一Draw。最终货币19，真实抽数11，提交次数2。
- 三窗口实际截图：`1280x720_altar_idle.png`、`1920x1080_altar.png`、`1600x1000_altar.png`；另有转场、停手蓄能和SSR结果页截图。
- `main_p21_altar.mp4`：Main实际带声片段17.16秒，359帧，平均20.93fps，1280×720；同次Unity监听器48kHz音轨，无削波。帧与声音记录用真实时间戳封装，未另拼旁白。
- `asset_import_check.json`、`material_packing_check.json`、`compile_status.json`、`protected_check.json`：原素材一致、打包通道准确、运行及Editor编译通过；Core、PrototypeGame、FastRevealView与已验收星云Shader保持原字节。
- 首轮发现MonoBehaviour字段初始化不能创建MaterialPropertyBlock，改至Build修复；输入验证遵守已有两帧新点击保护后通过。以最终证据为准。

## 运行与调参

打开 `Assets/_Game/Scenes/Main.unity` 点击Play，正常召唤并签署/跳过补签后进入地下圣殿。Editor菜单 `Tools/这次一定出/程序/P21-ALTAR/导入地下圣殿并接入Main配置` 可重复导入仅F08受管资产；验证菜单为同组验证Main输入音画并录制片段。需停止Play并保存当前编辑后运行导入工具。

调参资产 `Assets/_Game/Data/Presentation/P05FeelConfig.asset` 的P21栏：

| 参数 | 当前值/用途 |
|---|---|
| altarCameraWide / altarFocusWide | (0,7.2,-13.7)/(0,5.6,0)，室内入场远景 |
| altarCameraClose / altarFocusClose | (0,6.4,-11)/(0,4.9,0)，连击近景 |
| altarWideFov / altarCloseFov | 59/46度，交叉淡化同时推进镜头 |
| altarStageLift | 0.585米，F07统一抬高，不改模型源文件 |
| altarWarmLight / altarCoolLight | 5/8，火盆及水晶点光强度 |
| altarFlameFps | 12，16帧循环 |
| altarMistAlpha | 0.08，低雾轻量透明度 |
| altarBloom / altarExposure | 0.4/0.15，室内局部光晕/曝光 |

涉及Unity概念：FBX导入转换与节点层级、PBR粗糙度转平滑度、材质外部映射、透明图集UV、局部Volume与RenderTexture合成、统一舞台生命周期。Main全局质量设置不变，实际点光数量受当前URP档位的每物体灯光上限影响；首版使用实时光与有限雾，不做烘焙灯光。

限制及下一步：这是交付模块的首版Unity呈现，视觉与概念图的精细度不等同，需用户评审室内亮度/材质/构图。投射裂纹光束、新水晶碎裂效果、日语VITS两声线仍是独立后续项，本轮未实现。
