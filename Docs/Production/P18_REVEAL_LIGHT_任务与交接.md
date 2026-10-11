# P18-REVEAL-LIGHT

用户授权：碎片更快炸开，不宜看清；光圈调整模糊、散光和颜色。状态：Main集成及程序实测通过，用户于本轮回复“暂时验收通过”。本项暂时已验收，保留后续调优空间。

范围：只调整角色出现表现与调参配置、接入 Main。保留P17两段间隔与每张时长、已通过的星云、唯一Draw、逐张等待和同批跳过。

方案：碎片运动和打光分开计时，碎片短时间高速冲出并消隐；取消规则椭圆环的连续亮边，使用柔和、不均匀的散射光瓣与蓝紫色层次，保留人物周围暗区。

## 修改与调参

`RevealArrivalBurst` 的碎片使用独立归一化进度，高速向外冲出，拉长径向形状与光尾，降低实体碎片亮度并快速淡出。小段从总进度0.08开始，避免短促碎片被水晶白幕遮挡。光圈继续使用原本较长的进度，保留小爆发后黑影停留0.88秒、第二段1.84秒开始及SSR总时长4秒。两段顺序、脸部近景和姓名/稀有度/NEW时序不变。

`RevealArrival.shader` 去掉规则亮边和尖锐分区光线，改为连续、宽而不均匀的光瓣，用两层柔和衰减扩散，人物附近仍保留中心暗区。颜色由角色稀有度底色叠加蓝紫/青蓝层次，减弱灰白发亮的环。

调参入口 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`：

- `silhouetteBurstStart=0.08`：小段在白幕后开始，SSR为0.32秒。
- `revealShardTravelFraction=0.26`：占各段时长的比例；越小碎片越快。SSR两段约0.17/0.35秒。
- `revealLightSoftness=0.14`：越大光芒越模糊、扩散越宽。
- `revealLightScatter=0.8`：越大分布越不均匀、越少完整闭合环感。
- P17的 `revealLightGap=0.075` / 半径0.62、0.88仍可调节间隙与范围。

涉及Unity概念：同一演出内不同视觉层的独立时间曲线、径向位移与淡出、Sprite拉伸光尾、Shader连续角向变化与柔光衰减、ScriptableObject调参。

改动文件：P04FeelConfig.cs、RevealArrivalBurst.cs、RevealArrival.shader、P05FeelConfig.asset、P18RevealLightSetup.cs；P18接入工具保存Main。没有生成新美术、修改原稿或改动已验收星云。`FragmentAlpha` 供Editor观测碎片是否结束，`SmallPulse/BigPulse` 继续观测散光；Clear同时清两者。

复现：退出Play、保存场景后运行 `Tools/这次一定出/程序/P18-REVEAL-LIGHT/接入素材声音并保存Main`，再使用同组验证录制菜单。本轮用P18入口，旧菜单保留各自历史基准参数。

## 实测证据

2026-10-10 21:26:49（北京时间），Unity2022.3.53f1c1 Main Play最新一轮通过。证据目录 `Docs/Production/P18_REVEAL_LIGHT_Verification`：final_result.json、checks.json、compile_status.json、四张F06素材校验、已通过星云Shader哈希保护记录。

实际观测小/大段碎片先快速出现、随后归零但散光继续；黑影停留、姓名先于稀有度、逐张新确认与最后一张等待、动画点击只补完当前张、小/大段中途跳过清碎片/光圈/闪光并进入同批总览均通过。1280×720、1920×1080、1600×1000以及四档字图实测；真实手写单抽与补签十连仍分别一次Draw。输入由Main内GraphicRaycaster/ExecuteEvents及HandleEscape入口注入，未声称用户物理按键操作。

实际录制35.96秒、803帧、1280×720，声音来自同次Unity AudioListener的48kHz双声道混音，无削波。`main_p18_reveal_light_focus.mp4` 为完整版前10秒，保持实际速度和原音轨。截图 `1280x720_small_fast_shards.png` / `1280x720_large_fast_shards.png` / `1280x720_large_character_burst.png` 分别显示短促碎片与碎片消失后的散光。原素材未修改；Core、受保护UI、已通过星云Shader保持原样。等待用户评估最终视觉，不标为已验收。
