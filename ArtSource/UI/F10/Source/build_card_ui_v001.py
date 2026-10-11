"""F10 deterministic native UI geometry, exact glyphs and editable card previews."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageFilter,ImageOps
import numpy as np,json,zipfile,io,xml.etree.ElementTree as ET
R=Path(__file__).resolve().parent.parent;T=R/'Textures';P=R/'Preview';S=R/'Source';AA=2
palettes={'R':[(73,139,179),(169,231,250),(220,247,255)],'SR':[(96,51,148),(191,135,240),(249,218,255)],'SSR':[(140,83,30),(235,181,72),(255,246,199)],'UR':[(73,159,205),(191,132,246),(247,175,193),(237,204,132)]}
layout={'task':'F10-CARD-UI','version':'v001','canvas':[1024,1536],'origin':'top-left','pivot':[.5,.5],'inner_window':[48,64,976,1472],'frame':[0,0,1024,1536],'new':[72,64,392,184],'rarity':[768,56,984,272],'name_shade':[48,1250,976,1472],'name_ornament':[48,1250,976,1472],'element':[72,1336,224,1488],'name_text':[264,1352,920,1472],'face_safe':[170,280,854,900],'shared_geometry_all_rarities':True,'runtime_text':['NEW','character name'],'fixed_baked_text':['R','SR','SSR','UR'],'character_preview_source':'Source/PreviewInputs/thunder_miko_original.png','character_preview_calibration':'same character on all four UI levels, not actual rarity reassignment'}
(R/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')
def blank(size):return Image.new('RGBA',(size[0]*AA,size[1]*AA),(0,0,0,0))
def pts(points):return [(round(x*AA),round(y*AA)) for x,y in points]
def rgb(c):return '#%02x%02x%02x'%tuple(c[:3])
def finish(im):
 im=im.resize((im.width//AA,im.height//AA),Image.Resampling.LANCZOS);a=np.array(im);a[:,:,3][a[:,:,3]<3]=0;a[:,:,:3][a[:,:,3]==0]=0;return Image.fromarray(a,'RGBA')
def gem(draw,cx,cy,rx,ry,color,angle=0):
 v=np.array([[0,-ry],[rx,0],[0,ry],[-rx,0]],float);rot=np.array([[np.cos(angle),-np.sin(angle)],[np.sin(angle),np.cos(angle)]]);v=v@rot.T+np.array([cx,cy])
 center=(cx,cy);colors=[tuple(min(255,int(x*.7)) for x in color),tuple(min(255,int(x*1.10+15)) for x in color),color,tuple(min(255,int(x*.86)) for x in color)]
 for i in range(4):draw.polygon(pts([center,tuple(v[i]),tuple(v[(i+1)%4])]),fill=(*colors[i],245))
 draw.line(pts(list(v)+[v[0]]),fill=(244,241,255,215),width=AA)
def frame(kind):
 im=blank((1024,1536));d=ImageDraw.Draw(im);p=palettes[kind];mid=p[1]
 outer=[(20,56),(56,20),(968,20),(1004,56),(1004,1480),(968,1516),(56,1516),(20,1480),(20,56)]
 inner=[(34,62),(62,34),(962,34),(990,62),(990,1474),(962,1502),(62,1502),(34,1474),(34,62)]
 for points,width,color in [(outer,10,(*p[0],255)),(outer,3,(*p[-1],255)),(inner,2,(*mid,220))]:d.line(pts(points),fill=color,width=width*AA,joint='curve')
 for cx,cy,sx,sy in [(24,24,1,1),(1000,24,-1,1),(24,1512,1,-1),(1000,1512,-1,-1)]:
  v=[(cx,cy),(cx+sx*64,cy),(cx,cy+sy*64)];d.polygon(pts(v),fill=(*p[0],240));d.line(pts(v+[v[0]]),fill=(*p[-1],250),width=2*AA)
  gem(d,cx+sx*17,cy+sy*18,9,18,mid,angle=sx*sy*.65)
  d.line(pts([(cx+sx*72,cy),(cx+sx*150,cy+sy*13),(cx+sx*198,cy+sy*7)]),fill=(*mid,180),width=AA)
  d.line(pts([(cx,cy+sy*72),(cx+sx*13,cy+sy*142),(cx+sx*7,cy+sy*185)]),fill=(*mid,180),width=AA)
 for cx,cy,rx,ry in [(512,36,15,21),(512,1500,15,21),(29,768,10,20),(995,768,10,20)]:gem(d,cx,cy,rx,ry,mid)
 # Full rectangular inner window is genuinely empty in every frame.
 d.rectangle((48*AA,64*AA,975*AA,1471*AA),fill=(0,0,0,0))
 core=im.copy();halo=im.filter(ImageFilter.GaussianBlur(2.0*AA));a=np.array(halo);a[:,:,3]=np.uint8(a[:,:,3]*.18);halo=Image.fromarray(a,'RGBA');halo.alpha_composite(core)
 a=np.array(halo);a[64*AA:1472*AA,48*AA:976*AA]=0
 a[:8*AA]=0;a[-8*AA:]=0;a[:,:8*AA]=0;a[:,-8*AA:]=0
 result=finish(Image.fromarray(a,'RGBA'))
 # Keep final-pixel window exactly transparent after antialias resampling.
 arr=np.array(result);arr[64:1472,48:976]=0;result=Image.fromarray(arr,'RGBA')
 # Runtime UR supports real-time tint/UV modulation; static base has restrained iridescence.
 if kind=='UR':
  arr=np.array(result);y=np.arange(1536)/1536;stops=np.array(p,float);z=(y*3)%3;idx=np.floor(z).astype(int);f=z-idx;colors=stops[idx]*(1-f[:,None])+stops[idx+1]*f[:,None]
  brightness=np.mean(arr[:,:,:3],axis=2)/190;arr[:,:,:3]=np.uint8(np.clip(colors[:,None,:]*(.62+.38*brightness[:,:,None]),0,255));arr[:,:,:3][arr[:,:,3]==0]=0;result=Image.fromarray(arr,'RGBA')
 return result,outer,inner
def cinzel(size):
 f=ImageFont.truetype(str(S/'Fonts/Cinzel/Cinzel[wght].ttf'),size);f.set_variation_by_axes([700]);return f
for kind in palettes:
 im,outer,inner=frame(kind);im.save(T/('card_frame_'+kind+'_v001.png'))
 svg='<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1536"><g fill="none" stroke="'+rgb(palettes[kind][1])+'"><polyline points="'+' '.join('%s,%s'%q for q in outer)+'" stroke-width="8"/><polyline points="'+' '.join('%s,%s'%q for q in inner)+'" stroke-width="2"/></g></svg>'
 (S/'Vector'/('card_frame_'+kind+'_construction_v001.svg')).write_text(svg,encoding='utf-8')
 # Badge is a small translucent decorated shield, not a solid black square.
 badge=blank((512,512));d=ImageDraw.Draw(badge);p=palettes[kind];poly=[(66,115),(106,78),(406,78),(446,115),(430,388),(256,460),(82,388)]
 d.polygon(pts(poly),fill=(22,25,49,130));d.line(pts(poly+[poly[0]]),fill=(*p[1],210),width=4*AA)
 d.line(pts([(110,106),(402,106)]),fill=(*p[-1],170),width=AA);gem(d,256,84,13,22,p[1]);gem(d,256,423,11,17,p[1])
 fontsize={'R':224,'SR':210,'SSR':156,'UR':206}[kind];font=cinzel(fontsize*AA)
 box=d.textbbox((0,0),kind,font=font,stroke_width=2*AA);width=box[2]-box[0];height=box[3]-box[1]
 d.text(((512*AA-width)/2-box[0],(512*AA-height)/2-box[1]),kind,font=font,fill=(*p[-1],255),stroke_width=2*AA,stroke_fill=(*p[0],255))
 finish(badge).save(T/('card_rarity_'+kind+'_v001.png'))
# NEW: grey/white border mask and separate translucent indigo fill. No text.
poly=[(20,52),(44,24),(468,24),(492,52),(492,140),(468,168),(44,168),(20,140)]
border=blank((512,192));d=ImageDraw.Draw(border);d.line(pts(poly+[poly[0]]),fill=(255,255,255,245),width=4*AA)
for x in [28,484]:gem(d,x,96,7,13,(215,215,215))
mask=finish(border);a=np.array(mask);a[:,:,:3]=np.repeat(np.mean(a[:,:,:3],axis=2,keepdims=True).astype('uint8'),3,axis=2);a[:,:,:3][a[:,:,3]==0]=0;mask=Image.fromarray(a,'RGBA');mask.save(T/'card_new_border_mask_v001.png')
fill=blank((512,192));d=ImageDraw.Draw(fill);d.polygon(pts([(29,57),(48,33),(464,33),(483,57),(483,135),(464,159),(48,159),(29,135)]),fill=(24,25,56,180));finish(fill).save(T/'card_new_fill_v001.png')
# Name shade has no hard upper strip, fades completely at top and canvas margins.
yy,xx=np.mgrid[0:256,0:1024];u=yy/255;t=np.clip((u-.03)/.68,0,1);smooth=t*t*(3-2*t)
edge=np.clip(xx/20,0,1)*np.clip((1023-xx)/20,0,1)*np.clip((255-yy)/7,0,1)
arr=np.zeros((256,1024,4),np.uint8);arr[:,:,:3]=(14,17,38);arr[:,:,3]=np.uint8(smooth*edge*225);arr[:,:,:3][arr[:,:,3]==0]=0;Image.fromarray(arr,'RGBA').save(T/'card_name_shade_v001.png')
orn=blank((1024,256));d=ImageDraw.Draw(orn);d.line(pts([(42,24),(430,24)]),fill=(210,211,235,85),width=AA);d.line(pts([(592,24),(982,24)]),fill=(210,211,235,85),width=AA);gem(d,512,24,9,8,(194,199,225));d.line(pts([(64,87),(44,138),(64,229)]),fill=(209,212,237,95),width=AA);gem(d,64,166,8,15,(172,181,213));d.line(pts([(993,93),(979,150),(993,228)]),fill=(209,212,237,75),width=AA);finish(orn).save(T/'card_name_ornament_v001.png')
# Preview-only inputs, texts and element. Runtime UI textures never receive these.
window=layout['inner_window'];character=Image.new('RGBA',(1024,1536));im=ImageOps.fit(Image.open(S/'PreviewInputs/thunder_miko_original.png').convert('RGBA'),(928,1408),method=Image.Resampling.LANCZOS);character.alpha_composite(im,(48,64))
element=Image.new('RGBA',(1024,1536));element.alpha_composite(Image.open(S/'PreviewInputs/element_thunder_existing.png').convert('RGBA').resize((152,152),Image.Resampling.LANCZOS),(72,1336))
def placed(img,rect):
 result=Image.new('RGBA',(1024,1536));result.alpha_composite(img.resize((rect[2]-rect[0],rect[3]-rect[1]),Image.Resampling.LANCZOS),(rect[0],rect[1]));return result
name=Image.new('RGBA',(1024,1536));d=ImageDraw.Draw(name);font=ImageFont.truetype(str(S/'Fonts/LXGWWenKai/LXGWWenKai-Regular.ttf'),96);box=d.textbbox((0,0),'雷巫女',font=font);d.text((592-(box[2]-box[0])/2-box[0],1372-box[1]),'雷巫女',font=font,fill=(255,249,235,255),stroke_width=2,stroke_fill=(17,20,41,220))
newtext=Image.new('RGBA',(1024,1536));d=ImageDraw.Draw(newtext);f=cinzel(66);box=d.textbbox((0,0),'NEW',font=f);d.text((232-(box[2]-box[0])/2-box[0],105-box[1]),'NEW',font=f,fill=(252,249,232,255))
def save_ora(kind,layers,merged):
 xml=ET.Element('image',{'w':'1024','h':'1536','name':'F10 '+kind+' card preview'});stack=ET.SubElement(xml,'stack')
 with zipfile.ZipFile(S/('card_'+kind+'_layers_v001.ora'),'w') as z:
  z.writestr('mimetype','image/openraster',compress_type=zipfile.ZIP_STORED)
  for i,(label,im) in reversed(list(enumerate(layers))):
   path='data/layer_%02d.png'%i;b=io.BytesIO();im.save(b,format='PNG');z.writestr(path,b.getvalue());ET.SubElement(stack,'layer',{'name':label,'src':path,'opacity':'1','visibility':'visible','composite-op':'svg:src-over','x':'0','y':'0'})
  b=io.BytesIO();merged.save(b,format='PNG');z.writestr('mergedimage.png',b.getvalue());z.writestr('stack.xml',ET.tostring(xml,encoding='utf-8'))
for kind in palettes:
 layers=[('Original character preview only',character),('Name shade',placed(Image.open(T/'card_name_shade_v001.png'),layout['name_shade'])),('Name ornament',placed(Image.open(T/'card_name_ornament_v001.png'),layout['name_ornament'])),('Existing element preview only',element),('Character name preview only',name),('Frame',Image.open(T/('card_frame_'+kind+'_v001.png'))),('Rarity',placed(Image.open(T/('card_rarity_'+kind+'_v001.png')),layout['rarity'])),('NEW fill',placed(Image.open(T/'card_new_fill_v001.png'),layout['new'])),('NEW mask preview tint',placed(Image.open(T/'card_new_border_mask_v001.png'),layout['new'])),('NEW text preview only',newtext)]
 merged=Image.new('RGBA',(1024,1536))
 for _,im in layers:merged.alpha_composite(im)
 merged.save(P/('card_'+kind+'_full_v001.png'));save_ora(kind,layers,merged)
 without=Image.new('RGBA',(1024,1536))
 for _,im in layers[:-3]:without.alpha_composite(im)
 without.save(P/('card_'+kind+'_no_new_v001.png'))
guide=Image.open(P/'card_SR_full_v001.png').copy();d=ImageDraw.Draw(guide);gf=ImageFont.truetype(str(S/'Fonts/Cinzel/Cinzel[wght].ttf'),26)
for key in ['inner_window','new','rarity','name_shade','element','name_text','face_safe']:
 rect=layout[key];d.rectangle(rect,outline=(89,246,206,255),width=3);d.text((rect[0]+6,rect[1]+4),key,font=gf,fill=(255,249,220,255))
guide.save(P/'layout_safe_areas_v001.png')
small=Image.new('RGB',(1280,850),(18,24,38));d=ImageDraw.Draw(small)
for i,kind in enumerate(palettes):
 for y,size in [(38,(300,450)),(540,(180,270))]:
  im=Image.open(P/('card_'+kind+'_full_v001.png')).resize(size,Image.Resampling.LANCZOS);bg=Image.new('RGBA',size,(18,24,38,255));bg.alpha_composite(im);small.paste(bg.convert('RGB'),(i*320+(320-size[0])//2,y))
 d.text((i*320+15,10),kind+' / 300x450',font=gf,fill=(235,236,245));d.text((i*320+15,508),kind+' / 180x270',font=gf,fill=(235,236,245))
small.save(P/'cards_small_readability_v001.png')
for p in T.glob('*.png'):
 import shutil
 dst=S/'Raw'/p.name
 if not dst.exists():shutil.copy2(p,dst)
(S/'design_parameters_v001.json').write_text(json.dumps({'authoring':'native deterministic UI vectors + font rasterization; no AI redraw','antialias_scale':AA,'palette_rgb':palettes,'rarity_font':'Cinzel weight700','name_preview_font':'LXGWWenKai Regular','construction_svg':'outer/inner base tracks; full procedural corners/typography retained in Python and ORA','runtime_count':12},indent=2),encoding='utf-8')
print('F10 produced 12 standalone runtime textures, 4 editable ORA masters and actual-size previews')
