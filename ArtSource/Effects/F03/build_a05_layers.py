"""Original editable vector ornament design and local PNG export for A05.
No text or character images are drawn; SVG and PNG share the same geometry.
"""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import math,json,hashlib,numpy as np
BASE=Path(__file__).resolve().parent;ROOT=BASE.parents[2]
OUT=ROOT/'这次一定出/Assets/_Game/Art/Effects/F03'
SRC=BASE/'VectorSource';SRC.mkdir(exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
GOLD='#d9b56e';LIGHT='#fff0c1';BRASS='#8e622d';BLUE='#81cdfa'
records=[]
class Canvas:
    def __init__(self,w,h):
        self.w,self.h=w,h;self.im=Image.new('RGBA',(w*2,h*2));self.draw=ImageDraw.Draw(self.im);self.svg=[]
    def color(self,c):
        return tuple(int(c[i:i+2],16) for i in (1,3,5))+(255,)
    def circle(self,x,y,r,c=GOLD,width=2):
        self.draw.ellipse(((x-r)*2,(y-r)*2,(x+r)*2,(y+r)*2),outline=self.color(c),width=max(1,round(width*2)))
        self.svg.append(f'<circle cx="{x:.3f}" cy="{y:.3f}" r="{r:.3f}" fill="none" stroke="{c}" stroke-width="{width}"/>')
    def line(self,pts,c=GOLD,width=2):
        self.draw.line([(x*2,y*2) for x,y in pts],fill=self.color(c),width=max(1,round(width*2)),joint='curve')
        self.svg.append(f'<polyline points="{" ".join(f"{x:.3f},{y:.3f}" for x,y in pts)}" fill="none" stroke="{c}" stroke-width="{width}"/>')
    def poly(self,pts,c=GOLD,fill=False,width=2):
        if fill:self.draw.polygon([(x*2,y*2) for x,y in pts],fill=self.color(c))
        else:self.line(pts+[pts[0]],c,width)
        self.svg.append(f'<polygon points="{" ".join(f"{x:.3f},{y:.3f}" for x,y in pts)}" fill="{c if fill else "none"}" stroke="{c}" stroke-width="{0 if fill else width}"/>')
    def star(self,x,y,r,inner=None,n=4,c=GOLD,fill=False):
        inner=inner or r*.24
        pts=[(x+math.cos(-math.pi/2+i*math.pi/n)*(r if i%2==0 else inner),y+math.sin(-math.pi/2+i*math.pi/n)*(r if i%2==0 else inner)) for i in range(n*2)]
        self.poly(pts,c,fill)
    def finish(self,name,glow=0):
        im=self.im.resize((self.w,self.h),Image.Resampling.LANCZOS)
        if glow:
            halo=im.filter(ImageFilter.GaussianBlur(glow));halo.putalpha(halo.getchannel('A').point(lambda a:round(a*.42)));halo.alpha_composite(im);im=halo
        p=OUT/f'{name}_v001.png'
        if p.exists():raise RuntimeError('Existing version: '+str(p))
        im.save(p)
        svg=f'<svg xmlns="http://www.w3.org/2000/svg" width="{self.w}" height="{self.h}" viewBox="0 0 {self.w} {self.h}">'+''.join(self.svg)+'</svg>'
        sp=SRC/f'{name}_v001.svg';sp.write_text(svg,encoding='utf-8')
        a=im.getchannel('A');hist=a.histogram()
        records.append({'assetId':name,'path':str(p.relative_to(ROOT)).replace('\\','/'),'size':im.size,'alpha_extrema':a.getextrema(),'zero_alpha_pixels':hist[0],'center_alpha':a.getpixel((self.w//2,self.h//2)),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'source':str(sp.relative_to(ROOT)).replace('\\','/'),'method':'original vector design, local Pillow 2x raster export','glow_radius':glow,'status':'pending_unity_review'})
        return im

def pt(r,t,c=512):return (c+r*math.cos(t),c+r*math.sin(t))
def ring(name,kind):
    a=Canvas(1024,1024)
    radii={'giant':[470,460,435,410,384,374,354],'outer':[459,448,418,390],'inner':[349,341,320,278,264]}[kind]
    for i,r in enumerate(radii):a.circle(512,512,r,LIGHT if i%3==1 else GOLD,3 if i in [0,len(radii)-1] else 1.4)
    count=96 if kind=='giant' else 64 if kind=='outer' else 48
    for i in range(count):
        t=i*math.tau/count
        a.line([pt(radii[0]-2,t),pt(radii[0]-(20 if i%4==0 else 9),t)],LIGHT if i%4==0 else BRASS,2)
    rmid=(radii[1]+radii[2])/2
    count=24 if kind!='inner' else 12
    for i in range(count):
        t=i*math.tau/count;x,y=pt(rmid,t)
        a.star(x,y,14 if i%3==0 else 7,n=4,c=LIGHT if i%3==0 else GOLD,fill=i%3!=0)
        if i%3==0:
            a.circle(x,y,21,GOLD,1)
            a.line([pt(rmid-27,t-.025),pt(rmid-36,t),pt(rmid-27,t+.025)],GOLD,1.5)
    if kind=='giant':
        for i in range(8):
            t=i*math.tau/8
            a.poly([pt(491,t),pt(446,t-.03),pt(426,t),pt(446,t+.03)],GOLD,True)
            a.poly([pt(486,t),pt(453,t-.018),pt(442,t),pt(453,t+.018)],LIGHT,True)
        for i in range(24):
            t=i*math.tau/24;a.line([pt(373,t),pt(361,t+.03),pt(373,t+.06)],GOLD,2)
    if kind=='inner':
        for off in [0,math.pi/3]:
            a.poly([pt(301,off+i*math.tau/3) for i in range(3)],GOLD,False,2)
        for i in range(6):
            x,y=pt(279,i*math.tau/6);a.star(x,y,12,n=4,c=BLUE,fill=True)
    return a.finish(name,4)

# Paper and ornament share exactly the same canvas. All text/signatures remain UI layers.
paper=Canvas(1024,800)
paper.poly([(23,12),(1001,12),(1012,23),(1012,777),(1001,788),(23,788),(12,777),(12,23)],'#f4ecd8',True)
# Delicate ivory strokes confined to the margins, no writing in the live title/body/signature zones.
for x in [24,1000]:paper.line([(x,29),(x,771)],'#ddd0ad',2)
paper.finish('contract_paper')
border=Canvas(1024,800)
for inset,stroke in [(18,2.5),(29,1),(42,1)]:
    border.poly([(inset+14,inset),(1024-inset-14,inset),(1024-inset,inset+14),(1024-inset,800-inset-14),(1024-inset-14,800-inset),(inset+14,800-inset),(inset,800-inset-14),(inset,inset+14)],GOLD,False,stroke)
for x,y,sx,sy in [(40,40,1,1),(984,40,-1,1),(40,760,1,-1),(984,760,-1,-1)]:
    border.star(x,y,14,n=4,c=BRASS,fill=True)
    border.line([(x+sx*4,y+sy*25),(x+sx*23,y+sy*25),(x+sx*23,y+sy*5)],GOLD,1.6)
    border.circle(x+sx*8,y+sy*8,5,LIGHT,1)
border.finish('contract_border')
stamp=Canvas(512,512)
for r,w in [(222,5),(211,1.5),(197,2),(151,2)]:stamp.circle(256,256,r,GOLD,w)
for i in range(48):
    t=i*math.tau/48;stamp.line([(256+203*math.cos(t),256+203*math.sin(t)),(256+(215 if i%4==0 else 210)*math.cos(t),256+(215 if i%4==0 else 210)*math.sin(t))],LIGHT,2)
stamp.star(256,256,136,53,8,GOLD,False);stamp.star(256,256,111,36,4,LIGHT,False);stamp.star(256,256,41,9,4,GOLD,True)
for i in range(8):
    t=i*math.tau/8;stamp.star(256+174*math.cos(t),256+174*math.sin(t),10,n=4,c=LIGHT,fill=True)
stamp.finish('star_seal',3)
ring('ring_giant','giant');ring('ring_outer','outer');ring('ring_inner','inner')

# Analytic isolated light textures, no opaque background or scene baked in.
for name,w,h in [('shockwave',1024,1024),('light_pillar',512,1024)]:
    yy,xx=np.mgrid[:h,:w];x=(xx-w/2)/(w/2);y=(yy-h/2)/(h/2)
    if name=='shockwave':
        r=np.sqrt(x*x+y*y);theta=np.arctan2(y,x)
        intensity=.78*np.exp(-((r-.67)/.009)**2)+.36*np.exp(-((r-.63)/.024)**2)+.48*np.exp(-((r-.73)/.005)**2)
        rays=np.maximum(0,np.cos(theta*24))**18*np.exp(-((r-.73)/.16)**2)*.28
        intensity+=rays
    else:
        taper=(1-y*y).clip(0,1)**.7
        intensity=(.84*np.exp(-(x/.025)**2)+.30*np.exp(-(x/.13)**2)+.24*np.exp(-((np.abs(x)-.13)/.012)**2))*taper
        intensity+=.55*np.exp(-((y-.67)/.025)**2)*np.exp(-(x/.75)**2)
    alpha=(np.clip(intensity,0,1)*255).astype('uint8');alpha[alpha<2]=0
    rgba=np.zeros((h,w,4),dtype='uint8');rgba[:,:,:3]=[255,224,158];rgba[:,:,3]=alpha
    im=Image.fromarray(rgba);p=OUT/f'{name}_v001.png';im.save(p)
    hist=im.getchannel('A').histogram()
    records.append({'assetId':name,'path':str(p.relative_to(ROOT)).replace('\\','/'),'size':im.size,'alpha_extrema':im.getchannel('A').getextrema(),'zero_alpha_pixels':hist[0],'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'method':'original analytic local light texture','source':'ArtSource/Effects/F03/build_a05_layers.py','status':'pending_unity_review'})
(BASE/'A05_effects_manifest.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print('8 isolated layers exported; no text or usernames')
