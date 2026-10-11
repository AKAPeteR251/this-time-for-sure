from pathlib import Path
from PIL import Image,ImageDraw
import numpy as np,json,hashlib
r=Path(__file__).resolve().parent.parent;results=[]
files=sorted(r.glob('reveal_burst_*_v001.png'));assert len(files)==4
preview=Image.new('RGB',(1100,680),(18,27,41));d=ImageDraw.Draw(preview)
for i,p in enumerate(files):
 im=Image.open(p);a=np.array(im)[:,:,3];w,h=im.size
 assert im.mode=='RGBA' and h==1024 and w==(256 if 'streak' in p.name else 512)
 assert not(a[0].any() or a[-1].any() or a[:,0].any() or a[:,-1].any())
 ys,xs=np.where(a>0);box=[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)]
 assert box[0]>=int(w*.08) and box[1]>=int(h*.08) and box[2]<=w-int(w*.08) and box[3]<=h-int(h*.08),(p.name,box)
 results.append({'file':p.name,'size':[w,h],'mode':im.mode,'alpha_bbox':box,'borders_alpha_zero':True,'safe_margin_8_percent':True,'partial_alpha_pixels':int(((a>0)&(a<255)).sum()),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
 im.thumbnail((235,590));bg=Image.new('RGBA',im.size,(18,27,41,255));bg.alpha_composite(im);preview.paste(bg.convert('RGB'),(i*275+(275-im.width)//2,30));d.text((i*275+15,640),p.name.replace('reveal_burst_',''),fill=(225,235,242))
(r/'Review').mkdir(exist_ok=True);preview.save(r/'Review/F06_RevealBurst_preview_v001.jpg',quality=95)
(r/'Source/alpha_checks_v001.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
print(json.dumps(results,indent=2))
