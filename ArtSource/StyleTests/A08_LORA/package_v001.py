from pathlib import Path
from PIL import Image
import json,hashlib,zipfile
R=Path(__file__).resolve().parent;ROOT=R.parents[2];M=ROOT/'ArtSource/Metadata';checks=[]
def check(name,ok):
    if not ok:raise AssertionError(name)
    checks.append({'item':name,'result':'PASS'})
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
input_meta=json.loads((R/'input/input_sources.json').read_text(encoding='utf-8'))
check('Approved original unchanged',sha(Path(input_meta['original']))==input_meta['original_sha256'])
check('Shared gray input unchanged',sha(R/'input/ice_mage_gray_v001.png')==input_meta['test_input_sha256'])
graphs={}
for gid in ['B0','D1','R1','DR1']:
    graph=json.loads((ROOT/'ArtSource/Workflows'/f'A08_LORA_{gid}_api.json').read_text(encoding='utf-8'));graphs[gid]=graph
    run=json.loads((M/f'A08_LORA_{gid}_run.json').read_text(encoding='utf-8'));history=json.loads((M/f'A08_LORA_{gid}_history.json').read_text(encoding='utf-8'))
    check(gid+' successful execution history',history['status']['status_str']=='success' and history['status']['completed'])
    check(gid+' recorded prompt matches saved workflow',history['prompt'][2]==graph)
    check(gid+' common input hash',run['input_sha256']==input_meta['test_input_sha256'])
    check(gid+' output matches recorded hash',run['sha256']==sha(R/'outputs'/f'{gid}.png'))
    im=Image.open(R/'outputs'/f'{gid}.png');check(gid+' native generated 1024x1536 RGB',im.size==(1024,1536) and im.mode=='RGB')
    check(gid+' PNG embeds workflow and prompt','workflow' in im.info and 'prompt' in im.info)
    check(gid+' no unmatched LoRA or matrix failure log',not any(term in (R/'runtime'/f'{gid}_execution.log').read_text(encoding='utf-8',errors='replace').lower() for term in ['lora key not loaded','size mismatch','not loaded:']))
    ui=json.loads((ROOT/'ArtSource/Workflows'/f'A08_LORA_{gid}_ui.json').read_text(encoding='utf-8'));ids={n['id'] for n in ui['nodes']}
    check(gid+' UI graph node IDs match API',ids==set(map(int,graph)))
    check(gid+' UI links reference existing nodes',all(link[1] in ids and link[3] in ids for link in ui['links']))
for gid in ['D1','R1','DR1']:
    for nid,item in graphs[gid].items():
        if nid in ['20','21','12']:continue
        check(gid+' same public conditions node '+nid,item==graphs['B0'][nid])
mapping=json.loads((R/'runtime/lora_shape_mapping.json').read_text(encoding='utf-8'))
for item in mapping['reports']:check(item['file']+' 280 matched adapters',item['adapters']==item['matched_compatible']==280)
check('Four distinct generated PNGs',len({sha(R/'outputs'/f'{g}.png') for g in graphs})==4)
(R/'checks.json').write_text(json.dumps({'task':'A08-LORA','checks':checks,'scope':'Generated RGB comparison assets, not accepted character sprites','ui_limit':'LiteGraph JSON checked for node and link integrity; not imported in browser UI. Actual generation used corresponding API graph.'},ensure_ascii=False,indent=2),encoding='utf-8')
(R/'SHA256SUMS.txt').write_text('\n'.join(sha(p)+'  '+p.relative_to(R).as_posix() for p in sorted(R.rglob('*')) if p.is_file() and p.name!='SHA256SUMS.txt' and p.suffix!='.zip')+'\n',encoding='utf-8')
with zipfile.ZipFile(R/'A08_LORA_Comparison_v001.zip','w',zipfile.ZIP_DEFLATED) as z:
    for p in sorted(R.rglob('*')):
        if p.is_file() and p.suffix!='.zip':z.write(p,p.relative_to(R).as_posix())
with zipfile.ZipFile(R/'A08_LORA_Comparison_v001.zip') as z:check('Package CRC integrity',z.testzip() is None)
print('Checks passed:',len(checks),'Package bytes:',(R/'A08_LORA_Comparison_v001.zip').stat().st_size)
