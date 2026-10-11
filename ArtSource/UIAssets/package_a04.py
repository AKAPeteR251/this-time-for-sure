from pathlib import Path
import json,csv,zipfile
BASE=Path(__file__).resolve().parent; ROOT=BASE.parents[1]
records=json.loads((BASE/'asset_manifest.json').read_text(encoding='utf-8'))
final=[r for r in records if not r['assetId'].endswith('_original')]
manifest=ROOT/'Docs/Production/asset_manifest.csv'
with manifest.open(encoding='utf-8-sig',newline='') as f:
    reader=csv.DictReader(f); fields=reader.fieldnames; rows=list(reader)
updates={r['assetId']:r for r in final if r['assetId'] in ['bg_summon','fx_glint']}
for row in rows:
    if row['assetId'] in updates:
        if row['owner']!='Art':raise RuntimeError('Wrong ownership')
        r=updates[row['assetId']]
        row.update(relativeTarget=r['path'],width=r['width'],height=r['height'],alpha='true' if r['fully_transparent_pixels'] else 'false',status='pending_unity_review',notes='A04 实际素材已生成；真实PNG检查与本地叠放；待Unity导入运行验收')
known={row['assetId'] for row in rows}
for r in final:
    if r['assetId'] in updates:continue
    aid='a04_'+r['assetId']
    if aid in known:continue
    rows.append(dict(assetId=aid,owner='Art',phase='A04',characterId='',kind='ui_art_source',relativeTarget=r['path'],width=r['width'],height=r['height'],alpha='true' if r['fully_transparent_pixels'] else 'false',status='pending_unity_review',notes='UI搭建素材；角色职业稿为原型用途，无冻结角色ID；详见ArtSource/UIAssets/README.md'))
with manifest.open('w',encoding='utf-8',newline='') as f:
    writer=csv.DictWriter(f,fieldnames=fields);writer.writeheader();writer.writerows(rows)
dest=BASE/'A04_UI_assets_v001.zip'
with zipfile.ZipFile(dest,'w',zipfile.ZIP_DEFLATED) as z:
    for r in final:z.write(ROOT/r['path'],'UnityArt/'+r['path'].split('Assets/_Game/Art/',1)[1])
    for name in ['README.md','asset_manifest.json','asset_manifest.csv','generation_sources.json','export_a04.py','preview_a04.py']:
        z.write(BASE/name,name)
    for p in (BASE/'Prompts').glob('*.txt'):z.write(p,'Prompts/'+p.name)
print(f'Final PNG count: {len(final)}; zip bytes: {dest.stat().st_size}')
