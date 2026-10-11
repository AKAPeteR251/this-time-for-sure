from pathlib import Path
from PIL import Image
import hashlib,json,zipfile
F=Path(__file__).resolve().parent;ROOT=F.parents[3]
src=F/'source/original_tool_output.png';im=Image.open(src)
assert im.mode=='RGBA' and im.size==(1024,1536)
a=im.getchannel('A');assert a.getextrema()[0]==0
out=Image.new('RGBA',(1024,1536),(0,0,0,0));out.alpha_composite(im.resize((922,1382),Image.Resampling.LANCZOS),(51,77))
out.save(F/'SR003_character_v002.png');thumb=out.resize((256,384),Image.Resampling.LANCZOS);thumb.save(F/'SR003_thumb_v002.png')
r=[427,152,641,366];m=[round(r[0]*922/1024+51),round(r[1]*1382/1536+77),round(r[2]*922/1024+51),round(r[3]*1382/1536+77)]
out.crop(m).resize((256,256),Image.Resampling.LANCZOS).save(F/'SR003_avatar_v002.png')
qa=F/'QA';qa.mkdir(exist_ok=True)
def bg(img,c):
    b=Image.new('RGBA',img.size,c);b.alpha_composite(img);return b.convert('RGB')
pan=Image.new('RGB',(768,384))
for i,c in enumerate(['black','white','#808080']):pan.paste(bg(thumb,c),(256*i,0))
pan.save(qa/'alpha_black_white_gray_v002.jpg',quality=95)
bg(out,'#808080').save(qa/'full_gray_v002.jpg',quality=95)
bg(out.crop((260,650,675,1450)),'#808080').save(qa/'legs_feet_v002.jpg',quality=95)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
rec={'task':'A09-CHARACTERS','id':'SR003','version':'v002','status':'待视觉验收','route':'内置image_gen，两次局部编辑，transparent_background=true','source_reference':'ArtSource/Characters/SR003/Internal_A09_v001/source/original_tool_output.png','reference_sha256':sha(F.parent/'Internal_A09_v001/source/original_tool_output.png'),'source_alpha_range':a.getextrema(),'source_alpha_bbox':a.getbbox(),'export_size':out.size,'export_operation':'完整画布缩放922x1382、透明画布1024x1536放置(51,77)，未抠图','edge_alpha_max':[out.getchannel('A').crop(b).getextrema()[1] for b in [(0,0,1024,1),(0,1535,1024,1536),(0,0,1,1536),(1023,0,1024,1536)]],'avatar_crop':m,'prompt':'source/prompt.txt','model_seed_cfg':'未披露','sha256':{p.relative_to(F).as_posix():sha(p) for p in F.rglob('*') if p.is_file()}}
(F/'production_v002.json').write_text(json.dumps(rec,ensure_ascii=False,indent=2),encoding='utf-8')
zpath=F/'A09_SR003_TabiShoes_v002.zip'
with zipfile.ZipFile(zpath,'w',zipfile.ZIP_DEFLATED) as z:
    for p in F.rglob('*'):
        if p.is_file() and p.suffix!='.zip' and p.name!='package_check_v002.json':z.write(p,p.relative_to(ROOT).as_posix())
with zipfile.ZipFile(zpath) as z:assert z.testzip() is None
check={'zip_crc':'passed','bytes':zpath.stat().st_size,'sha256':sha(zpath),'alpha_range':a.getextrema(),'export_edges':rec['edge_alpha_max']}
(F/'package_check_v002.json').write_text(json.dumps(check,indent=2),encoding='utf-8');print(json.dumps(check))
