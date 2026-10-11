# P04-FEEL 交接

日期：2026-10-08。任务：P04-FEEL。状态：**Main 已集成并实际 Play 通过，三段带声片段已交付；视听风格与实体键盘手感待 C08/用户验收。** 实际语音已交，不是用字幕/提示音冒充旁白。没有标为“已验收”。

依据09/10/最新11、P03_REVEAL、U03_CLEAN和A05_REVEAL交接。Unity工程：`D:/vibe/这次一定出/这次一定出`。

## 已实现

共用仪式默认6.8秒，签名输入/自动补签的时间另算。Unity实时合成：金色重描当前笔迹/配置昵称 → 上升金线 → 厚重星印从上方落下 → 冲击波与短震动 → 契约升空 → 多层透视巨环反向旋转、晶体列阵与光柱 → 中央回吸收束 → 定稿最后一句与星门爆开 → 强光退出并进入第一张角色。共同仪式只播一次，本批最高等级只决定最终信号色，不泄露角色身份。

- Main经Unity Editor保存引用：原U02ArtTheme/六卡/真实内存入口、U03内置霞鹜文楷、A05八层图、实际中文旁白和原创合成样音、P04FeelConfig。UR绑定A05的完整卡图；未使用有缺损的透明候选。
- 保留P03签约提交边界：签署前不Draw/扣费；一笔松笔或签署跳过补配置用户名后，共用一次提交；补签后直接继续，无快速签署或第二次确认。
- 演出跳过/Esc才进同批总览，签署Esc/返回仍取消无消耗；原批顺序、NEW、收藏保持。
- 每张角色动画结束后自由停留，最后一张也等待。动画点击只完成当前张，新的确认输入才下一张；0.15秒短转场不重播仪式。等待时仅极低强度背景星光继续，不自动推进。
- 四档：R 0.75秒蓝色短闪/清亮短音；SR 1.1秒紫色聚环/短和弦；SSR 2.2秒压暗、双环蓄力、金色冲击、透明冰法师推近回落；UR 3.2秒金红双环重构、短收束、低频冲击和完整卡特写。姓名、称号、NEW依次出现；完成时强闪光/震动清零，操作固定角落。
- U03纯净玩家界面继续生效，详情无内部ID、无开发状态与玩家调试入口。内层文字/契约/字幕均为《星界回响》，收尾准确为“星门，为您而开！”。动态昵称和笔迹是Unity UI，不入视频资产。

## 片段与试听

下列均为实际Main图形Play的1280×720画面，声音是同场Unity AudioListener的内部混音；没有用离线音效轨重建游戏录像。

| 文件（D:/vibe/这次一定出/Docs/Production/P04_FEEL_Verification/） | 时长 | 内容 |
|---|---:|---|
| main_single_handwritten.mp4 | 8.96秒 | 手写松笔、完整仪式、单抽停留、最后确认、总览 |
| main_ten_auto_signed.mp4 | 23.40秒 | 签署跳过补“星旅者 Luna”、完整仪式、真实随机十连原序、十次等待确认、总览；其中一张演示动画点击只完成当前张 |
| main_four_tiers_fixed.mp4 | 18.24秒 | Main中的开发指定R/SR/SSR/UR四档对照及NEW；不Draw/扣费/入库，没有将固定批次冒充真实随机结果 |

旁白原音：`D:/vibe/这次一定出/这次一定出/Assets/_Game/Audio/P04/sacred_narration.wav`。

旁白加仪式音效试听：同目录 `ceremony_preview.wav`，离线试听混音，明确区别于上列实际Play录音。每段视频的原始监听器WAV位于对应 `*_capture/unity_listener_mix.wav`。

## 实际TTS与音源来源

本机实测可用Windows SAPI的Microsoft Huihui Desktop（简体中文），另枚举到Zira、Haruka、David。实际使用Huihui、Rate=3生成三句，再用本地ffmpeg裁掉首尾空白、atempo适配关键时间、轻微EQ与短反射。它是**桌面TTS样音**，不是专业真人配音或声线克隆；声线庄严度/听感交用户试听，不宣称正式声音艺术验收已完成。

定稿三句均有实际PCM语音：

