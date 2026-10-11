from pathlib import Path
root=Path(r'D:/vibe/这次一定出')
text='''# P05-CRYSTAL 交接

任务：P05-CRYSTAL。依据AGENTS、共享契约、13/14及P03/U03/P04/A05 v004。状态：Main已集成、实际Play通过；视听风格与实体鼠标/键盘手感待用户验收，不标“已验收”。Unity工程为 `D:/vibe/这次一定出/这次一定出`。

## 实现与边界

签名松笔/签署跳过补游戏用户名 → 原唯一Commit/Draw → 4.35秒金线、巨印砸下升空、双句旁白前奏 → v004巨环水晶等待连击 → 达阈值后最后一句与1.60秒终结蓄力 → 24帧真实破裂/全屏白光 → SSR001约1.9倍脸部近景回落 → 原序逐张确认。十连共用一次水晶，末张同样等待；演出跳过/Esc清音清白幕并进同批总览。

水晶能量仅新输入增长，基础0.065，0.28秒内连击按间隔插值到最高1.5倍。三张v004状态按能量交叉淡化，独立金色漏光、脉冲、震幅连续增加；不是三次点击。短震继续接收输入，0.25秒后收震，能量不下降、不自动增加/破裂。鼠标仅水晶区域左键按下计入；Space/Enter使用GetKeyDown，长按不自动重复。同帧按键/指针入口去重。

PointerDown记住阶段和角色索引；水晶按下的PointerClick不能穿透到角色，即使松开时阶段已改变。终结阶段忽略普通推进；角色初始两帧保护、动画点击只补完当前张，之后新确认才推进。右上跳过独立置于白幕/碎片上方。

冰法师脸部按透明Sprite实际等比尺寸、faceUv和faceTarget定位，不以脚底缩放。0.40—1.15秒回落完整立绘；0.65秒原创八角星/双环/月桂几何SSR徽记，0.85秒开始每0.12秒姓名逐字淡入上浮，1.25秒称号、1.48秒NEW分开出现。固定字位，前字不随后字重新排版。身份左下、人物中央偏右；计数/跳过/确认保留U03安全区。

保留稳定路径 `RevealIdentity`、`RevealArtworkSlot/UnknownSilhouette`、`RisingContract/SignatureName`。新增 `CrystalStage`、`CrystalHeroIdentity`。仅SSR001使用新近景和几何徽记，其他角色沿用既有演出；不宣称四档正式视觉全部验收。

## 实际证据

证据目录：`D:/vibe/这次一定出/Docs/Production/P05_CRYSTAL_Verification/`。

- final_result.json / checks.json：Main实际图形Play；真实单抽与十连只提交2次，资源30→29→19，累计11抽，十张原对象顺序/NEW/收藏保持。固定SSR只经PlayBatchForDebug，不Draw/扣费/入库，正式页面没有强制SSR入口。
- input_records.txt：慢点约0.38秒一次，16次达阈值；高频约0.08秒一次，12次达阈值，短震期间接受下一次。第五次后停手1.1秒保留0.325能量并收震。含前奏/蓄能/终结/角色阶段跳过或HandleEscape记录。
- main_crystal_slow_pause.mp4：带声慢点/停手/同批两张SSR，新收录与非NEW、首末张确认。
- main_crystal_high_frequency.mp4：带声高频蓄能、最后一句、破裂白光、脸部缩回、第二张动画快速补完与新确认。
- main_crystal_skip_cleanup.mp4：前奏、蓄能、终结、角色四阶段跳过；200ms后旁白/环境/蓄力停止且白幕为0。
- main_random_contract_ten.mp4：真实随机手写单抽、签署跳过补签十连、同批逐张确认；与指定SSR片段分开。
- 1280x720、1920x1080、1600x1000各有prelude/crystal/face/portrait截图，已目视核对；截图不替代Play。
- 每段 *_capture/ 保存实际带时间戳JPG、unity_listener_mix.wav及capture.json；encode_clips.py封装H.264/AAC，clip_manifest.json含流、PCM峰值/RMS、削波、SHA256。录音为Unity自身AudioListener混音，没有系统麦克风或其他应用声音，也没有离线重建音轨。
- 输入为真实Play内GraphicRaycaster、ExecuteEvents down/up/click/drag和公开HandleEscape入口。Space/Enter/Esc映射已接入，未宣称实体OS键盘或人工鼠标体验已验收。
- compile_status.json：Runtime/Editor编译exit=0，仅既有U02 DTO CS0649。protected_before/after核对7项不变；PrototypeGame.P05.diff为相对P04的最小UI改动。

## 声音与素材

使用实际v004完整/裂纹/深裂、独立漏光、24帧及巨环背景，复制到Art/Effects/F04/Crystal；原ArtSource未覆盖、未重命名。水晶本体冰蓝，SSR独立漏光/信号/徽记为金色，角色冰蓝不与稀有度绑定。

旁白沿用已交P04本地SAPI中文样音，三段实际PCM复制到Audio/F04；前两句随前奏，等待不循环旁白，只有低音量环境；“星门，为您而开！”仅达到阈值触发，1.60秒后爆裂，未压进0.2秒白光。爆裂音跨角色初始化保留，点击提示音最短间隔0.095秒、低增益；跳过75ms淡出。

VITS日语少女/成熟女性慢语速为用户已延期需求，本批没有下载/安装模型或更换声音。v004背景/裂纹含烘焙暖光，R/SR/UR独立信号虽按真实批次最高档着色，整套中性素材和其他徽记仍依赖A06后续，不能把整张RGBA染色称为四档准确验证。

## 改动与运行

工程内改动：Scripts/Presentation 下新增CrystalChargeView、RevealPointerGate、RaritySigilGraphic；扩展P04FeelConfig、SummonAudio、ContractCeremonyView、FastRevealView；PrototypeGame仅加阶段、输入路由、现有仪式协程/配置绑定与清理。新增Editor/P05CrystalSetup、Data/Presentation/P05FeelConfig.asset、Art/Effects/F04/Crystal、Audio/F04。Main通过Editor保存P05配置，原meta/GUID、U02主题、六卡、内置签名字体、useFixedMock=false保持。

不改Core、概率/扣费/收藏/存档、PrototypeUIAdapter、角色模型、UIPrototype场景和UI Prefab、包版本或大框架。程序只消费已计算结果。

打开Main按Play即可。复验菜单：`Tools/这次一定出/程序/P05-CRYSTAL/验证Main输入音画并录制片段`。接入菜单同层“接入素材声音并保存Main”，只更新明确管理资产和Main；脏场景/正在Play拒绝覆盖。P05.request支持prepare/verify/status。不要用旧P04接入菜单重新覆盖Main的P05引用。

## 接口与调参

新增阶段 `CrystalCharging` / `CrystalBreaking` 追加在enum末尾，旧阶段枚举值保持。FlowStageChanged继续广播。`HitCrystal()`仅蓄能阶段接收；`CrystalView.Energy/AcceptedHits/ShakeAmplitude/WhiteAlpha`用于开发观察，不展示玩家计数。AdvancePresentation仍是前奏/角色补完及等待确认，蓄能用HitCrystal；SkipPresentation/HandleEscape沿用同批总览。RequestSummon、签名/补签、CommitContract、PresentationBatch和CurrentPull语义不变。

Main的feelConfig指向P05FeelConfig：preludeSeconds=4.35、hitEnergy=.065、comboWindow=.28、comboMultiplier=1.5、hitShakeSeconds=.25、crystalShakePixels=12（1280参考，1920画布换算1.5）、ruptureFps=28、finalChargeSeconds=1.60、whiteSeconds=.22、faceZoom=1.9、faceUv=(.5,.8)、faceTarget=(.64,.68)、nameCharacterSeconds=.12。先调蓄能速度，再调短震/漏光，再调脸部UV/目标；大改最后一句时同步finalChargeSeconds。SSR动画在PrototypeGame为2.2秒，确认等待无限；master/voice/bed/effect/stopFade音量入口沿用P04。

Unity概念：ScriptableObject集中调参；uGUI Image透明混合与Sprite序列；IPointerDown/IPointerClick跨阶段隔离；协程把自动前奏、交互蓄能、终结、角色和确认拆开；MaskableGraphic网格绘原创徽记；RectTransform结合图像等比尺寸计算脸部焦点；AudioSource分段调度与淡出；Editor AssetDatabase保存真实引用、OnAudioFilterRead同场录音。
'''
(root/'Docs/Production/P05_CRYSTAL_交接.md').write_text(text,encoding='utf-8')
print('Handoff written')
