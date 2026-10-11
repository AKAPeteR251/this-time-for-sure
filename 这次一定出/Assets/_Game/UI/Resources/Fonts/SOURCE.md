# U03-CLEAN 签名字体

LXGW WenKai Regular（霞鹜文楷），v1.522，未修改字体。

- 作者与上游：https://github.com/lxgw/LxgwWenKai
- 固定版本：https://github.com/lxgw/LxgwWenKai/releases/tag/v1.522
- TTF：https://github.com/lxgw/LxgwWenKai/releases/download/v1.522/LXGWWenKai-Regular.ttf
- 许可：SIL Open Font License 1.1；完整版权与许可证见同目录 `OFL.txt`。
- 导入：Unity 动态字体，Include Font Data 开启，字体家族 `LXGW WenKai`。
- 用途：契约自动签名和契约成立时的用户名。动态昵称由 Unity Text 绘制，不写入美术。
- 发包时随游戏附上完整 `OFL.txt`，不单独销售字体。

`PrototypeGame.signatureFont` 显式引用优先；未设置时从 `Resources/Fonts/LXGWWenKai-Regular` 加载。UIPrototype Prefab 已显式引用。中文、英文与混合昵称运行证据在 `Docs/Production/U03_CLEAN_Verification/checks.json`。
