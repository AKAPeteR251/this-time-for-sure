from pathlib import Path
import hashlib,json,subprocess
root=Path(__file__).resolve().parents[3];here=Path(__file__).resolve().parent
project=root/'这次一定出'
before=json.loads((root/'Docs/Production/P05_CRYSTAL_Verification/protected_before.json').read_text(encoding='utf-8-sig'))
protected=[dict(file=p,unchanged=hashlib.sha256((root/p).read_bytes()).hexdigest()==h) for p,h in before.items()]
(here/'protected_check.json').write_text(json.dumps(protected,ensure_ascii=False,indent=2),encoding='utf-8');assert all(c['unchanged'] for c in protected)
art=root/'ArtSource/Effects/F05/RevealR3';dest=project/'Assets/_Game/Art/Effects/F05/RevealR3'
assets=[]
for p in sorted(dest.glob('*.png')):
    digest=hashlib.sha256(p.read_bytes()).hexdigest();source=hashlib.sha256((art/p.name).read_bytes()).hexdigest()
    assets.append(dict(file=p.name,sha256=digest,sourceUnchanged=digest==source,metaExists=p.with_suffix('.png.meta').exists()))
(here/'asset_import_check.json').write_text(json.dumps(assets,indent=2),encoding='utf-8');assert len(assets)==15 and all(a['sourceUnchanged'] and a['metaExists'] for a in assets)
compiles=[]
for kind in ['runtime','editor']:
    result=subprocess.run(['E:/Unity/2022.3.53f1c1/Editor/Data/NetCoreRuntime/dotnet.exe','E:/Unity/2022.3.53f1c1/Editor/Data/DotNetSdkRoslyn/csc.dll','@'+str(here/(kind+'.rsp'))],cwd=root,capture_output=True,text=True,encoding='utf-8',errors='replace')
    (here/(kind+'_compile.log')).write_text(result.stdout+result.stderr,encoding='utf-8');compiles.append(dict(target=kind,exitCode=result.returncode))
(here/'compile_status.json').write_text(json.dumps(compiles,indent=2),encoding='utf-8');assert all(c['exitCode']==0 for c in compiles)
print('15 assets intact with Unity meta; protected files unchanged; compile PASS.')
source=root/'ArtSource/Effects/F06/v001/RevealBurst'
target=project/'Assets/_Game/Art/Effects/F06/RevealBurst'
new_assets=[]
for p in sorted(target.glob('*.png')):
    from PIL import Image
    img=Image.open(p)
    new_assets.append(dict(file=p.name,size=img.size,mode=img.mode,sha256=hashlib.sha256(p.read_bytes()).hexdigest(),
        matchesSource=p.read_bytes()==(source/p.name).read_bytes(),metaExists=p.with_suffix('.png.meta').exists()))
assert len(new_assets)==4 and all(a['matchesSource'] and a['metaExists'] and a['mode']=='RGBA' for a in new_assets)
(here/'f06_asset_import_check.json').write_text(json.dumps(new_assets,indent=2),encoding='utf-8')
approved=(here/'approved_nebula_sha256.txt').read_text().strip()
actual=hashlib.sha256((project/'Assets/_Game/Art/Effects/F05/Shaders/RevealAtmosphere.shader').read_bytes()).hexdigest()
assert approved==actual
(here/'approved_nebula_check.json').write_text(json.dumps(dict(shaderUnchanged=True,sha256=actual),indent=2))
print('Four F06 assets imported unchanged; approved nebula shader unchanged.')
