from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import numpy as np,json,hashlib,zipfile
R=Path(__file__).resolve().parent.parent;ROOT=R.parents[2];Q=R/'QA';src=R/'source/LightElement_v002/R003_original_tool_output.png'
im=Image.open(src);assert im.mode=='RGBA' and im.size==(1024,1536)
out=Image.new('RGBA',im.size);out.alpha_composite(im.resize((922,1382),Image.Resampling.LANCZOS),(51,77))
out.save(R/'R003_character_light_v002.png');out.resize((256,384),Image.Resampling.LANCZOS).save(R/'R003_thumb_light_v002.png')
face=(368,168,624,424);out.crop(face).save(R/'R003_avatar_light_v002.png')
sheet=Image.new('RGB',(1080,596),(28,32,42));d=ImageDraw.Draw(sheet);f=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',23)
for i,(name,color) in enumerate([('BLACK',(0,0,0)),('WHITE',(255,255,255)),('GRAY',(128,128,128))]):
 bg=Image.new('RGBA',out.size,(*color,255));bg.alpha_composite(out);bg.convert('RGB').save(Q/f'R003_light_{name.lower()}_v002.png');sheet.paste(bg.convert('RGB').resize((360,540),Image.Resampling.LANCZOS),(i*360,48));d.text((i*360+10,10),name,font=f,fill=(235,239,247))
sheet.save(Q/'R003_light_alpha_QA_v002.png')
comp=Image.new('RGB',(800,640),(60,64,72));d=ImageDraw.Draw(comp)
for i,(name,pic) in enumerate([('Original green / v001',Image.open(R/'R003_character_internal_v001.png')),('Light element / v002',out)]):
 bg=Image.new('RGBA',pic.size,(128,128,128,255));bg.alpha_composite(pic);comp.paste(bg.convert('RGB').resize((384,576),Image.Resampling.LANCZOS),(i*400+8,56));d.text((i*400+8,12),name,font=f,fill=(245,243,232))
comp.save(Q/'R003_light_comparison_v002.png')
out.resize((512,768),Image.Resampling.LANCZOS).save(Q/'R003_light_preview_v002.png')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
a=np.array(out.getchannel('A'));y,x=np.where(a>0);assert a.min()==0 and a.max()>0;assert min(x.min()/1024,y.min()/1536,(1023-x.max())/1024,(1535-y.max())/1536)>=.049
meta={'task':'A07-PILOT','stage':'Light-element character color design','status':'actual RGBA trial ready; visual review pending; not Unity integrated','tool':'image_gen.imagegen built-in edit','tool_model_seed_cfg':'not exposed / not applicable; no local LoRA inference for this edit','transparent_background':True,'reference':'R003_character_internal_v001.png','reference_sha256':sha(R/'R003_character_internal_v001.png'),'prompt':'source/R003_light_v002_prompt.txt','source_sha256':sha(src),'export_sha256':sha(R/'R003_character_light_v002.png'),'dimensions':[1024,1536],'mode':out.mode,'alpha_range':[int(a.min()),int(a.max())],'transparent_pixels':int((a==0).sum()),'bounds':[int(x.min()),int(y.min()),int(x.max()+1),int(y.max()+1)],'light_palette_targets':{'ivory':'#F0DFA8','pale_gold':'#C6A15D','warm_white':'#FFF4D0'},'design_changes':['ivory outer cloak, dark lining, slim pale-gold hem','small four-ray sun emblem and narrow ivory/gold stripe on shield','warm-white crystal/lantern accents instead of orange'],'export_operation':'whole source canvas scaled to922x1382 and centered at51,77; native alpha preserved; no segmentation/recolor/posterization','face_crop_top_left':list(face),'face_uv_bottom_left':[496/1024,1-296/1536],'checks':['RGBA and 5%+ outer transparent margins','black/white/gray actual composites','ZIP CRC'],'limits':['Generative edit is not pixel-identical to approved reference; slight face/fold changes require visual review','HEX values guide color family, not exact rendered pixel equality','Existing Unity element icons and rarity frames unchanged']}
(R/'production_light_v002.json').write_text(json.dumps(meta,ensure_ascii=False,indent=2),encoding='utf-8')
with zipfile.ZipFile(R/'A07_R003_LightElement_v002.zip','w',zipfile.ZIP_DEFLATED) as z:
 files=[R/'R003_character_light_v002.png',R/'R003_thumb_light_v002.png',R/'R003_avatar_light_v002.png',R/'production_light_v002.json',src,R/'source/R003_light_v002_prompt.txt',Path(__file__),ROOT/'ArtSource/Metadata/六元素角色配色_v001.md']+list(Q.glob('R003_light_*v002.png'))
 for p in files:z.write(p,p.relative_to(R).as_posix() if p.is_relative_to(R) else 'Design/'+p.name)
with zipfile.ZipFile(R/'A07_R003_LightElement_v002.zip') as z:assert z.testzip() is None
print(json.dumps({'RGBA':out.mode,'alpha_range':meta['alpha_range'],'bounds':meta['bounds'],'zip_crc':'PASS'}))
