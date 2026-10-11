# P16-REVEAL-DOUBLE-BURST

用户已验收P15星云。新目标：角色黑影出现小炸裂，黑影转正式立绘时大炸裂；两段均有内圈打光、外圈碎片，参考用户截图的节奏和空间层次，使用原创程序效果与现有素材。

范围：FastRevealView/表现组件、Shader、配置及Main开发验证；不改抽卡模型/Core/概率/存档。保留签约与唯一Draw、动画输入仅补完当前张、每张/末张等待及同批跳过。星云参数沿用验收值，不更改星云Shader。

先用现有透明冰屑及程序内圈打光实现实际Main，两段不依赖新美术。若现有碎片形状限制最终效果，另列可选升级素材。

状态：Main已接入，实际运行检查通过，待用户视觉验收。星云沿用用户已验收版本。

## 实现与接口

Unity工程 `D:/vibe/这次一定出/这次一定出` 下：

- `Scripts/Presentation/RevealArrivalBurst.cs`：只读标准化演出进度，黑影段20片/正式角色段32片，局部打光、扩散光域与外圈碎片/光尾。现有冰屑Sprite与软星光Sprite复用；碎片角度、大小、距离有固定差异，内圈为原创程序Shader。冰法师蓝白光，其余使用真实角色稀有度色。
- `Art/Effects/F05/Shaders/RevealArrival.shader`：uGUI透明加法打光，中心柔光、不规则扩张边缘和放射光芒。效果在角色后方，角色本体保持清楚。
- `Scripts/Presentation/FastRevealView.cs`：黑影阶段小炸裂，立绘从黑转彩的起点启动大炸裂，附加短促低强度前景闪光。名字/稀有度/NEW时序与排版不改。暴露SmallBurstPulse/BigBurstPulse/BurstAlpha供验证；Finish立即清碎片/光尾/闪光。
- `Scripts/Presentation/RevealSky.cs`：新组件接入时关闭旧单段冰屑，星云/恒星/流星分支不改。
- `Scripts/Presentation/P04FeelConfig.cs`、`Data/Presentation/P05FeelConfig.asset`：新Shader引用及爆发参数；星云仍为P15验收参数0.06演化、0.0022漂移、0.035扭曲。
- `Editor/P16RevealDoubleBurstSetup.cs`：独立准备/实际Main Play入口与验证。

上述路径均在 `Assets/_Game` 下。动画中点击直接Animate(1)，两段爆发都立即消失，只完成当前张；Finish在正常完成、演出跳过时同样清理。组件不收Pointer事件，不影响已有空白确认表面。

实际画面首轮发现SSR黑影近景挡住小爆发，最终改为黑影阶段完整轮廓，Progress=.23的大爆发时接原1.9倍脸部近景，再于.54缩回完整立绘；冰法师雪花在转彩后渐入，避免先盖住小冲击。碎片加宽，大小爆发的差异更易观察。

## 运行与调参

停止Play且场景无未保存编辑后，菜单 `Tools/这次一定出/程序/P16-REVEAL-DOUBLE-BURST/接入素材声音并保存Main`，随后同级验证菜单或直接Main Play。报告目录 `Docs/Production/P16_REVEAL_DOUBLE_BURST_Verification`；RevealDoubleBurst.request支持prepare/verify/status，以final_result.json本次时间为准。

P05FeelConfig.asset参数（进度是当前角色动画的0—1，不是固定秒数）：

| 参数 | 值 | 作用 |
|---|---:|---|
| silhouetteBurstEnd | 0.22 | 小爆发持续到该进度，保持黑影 |
| characterBurstStart | 0.23 | 与立绘黑转彩的起点对齐 |
| characterBurstSpan | 0.39 | 大爆发持续的进度范围，结束后等待界面无残留 |
| silhouetteBurstRadius | 0.32 | 小爆发内圈范围，以屏幕高度为尺度 |
| characterBurstRadius | 0.78 | 大爆发内圈范围，更接近全画面冲击 |
| revealBurstStrength | 0.85 | 两段打光和碎片整体强度 |

