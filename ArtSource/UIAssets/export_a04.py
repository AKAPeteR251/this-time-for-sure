"""A04 non-destructive PNG export: copy originals, split grid atlases, measure alpha.
Does not repaint, remove backgrounds, or alter generated image content.
"""
from pathlib import Path
from PIL import Image
import json, hashlib, shutil, csv

ROOT = Path(__file__).resolve().parents[2]
BASE = Path(__file__).resolve().parent
ART = ROOT / '这次一定出/Assets/_Game/Art'
spec = json.loads((BASE / 'generation_sources.json').read_text(encoding='utf-8'))
records=[]

def record(p, asset_id, source, extra=None):
    im=Image.open(p)
    alpha=im.getchannel('A') if 'A' in im.getbands() else None
    hist=alpha.histogram() if alpha else None
    r={'assetId':asset_id,'path':str(p.relative_to(ROOT)).replace('\\','/'),
       'width':im.width,'height':im.height,'mode':im.mode,
       'alpha_extrema':alpha.getextrema() if alpha else None,
       'fully_transparent_pixels':hist[0] if hist else 0,
       'partly_transparent_pixels':sum(hist[1:255]) if hist else 0,
       'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'source':source,
       'status':'pending_unity_import_review'}
    if extra:r.update(extra)
    records.append(r)
    return im

def save(im, p, name, source, extra=None):
    p.parent.mkdir(parents=True,exist_ok=True)
    if p.exists():
        old=Image.open(p)
        if old.mode!=im.mode or old.size!=im.size or old.tobytes()!=im.tobytes():
            raise RuntimeError('Refusing to overwrite '+str(p))
    else:im.save(p)
    record(p,name,source,extra)

names={
 'rarity_frames_atlas':(2,2,['frame_R','frame_SR','frame_SSR','frame_UR']),
 'panels_atlas':(2,3,['button_normal','button_selected','button_pressed','button_disabled','panel_information','panel_tooltip']),
 'icons_effects_atlas':(4,3,['deco_star','icon_summon','icon_collection','icon_back','icon_currency','element_ice','element_light','element_thunder','element_wind','element_fire','fx_sparkle_blue','fx_sparkle_gold'])}

for key,src in spec.items():
    raw=BASE/'Originals'/f'{key}_v001.png'
    raw.parent.mkdir(parents=True,exist_ok=True)
    if not raw.exists():shutil.copy2(src,raw)
    im=record(raw,key+'_original',src)
    if key!='background':
        if 'A' not in im.getbands() or im.getchannel('A').getextrema()[0]!=0:
            raise RuntimeError('Real transparency missing: '+key)
    if key=='background':
        target=ART/'Backgrounds/UI_FrozenCastle_v001.png'
        save(im.convert('RGB').resize((1920,1080),Image.Resampling.LANCZOS),target,'bg_summon',src,{'derivation':'aspect-near landscape resample; original retained'})
    elif key=='ice_mage_cutout':
        # Fit the entire untrimmed canvas in the agreed portrait canvas without stretching.
        rgba=im.convert('RGBA')
        ratio=min(1024/rgba.width,1536/rgba.height)
        fit=rgba.resize((round(rgba.width*ratio),round(rgba.height*ratio)),Image.Resampling.LANCZOS)
        canvas=Image.new('RGBA',(1024,1536))
        canvas.alpha_composite(fit,((1024-fit.width)//2,(1536-fit.height)//2))
        save(canvas,ART/'Characters/UIIceMage/UIIceMage_character_v001.png','ui_ice_mage_character',src,{'characterId':None,'derivation':'whole canvas fitted, no crop; UI illustration not frozen roster ID'})
    else:
        cols,rows,labels=names[key]
        for i,label in enumerate(labels):
            c,r=i%cols,i//cols
            rect=(round(c*im.width/cols),round(r*im.height/rows),round((c+1)*im.width/cols),round((r+1)*im.height/rows))
            if key=='rarity_frames_atlas':
                # The decorative crowns of the lower frames rise above the mathematical midline.
                split=round(im.height*.46875)
                rect=(round(c*im.width/2),0 if r==0 else split,round((c+1)*im.width/2),split if r==0 else im.height)
            if key=='panels_atlas' and r==2:
                # Generated information plate is wider than an equal grid cell.
                split=round(im.width*.60)
                rect=(0,round(2*im.height/3),split,im.height) if c==0 else (split,round(2*im.height/3),im.width,im.height)
            cell=im.convert('RGBA').crop(rect)
            bbox=cell.getchannel('A').point(lambda v:255 if v>4 else 0).getbbox()
            if not bbox:raise RuntimeError('Empty atlas cell '+label)
            # Retain all alpha content in each grid cell; trimming just removes empty padding.
            exact=cell.getchannel('A').getbbox()
            cut=cell.crop(exact)
            padded=Image.new('RGBA',(cut.width+16,cut.height+16))
            padded.alpha_composite(cut,(8,8))
            if label.startswith('button_'):
                common=Image.new('RGBA',(832,336))
                common.alpha_composite(padded,((832-padded.width)//2,(336-padded.height)//2))
                padded=common
            if label.startswith('fx_'):
                fit=padded.copy()
                fit.thumbnail((496,496),Image.Resampling.LANCZOS)
                common=Image.new('RGBA',(512,512))
                common.alpha_composite(fit,((512-fit.width)//2,(512-fit.height)//2))
                padded=common
            extra={'grid_cell_top_left_pixels':rect,'trim_alpha_bbox':exact,
                   'cell_near_boundary':bbox[0]<2 or bbox[1]<2 or bbox[2]>cell.width-2 or bbox[3]>cell.height-2}
            if key=='rarity_frames_atlas':
                extra['center_alpha']=cell.getchannel('A').getpixel((cell.width//2,cell.height//2))
            version='v002' if key=='rarity_frames_atlas' or label.startswith(('button_','fx_')) else 'v001'
            asset_id='fx_glint' if label=='fx_sparkle_blue' else label
            save(padded,ART/'Effects/UIAssets'/f'{label}_{version}.png',asset_id,src,extra)

mock_names=['01_ice_mage_v001','02_light_priest_v001','03_wind_elf_archer_v002',
            '04_fire_warrior_v001','05_thunder_miko_v001','06_star_magical_girl_v001']
for name in mock_names:
    for suffix in ['', '_thumb']:
        source=ROOT/'ArtSource/StyleTests/RPG_Professions'/f'{name}{suffix}.png'
        dest=ART/'Characters/UIMockRoles'/f'{name}{suffix}.png'
        dest.parent.mkdir(parents=True,exist_ok=True)
        if dest.exists() and dest.read_bytes()!=source.read_bytes():raise RuntimeError('Existing asset differs '+str(dest))
        if not dest.exists():shutil.copy2(source,dest)
        record(dest,'ui_mock_'+name+suffix,str(source),{'characterId':None,'purpose':'approved style draft for UI prototype; not frozen roster mapping'})

(BASE/'asset_manifest.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
with (BASE/'asset_manifest.csv').open('w',newline='',encoding='utf-8-sig') as f:
    writer=csv.DictWriter(f,fieldnames=['assetId','path','width','height','mode','sha256','status'])
    writer.writeheader()
    writer.writerows({k:r[k] for k in writer.fieldnames} for r in records)
print(json.dumps(records,ensure_ascii=False,indent=2))
