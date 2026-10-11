# P01-ART 临时角色映射交接

日期：2026-10-08。执行：程序 agent。状态：待验收（本轮源码与文件核对完成）。

任务依据：`08_F02_美术接入任务单.md` 的 P01-ART。F01 内存数据、A04 目标文件及 C06 文件审查依赖已具备。本轮只修改现有六条 Name/Title，保留十二 ID、每条稀有度、5/4/2/1 数量、基础概率与资源逻辑。不接图片、不改场景，不引入新模型、正式门面或存档。

## 实际改动与唯一数据源

游戏源码仅改 `这次一定出/Assets/_Game/Scripts/Core/PrototypeGacha.cs` 六条构造数据中的 Name/Title。现有 `.meta` 保留，其余六条显示信息保留。

- 类型：`ThisTime.Game.PrototypeCard`，实际字段是 `string Id/Name/Title` 和 `CardRarity Rarity`。
- 数据源：`PrototypeGacha.Cards`（`readonly List<PrototypeCard>`）。名称和称号在这里维护，页面不另写一份。
- UI 入口：`IPrototypeUIAdapter.Cards`。`MemoryPrototypeUIAdapter` 返回持有的 PrototypeGacha.Cards；`FixedPrototypeUIAdapter` 也读取同一角色表。两种适配器都会得到更新后的文字。
- 结果入口：`PrototypePull.Card` 指向抽到的卡牌定义，UI 从该对象读取名字、称号和等级；`PrototypePull.IsNew` 保留原有计算。
- 已搜索 UI 源码，未发现这六名旧角色名字/旧高级称号硬编码，因此无需提交 UI 修正 diff。未修改 UI 文件。

## 唯一临时映射表

卡图与缩略图目录统一为 **`Assets/_Game/Art/Characters/UIMockRoles/`**（相对 Unity 工程）。下表是本轮 U02-ART 的引用依据；不从文件名推导角色 ID 或等级，不自动绑定旧 Characters/SSR001、Characters/UR001 图。

| ID | Rarity | Name | Title | 卡图文件 | 缩略图文件 |
|---|---|---|---|---|---|
| R001 | R | 星魔法少女 | 星辉契约者 | 06_star_magical_girl_v001.png | 06_star_magical_girl_v001_thumb.png |
| SR001 | SR | 光牧师 | 圣光祈愿者 | 02_light_priest_v001.png | 02_light_priest_v001_thumb.png |
| SR002 | SR | 风精灵弓箭手 | 风林游侠 | 03_wind_elf_archer_v002.png | 03_wind_elf_archer_v002_thumb.png |
| SR003 | SR | 雷巫女 | 雷霆祈愿者 | 05_thunder_miko_v001.png | 05_thunder_miko_v001_thumb.png |
| SSR001 | SSR | 冰法师 | 冰城守望者 | 01_ice_mage_v001.png | 01_ice_mage_v001_thumb.png |
| UR001 | UR | 火女战士 | 炽焰先锋 | 04_fire_warrior_v001.png | 04_fire_warrior_v001_thumb.png |

称号是本轮程序补充的临时显示文案，未冻结人物故事。其余 **R002/R003/R004/R005/SR004/SSR002** 继续使用明确占位图，不复用六张图假充十二名角色。

SSR001 主页/详情可另引用 `Assets/_Game/Art/Characters/UIIceMage/UIIceMage_character_v001.png` 的整人物透明层，角色显示信息仍从 SSR001 定义读取。UR001 仅使用完整卡图，不声称已有透明人物或分层资产。

## 实际验证与证据

1. 修改前后逐条比较十二条 ID/稀有度，顺序完全一致，数量仍为 R=5、SR=4、SSR=2、UR=1。
2. 修改前后屏蔽角色构造数据再比较其余源码，完全一致；实际替换仅六条 Name/Title。随机权重 70/24/5/1、初始资源 30、免费补充 10 和收藏计算均未改动。
3. 六张卡图及六张缩略图均存在，每条路径在 A04 manifest 中唯一，SHA256 全部匹配。精灵明确选择 v002。证据：`Verification/P01_ART_audit.json`。
4. 使用现有 Unity 2022.3.53f1c1 安装中的 NetCoreRuntime/dotnet.exe + DotNetSdkRoslyn/csc.dll、netstandard2.1 和已有 Unity/uGUI 引用，编译当前四份运行脚本（Core、UI、适配器、主题）。退出码 **0**，无编译诊断。证据：`Verification/P01_ART_compile_status.json`、`P01_ART_compile.log`；复跑参数在 `P01_ART_runtime.rsp`，命令如下。

```powershell
& 'E:/Unity/2022.3.53f1c1/Editor/Data/NetCoreRuntime/dotnet.exe' 'E:/Unity/2022.3.53f1c1/Editor/Data/DotNetSdkRoslyn/csc.dll' '@D:/vibe/这次一定出/Docs/Production/Verification/P01_ART_runtime.rsp'
```

编译产物 `Verification/P01_ART.Runtime.dll` 只用于校验，不复制到 Assets，避免重复类型。以上是独立编译与文件证据；没有实际 Unity Editor 导入或 Play，本任务不宣称素材绑定、运行显示或 Main 集成成功。

本轮常规终端和 Node 文件访问因 Windows 沙箱初始化故障无法启动，后续仅以自动审批通过的项目文件读写及本地编译完成本任务；没有对外发送项目素材。

## U02-ART 接入与复验

1. 在已有 Unity 执行 Assets → Refresh，等待编译；按 A04 清单导入 Sprite 并生成 .meta，不手写 GUID。
2. `PrototypePortrait.characterId` 使用上表 ID，`portrait` 引用对应卡图；缩略图按实际 UI 所需明确选取。Name/Title/Rarity 读取适配器数据，不照抄参考图文字。
3. 在 UIPrototype 设置 `useFixedMock=false`，继续使用内存抽卡适配器。固定 mock 只用于页面检查，不能代表随机抽卡验证。
4. 主页面及 SSR001 预览应显示“冰法师 / 冰城守望者”；UR001 预览应显示“火女战士 / 炽焰先锋”。通过固定 mock 顺序可检查其余四条文字；图片绑定、分辨率和最短流程由 U02-ART 实测交付。
5. U02-ART 独立运行验收后，再交 P02-FAST 集成 Main。本任务已完成所需映射交付，不等待旧完整 P01 的事务门面。

总指挥任务板建议 diff：仅将顶部 P01-ART 的“待派工”改为“待验收；六条 Name/Title 已更新，十二 ID/稀有度与其余代码不变，12 张目标图片哈希匹配，当前运行脚本编译通过；见 P01_ART_交接.md”。本轮未写共享任务板或他人任务单。

## Unity 学习点与调参

稳定 ID 用来连接卡图与收藏，名字变更不改变归属。普通 C# 角色表是显示数据源，Sprite 是 UI 图像引用，两者通过 characterId 对应。改临时姓名/称号只修改 PrototypeGacha.cs 的构造参数；换图在 U02 的 Portraits 引用中调整，保留 2:3 比例。不要为了换图调整等级或概率。