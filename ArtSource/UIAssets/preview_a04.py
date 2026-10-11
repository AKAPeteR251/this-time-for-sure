"""Local QA composites only; does not alter production assets."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageOps
import json
BASE=Path(__file__).resolve().parent
ROOT=BASE.parents[1]
out=BASE/'QA';out.mkdir(exist_ok=True)
records=json.loads((BASE/'asset_manifest.json').read_text(encoding='utf-8'))
items=[r for r in records if not r['assetId'].endswith('_original')]
sheet=Image.new('RGB',(1440,((len(items)+5)//6)*300),(23,32,48));draw=ImageDraw.Draw(sheet)
for i,r in enumerate(items):
    x=(i%6)*240;y=(i//6)*300
    tile=Image.new('RGBA',(224,260),(75,86,101,255))
    td=ImageDraw.Draw(tile)
    for xx in range(0,224,16):
        for yy in range(0,260,16):
            if (xx//16+yy//16)%2:td.rectangle((xx,yy,xx+15,yy+15),fill=(100,110,124,255))
    im=Image.open(ROOT/r['path']).convert('RGBA')
    fit=ImageOps.contain(im,(212,248))
    tile.alpha_composite(fit,((224-fit.width)//2,(260-fit.height)//2))
    sheet.paste(tile.convert('RGB'),(x+8,y+6))
    draw.text((x+8,y+271),r['assetId'],fill=(242,230,200))
    draw.text((x+8,y+286),f"{r['width']}x{r['height']}",fill=(170,190,211))
sheet.save(out/'UI_assets_contact_v001.jpg',quality=93)
bg=Image.open(ROOT/'这次一定出/Assets/_Game/Art/Backgrounds/UI_FrozenCastle_v001.png').convert('RGBA')
hero=Image.open(ROOT/'这次一定出/Assets/_Game/Art/Characters/UIIceMage/UIIceMage_character_v001.png').convert('RGBA')
hero=ImageOps.contain(hero,(750,1050))
bg.alpha_composite(hero,(960,15))
bg.convert('RGB').save(out/'Background_character_alpha_check_v001.jpg',quality=94)
print('QA contact and alpha composite saved')
