"""A01-B-SSR: preserve the approved image with low-denoise local refinement."""
import json, pathlib, urllib.request, uuid, shutil
from run_style_tests import ROOT, URL, request, execute, save

source=ROOT/'StyleTests'/'SSR001_dynamic_v001.png'
target=ROOT/'Characters'/'SSR001'/'SSR001_card_base_v001.png'
target.parent.mkdir(parents=True,exist_ok=True)
if target.exists():
    raise RuntimeError('Master v001 already exists; choose a new version instead of overwriting.')
shutil.copyfile(source,target.parent/'SSR001_approved_source_v001.png')

boundary='ArtBoundary'+uuid.uuid4().hex
body=(f'--{boundary}\r\nContent-Disposition: form-data; name="image"; filename="SSR001_approved_source_v001.png"\r\nContent-Type: image/png\r\n\r\n').encode()+source.read_bytes()
for key,value in [('type','input'),('subfolder','zheciyidingchu_art'),('overwrite','false')]:
    body+=(f'\r\n--{boundary}\r\nContent-Disposition: form-data; name="{key}"\r\n\r\n{value}').encode()
body+=f'\r\n--{boundary}--\r\n'.encode()
req=urllib.request.Request(URL+'/upload/image',data=body,headers={'Content-Type':'multipart/form-data; boundary='+boundary})
with urllib.request.urlopen(req,timeout=30) as response:
    upload=json.load(response)
save(ROOT/'Metadata'/'A01_B_SSR_upload.json',upload)
image_name='/'.join(filter(None,[upload.get('subfolder'),upload['name']]))
info=request('/object_info')
graph=json.loads((ROOT/'Workflows'/'A01_R01_dynamic_api.json').read_text(encoding='utf-8'))
graph['18']={'class_type':'LoadImage','inputs':{'image':image_name}}
graph['19']={'class_type':'ImageScale','inputs':{'image':['18',0],'upscale_method':'lanczos','width':1024,'height':1536,'crop':'disabled'}}
graph['2']={'class_type':'VAEEncodeTiled','inputs':{'pixels':['19',0],'vae':['1',0],'tile_size':512,'overlap':64,'temporal_size':64,'temporal_overlap':8}}
graph['8']={'class_type':'VAEDecodeTiled','inputs':{'samples':['10',0],'vae':['1',0],'tile_size':512,'overlap':64,'temporal_size':64,'temporal_overlap':8}}
graph['10']['inputs']['denoise']=0.22
graph['12']['inputs']['filename_prefix']='A01_B_SSR001_master_v001'
save(ROOT/'Metadata'/'A01_B_SSR_changes.json',{
 'task':'A01-B-SSR','approved_direction':'用户：这种可以',
 'reference_source':str(source.relative_to(ROOT)),
 'changes':['reference image input','lanczos resize 1024x1536','tiled VAE encode/decode','sampler denoise 0.22','output prefix'],
 'unchanged':'model, LoRA chain and strengths, prompt, seed, sampler type, CFG, steps, replay node',
 'intent':'Preserve the approved design; this is real img2img refinement, not just a resized master.'})
execute('A01_B_SSR_master',graph,info,target)
