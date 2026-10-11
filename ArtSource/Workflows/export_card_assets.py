"""Export reviewed full cards and derived thumbnails without overwriting assets."""
import hashlib,json,shutil
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
from run_style_tests import ROOT,save

project=ROOT.parent/'这次一定出'/'Assets'/'_Game'/'Art'/'Characters'
chosen={'SSR001':'SSR001_card_base_v002.png','UR001':'UR001_card_base_v001.png'}
records=[]
sheet=Image.new('RGB',(864,720),'#10141f')
font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',25)
small=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',17)
draw=ImageDraw.Draw(sheet)
draw.text((24,14),'高级卡两卡试制 · 正式尺寸完整图',font=font,fill='#ede7d8')
for index,(cid,name) in enumerate(chosen.items()):
    source=ROOT/'Characters'/cid/name
    with Image.open(source) as original:
        assert original.size==(1024,1536),original.size
        image=original.convert('RGB')
        assert original.mode in ('RGB','RGBA'),original.mode
    thumb=source.with_name(cid+'_thumb_'+name.rsplit('_',1)[1])
    if thumb.exists(): raise RuntimeError('Retain existing thumbnail version: '+str(thumb))
    image.resize((256,384),Image.Resampling.LANCZOS).save(thumb)
    destination=project/cid
    destination.mkdir(parents=True,exist_ok=True)
    for asset in (source,thumb):
        target=destination/asset.name
        if target.exists(): raise RuntimeError('Retain existing Unity version: '+str(target))
        shutil.copy2(asset,target)
        records.append({'characterId':cid,'path':str(target.relative_to(ROOT.parent)),
            'size':list(Image.open(target).size),'sha256':hashlib.sha256(target.read_bytes()).hexdigest(),
            'layers':'complete_image_only','unity_import_validation':'pending',
            'recommended_import':{'textureType':'Sprite (2D and UI)','spriteMode':'Single','maxSize':2048,
                'sRGB':True,'compression':'None for first visual comparison','preserveAspect':True}})
    sheet.paste(image.resize((384,576),Image.Resampling.LANCZOS),(24+index*432,60))
    title='SSR 雾璃 · 月港星图' if cid=='SSR001' else 'UR 烬星 · 终夜日蚀'
    draw.text((24+index*432,647),title,font=font,fill='#ede7d8')
draw.text((24,689),'完整图；无人物alpha / 背景补洞；Unity接入与UR设计待验收。',font=small,fill='#99adc4')
sheet.save(ROOT/'Characters'/'A01_B_two_card_sheet.png')
save(ROOT/'Metadata'/'A01_B_export_manifest.json',records)
print('Exported',len(records),'art assets and two-card sheet',flush=True)