碎片数量20/32、外圈最大距离0.46/0.92、单片长度和短促前景闪光在RevealArrivalBurst/FastRevealView中调整。SSR2.2秒动画中，小爆发约0.48秒；大爆发约0.86秒。R/SR/UR按已有各自动画时长缩放。若改黑转彩进度，需要一起调整characterBurstStart，避免闪光与立绘脱节。

涉及Unity概念：uGUI层级顺序、加法混合、Shader径向光场、RectTransform锚点/屏幕宽高比、Sprite复用、按标准化进度控制并即时清理表现。不新增粒子系统或大框架。

## 美术依赖

当前没有必须新增的美术，内圈打光用Shader，外圈用已有reveal_ice_shard_v003和star_soft_v003。参考图有更多不规则碎片轮廓及拖尾，现有单一冰屑是首版限制。若验收希望升级形状，可补下列可选素材，程序无需等待：

| 素材 | 规格 | 要求 |
|---|---|---|
| reveal_burst_shard_A/B/C_v001.png | 各512×1024，RGBA透明 | 三种原创不规则尖长碎片，尖端朝上；灰白/浅蓝可调色，不带角色/文字/背景；单片而非整圈构图 |
| reveal_burst_streak_v001.png | 256×1024，RGBA透明 | 单束有方向的尖端光尾，亮端朝上，尾部柔和消散，无硬矩形边，灰白可调色 |

源稿可放 `ArtSource/Effects/F06/RevealBurst/`，逐项独立PNG和manifest，原始生成稿保留；不截取参考游戏素材。当前两段内圈无需再生成整张背景。

## 验证范围

实际Main首张录制，截图黑影小炸裂、转彩大炸裂、完成后的等待；检查对应进度仅正确阶段发光，等待无残留，两个Shader无绘制错误。沿用每张/末张新确认、动画点击仅完成当前张、手写/跳过补签唯一Draw和同批演出跳过、三窗口/四档检查。输入为实际GraphicRaycaster+ExecuteEvents，Esc调用HandleEscape，不宣称物理键鼠测试。

声音沿用现有卡片入场音效，未新增专用两段爆裂音或日语VITS。本批视觉与声音节奏以实际片段验收。90/180秒星云相位预览仍是开发入口加速采样，放在实时录制后，不混入视频。

## 最终实测证据

2026-10-10 20:40:58（北京时间），Unity 2022.3.53f1c1实际Main Play最终PASS。

- `main_p16_double_burst.mp4`：32.579秒，1280×720，实际采集727帧，平均22.31帧/秒（不是GPU性能基准）。同场AudioListener音频48kHz双声道，削波0，没有替换音轨；原帧/时间戳/PCM与clip_manifest.json保留。
- `1280x720_black_silhouette.png`：完整黑影、小内圈打光、20片外圈碎片；`1280x720_large_character_burst.png`：角色转彩/近景，大光域、32片碎片和光尾。最终等待截图无爆发残留，另有三窗口与四档标识截图。
- `checks.json`：小/大阶段、等待清理、动画点击立即补齐且清光、Progress=.10时Esc与=.35时演出跳过立即清两段爆发及前景闪光并到同批总览均通过。名字先于稀有度、每张/末张新确认、手写/自动补签唯一Draw保持；真实资源30→29→19、抽数11、提交2次。
- 两个Shader实际绘制无错误，runtime/editor编译exitCode均0；七项保护文件未变、15项原素材哈希不变且.meta存在。Unity已退出Play，Main配置已保存。星云Shader未修改、演化速度仍0.06。

可选美术升级已单列 `F06_ART_REVEAL_BURST_可选素材清单.md`，便于直接交给美术。本批无需等待它们。运行检查通过不代表角色炸裂视觉已验收。
