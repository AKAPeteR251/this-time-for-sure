# P29-CARD-UI · F10 卡面接入

状态：Main 实测通过，视觉待验收（2026-10-11 11:29，北京时间）。

## 授权与范围
用户交付 F10_CharacterCardUI_v001.zip 后继续角色卡面工作。接入结果总览、图鉴和详情共用卡面；最小改动 PrototypeGame、UI 主题、相关运行表现和 Editor 导入/验证工具。允许 Main 实际 Play 与主题引用保存，不改变 Core、概率、存档、角色身份及已验收的星云/演出。

## 素材与排版
原稿保留 ArtSource/UI/F10/CharacterCardUI_v001。只导入 Textures 中十二张运行 PNG；依据 layout.json 的 1024×1536、左上原点坐标叠层。角色使用项目原立绘，等比填满内窗并裁切；四档卡框共用 2:3 比例。稀有度为素材字样，名字和 NEW 为运行文字，NEW 仍读取实际批次标记。没有额外称号。

## 接口与调参
PrototypeUITheme 增加 cardFrame/Badge 四档、cardNewMask/Fill、cardNameShade/Ornament 引用；F10Card 共用总览、图鉴、普通详情。cardNewSpeed=.36、cardNewBlink=.35，cardUrSpeed=.12 可调整。缺失素材时沿用旧卡面。

## Unity 概念
AspectRatioFitter 保持外卡比例，EnvelopeParent 等比填满头像窗，RectMask2D 裁掉超出部分；归一化锚点从美术母版转换。Image 叠加透明分层，真实 Button 仍回调原详情接口。渐变边框不改变 NEW 判定。

## 验证
Unity 2022.3.53f1c1 Editor 编译及十二张 Sprite 导入通过。P29CardUIVerify 在 Main 实际 Play 验证 1280×720、1920×1080、1600×1000 三窗口：十连十张保持2:3、四档引用正确、名字一致、仅前四张开发固定样例带NEW，其余无NEW。实际等待检查NEW边框相位和UR高光颜色随时间改变。单抽SR003卡面点击通过EventSystem真实射线进入对应详情；图鉴切换及已拥有卡面点击详情通过。

另行真实随机十连经签署页“跳过”补签后，唯一Draw计数1、抽取数10、资源30→20；演出跳过保持同批。固定四档样例只在Editor验证中调用，不改变真实随机入口。

证据：P29_CARD_UI_Verification/final_result.json、checks.json；同目录三窗口ten_cards截图、sr_single、sr_detail、random_ten、collection截图。截图来自实际Main屏幕，未用美术预览替代。

运行：Editor菜单 Tools/这次一定出/程序/P29-CARD-UI/导入F10卡面 可重复保存十二个引用；P29CardUISetup.Reports/P29.request 写prepare或verify可执行导入或Main验证。验证需停止Play并保存已有编辑，结束自动退出Play，不改Main场景结构。

改动：Scripts/UI/PrototypeGame.cs、PrototypeUITheme.cs、CardIridescence.cs；UI/Styles/U02ArtTheme.asset；UI/Sprites/F10/CharacterCardUI_v001十二PNG及Unity生成meta；Editor/P29CardUISetup.cs、P29CardUIVerify.cs及meta。

限制：原角色没有立绘的卡仍沿用轮廓；冰法师详情沿用原纯立绘欣赏布局。UR是对现有蓝色框亮部轻微彩色变换，不重绘交付原稿。图鉴未收录半透明和不可点规则保留。尚未收到用户本轮视觉验收；未改已验收的逐张演出、星云和水晶。
