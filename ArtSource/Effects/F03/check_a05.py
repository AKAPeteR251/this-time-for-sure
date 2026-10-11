from pathlib import Path
from PIL import Image,ImageOps,ImageDraw
import json,hashlib
BASE=Path(__file__).resolve().parent;ROOT=BASE.parents[2]
records=json.loads((BASE/'A05_effects_manifest.json').read_text(encoding='utf-8'))
qa=BASE/'QA';qa.mkdir(exist_ok=True)
sheet=Image.new('RGB',(1600,900),(13,23,42));d=ImageDraw.Draw(sheet)
for i,r in enumerate(records):
    im=Image.open(ROOT/r['path']).convert('RGBA');a=im.getchannel('A')
    assert a.histogram()[0]>0 and a.getextrema()[1]>0
    assert hashlib.sha256((ROOT/r['path']).read_bytes()).hexdigest()==r['sha256']
    if r['assetId'] in ['contract_border','ring_giant','ring_outer','ring_inner']:
        assert a.getpixel((im.width//2,im.height//2))==0
    if r['assetId']=='contract_border':
        assert a.crop((102,472,922,672)).getextrema()==(0,0)
    x=i%4*400;y=i//4*450
    tile=Image.new('RGBA',(380,410),(22,36,58,255));fit=ImageOps.contain(im,(368,390))
    tile.alpha_composite(fit,((380-fit.width)//2,(410-fit.height)//2))
    sheet.paste(tile.convert('RGB'),(x+10,y+6));d.text((x+15,y+420),r['assetId']+'  '+str(tuple(r['size'])),fill=(240,220,178))
sheet.save(qa/'A05_effects_contact_v001.jpg',quality=94)
result={'status':'PNG checks passed; Unity import pending','effects_count':len(records),'checks':['hashes','real alpha','hollow ring centers','signature area clear'],'U03_source':'Docs/Production/U03_CLEAN_交接.md','coordinate_origin':'bottom-left normalized','paper_screen_rect':[.205,.085,.795,.91],'signature_paper_rect':[.10,.16,.90,.41],'character_screen_rect':[.27,.15,.73,.93],'skip_screen_rect':[.86,.90,.97,.97],'confirm_screen_rect':[.80,.035,.96,.115]}
(BASE/'A05_checks.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8');print('8 effect PNGs verified, signature area and ring centers clear')
