# F05-ART-R3 素材交接

按 `F05_ART_R3_揭晓场景素材清单.md` 已交付15项独立PNG及1920×1080拼合预览。

目录：`D:/vibe/这次一定出/ArtSource/Effects/F05/RevealR3/`。

完整文件/规格/提示词/工具/导出处理/Unity导入建议见目录中的 `manifest.md`；像素级检查和逐文件哈希见 `validation.json`。

- 已交：星空、远近两层星云、圆形恒星、四芒星、流星、六臂雪花、R/SR/SSR/UR字图、独立横线、NEW文字和白色开放边框、单片冰屑。
- 冰法师直接复用现有透明立绘，未重画。所有生成原稿在raw_generated；没有覆盖v002。
- 15项尺寸/透明边缘、四档字图基线、两层星云左右循环RGBA均检查PASS；缩小字图/NEW和实际图层拼合已查看。原生生成尺寸和放大导出尺寸分别记录，4K底图是放大导出。
- 预览 `reveal_layout_preview_v003.png` 由交付素材和原角色实际拼合；中文名仅在预览示意，运行时保持动态排版。NEW彩色边框仅为示意，真正交付的是白色透明遮罩。
- 本任务没有修改Unity代码、场景、概率或存档，也没有导入Unity生成meta。程序仍需扩展四档标识/分层背景配置，验证UV漂移、恒星闪烁、偶发流星、NEW动态边框与Main页面。

状态：**素材已准备，素材检查通过；程序接入和用户视觉验收待验收**。

复验：在Windows运行目录中的export_assets.ps1可重建本批规范PNG和预览；check_assets.py仅只读检查图片、输出validation.json。Unity概念与调参说明在manifest最后一节。
