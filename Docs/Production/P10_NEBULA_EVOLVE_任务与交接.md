# P10-NEBULA-EVOLVE

状态：Main已接入，实际运行检查通过，待用户视觉验收。

用户授权：全屏不规则云团减少数量与杂乱；保留横向漂移，并增加纹理自身起伏变化；还原已有贴图的蓝紫粉尘感。Main集成沿用P09范围，只改星云表现与调参/验证入口。写实雪花、NEW、流星、抽卡输入和模型保持既有行为。

实现目标：纯程序噪声生成云团，独立控制横向漂移和三维噪声演化；柔和蓝紫粉尘，无白色轮廓筋纹。保留原贴图作为历史参考与关闭程序天空时的回退。

## 改动与接口

- `Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader`：星云分支改为四层三维噪声、局部形变、低频云团遮罩与细颗粒。云团横向移动，噪声深度单独变化，叠加不同位置的轻微呼吸。只使用蓝紫色，没有白色筋纹或线状轮廓。流星分支保持P09。
- `Assets/_Game/Scripts/Presentation/RevealSky.cs`：取消程序星云的两张贴图绑定；传入独立漂移时间、演化时间、疏密和尺度。旧贴图仍保留，在关闭程序天空时回退使用。
- `Assets/_Game/Scripts/Presentation/P04FeelConfig.cs`、`Data/Presentation/P05FeelConfig.asset`：新增三个调参字段，并保存本次值。既有星底贴图、星星Sprite仍使用美术素材；“纯Shader”仅指星云的生成。
- `Assets/_Game/Editor/P10NebulaEvolveSetup.cs`：独立菜单及请求入口，实际Main Play录制/输入检查。Main使用既有配置，未改角色数据、Core、概率和存档。

上述资产路径均相对Unity工程 `D:/vibe/这次一定出/这次一定出`。用户界面不新增调试内容。

## 如何运行

Unity菜单 `Tools/这次一定出/程序/P10-NEBULA-EVOLVE/接入素材声音并保存Main`，停止Play且场景无未保存编辑时使用。接着使用同目录的验证菜单，或直接打开Main并Play。验证会自动操作既有开发入口并保存实际画面/AudioListener混音；最终退出Play。

请求入口为 `Docs/Production/P10_NEBULA_EVOLVE_Verification/NebulaEvolve.request`，支持 `prepare`、`verify`、`status`。读取同目录runtime_status.json和final_result.json核对本次时间，勿用历史PASS代替新运行。

## 调参

选择 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`：

| 字段 | 本次值 | 作用 |
|---|---:|---|
| nebulaDrift | 0.004 | 横向漂移速度；0只停止平移，负值反向 |
| nebulaEvolution | 0.14 | 云团内部起伏、聚散与纹理变化速度；0停止形态演化，仍可漂移 |
| nebulaThreshold | 0.50 | 增大云团更少、留白更多；建议0.46—0.56 |
| nebulaScale | 2.8 | 增大云团更小、数量更多；降低形成更大的云团 |
| proceduralNebulaAlpha | 0.38 | 整体透明度，增大更亮 |
| nebulaWarp | 0.035 | 局部形变幅度，增大更扭曲 |
| nebulaGrain | 0.45 | 细粉尘颗粒强度，不改变云团数量 |

Shader中蓝色为(.14,.27,.61)，紫色为(.44,.20,.58)，细颗粒频率235。形态与颜色每次随独立视觉随机种子变化，不消耗抽卡随机流程。若需固定构图，只在开发入口固定CloudSeed。

涉及Unity概念：uGUI自定义材质；屏幕宽高比；三维噪声切片随时间变化；分形噪声叠加；噪声域局部形变；独立的非缩放时间参数；透明混合。它是2D程序视觉，并非体积星云模拟。

## 验证范围与限制

实际检查Shader支持/错误、程序材质不含星云贴图输入、漂移时间和演化时间同时增长、24秒停留中恒星/NEW/流星持续运行；还复核名字先于稀有度、动画点击只完成当前张、逐张和末张新确认、三窗口、四档标识、签署/补签唯一Draw和同批跳过。输入为实际GraphicRaycaster和ExecuteEvents合成事件，Esc使用既有HandleEscape，不宣称物理键鼠测试。

视觉是否达标仍由用户看片验收。声音沿用当前可运行版本；日语VITS两种女声需求仍按用户要求后续实现。本批没有新增音色或性能基准。

## 最终实测证据

2026-10-10 19:24:33（北京时间），Unity 2022.3.53f1c1实际Main Play最终PASS。最终参数为上表，前一轮偏亮、粗颗粒试验已经调淡并重新完整运行；交付片段对应最终轮。

- `P10_NEBULA_EVOLVE_Verification/main_p10_evolving_nebula.mp4`：32.134秒，1280×720，723帧，采集平均22.50帧/秒（不是GPU性能基准）。同场Unity AudioListener声音48kHz双声道，削波0，没有替换音轨。
- `1280x720_ssr_wait.png` 与 `1280x720_ssr_dynamic_wait.png`：同一张持续等待24秒前后，云团形态和颗粒分布变化，横向漂移同时运行；另有1920×1080、1600×1000截图和四档标识截图。视觉随机种子0.03435165，记录在sky_observation.json。
- `checks.json` / `final_result.json`：纯星云材质无贴图输入、两种时间参数持续变化、实际绘制后Shader无错误，既有揭晓/签约/确认/跳过流程通过。真实单抽十连资源30→29→19，总抽数11、提交2次。
- `compile_status.json`：runtime/editor均exitCode=0。`protected_check.json`七项保护文件未变；`asset_import_check.json`15项源素材哈希保持原样，Unity.meta存在。

运行结束已退出Play，Main配置已保存。Core、UI契约、角色模型、概率、存档以及已认可雪花/NEW均未扩展。
