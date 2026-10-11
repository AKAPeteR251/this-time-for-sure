# P14-NEBULA-DISTORT

状态：Main已接入，实际运行检查通过；用户已认可效果，速度后续按P15减慢。本文实录和调参表记录P14原版0.09，当前Main演化速度为P15的0.06。

用户要求：尝试通过扭曲使已有贴图脉络变化。本次沿用P13参考星云素材，加入不同区域、不同相位的局部旋转形变与柔和流场拉伸，使脉络缓慢弯折/拉伸再舒展。保留慢横漂、起伏、原素材细粉尘、Main抽卡输入与既有雪花/NEW/流星。

范围只含星云Shader、配置/绑定和独立Main验证入口；Core、模型、概率、存档不变。

## 文件与实现

Unity工程 `D:/vibe/这次一定出/这次一定出`：

- `Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader`：增加三个软边局部旋转域，中心分别约(.20,.58)、(.55,.28)、(.84,.67)，旋转角随不同时间相位缓慢变化；叠加由平滑势函数导出的流场位移，产生局部拉伸。变形作用于素材采样UV，保留原图的粉尘与脉络，不做图片淡换或重生成烟雾。宏观遮罩仍随慢漂移坐标连续移动。流星分支未改。
- `Assets/_Game/Scripts/Presentation/P04FeelConfig.cs`、`Data/Presentation/P05FeelConfig.asset`：新增nebulaDistortion=.035。
- `Assets/_Game/Scripts/Presentation/RevealSky.cs`：每帧把nebulaDistortion传给材质_Distortion，保留原素材绑定、构图和累计时间。
- `Assets/_Game/Editor/P14NebulaDistortSetup.cs`：独立准备/验证/录制入口，实际检查材质扭曲值与素材引用。

素材沿用P13/P09，两张源星云PNG及.meta不覆盖。NEW、雪花、随机流星、角色模型、抽卡Core未扩展。

## 运行与调参

停止Play且场景无未保存编辑后，Unity菜单 `Tools/这次一定出/程序/P14-NEBULA-DISTORT/接入素材声音并保存Main`，再使用同级验证菜单或直接Main Play。报告为 `Docs/Production/P14_NEBULA_DISTORT_Verification/`，NebulaDistort.request支持prepare/verify/status，以final_result.json本次时间为准。

`Assets/_Game/Data/Presentation/P05FeelConfig.asset`：

| 参数 | 本次值 | 作用 |
|---|---:|---|
| nebulaDistortion | 0.035 | 新局部旋转/拉伸强度；0关闭本次扭曲、回到P13运动；建议0.02—0.05 |
| nebulaEvolution | 0.09 | 扭曲、起伏、内部变化节奏；主旋转周期约56秒，其他区域约52/67秒 |
| nebulaDrift | 0.0022 | 保留慢横漂，不加速 |
| nebulaBillow | 0.025 | 保留不同区域上下起伏 |
| nebulaWarp | 0.035 | 原P13细微噪声扰动，与新扭曲独立 |
| nebulaComposition | 69.7511 | 用户P09参考构图 |
| proceduralNebulaAlpha / nebulaGrain | 0.40 / 0.30 | 保留参考明暗和细粉尘 |

0.035对应各旋转域中心处角度上限约24度，作用随距离柔和减弱；边缘无硬切。扭曲作用在连续UV上，各层素材仍保留本身细节。调大不会增加素材分辨率，过强会把细丝拉长或产生漩涡感，所以本批是可调的视觉试验，待用户看片确认。

涉及Unity概念：UV空间形变、按屏幕比例修正旋转、局部旋转域、势函数流场、材质运行时参数、保留宏观分布与局部纹理动态的分离。仍是2D素材Shader，不是体积模拟。

## 验证范围

实际Main首张停留24秒并录制正常速度画面与同场AudioListener混音，核对_Distortion实际绑定、原素材/参考种子、Shader实际绘制无错误、翻张累计时间连续。沿用三窗口/四档、手写/跳过补签唯一Draw、动画输入仅补完当前张、每张和末张新确认、同批演出跳过检查。

90/180秒截图是开发入口直接切到未来累计时间的实际Main相位预览，在正常片段录制结束后生成，不混入视频，不代表实际等待三分钟。输入为实际GraphicRaycaster+ExecuteEvents，Esc调用HandleEscape，未验证OS物理键鼠。声音沿用现有版本，日语VITS女声按用户要求后续实现。

## 最终实测

2026-10-10 20:15:50（北京时间），Unity 2022.3.53f1c1实际Main Play最终PASS。

- `main_p14_distorted_nebula.mp4`：成片32.72秒，1280×720，实际采集714帧/32.688秒，平均21.84帧/秒（不是GPU性能基准）。同场AudioListener音频48kHz双声道，削波0，没有替换音轨；原帧/时间戳/PCM与clip_manifest.json保留。
- 首张24秒等待前后截图可核对同一构图的脉络弯折/拉伸；另有三窗口/四档与90/180秒开发相位截图。素材粉尘细节保持，动态幅度是否合适仍待用户看片。
- `checks.json` / `final_result.json`：实际素材绑定、_Distortion=.035、参考种子69.7511、Shader绘制无错误、等待/翻张连续及既有签署/补签/新确认/同批跳过通过。真实资源30→29→19、抽数11、提交2次。
- runtime/editor编译exitCode均0；七项保护文件哈希未变、15项原始素材哈希未变且.meta存在。结束已停止Play，Main参数保存。

设nebulaDistortion=0可关闭本次新形变以对比P13运动；P13历史片段保留，不覆盖。运行PASS不等于视觉已验收。
