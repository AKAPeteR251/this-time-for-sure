"""F09 exact font typesetting, native editable ornament layers, technical RGBA packaging."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageFilter
import numpy as np,json,zipfile,io,xml.etree.ElementTree as ET,base64
R=Path(__file__).resolve().parent.parent;S=R/'Source';L=S/'Layers';W,H=2048,1536
for p in [L,R/'Textures',R/'Preview']:p.mkdir(parents=True,exist_ok=True)
def empty(size=(W,H)):return Image.new('RGBA',size,(0,0,0,0))
def normalize(im):
 a=np.array(im);a[:,:,3][a[:,:,3]>=240]=255;a[:,:,3][a[:,:,3]<3]=0;a[:,:,:3][a[:,:,3]==0]=0;return Image.fromarray(a,'RGBA')
# Nonsemantic export fitting and alpha normalization; untouched generated originals retained.
raw=normalize(Image.open(S/'Raw/paper_base_generated_v001.png').convert('RGBA'))
base=empty();base.alpha_composite(raw.resize((1920,1440),Image.Resampling.LANCZOS),(64,48));base=normalize(base);base.save(L/'01_paper_material.png')
# Independently editable native layers, all local wear stays away from signature area.
fold=empty();fd=ImageDraw.Draw(fold)
for pts in [[(110,345),(520,371),(945,345),(1490,373),(1910,350)],[(120,1370),(800,1358),(1350,1380),(1910,1360)],[(1810,110),(1790,440),(1820,790),(1800,1160)]]:
 fd.line(pts,fill=(91,64,35,18),width=3)
 fd.line([(x,y-4) for x,y in pts],fill=(255,249,221,22),width=4)
fold=fold.filter(ImageFilter.GaussianBlur(2));fold.putalpha(Image.fromarray(np.minimum(np.array(fold)[:,:,3],np.array(base)[:,:,3]).astype('uint8')));fold.save(L/'02_soft_folds.png')
stain=empty();d=ImageDraw.Draw(stain)
for box in [(85,470,190,620),(160,1320,310,1430),(1770,160,1900,280),(1770,940,1915,1110)]:d.ellipse(box,fill=(115,73,29,15))
stain=stain.filter(ImageFilter.GaussianBlur(28));stain.putalpha(Image.fromarray(np.minimum(np.array(stain)[:,:,3],np.array(base)[:,:,3]).astype('uint8')));stain.save(L/'03_peripheral_stains.png')
decor=empty();d=ImageDraw.Draw(decor)
for cx,cy,sx,sy in [(160,145,1,1),(1888,145,-1,1),(160,1391,1,-1),(1888,1391,-1,-1)]:
 def p(x,y):return (cx+sx*x,cy+sy*y)
 for off in [0,8]:d.line([p(0,130),p(0,0),p(140,0)],fill=(102,72,42,54),width=2 if off==0 else 1)
 d.line([p(-14,0),p(0,-14),p(14,0),p(0,14),p(-14,0)],fill=(102,72,42,75),width=3)
 d.line([p(26,0),p(100,0)],fill=(102,72,42,45),width=3)
decor.save(L/'04_faded_corner_ornaments.png')
paper=base.copy()
for im in [fold,stain,decor]:paper.alpha_composite(im)
paper=normalize(paper);paper.save(R/'Textures/contract_parchment_blank_v003.png')
allura_path=R/'Fonts/Allura/Allura-Regular.ttf';caveat_path=R/'Fonts/Caveat/Caveat[wght].ttf'
titlefont=ImageFont.truetype(str(allura_path),154)
bodyfont=ImageFont.truetype(str(allura_path),100)
brandfont=ImageFont.truetype(str(allura_path),74)
promptfont=ImageFont.truetype(str(allura_path),64)
text=empty();draw=ImageDraw.Draw(text);records=[]
def lettering(value,y,font,fontname,size,color=(69,40,23,238),cx=1024):
 box=draw.textbbox((0,0),value,font=font);x=cx-(box[2]-box[0])/2-box[0]
 draw.text((x,y-box[1]),value,font=font,fill=color)
 records.append({'text':value,'font':fontname,'weight':400 if fontname=='Allura' or size==50 else 500,'size_px':size,'x':x,'top':y,'baseline_y':y-box[1]+font.getmetrics()[0],'bbox':[x+box[0],y,x+box[2],y+box[3]-box[1]]})
lettering('Star Echo',170,brandfont,'Allura',74)
lettering('Covenant of the Stars',255,titlefont,'Allura',154)
for line,y in [('By this name, I awaken the echoes',475),('that slumber among the stars.',560),('Let this covenant be sealed,',690),('with the stars as witness.',775)]:lettering(line,y,bodyfont,'Allura',100)
lettering('Sign your name',1280,promptfont,'Allura',64,(80,48,26,180),cx=430)
# Slight native ink opacity variation, exact glyphs and wording remain font-derived.
arr=np.array(text);yy,xx=np.mgrid[0:H,0:W];factor=.94+.06*np.sin(xx*.027+yy*.043)**2;arr[:,:,3]=np.uint8(arr[:,:,3]*factor);text=Image.fromarray(arr,'RGBA');text.save(R/'Textures/contract_english_lettering_v003.png');text.save(L/'05_english_lettering.png')
# Exact-sized seal canvas; 112px minimum transparent outer margin.
wax=empty((1024,1024));raw_wax=normalize(Image.open(S/'Raw/wax_aged_crystal_generated_v003.png').convert('RGBA'))
wax.alpha_composite(raw_wax.resize((800,800),Image.Resampling.LANCZOS),(112,112));wax=normalize(wax);wax.save(R/'Textures/contract_wax_seal_red_v003.png')
# Independently authored soft contact ellipse, not a distant environment shadow.
shadow=empty((1024,1024));d=ImageDraw.Draw(shadow);d.ellipse((143,147,881,889),fill=(32,23,19,62));shadow=shadow.filter(ImageFilter.GaussianBlur(14));shadow.save(R/'Textures/contract_wax_shadow_v003.png')
seal_size=360;seal_xy=(1520,1130)
wax_layer=empty();wax_layer.alpha_composite(wax.resize((seal_size,seal_size),Image.Resampling.LANCZOS),seal_xy);wax_layer.save(L/'07_wax_seal_placed.png')
shadow_layer=empty();shadow_layer.alpha_composite(shadow.resize((seal_size,seal_size),Image.Resampling.LANCZOS),seal_xy);shadow_layer.save(L/'06_contact_shadow_placed.png')
merged=paper.copy();merged.alpha_composite(text);merged.alpha_composite(shadow_layer);merged.alpha_composite(wax_layer);merged.save(R/'Preview/contract_complete_preview_v003.png')
guide=merged.copy();dg=ImageDraw.Draw(guide);signature=[205,922,1475,1275];dg.rectangle(signature,outline=(54,133,150,255),width=4);dg.text((220,928),'DYNAMIC SIGNATURE SAFE AREA',font=ImageFont.truetype(str(caveat_path),36),fill=(40,120,140,255));dg.rectangle((1520,1130,1880,1490),outline=(180,81,50,255),width=3);guide.save(R/'Preview/layout_safe_areas_v003.png')
# OpenRaster: real independent editable layers; supported by Krita/GIMP and other ORA tools.
layers=[('Paper material',base),('Soft folds',fold),('Peripheral stains',stain),('Faded corner ornaments',decor),('Exact English lettering',text),('Contact shadow - placed',shadow_layer),('Red wax seal - placed',wax_layer)]
image=ET.Element('image',{'w':str(W),'h':str(H),'name':'F09 layered contract v003'});stack=ET.SubElement(image,'stack')
with zipfile.ZipFile(S/'F09_ContractParchmentWax_v003.ora','w') as z:
 z.writestr('mimetype','image/openraster',compress_type=zipfile.ZIP_STORED)
 for index,(name,im) in reversed(list(enumerate(layers))):
  path='data/layer_%02d.png'%index;b=io.BytesIO();im.save(b,format='PNG');z.writestr(path,b.getvalue());ET.SubElement(stack,'layer',{'name':name,'src':path,'opacity':'1.0','visibility':'visible','composite-op':'svg:src-over','x':'0','y':'0'})
 b=io.BytesIO();merged.save(b,format='PNG');z.writestr('mergedimage.png',b.getvalue());z.writestr('stack.xml',ET.tostring(image,encoding='utf-8'))
# Editable SVG exact text, embeds actual fonts so it is independent of OS-installed fonts.
svg=['<svg xmlns="http://www.w3.org/2000/svg" width="2048" height="1536" viewBox="0 0 2048 1536"><defs><style>']
for family,path in [('Allura',allura_path),('Caveat',caveat_path)]:svg.append('@font-face{font-family:'+family+';src:url(data:font/ttf;base64,'+base64.b64encode(path.read_bytes()).decode()+') format("truetype");}')
svg.append('</style></defs><g fill="#452817">')
for q in records:svg.append('<text x="%s" y="%s" font-family="%s" font-size="%s" font-weight="%s">%s</text>'%(q['x'],q['baseline_y'],q['font'],q['size_px'],q['weight'],q['text']))
svg.append('</g></svg>');(S/'contract_english_editable_v003.svg').write_text(''.join(svg),encoding='utf-8')
(S/'layout_v003.json').write_text(json.dumps({'canvas':[W,H],'signature_safe_rect_px':signature,'signature_safe_rect_percent':[10,60,72,83],'seal_sprite_rect_px':[1520,1130,1880,1490],'seal_center_px':[1700,1310],'seal_sprite_size':360,'all_text':records,'authoring':'imagegen paper/wax; actual fonts; native vector wear and soft contact shadow; geometric resampling and alpha normalization','paper_original_px':[1448,1086],'wax_original_px':[1254,1254],'rgba_export':'alpha>=240 normalized to255; alpha<3 removed; raw originals retained','model_parameters':'builtin imagegen; hidden seed/steps/model version not exposed'},ensure_ascii=False,indent=2),encoding='utf-8')
print('F09 assembled 5 requested images, 7 editable ORA layers, exact SVG text and safe area guide')
