# This Time, For Sure

《这次一定出》是一个由 AI 协作开发的 Unity 学习项目。内层抽卡与角色收藏游戏名为 **《星界回响》**。

当前原型已验证抽卡闭环、手写契约、点击水晶蓄能与破裂、角色揭晓和逐张确认，以及结果小卡/详情/图鉴展示。十二人的正式素材映射已完成，下一任务是将新素材接入现有游戏；三人编队、自动战斗与头像冷却大招仍处于后续规划。

## 打开工程

使用 **Unity 2022.3.53f1c1**、URP **14.0.11**、uGUI 与 TextMeshPro。

Unity 工程在仓库内的 `这次一定出/` 子目录。通过 Unity Hub 添加这个子目录，打开 `Assets/_Game/Scenes/Main.unity` 进入当前原型。首次打开由 Unity 重新生成 Library 等本地缓存。

本仓库用 **Git LFS** 保存图片、音频、视频、字体和 Blender 源文件。克隆前安装 Git LFS：

```sh
git lfs install
git clone https://github.com/AKAPeteR251/this-time-for-sure.git
cd this-time-for-sure
git lfs pull
```

## 项目目录

| 路径 | 内容 |
|---|---|
| `这次一定出/Assets/_Game/` | 游戏代码、运行素材、场景与 Editor 工具 |
| `这次一定出/Packages/` | Unity 包依赖 |
| `这次一定出/ProjectSettings/` | Unity 工程设置 |
| `ArtSource/` | 原始美术、工作流、参数与源文件 |
| `Docs/Production/` | 施工规划、共享接口、任务及验收记录 |

施工前阅读 [协作约定](AGENTS.md) 和 [施工包入口](Docs/Production/README.md)。最新角色接入依据为 [C17 / P33 任务单](Docs/Production/19_C17_角色素材映射与Git初始化.md) 与 [十二人素材映射](Docs/Production/C17_character_asset_mapping_v001.csv)；玩法原案见 [V0.1 游戏策划案](这次一定出_V0.1游戏策划案.md)。

Unity `.meta` 与资产一同提交，保持资源 GUID。Unity 缓存、本地模型权重、重复交付 ZIP 和逐帧录屏编号图由 `.gitignore` 排除；正式源稿、选定截图、报告及已编码演示视频纳入版本。
