# A05-BLENDER 通用空间段候选

日期：2026-10-08。状态：真实母版、720p样片与三帧已交付，待用户视觉评审；没有接入Unity/Main。

## 文件与实际证据

目录：`ArtSource/Scenes/SummonStage/`。

- 母版：`v002/A05_SummonStage_v002.blend`。
- 静音视频：`v002/A05_SummonSpace_720p_v002.mp4`，1280×720、H.264/yuv420p、24fps、132帧、5.5秒。
- 三帧：`v002/Review/01_establish_v002.png`、`02_giant_ring_v002.png`、`03_gate_burst_v002.png`，分别第1/55/105帧。
- 合图：`v002/Review/A05_three_frames_v002.jpg`；连续检查：`A05_space_storyboard_v002.jpg`。
- 可复跑脚本：`build_stage_v001.py`及`build_stage_v002.py`，两者均需保留；旧v001母版与三帧保留。
- 参数：`v002/parameters_v002.json`；解码/尺寸/帧数/哈希：`delivery_checks_v002.json`；132个原帧在`Frames/`。
- 实际日志：`v002_render.log`、`v002_encode.log`；初始探针：`Probe/eevee_probe_v001.png`及`capability_v001.json`。

实际工具：`E:/blender/blender-2.93.4-windows-x64/blender.exe`，2.93.4、build b7205031cec4。先实际EEVEE渲染320×180、8samples探针，随后渲染720p三帧与完整132帧。编码器是已有`E:/小丸/tools/ffmpeg.exe`，N-81989-g7845c13，libx264/preset medium/CRF18/faststart。没有安装或升级工具。

视频完整解码退出0、实际解码132帧，尺寸1280×720、24fps；所有PNG尺寸及母版/MP4/参数/三帧SHA256已检查。日志含本机插件卸载提示，但渲染退出0且帧齐全，不把卸载提示当渲染失败。检查通过不代表用户视觉或Unity运行验收。

## 场景、参数与分镜

原创紧凑圆台、三组黄铜星环、八枚低面数晶体、中央晶核、110个星点、三层冲击环与通道光线。没有角色、庞大场景、外部贴图、中文/UI/字幕/Logo/用户名或签名。全部几何、材质和运动由脚本生成。

侧前方建立→移向中轴→晶体列阵与反向星环→短收束→中央光与冲击爆开→穿越→金白光幕衔接Unity。v001目视发现晶体过曝，v002降低静态发光、加入扩张冲击环并调整镜头焦距，旧版保留。

EEVEE、24samples、AO、Bloom intensity0.055/radius6/threshold1、Filmic/Medium High Contrast、exposure-0.3、星点seed71008052。镜头关键帧28→25→31→34→30mm、near0.07/far120，实际值以.blend及JSON为准。

样片内建立0秒、列阵约2.25秒、爆开约4.33秒、光幕约5.46秒。参考11：Unity约1.15秒签名/盖印后开始空间段，整体约6.65秒；纸升空可由Unity叠加片头1.35秒。若要完全升空后才开始视频，应按旁白裁切/加速空间段，不把完整5.5秒再叠到2.5秒前段。手写等待时间不计入仪式。

无音轨，旁白和音效由P04另行同步；不声称已完成“星门，为您而开！”的音画验收。

## 评审后接入与调参

本轮只写ArtSource，不修改代码、场景、Prefab或Main，不自动将候选绑定到Unity。用户认可后，P04再将空间段通过VideoPlayer/RenderTexture接入，用户名、签名和2D角色留Unity；影片为不透明通用开场，不是十连成片。跳过立即停止视频并到既定同批总览，逐张确认仍由Unity处理，视频不得驱动重新抽取。

16:10建议中心裁切/遮罩，不能拉伸。末尾金白光幕是设计转场，不是黑帧；P04将其接到2D揭晓。强光时长、镜头冲击、旁白同步、三窗口和即时跳过均待实际集成验证。暂不制作1080p正式片，也不另建实时3D链。

复跑：Blender `--background --factory-startup --python build_stage_v002.py -- preview`生成母版及三帧；从母版用`--background <blend> -o <Frames/frame_> -F PNG -s 1 -e 132 -a`渲染，按24fps编码。正式变更先升版本，避免覆盖已评审稿；用户名不作为渲染参数。

涉及Unity概念：VideoPlayer准备/结束事件、RenderTexture、Canvas/RawImage层级、遮罩适配、跳过清理。调参顺序：先锁签约提交与跳过，再对齐旁白爆点；Blender调RingRig、CrystalArray、CentralCrystal发光、CameraAim曲线、FinalCurtain；Unity调开始offset、纸升空、结束光幕，不改抽卡数据。
