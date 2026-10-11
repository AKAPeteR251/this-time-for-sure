# P17-REVEAL-BURST-TUNE

用户授权：接入 F06_RevealBurst_v001.zip；增加双段间隔、放慢炸裂、光圈与人物留空隙。状态：Main集成完成，程序实测通过，视觉待用户验收。

范围：F06 四张透明 PNG、RevealArrivalBurst / RevealArrival Shader、FastRevealView 时序、演出配置及 Main 的每张时长。保持已验收星云、签约一次 Draw、逐张确认与跳过同批总览；不改 Core、角色模型或存档。

验证：Unity Main 实际 Play、带声片段、小爆发/黑影停留/大爆发截图、动画补完与跳过清理、三窗口及真实随机抽卡。证据与最终调参记录随完成补齐。

## 实现与调参

F06 三张碎片按轮换分配至两段的 20/32 个碎片节点，独立光尾置于碎片下方，全部关闭 Raycast Target。素材保留原稿及 ZIP，四张成品原样导入 `Assets/_Game/Art/Effects/F06/RevealBurst`，由 Unity 生成 meta。

在 Main 的 PrototypeGame 上调整每张演出时长：R 2.8秒、SR 3.3秒、SSR 4秒、UR 4.6秒。动画中点击仍立即补完当前张，等待阶段另一次点击才继续。

配置为 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`：`silhouetteBurstEnd=0.24`（小爆发结束）、`characterBurstStart=0.46`（大爆发开始）、`characterBurstSpan=0.34`（大爆发持续）。数值是每张总时长的比例，冰法师为小爆发0—0.96秒、黑影停留0.96—1.84秒、大爆发1.84—3.2秒，随后稀有度与NEW完成。姓名、立绘由黑变亮、脸部近景缩回、音效随第二段开始参数同步。

碎片与光圈扩张改为平滑加速再减速；小/大光圈半径 `0.62/0.88`，中心增加椭圆暗区。`revealLightGap=0.075` 控制人物周围留空，第二段脸部放大时再自动增大间隙。半径/间隙按画面高度归一化；不是固定像素。`revealBurstStrength=0.85` 控制整体强度；素材引用可直接换 Sprite。

涉及 Unity 概念：Sprite 透明通道与 Image 色乘；Transform 径向位移；uGUI additive Shader 与中心透明遮罩；归一化时间驱动各阶段；ScriptableObject 集中调参。`RevealArrivalBurst.Begin/Animate/Clear` 是表现接口，不读取或改变抽卡结果。

本轮改动：P04FeelConfig.cs、RevealArrivalBurst.cs、RevealArrival.shader、FastRevealView.cs、P05FeelConfig.asset、Main.unity、P17RevealBurstTuneSetup.cs、F06四个PNG及其meta。本轮不修改已通过的星云Shader；旧P16开发接入菜单保留旧基准参数，调本轮请使用 P17 菜单。

复现：Unity打开Main；菜单 `Tools/这次一定出/程序/P17-REVEAL-BURST-TUNE/接入素材声音并保存Main`，再使用同组实际验证录制菜单。仅在退出Play且场景已保存时接入。

## 实际验证

2026-10-10 21:11:22（北京时间），Unity 2022.3.53f1c1 Main Play实测通过。证据目录 `Docs/Production/P17_REVEAL_BURST_TUNE_Verification`：final_result.json、checks.json、小炸裂/黑影停留/大炸裂截图及1280×720、1920×1080、1600×1000等待截图。

录制36.43秒、812帧、1280×720；声音来自本次Unity AudioListener，48kHz双声道，无削波。`main_p17_burst_tune_focus.mp4` 是同一实际录像的前10秒截取，未改变播放速度或重做音轨；完整版 `main_p17_burst_tune.mp4`。

输入证据通过实际Main中的GraphicRaycaster/ExecuteEvents发送空白点击及水晶点击，Esc通过HandleEscape入口调用；不声称是用户手动按键。小/大爆发中途跳过返回同批总览、清光圈/碎片/前景闪光；动画中点击只补完当前张、不穿透；每张及末张保持新确认。真实手写单抽与补签十连仍分别只Draw一次。四档稀有度使用各自真实结果字图。

核验F06四张PNG与原稿一致，Unity meta齐全；已验收星云Shader哈希与本轮开始时一致；Core及受保护UI文件未改。双端编译通过。美术包包含的Blender母版和生成脚本仅归档，未执行或用它们替换场景。后续依赖：用户验收双段节奏和光圈间距。
