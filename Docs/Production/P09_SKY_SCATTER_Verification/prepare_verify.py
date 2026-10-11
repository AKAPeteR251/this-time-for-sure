from pathlib import Path
import subprocess
root=Path(__file__).resolve().parents[3];here=Path(__file__).resolve().parent;project=root/'这次一定出'
text=(project/'Assets/_Game/Editor/P08NebulaDustSetup.cs').read_text(encoding='utf-8-sig')
text=text.replace('P08NebulaDust','P09SkyScatter').replace('P08_NEBULA_DUST_Verification','P09_SKY_SCATTER_Verification').replace('NebulaDust.request','SkyScatter.request').replace('ThisTime.NebulaDustVerify','ThisTime.SkyScatterVerify').replace('P08-NEBULA-DUST','P09-SKY-SCATTER')
text=text.replace('config.nebulaGrain=.3f;', 'config.nebulaGrain=.3f;config.meteorSeconds=3.2f;config.meteorFadeSeconds=2.2f;config.meteorOpacity=.28f;')
text=text.replace('Main接入P08粉尘星云Shader，保留已认可写实雪花/NEW及P07流星。','Main接入全屏随机星云与低亮度/慢速/慢淡出的随机流星，保持写实雪花与NEW。')
text=text.replace('main_p08_dust_nebula','main_p09_scattered_sky').replace('t<12.0f','t<24.0f')
text=text.replace('P08粉尘星云、已认可NEW/写实雪花','P09全屏星云与随机淡流星、已认可NEW/写实雪花')
needle='Require(sky.NebulaTime-nebulaTime>.30f,"星云3D噪声时间持续演化");'
extra='''
        var flights=sky.MeteorFlights;Require(flights.Length>=3,"观察到至少3条随机流星轨迹");
        Require(flights.Select(f=>f.start).Distinct().Count()==flights.Length,"多处流星起点");
        Require(flights.All(f=>f.peak<=.31f&&f.seconds>=2.8f&&f.fade>=2.2f),"低亮度慢飞行与2.2秒淡出");
        Require(Mathf.Abs(nebula.material.GetFloat("_CloudSeed")-sky.CloudSeed)<.001f,"Shader实际使用随机云团分布种子");
        File.WriteAllText(Path.Combine(Reports,"sky_observation.json"),JsonUtility.ToJson(new SkyObservation{seed=sky.CloudSeed,flights=flights},true));
'''
assert needle in text;text=text.replace(needle,needle+extra)
text=text.replace('    [Serializable]private sealed class Report','    [Serializable]private sealed class SkyObservation{public float seed;public RevealSky.MeteorFlight[] flights;}\n    [Serializable]private sealed class Report')
(project/'Assets/_Game/Editor/P09SkyScatterSetup.cs').write_text(text,encoding='utf-8-sig')
for kind in ['runtime','editor']:
    prev=(root/'Docs/Production/P08_NEBULA_DUST_Verification'/(kind+'.rsp')).read_text(encoding='utf-8-sig')
    lines=[s for s in prev.splitlines() if not s.startswith('"') and not s.startswith('/out:') and not 'runtime.dll"' in s]
    lines.append('/out:"'+str(here/(kind+'.dll'))+'"')
    if kind=='editor':lines.append('/reference:"'+str(here/'runtime.dll')+'"')
    lines+=['"'+str(f)+'"' for f in (project/'Assets/_Game').rglob('*.cs') if ('Editor' in f.parts)==(kind=='editor')]
    (here/(kind+'.rsp')).write_text('\n'.join(lines),encoding='utf-8-sig')
    result=subprocess.run(['E:/Unity/2022.3.53f1c1/Editor/Data/NetCoreRuntime/dotnet.exe','E:/Unity/2022.3.53f1c1/Editor/Data/DotNetSdkRoslyn/csc.dll','@'+str(here/(kind+'.rsp'))],cwd=root,capture_output=True,text=True,encoding='utf-8',errors='replace')
    (here/(kind+'_compile.log')).write_text(result.stdout+result.stderr,encoding='utf-8');print(kind,result.returncode);assert result.returncode==0
for file in ['finalize_evidence.py','encode_clips.py']:(here/file).write_bytes((root/'Docs/Production/P08_NEBULA_DUST_Verification'/file).read_bytes())
