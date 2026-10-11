from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import numpy as np,json,hashlib,zipfile,shutil
R=Path(__file__).resolve().parent;ROOT=R.parents[2];Q=R/'QA';Q.mkdir(exist_ok=True)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',22)
groups=['INPUT','B0','D1','R1','DR1'];labels={'INPUT':'SOURCE / gray','B0':'B0 / 0 + 0','D1':'D1 / .30 + 0','R1':'R1 / 0 + .30','DR1':'DR1 / .30 + .30'}
paths=[R/'input/ice_mage_gray_v001.png']+[R/'outputs'/f'{g}.png' for g in groups[1:]]
images=[Image.open(p).convert('RGB') for p in paths]
assert all(im.size==(1024,1536) for im in images)
crops={'face':(402,216,626,440),'hair':(667,423,891,647),'stockings':(147,637,371,861),'snow_crystal':(63,41,287,265),'staff_grip':(172,366,396,590)}
def canvas(size):return Image.new('RGB',size,(28,32,42))
full=canvas((1380,458));d=ImageDraw.Draw(full)
for i,(g,im) in enumerate(zip(groups,images)):
    d.text((i*276+10,10),labels[g],font=font,fill=(240,244,250));full.paste(im.resize((256,384),Image.Resampling.LANCZOS),(i*276+10,56))
full.save(Q/'full_comparison.png')
sheet=canvas((1380,len(crops)*280+48));d=ImageDraw.Draw(sheet)
for i,g in enumerate(groups):d.text((i*276+10,10),labels[g],font=font,fill=(240,244,250))
for row,(key,rect) in enumerate(crops.items()):
    for i,im in enumerate(images):
        d.text((i*276+10,48+row*280),key+' / 100%',font=font,fill=(194,208,227));tile=im.crop(rect);sheet.paste(tile,(i*276+10,84+row*280));tile.save(Q/f'{groups[i]}_{key}_100.png')
sheet.save(Q/'face_hair_stockings_staff_comparison.png')
avatars=canvas((1380,372));d=ImageDraw.Draw(avatars)
for i,(g,im) in enumerate(zip(groups,images)):
    tile=im.crop((388,190,644,446));tile.save(Q/f'{g}_avatar_256.png');avatars.paste(tile,(i*276+10,48));avatars.paste(tile.resize((64,64),Image.Resampling.LANCZOS),(i*276+10,308));d.text((i*276+10,10),labels[g],font=font,fill=(240,244,250))
avatars.save(Q/'avatar_comparison.png')
stats={};source=np.array(images[0],dtype=float);baseline=np.array(images[1],dtype=float)
for g,im in zip(groups[1:],images[1:]):
    arr=np.array(im,dtype=float);stats[g]={'sha256':hashlib.sha256((R/'outputs'/f'{g}.png').read_bytes()).hexdigest(),'mean_absolute_difference_from_input':float(np.mean(abs(arr-source))),'mean_absolute_difference_from_B0':float(np.mean(abs(arr-baseline))),'pixel_difference_from_B0_count':int(np.count_nonzero(np.any(arr!=baseline,axis=2)))}
    stats[g]['crop_mean_absolute_difference_from_B0']={key:float(np.mean(abs(arr[y1:y2,x1:x2]-baseline[y1:y2,x1:x2]))) for key,(x1,y1,x2,y2) in crops.items()}
(Q/'comparison_parameters.json').write_text(json.dumps({'canvas':[1024,1536],'crop_origin':'top-left','crops':crops,'avatar_crop':[388,190,644,446],'crop_scale':'100% native pixels','metrics':stats,'metric_limit':'pixel differences prove altered output; they do not establish beauty, identity preservation or effective LoRA gain'},indent=2),encoding='utf-8')
for gid in groups[1:]:
    for suffix in ['api','ui']:shutil.copy2(ROOT/'ArtSource/Workflows'/f'A08_LORA_{gid}_{suffix}.json',R/'runtime'/f'A08_LORA_{gid}_{suffix}.json')
    for suffix in ['run','history']:shutil.copy2(ROOT/'ArtSource/Metadata'/f'A08_LORA_{gid}_{suffix}.json',R/'runtime'/f'A08_LORA_{gid}_{suffix}.json')
print('Native-scale crop and full/256/64 avatar comparisons saved',flush=True)