> 以此真名，缔结星约。
> 让群星回应，让命运见证——
> 星门，为您而开！

实际开始时间约0.06、1.75、5.00秒；最后一句结束约6.54秒，接6.8秒收束。第二句SAPI源以句号生成，破折号悬念由4.25—5秒的声场收束/停顿和字幕呈现。未扩展角色配音。

当前工具目录没有可调用的专用语音生成工具；ComfyUI实查注册了ElevenLabs/FishAudio/HeyGen云API语音节点，但注册不等于凭证、模型或可用生成已经验证，本轮未调用这些服务。能力记录为 `ArtSource/Audio/P04/tts_capabilities.json`。本轮确实运行的是本地SAPI，未复制Windows语音引擎到工程，游戏运行只播已生成WAV。

钟/低鼓式冲击、谐波垫音、扫描蓄力、四档揭晓、确认和跳过均由 `ArtSource/Audio/P04/build_audio.py` 的确定性NumPy合成生成，没有使用第三方音乐。垫音是合成和声，未冒称录制合唱。原始/拟合语音、生成脚本 `generate_sapi.ps1`、manifest与SHA256均在该目录。最初Rate=2音轨是能力探测保留稿，最终实际用Rate=3及audio_manifest。

## 改动文件与接口范围

| 工程内文件 | 内容 |
|---|---|
| Scripts/Presentation/ContractCeremonyView.cs | 新仪式及定稿字幕；保留U03的RisingContract/SignatureName路径 |
| Scripts/Presentation/FastRevealView.cs | 四档运动/构图、分次身份信息、清光、轻余光与短转场；保留RevealIdentity和RevealArtworkSlot/UnknownSilhouette |
| Scripts/Presentation/P04FeelConfig.cs + meta | 图层/音轨/关键时间/强度/音量配置 |
| Scripts/Presentation/SummonAudio.cs + meta | 五个2D音源、DSP计划旁白/垫音、揭晓/确认/跳过、约75ms淡出与实际触发记录 |
| Scripts/Presentation/CelestialLinesGraphic.cs + meta | 实时金线、晶体、射线的uGUI几何 |
| Scripts/UI/PrototypeGame.cs | 仅按U03交接接配置/音源、传本批最高色信号、等待余光、0.15秒转场、确认/跳过声音；签约与Draw逻辑不改 |
| Editor/P04FeelSetup.cs + meta | 可重跑素材/音轨导入、Main保存、Play复验、实际画面和内部混音录制 |
| Data/Presentation/P04FeelConfig.asset + meta | Editor生成的Main引用配置 |
| Scenes/Main.unity | Editor保存上述配置、内置字体、节奏与UR完整卡引用，保留GUID |
| Audio/P04/*.wav + meta | 13个实际语音/合成样音/试听文件及Unity导入数据 |
| Art/Effects/F03/上述八图的meta、UR完整卡的meta | Unity Sprite导入设置；未改原始PNG |

所有新meta/GUID均由Unity生成，引用通过AssetDatabase取得。U03到P04的UI最小改动在 `P04_FEEL_Verification/PrototypeGame.U03.diff`。未扩账号、OCR、规则、存档或大型框架；没有改Core、UIAdapter、UITheme、U03 UI场景/Prefab、U02主题、BuildSettings和Main.meta，8项保护SHA核对全部一致。A05源PNG未改，素材接入是Sprite导入。

没有发现项目中已交付并验收的Blender空间段或.blend，未等待或制作Blender前置；本轮完成2D/2.5D路线。

## 运行、复验与调参

1. Unity 2022.3.53f1c1打开工程并等待导入，打开Main按Play即可。默认昵称“召唤者”；修改PlayablePrototype上的 `playerDisplayName` 后，签署跳过与仪式读取同一值。
2. Editor菜单 `Tools/这次一定出/程序/P04-FEEL/接入素材声音并保存Main` 只导入明确管理的A05/音频资产、更新配置引用、保存Main。它会重设Main的默认演出时长；配置里的强度/音量数值保留。已有脏场景或Play时拒绝覆盖。
3. 菜单 `Tools/这次一定出/程序/P04-FEEL/验证Main输入音画并录制片段` 自动在独立Play内存复验、录制三个片段并退出Play；不会保存运行时昵称/调试批次。封装执行 `python Docs/Production/P04_FEEL_Verification/encode_clips.py`，本机ffmpeg路径为E:/FormatFactory/ffmpeg.exe；录音只取Unity混音，不读取系统麦克风/其他应用。
4. Main的PrototypeGame Inspector：昵称、签名字体、autoSignatureSeconds=0.4、ceremonySeconds=6.8、R/SR/SSR/UR=0.75/1.1/2.2/3.2。自由等待不计入动画时长。
5. 打开feelConfig指向的P04FeelConfig：sealImpact=.72、ringsAppear=1.35、contraction=4.25、gateBurst=5.85、字幕第二句1.75/最后句5；effectStrength=.85、shakePixels=12、giantRingScale=1.65、ringRotation=110。主音量.8、旁白.95、垫音.32、音效.65、停止淡出.075秒。先调音量和冲击时刻，再调环/光/震动；角色停留应保持安静清楚。

音轨按referenceSeconds=6.8制作。改变Main共同仪式时长会按比例改播放速度/音高和图层关键时间；若正式更换旁白或大改语速，应重对齐WAV与字幕关键时刻。更换素材/声音无需修改Draw。

## 实测证据与边界

最终Main Play报告：2026-10-08 19:09:38（UTC+8），Unity2022.3.53f1c1，`final_result.json`为passed。`checks.json`包含802条记录（大量重复文案扫描，不是802个独立用例）与73次音频事件。

- 单抽/十连真实内存仅提交2次，资源30→29→19，累计11抽；取消签署无扣费；补签自动继续；十连按同一对象顺序逐张停留且最后等待，NEW/收藏不被演出改变。
- 实际Play用GraphicRaycaster首命中+ExecuteEvents down/up/click，手写25点drag/up；演出点击仅当前张、三个阶段跳过到同批；等待Esc通过HandleEscape入口。没有声称实体鼠标/OS键盘自动化，键盘映射仍是现有GetKeyDown；实际按键手感留用户复核。
- 三处跳过后180ms检查旁白/垫音/蓄力均已停止，没有重新抽取或后续音源拖尾。四档揭晓均有实发事件和同场混音。
- 实际盖印触发约.714—.718秒，爆开约5.846—5.848秒，时间线落点最大触发误差约5.86ms，见audio_sync.json。这是Unity触发与视觉时间线对齐，不是测量硬件扬声器延迟。
- 三片均包含H.264画面与AAC声音，内部原音48000Hz双声道、非零RMS，PCM削波样本0。采集目标24fps，实际约23fps，按帧实时时戳封装；不是性能基准。clip_manifest记录时长、采样、SHA256及流信息。
- Main三尺寸1280×720、1920×1080、1600×1000实测，19张实际PNG；SSR/UR保持比例，强光已退出脸部，身份/进度/跳过/确认不冲突。截图已目视检查；它们不替代上述实际Play。
- Runtime/Editor最终编译exit=0，旧U02 JSON DTO的CS0649警告仍在，P04无编译错误。

首次Main试跑发现契约节点路径被改导致U03初始化中断，已恢复稳定路径；复验工具改为匹配按钮子图形并等待切页渲染后再点击。交付证据为修复后完整通过的重跑，不使用早期失败录制。

## 下一步与学习点

C08验收看/听新片段并实按鼠标、Space/Enter/Esc，确认庄严程度、冲击/声音强度与长时间欣赏手感。桌面TTS声线仍可换正式配音；未有缺语音，实际样音已交。UR透明层仍待A05后续精修，当前完整卡不阻塞Main。未来有经过评审的Blender通用空间段再替换，动态签名/昵称和结果交互仍留Unity。

涉及Unity概念：Sprite透明图层、RectTransform缩放/倾斜/逆向旋转、MaskableGraphic即时线条、CanvasGroup升空淡出；协程分开动画和确认；AudioSource/PlayScheduled按DSP对齐、短淡出清尾；ScriptableObject暴露调参；Editor AssetDatabase/场景保存建立真实引用；Play截图与OnAudioFilterRead记录同场音画证据。
