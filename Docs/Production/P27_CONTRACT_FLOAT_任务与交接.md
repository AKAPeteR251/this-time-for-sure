# P27-CONTRACT-FLOAT — 契约漂入与环境火星

状态：已集成Main、实际Play通过，视觉待验收。

授权：用户要求契约不突然出现，而是飘入并持续缓慢漂浮，画面有飘动火星。范围仅签署及契约演出、必要配置和验证；保留燃烧、法阵运镜、唯一Draw与输入规则，不改Core/概率/存档。

方案：整张纸从下方斜向进入，轻微倾斜回正，抵达后慢速浮动；签署页与盖印/燃烧的纸根继承同一运动起点，火星使用同一全局时钟和确定的分散位置，不在切换时重置。

## 实现与改动

- `Scripts/Presentation/ContractPaperMotion.cs`：签署纸根以0.95秒从(-230,-640) UI偏移和-9°倾斜飘入，缓入停稳；仍原尺寸，抵达后约7 UI单位慢速浮动及轻微摆动。`BeginEntrance` 每次召唤重新进入；`Adopt` 将签署的起始时间交给盖印/燃烧纸根，避免重新入场或跳回中心。
- 同文件 `ContractEmbersGraphic`：30枚分散暖色软火星，慢慢向上并横向漂移，明暗轻微变化；无图片、无输入命中，签署和演出使用同一时钟/位置。
- `PrototypeGame.cs`：Signing纸容器增加漂浮组件与火星；召唤时启动漂入，将组件交给ceremonyView.Begin。无流程/扣费规则变更。
- `ContractCeremonyView.cs`：盖印与燃烧继承纸根运动；火星仅处于签署/契约演出阶段，随阶段隐藏，不增加祭坛火焰。
- `ContractBurnView.cs` / `ContractBurn.shader`：纸面坐标改成屏幕二维基向量求逆，使缓慢倾斜时每一层仍按同一纸面坐标烧穿。
- `P04FeelConfig.cs` / `P05FeelConfig.asset`：保存漂入、浮动与火星调参。
- `Editor/P27ContractFloatVerify.cs`：实际Main验证与音画录制入口。

不改Main场景文件，沿用现有运行UI搭建和配置引用；无新美术、Core、概率、存档或角色模型变更。

## 实测

2026-10-11 10:23（北京时间），Unity 2022.3.53f1c1图形Editor实际Main Play通过。证据位于 `P27_CONTRACT_FLOAT_Verification/`：

- `final_result.json` / `checks.json`：刚进入纸位于下方且进度不足20%，抵达后原尺寸，停留1.8秒位置持续缓慢变化；火星实际网格绘制；签署切盖印的纸根位置差小于2 UI单位。
- `1280x720_contract_arriving.png` / `1280x720_contract_float.png`：实际漂入与停留截图，已查看；火星在背景可见。
- 三段燃烧及另外两窗口截图：已核对纸仍原尺寸、动态层同步烧穿；手写松笔、跳过补签唯一Draw，最终提交2次/总抽11/资源19。
- 燃烧中Esc/跳过、点击补完仍清纸和灰烬，并进入同批结果或水晶阶段；真实射线/指针点击水晶仍有效。
- `main_p27_contract_float.mp4` / `clip_manifest.json`：约11.49秒、247帧，平均21.5fps，48kHz音轨无削波；实际Main帧时间戳画面与同次Unity AudioListener混音，含漂入、停留、手写、再次漂入补签、燃烧和运镜。未重建声音。

验证使用Editor驱动既有入口与EventSystem实际射线/指针事件，未声称人工键鼠验收；视觉待用户验收。

## 如何调参与Unity概念

选择 `Assets/_Game/Data/Presentation/P05FeelConfig.asset` 的「P27 契约漂入与环境火星」：

- `contractArriveSeconds`：漂入历时，当前0.95秒。
- `contractFloatPixels`：浮动幅度，当前7，按1920参考UI单位；不是越大越自然。
- `contractFloatSpeed`：当前0.6，控制停留浮动的速度。
- `contractEmberCount`：火星数量30。
- `contractEmberAlpha`：火星透明度0.5。

入口位置(-230,-640)、初始倾斜-9°和抵达后摆动0.8°在ContractPaperMotion.ApplyNow中。燃烧仍使用P26参数；保存/重新接配置可使用P26导入菜单，P27验证菜单为 `Tools/这次一定出/程序/P27-CONTRACT-FLOAT/验证Main输入音画并录制片段`。

涉及RectTransform父容器运动与子纸面等比、CanvasGroup入场透明度、unscaledTime跨页面继承、UI程序软粒子、屏幕坐标基向量。不使用纸张物理或真实3D卷曲；不新增火焰音效。
