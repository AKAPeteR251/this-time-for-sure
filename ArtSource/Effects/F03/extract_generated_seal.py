from pathlib import Path
import json,hashlib
import numpy as np,torch
from PIL import Image,ImageFilter
from segment_anything import sam_model_registry,SamPredictor
BASE=Path(__file__).resolve().parent;ROOT=BASE.parents[2]
source=BASE/'LocalGenerationSmoke/star_fullimage_v001.png'
weight=Path('E:/ComfyUI-aki-v3/ComfyUI-aki-v3/ComfyUI/models/sams/sam_vit_b_01ec64.pth')
im=Image.open(source).convert('RGB')
model=sam_model_registry['vit_b'](checkpoint=str(weight)).to('cuda' if torch.cuda.is_available() else 'cpu')
predictor=SamPredictor(model);predictor.set_image(np.array(im))
with torch.inference_mode():
    masks,scores,_=predictor.predict(point_coords=np.array([[256,256],[126,273],[368,279]]),point_labels=np.array([1,1,1]),box=np.array([59,65,453,449]),multimask_output=True)
index=int(np.argmax(scores));alpha=Image.fromarray(masks[index].astype('uint8')*255).filter(ImageFilter.GaussianBlur(.45))
rgba=im.convert('RGBA');rgba.putalpha(alpha)
out=ROOT/'这次一定出/Assets/_Game/Art/Effects/F03/star_seal_anima_v001.png';rgba.save(out)
bg=Image.new('RGBA',rgba.size,(17,28,49,255));bg.alpha_composite(rgba);bg.convert('RGB').save(BASE/'QA/star_seal_anima_alpha_v001.jpg',quality=94)
meta={'path':str(out.relative_to(ROOT)).replace('\\','/'),'source':str(source.relative_to(ROOT)).replace('\\','/'),'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'size':im.size,'method':'local Anima generation, local SAM vit_b mask, 0.45px mask antialias','scores':scores.tolist(),'selected':index,'real_alpha_extrema':alpha.getextrema(),'zero_alpha_pixels':alpha.histogram()[0],'status':'pending_visual_review'}
(BASE/'A05_anima_seal_metadata.json').write_text(json.dumps(meta,ensure_ascii=False,indent=2),encoding='utf-8');print('Generated gold seal extracted locally')
