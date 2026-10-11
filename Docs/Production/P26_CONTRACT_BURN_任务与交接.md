# P26-CONTRACT-BURN — 原尺寸契约燃烧

状态：已集成 Main，实际 Play 与截帧检查通过，视觉待验收。

授权：用户要求先实现 Shader 烧掉契约。本次修改签约后纸张视觉及必要配置/验证工具，替代纸张缩小升空；保留签名、落印、法阵与3D运镜、唯一Draw和所有跳过入口。无需新美术，不改Core、规则或存档。

方案：原尺寸原位置契约，落印后底部不规则燃烧上移；焦边、橙色发光火舌、纸面及文字签名蜡印共享纸面坐标溶解；少量灰烬飞散，然后露出法阵。

## 实现和文件

- `Scripts/Presentation/ContractBurnView.cs`：为纸、英文、动态Text/手写Graphic、蜡印和接触影分别创建运行材质，统一屏幕纸面矩形、进度与时间；少量程序灰烬碎片，不需图片；销毁时释放材质。
- `Art/Effects/F04/Crystal/Effects/ContractBurn.shader`：原纹理透明合成、两层噪声燃烧边界、焦边与跃动火舌；支持uGUI字体采样、Stencil及RectMask2D裁剪。
- `ContractCeremonyView.cs`：新配置有效时替换旧缩小/升空；纸比例与位置始终不变，放在法阵前景，烧穿后显露后景；Begin重置，Finish立即清燃烧/纸/灰烬。
- `P04FeelConfig.cs` / `Data/Presentation/P05FeelConfig.asset`：保存Shader引用与燃烧参数。
- `Editor/P26ContractBurnSetup.cs` / `P26ContractBurnVerify.cs`：重复导入及实际Main验证和录制。

Main 沿用同一配置引用，未改场景文件、Core、概率、存档和角色模型。P23的纸张升空行为由本任务替换；P24运镜与P25背景连续性保留。

## 运行和调参

正常召唤→签约/跳过补签→酒红蜡印落下→原尺寸纸张烧尽→法阵→3D水晶。

选 `Assets/_Game/Data/Presentation/P05FeelConfig.asset` 的「P26 契约燃烧」：

- `contractBurnStart`：盖印后开始燃烧的时间，当前1.05秒。
- `contractBurnSeconds`：燃烧历时，当前1.5秒，2.55秒完全烧尽，接前奏末段运镜。
- `contractBurnEdge`：焦边/火舌尺度，当前0.06。
- `contractBurnShader`：保持当前引用；暂无新增游戏内按钮。

Editor菜单 `Tools/这次一定出/程序/P26-CONTRACT-BURN/保存契约燃烧配置` 可重新接入；同菜单「验证Main输入音画并录制片段」为开发验证入口。

Unity概念：同一Shader控制不同UI图层，但字体与笔迹使用各自纹理；屏幕纸面坐标让各层同时烧穿，避免uGUI批处理改变物体局部坐标造成错位；VertexHelper生成少量灰烬，CanvasGroup只在烧尽或跳过时统一隐藏。

## 实测证据

2026-10-11 10:12（北京时间），Unity 2022.3.53f1c1 图形Editor实际Main Play通过。目录 `P26_CONTRACT_BURN_Verification/`：

- `final_result.json` / `checks.json`：手写松笔单抽和用户名「星旅者」跳过补签十连分别只提交一次，资源19、总抽11、提交2；所有纸面层使用同一燃烧Shader与进度；烧尽接3D且水晶可点。
- `burn_samples.json`：约18%、50%、78%进度纸面始终 scale(1,1,1)、原位置原旋转；同一纸面宽高。
- `1280x720_burn_18.png`、`_50.png`、`_78.png`、`_complete.png`：已逐张查看，烧穿边缘逐步上移，名字/蜡印随底部烧去，无缩小。
- `1280x720_handwritten_burning.png`：实际指针轨迹也进入燃烧；1920×1080、1600×1000图核对等比及燃烧中跳过清理。
- `burn_visual_check.json`：固定截图区域暖色纸面占比逐段减少，补充进度断言，防止参数正确但视觉提前烧完。
- `main_p26_contract_burn.mp4` / `clip_manifest.json`：实际Main逐帧时间戳画面与同次AudioListener混音；沿用现有旁白、盖印和背景声，本批未制作独立火焰音效。

燃烧中Esc/跳过、动画点击补完均清纸和灰烬并保持同批，不重复计算。验证由Editor驱动已有入口及EventSystem真实射线/指针事件，不冒充人工键鼠验收。

首次截图发现局部坐标在uGUI批处理后偏移导致过早烧完，已改为屏幕纸面坐标并重新实测。当前为程序火舌与灰烬的第一版，没有火焰体积模拟、真实纸张卷曲或新美术；视觉待用户验收。
