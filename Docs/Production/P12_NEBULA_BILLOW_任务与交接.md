# P12-NEBULA-BILLOW

状态：Main已接入，实际运行检查通过，待用户视觉验收。

用户反馈：P11大体效果可接受，但横向速度过快、少了起伏。本次只降低星云漂移速度，增强局部柔和起伏和内部变化，保留蓝紫粉尘、细丝、分布、雪花、NEW、流星及抽卡契约。

目标参数：横向速度0.007→0.0022（约原来的31%）；形态演化0.055→0.09；新增nebulaBillow=0.025控制局部上下起伏，不是整张画面上下移动。保留宏观结构稳定与翻张连续性，禁止重新随机生成造成闪现。

## 文件与接口

Unity工程 `D:/vibe/这次一定出/这次一定出` 下：

- `Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader`：增加不同空间/时间相位的局部纵向噪声域起伏，加强缓慢局部形变，加6%柔和密度呼吸。沿用原宏观结构、细丝、暗隙与粉尘，流星分支不变。
- `Assets/_Game/Scripts/Presentation/P04FeelConfig.cs`、`Data/Presentation/P05FeelConfig.asset`：nebulaBillow字段及本次参数。
- `Assets/_Game/Scripts/Presentation/RevealSky.cs`：传递_Billow，其余沿用累计星云时间与种子连续性。
- `Assets/_Game/Editor/P12NebulaBillowSetup.cs`：独立Main准备、实际Play与录制入口；沿用既有确认/Draw/跳过检查。

Main沿用已引用配置，无新增模型、Core/概率/存档或页面按钮，原始素材未覆盖。

## 运行与调参

Unity停止Play且无未保存场景编辑后，菜单 `Tools/这次一定出/程序/P12-NEBULA-BILLOW/接入素材声音并保存Main`，然后同级验证菜单或直接Main Play。报告目录为 `Docs/Production/P12_NEBULA_BILLOW_Verification`；NebulaBillow.request支持prepare/verify/status，检查final_result.json本次时间。

在 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`：

| 参数 | 本次 | 作用 |
|---|---:|---|
| nebulaDrift | 0.0022 | 横向速度；原P11为0.007，约下降69% |
| nebulaBillow | 0.025 | 不同区域上下起伏幅度；0关闭局部上下起伏，建议0.015—0.035 |
| nebulaEvolution | 0.09 | 起伏、细丝和内部粉尘变化节奏；降低更慢 |
| nebulaWarp | 0.035 | 内部形变幅度，shader倍率由5调到8 |
| nebulaFilaments / nebulaGrain | 0.55 / 0.45 | 保留P11脉络和细粉尘 |
| proceduralNebulaAlpha / nebulaThreshold / nebulaScale | 0.42 / 0.50 / 2.8 | 保留亮度、分布和纹理尺度 |

nebulaBillow=0.025是归一化屏幕高度的局部位移上限，720高画面约18像素；两项正弦按位置错开相位，主节奏约30秒，次节奏约44秒。没有整屏统一上下移动，也不以透明度闪烁代替起伏。密度呼吸最大6%，保持连续无硬切。

涉及Unity概念：Shader噪声域位移、位置相关的波动相位、独立漂移与内部演化参数、非缩放累计时间、uGUI材质参数。效果为2D Shader，不是体积模拟。

## 验证与限制

实际Main录制首张正常停留24秒，检查慢漂移与内部演化参数持续运行、翻张/重新进入种子和累计时间连续、Shader实际绘制无错误；复核三窗口/四档、签署和跳过补签唯一Draw、动画点击仅完成当前张、每张及末张确认与同批演出跳过。

90/180秒相位截图由开发入口切换累计时间后实际Main绘制，放在实录结束后单独检查，不混入正常速度的视频，也不宣称实际等待三分钟。输入用实际GraphicRaycaster+ExecuteEvents，Esc用HandleEscape，不宣称OS物理键鼠验证。

视觉待用户看片验收；日语VITS女声仍按用户要求后续实现，声音沿用现有版本。

## 最终实测

2026-10-10 19:59:11（北京时间）Unity 2022.3.53f1c1实际Main Play最终PASS。

- `main_p12_slow_billow_nebula.mp4`：32.267秒，1280×720，725帧，采集平均22.47帧/秒（不是性能基准）。同场AudioListener声音48kHz双声道，削波0；原始帧/时间戳/PCM和clip_manifest.json保留。
- 首张24秒等待前后截图、三窗口/四档、90/180秒开发相位截图保留。实际慢漂移与局部起伏参数已绑定，Shader实际绘制无编译错误；等待/翻张/重新进入时种子和累计时间连续。
- `checks.json` / `final_result.json`：既有确认、签约补签、动画点击、跳过检查通过；真实单抽十连资源30→29→19、总抽数11、提交2次。
- runtime/editor编译exitCode均0；七项保护文件哈希未变，15项原始素材哈希未变且.meta存在。结束后已停止Play，Main配置保存。

运行PASS不代表用户视觉已验收，后续以本次实录的漂移与起伏节奏为准。
