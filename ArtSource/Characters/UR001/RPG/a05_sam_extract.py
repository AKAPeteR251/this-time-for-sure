from pathlib import Path
import json,time,hashlib
import numpy as np
import torch
from PIL import Image
from segment_anything import sam_model_registry,SamPredictor
BASE=Path(__file__).resolve().parent;ROOT=BASE.parents[3]
SOURCE=ROOT/'ArtSource/StyleTests/RPG_Professions/04_fire_warrior_v001.png'
WEIGHT=Path('E:/ComfyUI-aki-v3/ComfyUI-aki-v3/ComfyUI/models/sams/sam_vit_b_01ec64.pth')
out=BASE/'SAM_v001';out.mkdir(parents=True,exist_ok=True)
start=time.time();device='cuda' if torch.cuda.is_available() else 'cpu'
model=sam_model_registry['vit_b'](checkpoint=str(WEIGHT)).to(device)
predictor=SamPredictor(model)
im=Image.open(SOURCE).convert('RGB');predictor.set_image(np.array(im))
positive=[[409,277],[420,366],[530,152],[667,284],[728,398],[638,604],[353,495],[353,743],[159,855],[602,975],[607,1100],[331,989],[184,283],[509,737],[682,928]]
negative=[[39,173],[85,427],[708,24],[32,1078],[493,1064],[243,179],[94,346],[673,709]]
with torch.inference_mode():
    masks,scores,logits=predictor.predict(point_coords=np.array(positive+negative),point_labels=np.array([1]*len(positive)+[0]*len(negative)),multimask_output=True)
for i,mask in enumerate(masks):
    a=Image.fromarray(mask.astype('uint8')*255);a.save(out/f'mask_{i}.png')
    rgba=im.convert('RGBA');rgba.putalpha(a);rgba.save(out/f'cutout_{i}.png')
    bg=Image.new('RGBA',im.size,(36,67,88,255));bg.alpha_composite(rgba);bg.convert('RGB').save(out/f'preview_{i}.jpg',quality=93)
meta={'method':'local SAM vit_b point prompted; no repainting','source':str(SOURCE),'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'weight':str(WEIGHT),'weight_sha256':hashlib.sha256(WEIGHT.read_bytes()).hexdigest(),'device':device,'torch':torch.__version__,'points_positive':positive,'points_negative':negative,'scores':scores.tolist(),'areas':[int(m.sum()) for m in masks],'elapsed_seconds':round(time.time()-start,3),'status':'pending_visual_review','original_size':im.size}
(out/'run_metadata.json').write_text(json.dumps(meta,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'scores':meta['scores'],'areas':meta['areas'],'seconds':meta['elapsed_seconds']}))
