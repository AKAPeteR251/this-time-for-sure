import hashlib, json
from pathlib import Path
root=Path('E:/ComfyUI-aki-v3/ComfyUI-aki-v3/ComfyUI/models')
target=Path(__file__).resolve().parents[1]/'Metadata'/'A01_B_model_hashes.json'
files=['unet/anima_turboV11.safetensors','clip/anima_baseV10_txt.safetensors',
 'vae/qwenImage_qwenImageVAE.safetensors','loras/anima/Flatline.safetensors',
 'loras/anima/chen-bin_v4.0.safetensors','loras/anima/bakav3-000012.safetensors']
rows=[]
for name in files:
    path=root/name
    digest=hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda:stream.read(4*1024*1024),b''):
            digest.update(chunk)
    rows.append({'relative_model':name,'bytes':path.stat().st_size,'sha256':digest.hexdigest()})
    target.write_text(json.dumps(rows,indent=2),encoding='utf-8')
    print(name,flush=True)
