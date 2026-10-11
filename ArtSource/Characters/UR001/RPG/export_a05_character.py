from pathlib import Path
from PIL import Image
import json,hashlib,shutil
BASE=Path(__file__).resolve().parent;ROOT=BASE.parents[3]
source=ROOT/'ArtSource/StyleTests/RPG_Professions/04_fire_warrior_v001.png'
mother=BASE/'UR001_fire_source_v001.png'
if not mother.exists():shutil.copy2(source,mother)
im=Image.open(source).convert('RGB');full=im.resize((1024,1536),Image.Resampling.LANCZOS)
dest=ROOT/'这次一定出/Assets/_Game/Art/Characters/UR001/RPG';dest.mkdir(parents=True,exist_ok=True)
fullpath=dest/'UR001_fire_card_base_v001.png'
if fullpath.exists():raise RuntimeError('Refusing overwrite '+str(fullpath))
full.save(fullpath)
candidate=Image.open(BASE/'SAM_v002/cutout_v002.png').resize((1024,1536),Image.Resampling.LANCZOS)
cp=BASE/'UR001_fire_character_candidate_v002.png';candidate.save(cp)
records=[]
for p,state in [(mother,'approved original preserved'),(fullpath,'complete card, pending Unity import'),(cp,'real alpha candidate; visual defects remain; do not bind as final')]:
    image=Image.open(p);alpha=image.getchannel('A') if 'A' in image.getbands() else None
    records.append({'path':str(p.relative_to(ROOT)).replace('\\','/'),'size':image.size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'alpha_extrema':alpha.getextrema() if alpha else None,'zero_alpha_pixels':alpha.histogram()[0] if alpha else 0,'status':state})
meta={'task':'A05-REVEAL','characterId':'UR001','source':str(source.relative_to(ROOT)).replace('\\','/'),'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'identity_preserved':'all visible RGB pixels from original; no diffusion repaint or new pose','resample':'768x1152 to 1024x1536, same entire canvas origin; no new detail claimed','source_clipping':'right hair/cape and sword tip and lower boot clipped in original; not reconstructed','mask_limitations':'SAM v002 improves knee armor but fine hair, dark cape/garment edges retain defects and remnants; not accepted as final character layer','default_for_P04':str(fullpath.relative_to(ROOT)).replace('\\','/'),'U03_placement':'x0.27-0.73 y0.15-0.93; preserve aspect; no face particles','files':records}
(BASE/'A05_character_metadata.json').write_text(json.dumps(meta,ensure_ascii=False,indent=2),encoding='utf-8');print('Full card delivered; real-alpha candidate isolated from final Unity binding')
