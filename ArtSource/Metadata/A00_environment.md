# A00 本地美术环境核对

任务：A00；检查日期：2026-10-07。状态：已完成，待项目验收。512×768 测试已真实成功出图，用时 105.55 秒（包含本次加载开销）。证据：`A00_smoke_run.json`、`A00_smoke_history.json`、`ArtSource/StyleTests/A00_smoke.png`。

- 安装：`E:/ComfyUI-aki-v3/ComfyUI-aki-v3/ComfyUI`。
- 当前使用服务：`http://127.0.0.1:8188`，实际启动参数见 `A00_system_stats.json`。使用已启动的现有服务；先前自行启动的进程已停止。
- GPU：NVIDIA GeForce RTX 3060 Laptop GPU，6144 MiB；系统内存约 16 GB。
- ComfyUI 0.37.0，Python 3.13.11，PyTorch 2.9.1+cu130。服务采用动态显存，不能将早先独立服务的 lowvram 标志当成当前配置。
- 用户提供的 PNG：`C:/Users/82374/Downloads/ComfyUI_00348_.png`，832×1216；含 `prompt` 元数据，不含普通 `workflow` 元数据。
- 无损提取 API 原件：`ArtSource/Workflows/user_00348_prompt.json`。未覆盖用户文件。
- 模型：`models/unet/anima_turboV11.safetensors`；实际头部张量为 BF16，文件 4,182,230,656 字节。
- 编码器：`models/clip/anima_baseV10_txt.safetensors`，1,192,135,096 字节；沿用用户的 CLIPLoader 参数。
- VAE：`models/vae/qwenImage_qwenImageVAE.safetensors`，253,806,246 字节。
- 原工作流 LoRA：`anima/Flatline.safetensors`（0.6/0.6）、`anima/chen-bin_v4.0.safetensors`（0.6/0.6）、`anima/bakav3-000012.safetensors`（0.8/0.8），均已核对文件存在。
- 自定义节点：`AnimaLayerReplayPatcher`（comfyui-anima-enhancer）、`ClownsharKSampler_Beta`（RES4LYF），已通过当前服务 `/object_info` 核对。
- 用户采样：dpmpp_2m、sgm_uniform、8 steps、CFG 1、eta 0.5；replay/spectrum 关闭；沿用配置。
- 自定义节点的精确提交版本、峰值显存尚未取得，不猜测。

小图自检：角色头部完整、银灰发和蓝白制服可辨识；道具形状偏向小灯，背景亮度偏高。测试只能证明执行链可用，不代表正式美术通过。输出为完整图，没有人物 alpha 或可移动背景层。

## 复跑

启动同一套 ComfyUI 后，在项目根目录运行：

```powershell
python -u ArtSource/Workflows/run_style_tests.py
```

脚本先运行 512×768 环境测试，成功后依次提交三张 768×1152 的选型稿。每次运行会生成新 ComfyUI 输出；项目命名稿会被该脚本更新，正式生产前需另存版本。不得把选型稿当正式 1024×1536 母版。

每张保存 API 格式、从真实节点 schema 重建的普通 UI 格式、prompt_id、history、耗时、PNG 哈希。普通 UI 布局为重建，不是 PNG 原件含有的原始布局。

## 本轮边界

没有下载模型、安装节点、修改 Unity 脚本或场景。原图的既有角色提示词已经替换为原创“雾璃”；三种候选只改变画风段，使用同一 seed、主体、采样和 LoRA。种子不能保证姿势、服装细节完全一致。

当前不制作其余角色。A01-A 完成后须由用户选画风，再进入正式 SSR 与 UR 试制。
