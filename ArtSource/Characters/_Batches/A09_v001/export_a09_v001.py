from pathlib import Path
import json, hashlib, zipfile
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parents[4]
BATCH=Path(__file__).resolve().parent
entries=json.loads((ROOT/'ArtSource/Metadata/A09_CHARACTERS_inputs_full_v001.json').read_text(encoding='utf-8'))
font=ImageFont.truetype(str(ROOT/'ArtSource/UI/F10/Source/Fonts/LXGWWenKai/LXGWWenKai-Regular.ttf'),22)
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def background(im,color):
    bg=Image.new('RGBA',im.size,color);bg.alpha_composite(im);return bg.convert('RGB')
contact=Image.new('RGB',(1280,856),'#30323c');faces=Image.new('RGB',(1280,576),'#30323c')
checks=[]
for i,e in enumerate(entries):
    folder=ROOT/f'ArtSource/Characters/{e["id"]}/Internal_A09_v001'; src=folder/'source/original_tool_output.png'
    im=Image.open(src); assert im.mode=='RGBA', (e['id'],im.mode)
    alpha=im.getchannel('A'); assert alpha.getextrema()[0]==0
    assert im.size==(1024,1536),im.size
    bounds=alpha.getbbox()
    # Preserve entire native canvas; resize and add transparent margins, no segmentation or painting.
    out=Image.new('RGBA',(1024,1536),(0,0,0,0));out.alpha_composite(im.resize((922,1382),Image.Resampling.LANCZOS),(51,77))
    target=folder/f'{e["id"]}_character_v001.png';out.save(target)
    thumb=out.resize((256,384),Image.Resampling.LANCZOS);thumb.save(folder/f'{e["id"]}_thumb_v001.png')
    r=e['face_rect'];mapped=[round(r[0]*922/1024+51),round(r[1]*1382/1536+77),round(r[2]*922/1024+51),round(r[3]*1382/1536+77)]
    avatar=out.crop(mapped).resize((256,256),Image.Resampling.LANCZOS);avatar.save(folder/f'{e["id"]}_avatar_v001.png')
    qa=folder/'QA';qa.mkdir(exist_ok=True)
    panel=Image.new('RGB',(768,384))
    for k,c in enumerate(['#000000','#ffffff','#808080']): panel.paste(background(thumb,c),(256*k,0))
    panel.save(qa/'alpha_black_white_gray_v001.jpg',quality=95)
    background(out,'#808080').save(qa/'full_gray_v001.jpg',quality=93)
    hist=out.getchannel('A').histogram();hist0=alpha.histogram(); b=out.getchannel('A').getbbox()
    rec={'task':'A09-CHARACTERS','status':'待视觉验收','id':e['id'],'name':e['name'],'element':e['element'],'age':e['age'],'role':e['role'],'generation':'内置 image_gen，transparent_background=true；未使用本地 Anima/LoRA','model_seed_cfg':'工具未披露，未记录推测值','source':str(src.relative_to(ROOT)).replace('\\','/'),'source_sha256':sha(src),'native_size':im.size,'native_alpha_range':alpha.getextrema(),'native_alpha_bbox':bounds,'native_zero_alpha_fraction':hist0[0]/sum(hist0),'export_size':out.size,'export_operation':'原生完整画布缩放为 922x1382，放置于 1024x1536 的 (51,77)，未重新抠图、未裁切人物','alpha_range':out.getchannel('A').getextrema(),'alpha_bbox':b,'zero_alpha_fraction':hist[0]/sum(hist),'edge_alpha_max':[out.getchannel('A').crop(box).getextrema()[1] for box in [(0,0,1024,1),(0,1535,1024,1536),(0,0,1,1536),(1023,0,1024,1536)]],'avatar_crop_px':mapped,'face_center_uv_bottom_origin':[(mapped[0]+mapped[2])/2048,1-(mapped[1]+mapped[3])/3072],'references':[{'path':p,'sha256':sha(ROOT/p)} for p in e['references']],'prompt':'source/prompt.txt','output_sha256':sha(target),'review':e.get('review','初版候选，脸、道具和边缘需视觉评审')}
    rec['native_edge_alpha_max']=[alpha.crop(box).getextrema()[1] for box in [(0,0,1024,1),(0,1535,1024,1536),(0,0,1,1536),(1023,0,1024,1536)]]
    (folder/'production_v001.json').write_text(json.dumps(rec,ensure_ascii=False,indent=2),encoding='utf-8');checks.append(rec)
    x=(i%5)*256;y=(i//5)*428;contact.paste(background(thumb,'#60616b'),(x,y));ImageDraw.Draw(contact).text((x+8,y+390),f'{e["id"]} {e["name"]} · {e["element"]}',font=font,fill='white')
    faces.paste(background(avatar,'#808080'),(x,(i//5)*288));ImageDraw.Draw(faces).text((x+8,(i//5)*288+258),e['id']+' '+e['name'],font=font,fill='white')
contact.save(BATCH/'A09_characters_contact_v001.jpg',quality=95);faces.save(BATCH/'A09_avatars_contact_v001.jpg',quality=95)
(BATCH/'checks_v001.json').write_text(json.dumps(checks,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps([{'id':r['id'],'native_bbox':r['native_alpha_bbox'],'native_alpha':r['native_alpha_range'],'native_clear':round(r['native_zero_alpha_fraction'],3),'export_bbox':r['alpha_bbox'],'edges':r['edge_alpha_max']} for r in checks],ensure_ascii=False,indent=2))
