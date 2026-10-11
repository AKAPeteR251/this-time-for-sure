from pathlib import Path
import json,struct
R=Path(__file__).resolve().parent
C=Path('E:/ComfyUI-aki-v3/ComfyUI-aki-v3/ComfyUI/models')
def header(path):
    with path.open('rb') as f:n=struct.unpack('<Q',f.read(8))[0];return json.loads(f.read(n))
model=header(C/'unet/anima_turboV11.safetensors')
mapping={'lora_unet_'+k.removeprefix('model.diffusion_model.').removesuffix('.weight').replace('.','_'):k for k in model if k.startswith('model.diffusion_model.') and k.endswith('.weight')}
reports=[]
for name in ['anima_context_detailer_base10.safetensors','reina_v1_epoch14.safetensors']:
    h=header(C/'loras/anima'/name);checks=[]
    for k in h:
        if not k.endswith('.lora_down.weight'):continue
        prefix=k.removesuffix('.lora_down.weight');target=mapping.get(prefix);up=h.get(prefix+'.lora_up.weight');down=h[k]
        compatible=bool(target and up and len(model[target]['shape'])==2 and down['shape'][1]==model[target]['shape'][1] and up['shape'][0]==model[target]['shape'][0] and down['shape'][0]==up['shape'][1])
        checks.append({'adapter':prefix,'target':target,'shape_compatible':compatible,'target_shape':model[target]['shape'] if target else None,'down':down['shape'],'up':up['shape'] if up else None})
    reports.append({'file':name,'adapters':len(checks),'matched_compatible':sum(x['shape_compatible'] for x in checks),'unmatched_or_incompatible':[x for x in checks if not x['shape_compatible']],'pairs':checks})
(R/'runtime/lora_shape_mapping.json').write_text(json.dumps({'method':'Read real safetensors headers; map model.diffusion_model weight names using local comfy/lora.py generic lora_unet rule, validate matrix dimensions. Combined with actual inference logs and distinct outputs; no separate GPU model loaded.','reports':reports},indent=2),encoding='utf-8')
print([(x['file'],x['adapters'],x['matched_compatible']) for x in reports])
