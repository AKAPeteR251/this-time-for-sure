from pathlib import Path
import json,hashlib,difflib,re
root=Path(r'D:/vibe/这次一定出');out=root/'Docs/Production/P05_CRYSTAL_Verification'
original=json.loads((out/'protected_before.json').read_text(encoding='utf-8'))
after={k:hashlib.sha256((root/k).read_bytes()).hexdigest() for k in original}
(out/'protected_after.json').write_text(json.dumps(after,indent=2),encoding='utf-8')
assert original==after,'Protected file modified'
base=(root/'Docs/Production/P04_FEEL_Verification/PrototypeGame.U03.before.txt').read_text(encoding='utf-8-sig').splitlines(keepends=True)
diff=(root/'Docs/Production/P04_FEEL_Verification/PrototypeGame.U03.diff').read_text(encoding='utf-8-sig').splitlines(keepends=True)
result=[];cursor=0
for line in diff:
 if line.startswith('@@'):
  start=int(re.search(r'@@ -(\d+)',line)[1])-1
  result.extend(base[cursor:start]);cursor=start
 elif line.startswith('---') or line.startswith('+++'):continue
 elif line.startswith(' '):result.append(base[cursor]);cursor+=1
 elif line.startswith('-'):cursor+=1
 elif line.startswith('+'):result.append(line[1:])
result.extend(base[cursor:])
now=(root/'这次一定出/Assets/_Game/Scripts/UI/PrototypeGame.cs').read_text(encoding='utf-8-sig').splitlines(keepends=True)
(out/'PrototypeGame.P04.before.txt').write_text(''.join(result),encoding='utf-8')
(out/'PrototypeGame.P05.diff').write_text(''.join(difflib.unified_diff(result,now,fromfile='P04 PrototypeGame',tofile='P05 PrototypeGame')),encoding='utf-8')
(out/'compile_status.json').write_text(json.dumps({'runtimeExit':0,'editorExit':0,'warnings':'既有U02ArtSetup JSON DTO CS0649；P05无编译错误','protectedFiles':len(original)},ensure_ascii=False,indent=2),encoding='utf-8')
print('Protected hashes unchanged:',len(original))
