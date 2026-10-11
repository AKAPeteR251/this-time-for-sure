# P23-CONTRACT — F09 v003 写实契约接入

状态：已完成 Main 集成与实际 Play 验证，视觉待验收。

授权：用户提供 F09_ContractParchmentWax_v003.zip 并要求继续接入。明确范围为 Main 签署页和签约演出，以及必要的配置、导入与验证工具；不改 Core、规则、存档、已验收星云和角色揭示。

## 目标

- v003 4:3 羊皮纸和英文连笔文字独立图层等比合成；去掉旧纸框与中文正文。
- 签名区域依素材坐标 (205,922)—(1475,1275)，游戏用户名动态显示，手写松笔完成、签署跳过补签继续不变。
- 酒红圆环水晶蜡印白色 Tint；落印时接触影出现，印面跟纸升空，不重复出现第二枚印。
- 实际 Main Play 核对输入、一次 Draw/扣费、三窗口比例、盖印与跳过清理，交付截图和带声片段。

## Unity 概念与调参

Image 分层透明合成；4:3 容器等比适配；RectTransform 锚点使用左下原点，素材像素坐标由左上原点转换；字迹保存为区域内归一化轨迹，签署页和演出使用同一签名框。

## 改动与接口

- `Assets/_Game/UI/Sprites/F09/ContractParchmentWax_v003/`：四张运行 PNG，保留 v003 原文件内容；Sprite 单图、2048 最大尺寸、无压缩、Clamp，无 mipmap；Unity 生成 meta。
- `Scripts/Presentation/ContractPaperLayout.cs`：共享 4:3 纸面和英文层排版。`Available / Fit / Layer / Place` 仅负责视觉；不创建数据模型。
- `Scripts/UI/PrototypeGame.cs`：Signing 使用新纸面，笔迹仍以归一化坐标输入；更新演出 SignatureName 的子节点路径。
- `Scripts/Presentation/ContractCeremonyView.cs`：相同留白签名框、落印时出现独立影层、白色 Tint 保留酒红印面；印和影归属纸根，随纸升空缩小；Finish 清印与影。
- `Scripts/Presentation/P04FeelConfig.cs` / `Data/Presentation/P05FeelConfig.asset`：新文字层、接触影及布局配置。旧素材继续保留。
- `Editor/P23ContractSetup.cs` / `P23ContractVerify.cs`：可重复导入配置与 Main 验证工具，录制仅采集本次 Unity AudioListener 混音。

Main 仍使用当前配置引用，由运行 UI 搭建新纸面；未改 Main 场景文件、Core、随机规则和存档。

## 调参入口

选中 `Assets/_Game/Data/Presentation/P05FeelConfig.asset`，在「F09 写实羊皮纸与蜡印」调整：

- `contractAspect`：当前 4/3；纸与字层整体等比。
- `contractSignatureRect`：左下原点的 (左,下,右,上)，当前由签名像素矩形准确换算。
- `contractWaxRect`：相同坐标制，当前对应 (1520,1130)—(1880,1490)，印与影同步。
- `contractWaxShadowAlpha`：接触影透明度，当前 0.7。
- 原有 `sealImpact`：落印声、接触影和落地同步时间，当前 0.72 秒；不要调至 0.4 秒之前。

落印初始放大 3.4 倍、纵向下降 800 UI 单位在 ContractCeremonyView 中；1.05—2.4 秒纸与印共同升空。保留现有签名成立金线与旁白，动态用户名没有进入图片或视频素材。

## 运行与证据

打开 Main，点击单次/十连召唤进入签署页。在留白写一笔并松开，或点击右上跳过自动补用户名，继续盖印与水晶阶段；签署返回/Esc不扣费，演出跳过/Esc到同批总览。

重新导入：Editor 菜单 `Tools/这次一定出/程序/P23-CONTRACT/保存F09契约引用`；实际验证：同菜单下 `验证Main输入音画并录制片段`（开发入口，启动新的内存 Play 会话）。

2026-10-11 09:23（北京时间），Unity 2022.3.53f1c1 图形 Editor 实际 Main Play 通过。验证使用 GraphicRaycaster 和 EventSystem 指针事件；Esc 使用既有 HandleEscape 入口，不冒充人工键鼠验收。证据在 `P23_CONTRACT_Verification/`：

- `final_result.json` / `checks.json`：1280×720、1920×1080、1600×1000 等比与安全区；手写松笔前不扣费；松笔唯一单抽；用户名「星旅者」跳过补签；十连直接继续；最终 2 次提交、11 抽、资源 19；演出 Esc 保持同批且清白幕、印与影。
- `1280x720_signing.png`、另外两尺寸 signing 图：未签署无蜡印，英文排版和留白可见。
- `1280x720_handwritten.png`、`1280x720_wax_landed.png`、`1280x720_contract_rising.png`：实际笔迹、动态名字、酒红印落地和升空。
- `main_p23_contract.mp4`：实际 Main 连续画面与同次 AudioListener 音轨，约 5.64 秒、122 帧、平均 21.63fps；48kHz 音频，未发生削波。封装依据逐帧时间戳，不重建音轨。
- `clip_manifest.json`：片段哈希、画面/声音参数；`setup_status.txt`：配置导入完成。

限制：这是 UI 分层羊皮纸与蜡印，褶皱和印面材质来自素材，不是纸张物理或可变形 3D 蜡。本批没有重做旁白；VITS 日语少女/成熟女性声音仍是既有后续需求。祭坛打光、火焰和水晶破裂视觉仍按原待办执行。
