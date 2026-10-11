from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import numpy as np,json,hashlib,zipfile
R=Path(__file__).resolve().parent.parent;ROOT=R.parents[2];Q=R/'QA';Q.mkdir(exist_ok=True)
src=R/'source/InternalTransparent_v001/R003_original_tool_output.png';im=Image.open(src)
assert im.mode=='RGBA' and im.size==(1024,1536)
# Technical whole-canvas export: preserve aspect ratio and generated alpha.
# No painting, segmentation, recoloring, or silhouette tightening.
out=Image.new('RGBA',(1024,1536));out.alpha_composite(im.resize((922,1382),Image.Resampling.LANCZOS),(51,77))
dest=R/'R003_character_internal_v001.png';out.save(dest)
out.resize((256,384),Image.Resampling.LANCZOS).save(R/'R003_thumb_internal_v001.png')
face=(358,166,588,396);out.crop(face).resize((256,256),Image.Resampling.LANCZOS).save(R/'R003_avatar_internal_v001.png')
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',23);sheet=Image.new('RGB',(1080,596),(30,33,42));d=ImageDraw.Draw(sheet)
for i,(name,color) in enumerate([('BLACK',(0,0,0)),('WHITE',(255,255,255)),('GRAY',(128,128,128))]):
    bg=Image.new('RGBA',out.size,(*color,255));bg.alpha_composite(out);bg.convert('RGB').save(Q/f'R003_internal_{name.lower()}_v001.png');sheet.paste(bg.convert('RGB').resize((360,540),Image.Resampling.LANCZOS),(i*360,48));d.text((i*360+15,12),name,font=font,fill=(235,240,248))
sheet.save(Q/'R003_internal_alpha_QA_v001.png')
out.resize((512,768),Image.Resampling.LANCZOS).save(Q/'R003_internal_preview_v001.png')
out.crop(face).save(Q/'R003_internal_face_100_v001.png')
a=np.array(out.getchannel('A'));ys,xs=np.where(a>0);vy,vx=np.where(a>=16)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
cx=(face[0]+face[2])/2;cy=(face[1]+face[3])/2
record={'task':'A07-PILOT','stage':'Internal image tool transparent trial from user-approved R style anchor03','status':'actual transparent asset produced; visual acceptance pending; not Unity integrated','approved_reference':'ArtSource/Characters/R003/source/R003_candidate_03_v001.png','reference_sha256':sha(R/'source/R003_candidate_03_v001.png'),'tool':'image_gen.imagegen built-in','tool_model_version':'not exposed','seed_cfg_sampler':'not applicable / not exposed; no fabricated Anima parameters','transparent_background':True,'prompt':'source/R003_internal_transparent_v001_prompt.txt','original_tool_output':str(src),'original_sha256':sha(src),'export':str(dest),'export_sha256':sha(dest),'dimensions':[1024,1536],'mode':out.mode,'alpha_range':[int(a.min()),int(a.max())],'fully_transparent_pixels':int((a==0).sum()),'alpha_nonzero_bounds':[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)],'visible_alpha16_bounds':[int(vx.min()),int(vy.min()),int(vx.max()+1),int(vy.max()+1)],'transparent_outer_padding_pixels':[int(xs.min()),int(ys.min()),int(1023-xs.max()),int(1535-ys.max())],'export_operations':['resample ENTIRE generated canvas 1024x1536 to922x1382 preserving aspect within integer rounding','center on transparent1024x1536 at51,77','preserve native generated alpha; no SAM, background thresholding or semantic Python edit'],'avatar_crop_top_left':list(face),'face_center_pixels_top_left':[cx,cy],'face_uv_bottom_left':[cx/1024,1-cy/1536],'thumbnail_size':[256,384],'avatar_size':[256,256],'style_anchor':'User explicitly chose candidate03 for modest R-rarity rendering/design; candidate04 is not the selected anchor.','source_limit':'Reference-preserving generative reconstruction, not pixel-identical segmentation; face/design need visual acceptance.','validation':['RGBA alpha contains real0 and nonzero values','All four margins contain at least5% transparent canvas','Black/white/gray actual alpha composites created','Original source and tool output kept unchanged','No Unity asset/code/scene modified']}
assert a.min()==0 and a.max()>0
assert min(record['transparent_outer_padding_pixels'][0]/1024,record['transparent_outer_padding_pixels'][1]/1536,record['transparent_outer_padding_pixels'][2]/1024,record['transparent_outer_padding_pixels'][3]/1536)>=.049
(R/'production_internal_v001.json').write_text(json.dumps(record,ensure_ascii=False,indent=2),encoding='utf-8')
with zipfile.ZipFile(R/'A07_R003_InternalTransparent_v001.zip','w',zipfile.ZIP_DEFLATED) as z:
    files=[dest,R/'R003_thumb_internal_v001.png',R/'R003_avatar_internal_v001.png',R/'production_internal_v001.json',R/'交接_内置透明v001.md',src,R/'source/R003_internal_transparent_v001_prompt.txt',R/'source/R003_candidate_03_v001.png',Path(__file__)]
    files+=list(Q.glob('R003_internal_*v001.png'))
    for p in files:z.write(p,p.relative_to(R).as_posix())
with zipfile.ZipFile(R/'A07_R003_InternalTransparent_v001.zip') as z:assert z.testzip() is None
print(json.dumps({'alpha_range':record['alpha_range'],'transparent_pixels':record['fully_transparent_pixels'],'bounds':record['alpha_nonzero_bounds'],'face_uv':record['face_uv_bottom_left'],'package_crc':'PASS'},ensure_ascii=False))
