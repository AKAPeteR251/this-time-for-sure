"""Deterministic authored PBR data and loopable VFX, no lighting baked in."""
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw,ImageFilter
import json
ROOT=Path(__file__).resolve().parent.parent;T=ROOT/'Textures';T.mkdir(parents=True,exist_ok=True)
N=2048;rng=np.random.default_rng(8081010)
def save(name,array):Image.fromarray(np.uint8(np.clip(array,0,1)*255)).save(T/name)
def periodic_noise(n,amplitude):
 y,x=np.mgrid[0:N,0:N]/N;v=np.zeros((N,N),np.float32)
 for k in range(n):
  fx=int(rng.integers(1,35));fy=int(rng.integers(1,35));phase=float(rng.random()*6.283)
  v+=np.sin((x*fx+y*fy)*6.283185307+phase).astype(np.float32)/n
 return v*amplitude
y,x=np.mgrid[0:N,0:N]/N
noise=periodic_noise(18,.12)
cy=y*4;cx=x*4+(np.floor(cy)%2)*.5
edge=np.minimum(np.minimum(cx%1,1-cx%1),np.minimum(cy%1,1-cy%1))
grout=np.clip(edge/.016,0,1)
cell=np.sin(np.floor(cx)*3.11+np.floor(cy)*4.31)*.028
height=.035*grout+noise*.025
stone=np.stack([.105+noise*.15+cell,.145+noise*.18+cell,.205+noise*.24+cell],axis=-1)*( .58+.42*grout[:,:,None])
save('stone_basecolor_2048_v001.png',stone)
save('stone_roughness_2048_v001.png',.76+noise*.4+(1-grout)*.10)
save('stone_metallic_2048_v001.png',np.zeros((N,N)))
goldnoise=periodic_noise(20,.15)
save('antique_gold_basecolor_2048_v001.png',np.stack([.64+goldnoise*.18,.43+goldnoise*.13,.19+goldnoise*.06],axis=-1))
save('antique_gold_roughness_2048_v001.png',.38+goldnoise*.4)
save('antique_gold_metallic_2048_v001.png',np.full((N,N),.86))
def normal(h,strength):
 dx=(np.roll(h,-1,axis=1)-np.roll(h,1,axis=1))*strength;dy=(np.roll(h,-1,axis=0)-np.roll(h,1,axis=0))*strength
 z=np.ones_like(h);v=np.stack([-dx,dy,z],axis=-1);v/=np.linalg.norm(v,axis=-1,keepdims=True);return v*.5+.5
save('stone_normal_2048_v001.png',normal(height,180))
save('antique_gold_normal_2048_v001.png',normal(goldnoise*.003,180))
# Emission mask, no glow; original concentric geometric ritual lines.
im=Image.new('L',(N,N),0);d=ImageDraw.Draw(im);c=N/2
for r,w in [(850,8),(810,3),(630,5),(600,3),(345,5),(320,3)]:d.ellipse((c-r,c-r,c+r,c+r),outline=255,width=w)
for i in range(24):
 a=i*2*np.pi/24
 for r0,r1 in [(815,845),(605,627)]:d.line((c+r0*np.cos(a),c+r0*np.sin(a),c+r1*np.cos(a),c+r1*np.sin(a)),fill=255,width=5)
for i in range(8):
 a=i*2*np.pi/8;rad=740;px=c+rad*np.cos(a);py=c+rad*np.sin(a)
 pts=[(px+38*np.cos(a+j*np.pi/2),py+38*np.sin(a+j*np.pi/2)) for j in range(4)];d.line(pts+[pts[0]],fill=255,width=6)
im.save(T/'altar_emission_mask_2048_v001.png')
# 16 periodic analytical flame frames: phase repeats exactly at t=2pi, no pot/background.
S=512;yy,xx=np.mgrid[0:S,0:S].astype(np.float32);u=(xx+.5)/S*2-1;v=1-(yy+.5)/S
atlas=Image.new('RGBA',(2048,2048));flameframes=[]
for i in range(16):
 phase=i*2*np.pi/16;field=np.zeros_like(u)
 for j,(offset,top,width) in enumerate([(-.19,.71,.14),(.0,.9,.22),(.20,.68,.13)]):
  tip=top+.055*np.sin(phase+j*2.2);q=(v-.09)/(tip-.09);qc=np.clip(q,0,1)
  center=offset+.065*np.sin(phase*1+qc*6+j)*qc
  w=width*(1-qc)**.85+.002
  f=np.exp(-((u-center)/w)**2*1.3)*np.sin(qc*np.pi)**.48
  f*=((q>0)&(q<1));field=np.maximum(field,f)
 alpha=np.clip(field*1.45,0,1)
 heat=np.clip(field*.7+(1-v)*.35,0,1)
 rgb=np.stack([np.ones_like(u),.24+.70*heat,.025+.48*heat**3],axis=-1)
 rgba=np.dstack([rgb,alpha]);rgba[:,:,:3]*=(alpha>0)[:,:,None]
 frame=Image.fromarray(np.uint8(np.clip(rgba,0,1)*255),'RGBA');flameframes.append(frame);atlas.paste(frame,((i%4)*S,(i//4)*S))
atlas.save(T/'flame_amber_flipbook_4x4_2048_v001.png')
flameframes[0].save(ROOT/'Source/flame_frame00_reference_v001.png')
# Neutral, soft, no scene-specific illumination.
M=1024;gy,gx=np.mgrid[0:M,0:M]/M
mist=np.zeros((M,M),np.float32)
for j in range(12):
 px=float(rng.uniform(.12,.88));py=float(rng.uniform(.30,.70));sx=float(rng.uniform(.09,.19));sy=float(rng.uniform(.05,.10))
 mist+=np.exp(-((gx-px)/sx)**2-((gy-py)/sy)**2)*.10
fade=np.sin(gx*np.pi)**2*np.sin(gy*np.pi)**2
alpha=np.clip(mist*fade,0,.45);save('floor_mist_1024_v001.png',np.dstack([np.ones_like(alpha)*.86,np.ones_like(alpha)*.86,np.ones_like(alpha)*.86,alpha]))
M=512;gy,gx=np.mgrid[0:M,0:M]/M;dust=np.exp(-(((gx-.5)/.16)**2+((gy-.5)/.16)**2)*2)
save('dust_soft_512_v001.png',np.dstack([np.ones_like(dust),np.ones_like(dust),np.ones_like(dust),dust]))
meta={'seed':8081010,'authoring':'deterministic periodic height/noise PBR maps and analytical periodic VFX; not AI imagery, no baked lights','basecolor':'sRGB','normal':'tangent-space OpenGL +Y; linear/Normal Map import','metallic_roughness_mask':'linear single-channel grayscale; separate files, not packed','smoothness':'1 - roughness','flame':{'grid':[4,4],'frames':16,'cell_px':[512,512],'order':'row-major, top-left to bottom-right','reference_fps':12,'loop_period_seconds':16/12,'periodic_phase':'i*2pi/16; virtual frame16 equals frame0','alpha':'straight RGBA; each cell has transparent margin'},'mist':'RGBA neutral; linear alpha; tint/light/motion in Unity'}
(ROOT/'Source/texture_parameters_v001.json').write_text(json.dumps(meta,ensure_ascii=False,indent=2),encoding='utf-8')
print('F08 authored 12 PBR/VFX texture files')
