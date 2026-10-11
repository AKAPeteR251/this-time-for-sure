from pathlib import Path
import json, hashlib, difflib, subprocess
root=Path(__file__).resolve().parents[3]
here=Path(__file__).resolve().parent
before=json.loads((root/'Docs/Production/P05_CRYSTAL_Verification/protected_before.json').read_text(encoding='utf-8-sig'))
checks=[]
for relative, expected in before.items():
    actual=hashlib.sha256((root/relative).read_bytes()).hexdigest()
    checks.append(dict(file=relative,unchanged=actual==expected,sha256=actual))
(here/'protected_check.json').write_text(json.dumps(checks,ensure_ascii=False,indent=2),encoding='utf-8')
assert all(x['unchanged'] for x in checks), checks
for name in ['FastRevealView','PrototypeGame']:
    old=(here/(name+'.before.txt')).read_text(encoding='utf-8-sig').splitlines(True)
    source=next((root/'这次一定出/Assets/_Game/Scripts').rglob(name+'.cs'))
    new=source.read_text(encoding='utf-8-sig').splitlines(True)
    (here/(name+'.diff')).write_text(''.join(difflib.unified_diff(old,new,fromfile=name+'.before',tofile=name+'.cs')),encoding='utf-8')
compiler='E:/Unity/2022.3.53f1c1/Editor/Data/DotNetSdkRoslyn/csc.dll'
runtime='E:/Unity/2022.3.53f1c1/Editor/Data/NetCoreRuntime/dotnet.exe'
results=[]
for name in ['runtime','editor']:
    result=subprocess.run([runtime,compiler,'@'+str(here/(name+'.rsp'))],cwd=root,text=True,encoding='utf-8',errors='replace',capture_output=True)
    (here/(name+'_compile.log')).write_text(result.stdout+result.stderr,encoding='utf-8')
    results.append(dict(target=name,exitCode=result.returncode))
(here/'compile_status.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
assert all(x['exitCode']==0 for x in results), results
print('Protected files unchanged; runtime/editor compilation passed.')
