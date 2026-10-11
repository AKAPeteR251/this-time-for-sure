from pathlib import Path
import subprocess
root=Path(__file__).resolve().parents[3];here=Path(__file__).resolve().parent;project=root/'这次一定出'
text=(project/'Assets/_Game/Editor/P06RevealR3Setup.cs').read_text(encoding='utf-8-sig')
text=text.replace('P06RevealR3','P07SkyFeel').replace('P06_REVEAL_R3_Verification','P07_SKY_FEEL_Verification').replace('RevealR3.request','SkyFeel.request').replace('ThisTime.RevealR3Verify','ThisTime.SkyFeelVerify').replace('P06-REVEAL-R3','P07-SKY-FEEL')
text=text.replace('config.iceBackdropAlpha=.32f;', '''config.iceMageBackdrop=Sprite("Assets/_Game/Art/Effects/F04/RevealIdentityR2/ice_snowflake_v002.png");
        config.revealAtmosphereShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader");
        if(config.revealAtmosphereShader==null||ShaderUtil.ShaderHasError(config.revealAtmosphereShader))throw new InvalidOperationException("P07 Shader导入失败");
        config.proceduralRevealSky=true;config.nebulaEvolution=.09f;config.proceduralNebulaAlpha=.44f;
        config.newGradientSpeed=.36f;config.newBlinkStrength=.35f;config.newBlinkFrequency=1.6f;
        config.iceBackdropAlpha=.22f;''')
text=text.replace('Main保存15项R3分层素材，保留现有水晶/语音/人物引用。','Main接入P07程序星云/流星、3倍NEW流动+闪烁、第一版写实雪花，保留水晶/语音/人物。')
text=text.replace('main_r3_layered_reveal','main_p07_shader_sky')
text=text.replace('Vector2 cloud=sky.CloudOffset;float star=sky.FirstStarAlpha,phase=border.Phase,meteorPeak=0;', '''Require(sky.UsesProcedural&&ui.feelConfig.revealAtmosphereShader.isSupported,"实际使用受支持的程序星云/流星Shader");
        Require(ui.feelConfig.iceMageBackdrop.name=="ice_snowflake_v002","第一版写实雪花引用");
        Vector2 cloud=sky.CloudOffset;float star=sky.FirstStarAlpha,phase=border.Phase,meteorPeak=0,nebulaTime=sky.NebulaTime,pulseMin=1,pulseMax=0;
        Require(Mathf.Abs(border.speed-.36f)<.001f,"NEW流动3倍速度");''')
text=text.replace('meteorPeak=Mathf.Max(meteorPeak,sky.MeteorAlpha);yield return null;', 'meteorPeak=Mathf.Max(meteorPeak,sky.MeteorAlpha);pulseMin=Mathf.Min(pulseMin,border.Pulse);pulseMax=Mathf.Max(pulseMax,border.Pulse);yield return null;')
text=text.replace('yield return Capture("1280x720_ssr_dynamic_wait");', '''Require(sky.NebulaTime-nebulaTime>.30f,"星云3D噪声时间持续演化");Require(pulseMax-pulseMin>.8f,"NEW柔和闪烁实际持续变化");
        Require(!ShaderUtil.ShaderHasError(ui.feelConfig.revealAtmosphereShader),"实际绘制后Shader无编译错误");
        yield return Capture("1280x720_ssr_dynamic_wait");
        while(border.Pulse>.08f)yield return null;yield return Capture("1280x720_new_dim");
        while(border.Pulse<.92f)yield return null;yield return Capture("1280x720_new_bright");''')
text=text.replace('R3分层动态、四档字图','P07程序星云流星、NEW加速闪烁、写实雪花、四档字图')
text=text.replace('Require((sky.CloudOffset-cloud).sqrMagnitude>.00001f,"等待中双层星云持续漂移");','')
(project/'Assets/_Game/Editor/P07SkyFeelSetup.cs').write_text(text,encoding='utf-8-sig')
for kind in ['runtime','editor']:
    prev=(root/'Docs/Production/P06_REVEAL_R3_Verification'/(kind+'.rsp')).read_text(encoding='utf-8-sig')
    lines=[s for s in prev.splitlines() if not s.startswith('"') and not s.startswith('/out:') and not ('runtime.dll"' in s)]
    lines.append('/out:"'+str(here/(kind+'.dll'))+'"')
    if kind=='editor':lines.append('/reference:"'+str(here/'runtime.dll')+'"')
    lines+=['"'+str(f)+'"' for f in (project/'Assets/_Game').rglob('*.cs') if ('Editor' in f.parts)==(kind=='editor')]
    (here/(kind+'.rsp')).write_text('\n'.join(lines),encoding='utf-8-sig')
    result=subprocess.run(['E:/Unity/2022.3.53f1c1/Editor/Data/NetCoreRuntime/dotnet.exe','E:/Unity/2022.3.53f1c1/Editor/Data/DotNetSdkRoslyn/csc.dll','@'+str(here/(kind+'.rsp'))],cwd=root,capture_output=True,text=True,encoding='utf-8',errors='replace')
    (here/(kind+'_compile.log')).write_text(result.stdout+result.stderr,encoding='utf-8');print(kind,result.returncode,result.stdout[-600:]);assert result.returncode==0
for file in ['finalize_evidence.py','encode_clips.py']:(here/file).write_bytes((root/'Docs/Production/P06_REVEAL_R3_Verification'/file).read_bytes())
