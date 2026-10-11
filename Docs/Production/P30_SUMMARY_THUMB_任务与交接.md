# P30-SUMMARY-THUMB

状态：Main 实测通过，视觉待验收（2026-10-11 11:34，北京时间）。

用户要求结算界面参考 BA 改为无名字头像小卡。本任务仅修改结果总览布局和小卡显示，保留实际稀有度/NEW/原序/点击查看详情；图鉴和详情继续使用 F10，抽卡模型、签署、水晶、演出和存档不改。允许 Main 实际 Play 验证。

头像由原立绘上半部等比裁切，不替换角色。用轻微斜边几何小卡、稀有度色边和底栏字样，不新增星级规则。

## 改动与调参
PrototypeGame 的 ShowSummary 单独调用 SummaryThumbnail：十连两排五张，单抽居中一张。小卡宽高比 .90，斜边由 SummaryTileGraphic 共用的几何变换绘制；头像、底色、边框形状一致。没有名字区、元素图标或F10大卡框，NEW与稀有度仍读取同批结果。PrototypeUITheme.summaryFaceCrop 默认 (.18,.56,.64,.42)，坐标左下原点，可在U02ArtTheme调整头像范围。头像保持源图比例，超出部分裁切。

改动文件：Scripts/UI/PrototypeGame.cs、PrototypeUITheme.cs、新增SummaryTileGraphic.cs及meta；Editor/P30SummaryThumbVerify.cs及meta。Main场景结构及原角色素材没有改动。

## 实际验证
Unity 2022.3.53f1c1 编译通过。Main实际Play检查1280×720、1920×1080、1600×1000：十连十张、小卡比例、稀有度/NEW对应、结算不展示名字、每个头像/底色/边框CanvasRenderer实际生成四顶点。检查并查看真实截图确认头像已绘制。首轮缺CanvasRenderer导致图层不显示，增加显式依赖后重新完整验证通过，以最终03:34:58 UTC结果为准。

单抽小卡通过EventSystem真实射线点击进入原F10详情；图鉴仍保留原卡面并可点击已拥有角色。真实随机十连由签署页跳过补签产生唯一Draw，抽取10张、资源30→20；演出跳过保持同批。固定四档样例仅Editor验证入口。

证据：P30_SUMMARY_THUMB_Verification/final_result.json、checks.json，三窗口ten_cards截图、sr_single、sr_detail、random_ten、collection截图。写该目录P30.request=verify可重复执行Main验证，结束退出Play。图片来自真实Main，没有用参考图替代。

## Unity 概念与限制
MaskableGraphic与CanvasRenderer绘制斜边网格，Sprite的UV用于等比头像裁切；AspectRatioFitter在不同窗口保持小卡比例。文本和Button仍使用原uGUI接口。

未提供立绘的角色继续显示中性轮廓；本轮不生成新头像美术。背景和结算页底部按钮沿用项目现有设计，只参考BA的小图排布。用户尚未本轮视觉验收。
