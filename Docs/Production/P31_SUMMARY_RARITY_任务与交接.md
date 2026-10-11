# P31-SUMMARY-RARITY

状态：Main 实测通过，视觉待验收（2026-10-11 11:47，北京时间）。

用户要求结算头像小卡UR边框为渐变彩色闪烁框，并优化四档稀有度文字居中。本任务只修改小卡边框和稀有度字排版，允许Main实际Play验证，不改变图鉴、详情、抽卡数据、已验收演出或原始素材。

## 实现与文件
新增 Scripts/UI/SummaryRainbowFrame.cs：沿斜边四周分段彩色渐变，随时间流动和闪烁；透明内区保留头像与底栏。PrototypeGame 的 SummaryThumbnail 只对UR叠加此框，其他档位保留静态色边。

四档字字号34→30，文字区水平中心按底栏斜边几何改为 .443；新增 RarityOpticalCenter.cs 根据 CanvasRenderer 实际渲染字形的包围盒修正字体留白，让可见字形水平/垂直居中。避免仅以字体行高居中导致偏下。没有修改角色名和结果数据。

PrototypeUITheme 新增 summaryUrSpeed=.35（每秒相位）、summaryUrBlink=.32（闪烁幅度）；SummaryRainbowFrame.blinkFrequency=1.6Hz。均为程序UI参数，无新美术依赖。新Editor验证工具 P31SummaryRarityVerify.cs，Unity生成meta保留。

## Main实际验证
Unity 2022.3.53f1c1 编译通过。Main实际Play验证1280×720、1920×1080、1600×1000：UR边框网格256顶点实际渲染；等待.23秒后相位和闪烁强度实际改变；每张稀有度文字渲染轮廓中心距目标小于2个UI单位。截图复核头像、斜边、字形居中和多色边框。

仍验证真实UI射线点击详情、图鉴切换和签署跳过补签后真实随机唯一Draw（30→20，抽取10张），演出跳过同批。最终 final_result.json 时间03:47:18 UTC；首轮TextGenerator与实际渲染轮廓不同导致检查失败，现改用实际CanvasRenderer包围盒后通过。

证据目录：P31_SUMMARY_RARITY_Verification，含checks.json、final_result.json、三窗口ten_cards及single/detail/collection/random截图。可向该目录P31.request写verify重复运行，结束自动退出Play。

## Unity概念与限制
MaskableGraphic通过顶点颜色在边框内插渐变，Update只更新边框网格；CanvasRenderer给出真正可见字形的几何中心。闪烁不影响Button射线或稀有度判定。截图只显示某一相位，动态已由实际运行等待检查；用户视觉待验收。
