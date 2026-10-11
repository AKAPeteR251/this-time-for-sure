from pathlib import Path
import json,time,urllib.request,urllib.parse,hashlib
BASE=Path(__file__).resolve().parent;ROOT=BASE.parents[2]
def call(route,data=None):
    req=urllib.request.Request('http://127.0.0.1:8188'+route,data=json.dumps(data).encode() if data else None,headers={'Content-Type':'application/json'})
    return json.load(urllib.request.urlopen(req,timeout=30))
graph=json.loads((ROOT/'ArtSource/Workflows/user_00348_prompt.json').read_text(encoding='utf-8'))
graph['2']['inputs'].update(width=512,height=512,batch_size=1)
graph['4']['inputs']['text']='A single ornate antique gold celestial star seal, precisely centered symmetrical eight point star enclosed in concentric brass circles and tiny diamonds, premium fantasy anime game prop icon, sharp elegant linework, clean cel shading, isolated on plain white background, no characters, no letters, no words, no logo, no watermark, no text, generous empty padding.'
graph['5']['inputs']['text']='person, girl, character, face, text, lettering, signature, watermark, logo, clutter, multiple objects, cropped, blurry'
graph['10']['inputs']['seed']=71008051;graph['12']['inputs']['filename_prefix']='A05_capability_star'
out=BASE/'LocalGenerationSmoke';out.mkdir(exist_ok=True)
info=call('/object_info')
missing=[n['class_type'] for n in graph.values() if n['class_type'] not in info]
if missing:raise RuntimeError(missing)
(out/'api.json').write_text(json.dumps(graph,indent=2),encoding='utf-8')
start=time.time();submitted=call('/prompt',{'prompt':graph,'client_id':'A05-art-local'});pid=submitted['prompt_id']
(out/'submission.json').write_text(json.dumps(submitted),encoding='utf-8');print('submitted '+pid,flush=True)
while time.time()-start<600:
    history=call('/history/'+pid)
    if pid in history:
        result=history[pid];(out/'history.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
        if result.get('status',{}).get('status_str')=='error':raise RuntimeError(result['status'])
        images=[im for o in result.get('outputs',{}).values() for im in o.get('images',[])]
        if images:
            data=urllib.request.urlopen('http://127.0.0.1:8188/view?'+urllib.parse.urlencode(images[0]),timeout=30).read()
            (out/'star_fullimage_v001.png').write_bytes(data)
            (out/'result.json').write_text(json.dumps({'prompt_id':pid,'seconds':round(time.time()-start,2),'sha256':hashlib.sha256(data).hexdigest(),'purpose':'local generation capability test, not alpha production asset'},indent=2),encoding='utf-8')
            print('local generation complete',flush=True);break
    time.sleep(3)
else:raise TimeoutError(pid)
