from pathlib import Path
import json,hashlib,csv,zipfile
from PIL import Image
BASE=Path(__file__).resolve().parent;ROOT=BASE.parents[2]
records=json.loads((BASE/'A05_effects_manifest.json').read_text(encoding='utf-8'))
seal=json.loads((BASE/'A05_anima_seal_metadata.json').read_text(encoding='utf-8'))
seal['assetId']='star_seal_anima';seal['status']='PNG alpha and local visual check passed; Unity pending'
records.append(seal)
character=ROOT/'ArtSource/Characters/UR001/RPG'
cm=json.loads((character/'A05_character_metadata.json').read_text(encoding='utf-8'))
full=cm['files'][1];full['assetId']='UR001_fire_card';records.append(full)
candidate=cm['files'][2];candidate['assetId']='UR001_fire_character_candidate'
for r in records+[candidate]:
    p=ROOT/r['path'];assert p.exists();assert hashlib.sha256(p.read_bytes()).hexdigest()==r['sha256']
    im=Image.open(p);assert tuple(r['size'])==im.size
    if im.mode=='RGBA':assert im.getchannel('A').histogram()[0]>0
# prove original RGB survived the actual mask extraction before resampling
src=Image.open(ROOT/cm['source']).convert('RGB')
masked=Image.open(character/'SAM_v002/cutout_v002.png')
assert masked.convert('RGB').tobytes()==src.tobytes()
final={'task':'A05-REVEAL','ready_for_integration':records,'not_ready_candidate':[candidate],
       'unity_runtime_validation':'not performed','original_pixels_preserved_pre_resampling':True,
       'reuse_paths':['这次一定出/Assets/_Game/Art/Characters/UIIceMage/UIIceMage_character_v001.png','这次一定出/Assets/_Game/Art/Effects/UIAssets/fx_sparkle_blue_v002.png','这次一定出/Assets/_Game/Art/Effects/UIAssets/fx_sparkle_gold_v002.png']}
(BASE/'A05_delivery_manifest.json').write_text(json.dumps(final,ensure_ascii=False,indent=2),encoding='utf-8')
with (BASE/'A05_delivery_manifest.csv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.writer(f);w.writerow(['assetId','path','width','height','sha256','status'])
    for r in records+[candidate]:w.writerow([r['assetId'],r['path'],*r['size'],r['sha256'],r['status']])
zip_path=BASE/'A05_REVEAL_v001.zip'
with zipfile.ZipFile(zip_path,'w',zipfile.ZIP_DEFLATED) as z:
    for r in records:z.write(ROOT/r['path'],'Ready/'+r['path'].split('Assets/_Game/Art/',1)[1])
    z.write(ROOT/candidate['path'],'Candidates/'+Path(candidate['path']).name)
    z.write(character/'UR001_fire_source_v001.png','Originals/UR001_fire_source_v001.png')
    z.write(BASE/'LocalGenerationSmoke/star_fullimage_v001.png','Originals/star_fullimage_v001.png')
    z.write(ROOT/'ArtSource/Metadata/A05_REVEAL_交接.md','A05_REVEAL_交接.md')
    for folder in ['VectorSource','QA','LocalGenerationSmoke']:
        for p in (BASE/folder).glob('*'):
            if p.is_file() and p.suffix in ['.svg','.jpg','.json']:z.write(p,folder+'/'+p.name)
    for p in BASE.glob('*'):
        if p.suffix in ['.json','.csv','.py']:z.write(p,p.name)
    for p in character.glob('*.py'):z.write(p,'CharacterSource/'+p.name)
    z.write(character/'A05_character_metadata.json','CharacterSource/A05_character_metadata.json')
    for folder in ['SAM_v001','SAM_v002']:
        for p in (character/folder).glob('*'):
            if p.suffix in ['.png','.jpg','.json']:z.write(p,'CharacterSource/'+folder+'/'+p.name)
with zipfile.ZipFile(zip_path) as z:assert z.testzip() is None
print('10 ready PNGs (9 effects + complete card), 1 alpha candidate; source pixel preservation, SHA256 and ZIP checked')
