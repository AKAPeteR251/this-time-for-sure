# P32-SUMMARY-AURA

状态：已验收。用户于2026-10-11明确确认“效果可以 验收通过”；Main实测通过于同日11:52（北京时间）。

用户授权：结算小卡NEW放左上角一半出框并描边；UR外围粉光、SSR外围金光，两档加浮动白色碎片。仅修改结算卡表现及Main实际验证，保留UR渐变闪烁、无名字小卡与详情点击，不改角色模型、随机或已验收演出。

## 实现与调参
PrototypeGame.SummaryThumbnail 的 NEW 锚点改为(0,.92)到(.50,1.08)，中心跨越上框线，轻微倾斜-8度，保留暖白文字，增加深色2UI单位描边；继续按实际可见字形居中。NEW判定仍使用原批次IsNew。

新增 Scripts/UI/SummaryCardAura.cs：绘制16层外围透明渐变带，UR为粉色(1,.32,.72)，SSR为金色(1,.73,.20)；底层叠在卡面背后，不覆盖头像。十个白色菱形碎片沿外围缓慢漂浮并轻微明灭，均无射线命中。保留UR原彩色流动闪烁框。

PrototypeUITheme 新增 summaryAuraStrength=.75、summaryAuraWidth=.28，分别控制外围光强和扩散宽度；碎片速度在SummaryCardAura.Update的.12及沿周长的.19系数，碎片数量10、大小约卡宽1.3%。本轮无新美术依赖，未改原素材。

改动文件：Scripts/UI/PrototypeGame.cs、PrototypeUITheme.cs，新增SummaryCardAura.cs和Unity生成meta；新增Editor/P32SummaryAuraVerify.cs及meta。本轮不改Main场景结构。

## 实际验证
Unity2022.3.53f1c1编译通过。Main实际Play检查1280×720、1920×1080、1600×1000：固定原序样例只有一张UR与两张SSR具有外围光，粉/金档位正确；柔光及白色碎片296顶点实际渲染；等待.25秒后碎片位置改变。NEW文字实际渲染轮廓跨越上框线、有Outline，四档稀有度继续居中。截图复核卡面、外光、白碎片与出框NEW。

原流程检查仍通过：详情点击使用真实EventSystem射线，图鉴原F10卡面正常；另行签署补签后真实随机十连唯一Draw，资源30→20、抽取10张，演出跳过保持同批。最终运行时间03:52:42UTC。

证据目录P32_SUMMARY_AURA_Verification：final_result.json、checks.json、三窗口ten_cards与single/detail/collection/random截图。可向同目录P32.request写verify重复Main验证，结束自动退出Play。

## Unity概念与限制
MaskableGraphic使用顶点Alpha绘制柔光、用独立菱形网格绘制碎片；NEW利用超出父级的锚点和Outline描边。本页无裁切遮罩，因此允许文字及柔光伸到框外；所有效果raycastTarget=false，保留原点击行为。截图显示某一时刻，漂浮和闪烁在实际运行中检查。用户本轮视觉已验收。


验收基线：当前结算头像小卡、无名字排布、四档稀有度居中、UR渐变闪烁框、NEW左上半出框描边、UR粉色外围光、SSR金色外围光及浮动白色碎片。当前参数记录于本文，后续修改以本版本为对照。

