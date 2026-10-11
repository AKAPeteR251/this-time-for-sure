from pathlib import Path
import json,hashlib,zipfile
ROOT=Path(__file__).resolve().parents[4]; BATCH=Path(__file__).resolve().parent
entries=json.loads((ROOT/'ArtSource/Metadata/A09_CHARACTERS_inputs_full_v001.json').read_text(encoding='utf-8'))
paths=[]
for e in entries:
    paths.extend(p for p in (ROOT/f'ArtSource/Characters/{e["id"]}/Internal_A09_v001').rglob('*') if p.is_file())
paths.extend(p for p in BATCH.iterdir() if p.is_file() and p.suffix!='.zip' and p.name not in ['SHA256SUMS_v001.txt','package_check_v001.json'])
paths.extend(ROOT/'ArtSource/Metadata'/name for name in ['A09_CHARACTERS_任务单.md','A09_CHARACTERS_inputs_full_v001.json','六元素角色配色_v001.md','R等级风格锚定_03_v001.md'])
paths=sorted(set(paths))
manifest=BATCH/'SHA256SUMS_v001.txt'
manifest.write_text('\n'.join(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+p.relative_to(ROOT).as_posix() for p in paths)+'\n',encoding='utf-8')
paths.append(manifest)
archive=BATCH/'A09_CharacterPortraits_v001.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    for p in paths:z.write(p,p.relative_to(ROOT).as_posix())
with zipfile.ZipFile(archive) as z:
    bad=z.testzip(); count=len(z.infolist())
    assert bad is None,bad
result={'task':'A09-CHARACTERS','status':'待视觉验收','characters':len(entries),'files':count,'zip_crc':'passed','bytes':archive.stat().st_size,'sha256':hashlib.sha256(archive.read_bytes()).hexdigest()}
(BATCH/'package_check_v001.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8');print(json.dumps(result,ensure_ascii=False))
