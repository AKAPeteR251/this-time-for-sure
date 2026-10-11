"""A08 isolated sequential ComfyUI comparison; never modifies existing art."""
from pathlib import Path
from PIL import Image
import requests, json, time, hashlib, copy, struct, traceback, uuid
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'ArtSource/StyleTests/A08_LORA'; WF=ROOT/'ArtSource/Workflows'; META=ROOT/'ArtSource/Metadata'
HOST='http://127.0.0.1:8188'
COMFY=Path('E:/ComfyUI-aki-v3/ComfyUI-aki-v3/ComfyUI')
def save(path,obj):path.write_text(json.dumps(obj,ensure_ascii=False,indent=2),encoding='utf-8')
def sha(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
    return h.hexdigest()
def get(route):
    r=requests.get(HOST+route,timeout=30);r.raise_for_status();return r.json()
def ui_graph(api,info):
    """Build real LiteGraph links from node schemas, not a decorative diagram."""
    ui={'last_node_id':max(map(int,api)),'last_link_id':0,'nodes':[],'links':[],'groups':[], 'config':{},'extra':{'ds':{'scale':0.65,'offset':[30,30]}},'version':0.4}
    nodes={};socket_inputs={}
    for order,(nid,item) in enumerate(api.items()):
        schema=info[item['class_type']];outs=schema['output']; names=schema.get('output_name',outs)
        node={'id':int(nid),'type':item['class_type'],'pos':[(order%5)*440,(order//5)*360],'size':[410,320],'flags':{},'order':order,'mode':0,'inputs':[],'outputs':[{'name':names[i],'type':t,'links':[]} for i,t in enumerate(outs)],'properties':{'Node name for S&R':item['class_type']},'widgets_values':[],'title':item.get('_meta',{}).get('title',item['class_type'])}
        spec={**schema['input'].get('required',{}),**schema['input'].get('optional',{})}
        for key,val in item['inputs'].items():
            if isinstance(val,list) and len(val)==2 and str(val[0]) in api:
                kind=spec.get(key,['*'])[0]; kind=kind if isinstance(kind,str) else '*'
                socket_inputs[(nid,key)]=len(node['inputs']);node['inputs'].append({'name':key,'type':kind,'link':None})
            else:
                node['widgets_values'].append(val)
                if item['class_type']=='KSampler' and key=='seed':node['widgets_values'].append('fixed')
        nodes[nid]=node;ui['nodes'].append(node)
    for nid,item in api.items():
        for key,val in item['inputs'].items():
            if (nid,key) not in socket_inputs:continue
            source,slot=str(val[0]),val[1];target_slot=socket_inputs[(nid,key)];lid=len(ui['links'])+1;typ=nodes[source]['outputs'][slot]['type']
            ui['links'].append([lid,int(source),slot,int(nid),target_slot,typ]);nodes[nid]['inputs'][target_slot]['link']=lid;nodes[source]['outputs'][slot]['links'].append(lid)
    ui['last_link_id']=len(ui['links']);return ui
positive="One adult 24-year-old ice sorceress, preserve the source character's exact face, icy blue eyes, long silver-white hair, huge white-and-blue pointed witch hat with snowflake decorations, white and ice-blue off-shoulder fitted slit gown, dark thigh-high stockings and black ankle boots. Preserve the source's seated floating bent-knee pose, complete snowflake staff in one hand and other palm extended, all clothing shapes and the same full-body composition. Clean fine anime linework, precise cel shading with restrained soft gradients, clean skin colors, readable eyes, carefully rendered hair strands, stocking material, fabric folds and crystal facets. Plain mid-gray background, no new scenery or additional accessories. No text, UI, card frame, logo, watermark or separate floating particles."
negative="blurry, muddy colors, excessive microtexture, gritty skin, oversharpening, noise, photorealistic, 3D render, extra character, changed face, changed hairstyle, changed costume, extra limbs, malformed hands, extra fingers, broken wrist, distorted staff, cropped head, cropped feet, cropped weapon, face hidden, dense effects, background scenery, text, logo, watermark, card frame, checkerboard"
groups={'B0':(0,0),'D1':(.30,0),'R1':(0,.30),'DR1':(.30,.30)}
def run():
    for _ in range(90):
        try:stats=get('/system_stats');break
        except requests.RequestException:time.sleep(2)
    else:raise RuntimeError('ComfyUI did not become reachable')
    queue=get('/queue');save(OUT/'runtime/initial_queue.json',queue)
    if queue['queue_running'] or queue['queue_pending']:raise RuntimeError('Existing queue busy; no work submitted or interrupted')
    info=get('/object_info');save(OUT/'runtime/system_stats.json',stats)
    base=json.loads((WF/'A01_B_SSR_master_v002_api.json').read_text(encoding='utf-8'))
    required={n['class_type'] for n in base.values()};missing=required-set(info)
    if missing:raise RuntimeError('Required nodes missing: '+str(missing))
    save(OUT/'runtime/required_node_schemas.json',{k:info[k] for k in required})
    models=[]
    for rel in ['diffusion_models/anima_turboV11.safetensors','text_encoders/anima_baseV10_txt.safetensors','vae/qwenImage_qwenImageVAE.safetensors','loras/anima/Flatline.safetensors','loras/anima/chen-bin_v4.0.safetensors','loras/anima/bakav3-000012.safetensors','loras/anima/anima_context_detailer_base10.safetensors','loras/anima/reina_v1_epoch14.safetensors','sams/sam_vit_b_01ec64.pth']:
        path=COMFY/'models'/rel
        if not path.exists():
            alt=rel.replace('diffusion_models/','unet/').replace('text_encoders/','clip/');path=COMFY/'models'/alt
        if not path.exists():raise FileNotFoundError(path)
        item={'path':str(path),'bytes':path.stat().st_size,'sha256':sha(path)}
        if 'detailer' in rel or 'reina' in rel:
            with path.open('rb') as f: count=struct.unpack('<Q',f.read(8))[0];header=json.loads(f.read(count))
            item.update(metadata=header.get('__metadata__',{}),tensor_count=len(header)-int('__metadata__' in header),tensor_names=[k for k in header if k!='__metadata__'])
        models.append(item)
    save(OUT/'runtime/model_checks.json',models)
    orig=ROOT/'ArtSource/UIAssets/Originals/ice_mage_cutout_v001.png'
    image=Image.open(orig).convert('RGBA');gray=Image.new('RGBA',image.size,(128,128,128,255));gray.alpha_composite(image)
    input_path=OUT/'input/ice_mage_gray_v001.png';gray.convert('RGB').save(input_path)
    save(OUT/'input/input_sources.json',{'original':str(orig),'original_sha256':sha(orig),'test_input_sha256':sha(input_path),'size':image.size,'operation':'straight alpha composite over RGB 128 gray; full original canvas; no crop or stretch','positive':positive,'negative':negative})
    with input_path.open('rb') as f:
        r=requests.post(HOST+'/upload/image',files={'image':('A08_ice_mage_gray_v001.png',f,'image/png')},data={'type':'input','subfolder':'zheciyidingchu_art/A08_LORA','overwrite':'false'},timeout=30);r.raise_for_status();uploaded=r.json()
    save(OUT/'runtime/input_upload.json',uploaded);load_name=(uploaded.get('subfolder','')+'/'+uploaded['name']).lstrip('/')
    loras=info['LoraLoader']['input']['required']['lora_name'][0]
    def lora_name(filename):
        return next(v for v in loras if v.replace('\\','/').endswith('/'+filename))
    for gid,(detailer,reina) in groups.items():
        if (OUT/'outputs'/f'{gid}.png').exists():print(gid+' already exists; retained',flush=True);continue
        api=copy.deepcopy(base);api['4']['inputs']['text']=positive;api['5']['inputs']['text']=negative
        api['10']['inputs']['seed']=71008031;api['18']['inputs']['image']=load_name;api['12']['inputs']['filename_prefix']='A08_LORA_'+gid+'_v001'
        api['20']={'class_type':'LoraLoader','inputs':{'lora_name':lora_name('anima_context_detailer_base10.safetensors'),'strength_model':detailer,'strength_clip':0,'model':['17',0],'clip':['17',1]},'_meta':{'title':'A08 Detailer model only'}}
        api['21']={'class_type':'LoraLoader','inputs':{'lora_name':lora_name('reina_v1_epoch14.safetensors'),'strength_model':reina,'strength_clip':0,'model':['20',0],'clip':['20',1]},'_meta':{'title':'A08 Reina model only'}}
        api['9']['inputs']['model']=['21',0];api['4']['inputs']['clip']=['21',1];api['5']['inputs']['clip']=['21',1]
        ui=ui_graph(api,info);save(WF/f'A08_LORA_{gid}_api.json',api);save(WF/f'A08_LORA_{gid}_ui.json',ui)
        log=OUT/'runtime/comfy_stderr.log';offset=log.stat().st_size if log.exists() else 0
        started=time.time();r=requests.post(HOST+'/prompt',json={'prompt':api,'client_id':str(uuid.uuid4()),'extra_data':{'extra_pnginfo':{'workflow':ui}}},timeout=30)
        submission=r.json();save(META/f'A08_LORA_{gid}_run.json',{'started_unix':started,'submission':submission,'input_sha256':sha(input_path),'detailer':detailer,'reina':reina,'status':'submitted'})
        r.raise_for_status();pid=submission['prompt_id'];print('Submitted '+gid+' '+pid,flush=True)
        while True:
            history=get('/history/'+pid)
            if pid in history:
                h=history[pid]
                if h.get('status',{}).get('completed') or h.get('status',{}).get('status_str')=='error':break
            if time.time()-started>1800:raise TimeoutError(gid+' exceeded 30 minutes; prompt not interrupted')
            time.sleep(3)
        save(META/f'A08_LORA_{gid}_history.json',h)
        log_bytes=log.read_bytes()[offset:] if log.exists() else b'';(OUT/'runtime'/f'{gid}_execution.log').write_bytes(log_bytes)
        record={'task':'A08-LORA','group':gid,'prompt_id':pid,'submission':submission,'elapsed_seconds':round(time.time()-started,2),'input_sha256':sha(input_path),'seed':71008031,'detailer_model':detailer,'reina_model':reina,'new_lora_clip':0,'api':str(WF/f'A08_LORA_{gid}_api.json'),'ui':str(WF/f'A08_LORA_{gid}_ui.json'),'status':h.get('status')}
        outputs=h.get('outputs',{}).get('12',{}).get('images',[])
        if not outputs:save(META/f'A08_LORA_{gid}_run.json',record);raise RuntimeError(gid+' produced no SaveImage output; see history')
        o=outputs[0];r=requests.get(HOST+'/view',params=o,timeout=60);r.raise_for_status();dest=OUT/'outputs'/f'{gid}.png';dest.write_bytes(r.content)
        result=Image.open(dest);record.update(output=str(dest),sha256=sha(dest),dimensions=result.size,mode=result.mode,status='generated_test_only_pending_review',lora_warning_lines=[line for line in log_bytes.decode('utf-8',errors='replace').splitlines() if 'lora' in line.lower() or 'NOT LOADED' in line])
        save(META/f'A08_LORA_{gid}_run.json',record);print('Completed '+gid+' '+str(record['elapsed_seconds'])+' sec',flush=True)
    print('All four groups generated; comparison and visual review still required',flush=True)
if __name__=='__main__':
    try:run()
    except Exception:
        (OUT/'runtime/runner_error.txt').write_text(traceback.format_exc(),encoding='utf-8');raise
