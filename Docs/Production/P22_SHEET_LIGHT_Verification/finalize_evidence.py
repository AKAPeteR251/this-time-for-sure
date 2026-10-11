from pathlib import Path
import hashlib,json,subprocess
root=Path(__file__).resolve().parents[3];here=Path(__file__).resolve().parent;project=root/'这次一定出'
before=json.loads((here/'protected_before.json').read_text(encoding='utf-8'))
protected=[dict(file=p,unchanged=hashlib.sha256((root/p).read_bytes()).hexdigest()==h) for p,h in before.items()]
assert all(p['unchanged'] for p in protected)
(here/'protected_check.json').write_text(json.dumps(protected,ensure_ascii=False,indent=2),encoding='utf-8')
source=root/'ArtSource/Scenes/F08/UndergroundAltar';dest=project/'Assets/_Game/Art/Backgrounds/F08/UndergroundAltar'
assets=[]
for p in [dest/'F08_UndergroundAltar_v001.fbx']+sorted((dest/'Textures').glob('*_v001.png')):
    original=source/('Models' if p.suffix=='.fbx' else 'Textures')/p.name
    assets.append(dict(file=p.name,sha256=hashlib.sha256(p.read_bytes()).hexdigest(),sourceUnchanged=p.read_bytes()==original.read_bytes(),metaExists=p.with_suffix(p.suffix+'.meta').exists()))
assert len(assets)==13 and all(a['sourceUnchanged'] and a['metaExists'] for a in assets)
(here/'asset_import_check.json').write_text(json.dumps(assets,indent=2),encoding='utf-8')
compiles=[]
for kind in ['runtime','editor']:
    rsp=here/(kind+'.rsp');lines=rsp.read_text(encoding='utf-8-sig').splitlines()
    for assembly in ['Unity.RenderPipelines.Universal.Runtime','Unity.RenderPipelines.Core.Runtime']:
        line=f'/reference:"{project.as_posix()}/Library/ScriptAssemblies/{assembly}.dll"'
        if line not in lines:lines.insert(3,line)
    rsp.write_text('\n'.join(lines),encoding='utf-8')
    result=subprocess.run(['E:/Unity/2022.3.53f1c1/Editor/Data/NetCoreRuntime/dotnet.exe','E:/Unity/2022.3.53f1c1/Editor/Data/DotNetSdkRoslyn/csc.dll','@'+str(rsp)],cwd=root,capture_output=True,text=True,encoding='utf-8',errors='replace')
    (here/(kind+'_compile.log')).write_text(result.stdout+result.stderr,encoding='utf-8');compiles.append(dict(target=kind,exitCode=result.returncode))
(here/'compile_status.json').write_text(json.dumps(compiles,indent=2),encoding='utf-8');assert all(c['exitCode']==0 for c in compiles)
from PIL import Image
import numpy as np
packing=[]
for p in ['stone','antique_gold']:
    metal=np.asarray(Image.open(source/f'Textures/{p}_metallic_2048_v001.png').convert('L'))
    rough=np.asarray(Image.open(source/f'Textures/{p}_roughness_2048_v001.png').convert('L'))
    packed=np.asarray(Image.open(dest/f'Textures/{p}_metallic_smoothness_2048_p21.png').convert('RGBA'))
    good=np.array_equal(packed[:,:,0],metal) and np.array_equal(packed[:,:,3],255-rough)
    packing.append(dict(material=p,redMetallicAlphaOneMinusRoughness=good));assert good
(here/'material_packing_check.json').write_text(json.dumps(packing,indent=2),encoding='utf-8')
result=json.loads((here/'final_result.json').read_text(encoding='utf-8-sig'));assert result['stage']=='passed'
checks=json.loads((here/'checks.json').read_text(encoding='utf-8-sig'))
print('PASS: protected files intact; 13 F08 source assets unchanged with meta; metallic/smoothness channels exact; runtime/editor compile; Main '+str(len(checks['checks']))+' checks; unique Draw commits '+str(checks['commits']))
