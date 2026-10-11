"""Read-only PNG validation. Creates JSON metrics; does not change any image."""
from pathlib import Path
from PIL import Image
import json, hashlib

root=Path(__file__).resolve().parent
jobs=json.loads((root/'generation_jobs.json').read_text(encoding='utf-8'))
sizes={'BG01':(3840,2160),'BG02':(2048,1024),'BG03':(2048,1024),'CH01':(2048,2048),'FX01':(256,256),'FX02':(256,256),'FX03':(1024,256),'FX04':(256,512),'UI01':(1024,512),'UI02':(1024,512),'UI03':(1024,512),'UI04':(1024,512),'UI05':(1024,128),'UI06':(512,192),'UI07':(768,256)}
metrics=[]
for j in jobs:
    p=root/j['file']
    with Image.open(p) as source:
        mode=source.mode
        im=source.convert('RGBA')
    assert im.size==sizes[j['id']], j['file']+' size'
    alpha=im.getchannel('A')
    hist=alpha.histogram()
    edges=[im.crop((0,0,im.width,1)), im.crop((0,im.height-1,im.width,im.height))]
    horizontal_delta=None
    if j['id'] in ['BG02','BG03']:
        l=list(im.crop((0,0,1,im.height)).getdata())
        r=list(im.crop((im.width-1,0,im.width,im.height)).getdata())
        horizontal_delta=max(abs(a-b) for v,w in zip(l,r) for a,b in zip(v,w))
        assert horizontal_delta==0,'nebula wrap seam'
    else:
        edges.extend([im.crop((0,0,1,im.height)),im.crop((im.width-1,0,im.width,im.height))])
    edge_alpha=max(e.getchannel('A').getextrema()[1] for e in edges)
    if j['id']=='BG01':
        assert alpha.getextrema()==(255,255),'base must be opaque'
    else:
        assert edge_alpha==0,'sprite alpha touches boundary '+j['id']
        assert hist[0]>0 and sum(hist[1:])>0,'missing real transparency or empty'
    core=alpha.point(lambda a:255 if a>200 else 0).getbbox()
    if j['id'] in ['UI01','UI02','UI03','UI04']:
        assert abs(core[1]-104)<=3 and abs(core[3]-360)<=3,'rarity cap/baseline '+j['id']
    with Image.open(root/'raw_generated'/j['file']) as raw:
        native_size=raw.size
    metrics.append(dict(id=j['id'],file=j['file'],size=im.size,native_size=native_size,mode=mode,alpha_extrema=alpha.getextrema(),transparent_fraction=round(hist[0]/(im.width*im.height),5),edge_alpha=edge_alpha,horizontal_rgba_delta=horizontal_delta,core_bbox=core,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
character=root.parents[2]/'UIAssets/Originals/ice_mage_cutout_v001.png'
report={'status':'PASS','asset_count':len(metrics),'assets':metrics,'character_source':str(character),'character_sha256':hashlib.sha256(character.read_bytes()).hexdigest(),'preview_size':Image.open(root/'reveal_layout_preview_v003.png').size,'notes':['Pixels checked on exported files, not only prompts.','Generated native sizes are recorded separately; larger deliverables are resized exports.','Unity import and runtime motion not tested in this art task.']}
(root/'validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('PASS',len(metrics),'assets; alpha edges, sizes, nebula seams, rarity baseline')
