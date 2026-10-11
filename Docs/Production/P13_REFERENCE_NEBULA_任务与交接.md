# P13-REFERENCE-NEBULA

状态：Main已接入，实际运行检查通过，待用户视觉验收。

用户明确参考P09的1280x720_ssr_wait.png，希望该星云配合起伏。P10—P12纯噪声没有保留参考的细密粉尘脉络，因此本次恢复P09源素材采样，用Shader对已有纹理做慢漂移、局部形变与上下起伏；不再以纯噪声重画云团。固定参考构图种子69.7511，避免随机构图偏向一角，翻张不重置。横向速度保留P12的0.0022。

只改Main星云材质、表现绑定、配置与开发验证；不改Core、角色、概率、存档及签约/Draw/确认/跳过契约。NEW、写实雪花、随机流星保持既有版本。

## 改动文件与实现

Unity工程 `D:/vibe/这次一定出/这次一定出` 下：

- `Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader`：恢复P09两张纹理的三次旋转/缩放采样、粉尘混色与细颗粒；三层空间遮罩随漂移坐标移动，取消时间噪声重新生成大块云团。UV增加局部正弦起伏和低幅噪声形变，密度轻微呼吸，保留素材纹理细节。流星分支不变。
- `Assets/_Game/Scripts/Presentation/RevealSky.cs`：恢复_NebulaFarTex/_NebulaNearTex绑定，在Build时采用参考构图；保留累计时间、翻张连续性和1.6秒淡入。
- `Assets/_Game/Scripts/Presentation/P04FeelConfig.cs`、`Data/Presentation/P05FeelConfig.asset`：新增nebulaComposition=69.7511，采用参考P09的0.40透明度、0.30颗粒值。
- `Assets/_Game/Editor/P13ReferenceNebulaSetup.cs`：独立准备、Main实际Play与录制入口；实际检查两张原素材绑定与参考种子，取消旧的“材质无贴图”断言。

星云使用现有 `Art/Effects/F05/RevealR3/reveal_nebula_far_v003.png` 与 `reveal_nebula_near_v003.png`，源图不修改。此版本是素材纹理结合Shader形变，不是纯程序噪声星云；参考图是质感/构图目标，新增漂移和形变意味着截图不会逐像素一致。

## 运行及调参

停止Play且场景无未保存编辑后，Unity菜单 `Tools/这次一定出/程序/P13-REFERENCE-NEBULA/接入素材声音并保存Main`，再使用同级验证菜单或直接Main Play。报告为 `Docs/Production/P13_REFERENCE_NEBULA_Verification/`；ReferenceNebula.request支持prepare/verify/status，查看final_result.json本次时间。

选择 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`：

| 参数 | 本次值 | 调整作用 |
|---|---:|---|
| nebulaComposition | 69.7511 | P09参考构图；改变会同时改变三个素材采样方向/偏移，不自动随机切换 |
| nebulaDrift | 0.0022 | 保留P12慢横向漂移，0停横移 |
| nebulaBillow | 0.025 | 局部上下起伏幅度；0关闭起伏 |
| nebulaEvolution | 0.09 | 起伏/噪声形变节奏，降低更缓慢 |
| nebulaWarp | 0.035 | 局部纹理形变幅度，建议0.02—0.045，过大损伤脉络 |
| proceduralNebulaAlpha | 0.40 | P09总体明暗 |
| nebulaGrain | 0.30 | P09细粉尘强度 |

nebulaThreshold/Scale/Filaments是P10—P12纯噪声版本的历史参数，不参与P13素材星云。nebulaFarSpeed/NearSpeed只用于关闭程序天空时的传统贴图回退。改composition需要重建Main运行实例；其余字段运行时可调。

起伏通过不同区域的不同相位对采样坐标做柔和形变，同时低频噪声持续改变局部纹理，不只是把图片整张平移。宏观遮罩不再按时间出现/消失，翻张不切换构图。现有素材为二维底图，尚非体积星云模拟。

涉及Unity概念：TextureImporter.Repeat、旋转UV、多层透明度合成、Shader局部采样坐标形变、空间遮罩与非缩放累计时间。没有新增素材或大框架。

## 验证说明

首张正常等待24秒并录制实际Main画面与同场AudioListener混音；检查原素材实际采样、参考种子、慢漂移/起伏参数、Shader绘制无错误、翻张种子与累计时间连续。保留既有签约/补签唯一Draw、每张及末张新确认、动画点击仅完成当前张、同批演出跳过、三窗口和四档标识检查。

90/180秒截图为开发入口直接切到未来星云时间后实际绘制的相位预览，在实时录制结束后单独生成，不混入视频，不宣称实际等待三分钟。输入为GraphicRaycaster+ExecuteEvents，Esc调用HandleEscape，不宣称物理键鼠测试。日语VITS女声仍后续实现。

## 最终实测证据

2026-10-10 20:08:09（北京时间），Unity 2022.3.53f1c1实际Main Play最终PASS。

- `main_p13_reference_nebula.mp4`：成片32.2秒，1280×720，实际采集724帧/32.187秒，平均22.49帧/秒（不是GPU性能基准）；同场AudioListener声音48kHz双声道，削波0，未替换声音。原始帧、时间戳、PCM与clip_manifest.json保留。
- `1280x720_ssr_wait.png` 对照用户P09参考，恢复原素材的蓝紫细密粉尘、纹理脉络；`1280x720_ssr_dynamic_wait.png`为同张正常停留24秒后画面。两张用于查看细节保持，动态节奏以实录为准。另有三窗口/四档与90/180秒开发相位预览。
- `checks.json` / `final_result.json`：实际原素材绑定、参考构图种子、慢漂移和起伏参数、翻张连续性、Shader无绘制错误与既有签约/确认/跳过流程通过；真实资源30→29→19、抽数11、提交2次。
- runtime/editor编译exitCode均0；七项保护文件未变，15项源素材哈希不变且Unity.meta存在。结束已退出Play、Main配置保存。

本任务使用用户指定参考恢复质感，仍待用户验收动态起伏；不把运行检查PASS当作视觉已验收。
