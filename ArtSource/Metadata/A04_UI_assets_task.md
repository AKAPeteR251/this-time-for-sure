# A04-UIASSET：UI 搭建素材包

状态：制作完成，待 Unity 导入与运行验收。

依赖：用户认可的 RPG 角色画风、A03 三页 UI 参考图、共享契约与美术职责。此次补充背景、透明冰法师立绘、四种稀有度卡框、按钮与面板、装饰与光效。仅处理美术，不修改 UI 脚本或场景。

生成素材中的文字由 UI 使用 TMP 实现，不生成中文标签。卡框、面板和装饰使用透明底，图集需在 Unity Sprite Editor 中按明确网格切分。生成结果作为原始素材保留，尚未运行或导入的部分不标为已验收。

工具限制：本地终端启动报 `helper_unknown_error: setup refresh had errors`。如果归档复制仍不可运行，记录工具原始输出的绝对路径与准确导入方式，不编造归档、像素或哈希核验。

结果：通过可用执行路径恢复文件操作，已归档五张生成母图，导出 24 张独立新素材，并复制六张已认可职业稿与六张缩略图。全部最终文件位于 Unity Art 目录；来源、实际尺寸、alpha 与 SHA256 在 `ArtSource/UIAssets/asset_manifest.json`。提示词与导入说明位于同目录。上次 A03 结果页 v002 也已归档。

实际验证：23 张新素材有真实 alpha；四种卡框中心 alpha 为 0；四种按钮画布均为 832×336；背景为 1920×1080、立绘为 1024×1536、星芒为 512×512。已完成本地透明叠放与全部素材接触表检查；未运行 Unity，不标记为已验收。

学习与调参：详见 `ArtSource/UIAssets/README.md` 中 Sprite、Mask、Preserve Aspect、Border、Sprite Swap 与 Canvas Scaler 说明。
