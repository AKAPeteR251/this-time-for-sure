from pathlib import Path
import subprocess
root=Path(__file__).resolve().parents[3];here=Path(__file__).resolve().parent;project=root/'这次一定出'
text=(project/'Assets/_Game/Editor/P07SkyFeelSetup.cs').read_text(encoding='utf-8-sig')
text=text.replace('P07SkyFeel','P08NebulaDust').replace('P07_SKY_FEEL_Verification','P08_NEBULA_DUST_Verification').replace('SkyFeel.request','NebulaDust.request').replace('ThisTime.SkyFeelVerify','ThisTime.NebulaDustVerify').replace('P07-SKY-FEEL','P08-NEBULA-DUST')
text=text.replace('config.proceduralNebulaAlpha=.44f;','config.proceduralNebulaAlpha=.40f;config.nebulaWarp=.035f;config.nebulaGrain=.3f;')
text=text.replace('Main接入P07程序星云/流星、3倍NEW流动+闪烁、第一版写实雪花，保留水晶/语音/人物。','Main接入P08粉尘星云Shader，保留已认可写实雪花/NEW及P07流星。')
text=text.replace('main_p07_shader_sky','main_p08_dust_nebula')
text=text.replace('实际使用受支持的程序星云/流星Shader','实际使用受支持的粉尘星云Shader')
text=text.replace('P07程序星云流星、NEW加速闪烁、写实雪花','P08粉尘星云、已认可NEW/写实雪花')
text=text.replace('t<4.0f','t<12.0f')
needle='Require(sky.UsesProcedural&&ui.feelConfig.revealAtmosphereShader.isSupported,"实际使用受支持的粉尘星云Shader");'
text=text.replace(needle,needle+'''
        var nebula=Page(ui,"Reveal").GetComponentsInChildren<RawImage>().Single(x=>x.name=="ProceduralNebula");
        Require(nebula.material.GetTexture("_NebulaFarTex")==ui.feelConfig.nebulaFar&&nebula.material.GetTexture("_NebulaNearTex")==ui.feelConfig.nebulaNear,"Shader实际采样现有粉尘素材");''')
(project/'Assets/_Game/Editor/P08NebulaDustSetup.cs').write_text(text,encoding='utf-8-sig')
for kind in ['runtime','editor']:
    prev=(root/'Docs/Production/P07_SKY_FEEL_Verification'/(kind+'.rsp')).read_text(encoding='utf-8-sig')
    lines=[s for s in prev.splitlines() if not s.startswith('"') and not s.startswith('/out:') and not 'runtime.dll"' in s]
    lines.append('/out:"'+str(here/(kind+'.dll'))+'"')
    if kind=='editor':lines.append('/reference:"'+str(here/'runtime.dll')+'"')
    lines+=['"'+str(f)+'"' for f in (project/'Assets/_Game').rglob('*.cs') if ('Editor' in f.parts)==(kind=='editor')]
    (here/(kind+'.rsp')).write_text('\n'.join(lines),encoding='utf-8-sig')
    result=subprocess.run(['E:/Unity/2022.3.53f1c1/Editor/Data/NetCoreRuntime/dotnet.exe','E:/Unity/2022.3.53f1c1/Editor/Data/DotNetSdkRoslyn/csc.dll','@'+str(here/(kind+'.rsp'))],cwd=root,capture_output=True,text=True,encoding='utf-8',errors='replace')
    (here/(kind+'_compile.log')).write_text(result.stdout+result.stderr,encoding='utf-8');print(kind,result.returncode);assert result.returncode==0
for file in ['finalize_evidence.py','encode_clips.py']:(here/file).write_bytes((root/'Docs/Production/P07_SKY_FEEL_Verification'/file).read_bytes())
