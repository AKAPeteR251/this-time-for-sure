from pathlib import Path
from PIL import Image
import sys,copy,time,json,requests,uuid
ROOT=Path(__file__).resolve().parents[4];S=Path(__file__).resolve().parent;BASE=S.parent
sys.path.insert(0,str(ROOT/'ArtSource/StyleTests/A08_LORA'))
from run_a08_v001 import get,save,sha,ui_graph,HOST
POSITIVE="One original adult 27-year-old female anime fantasy RPG shield guardian, full-body portrait, clean fine anime linework, precise cel shading with restrained soft gradients, luminous expressive eyes, clean skin colors, carefully constructed costume, crisp readable silhouette, beautifully rendered silver armor, cloth and matte stockings. Deep brown short bob haircut, gray-green eyes, a slightly stubborn embarrassed small smile with protective warm eye contact. Silver-gray protective breastplate covering her chest, thick shoulder armor and forearm guards, dark green short cape, fitted armored short skirt, narrow clearly shaped cutout at one side of the waist, a little exposed skin at upper shoulders. Opaque charcoal-gray matte pantyhose fully covering both legs continuously from the skirt to the boots, metal kneepads and short silver metal armored boots, no bare thigh gap. One broad sturdy shield held in her left arm in front of one side of the body, a small warm amber lantern attached to the shield rim, a short sword sheathed at her right hip with her right hand resting naturally near the hilt. Three-quarter body angle, one foot slightly forward, stable balanced standing stance, shoulders subtly turned toward viewer, clear face and both complete hands. Her shield and armor have a substantial protective silhouette, her face remains unobscured. Portrait 2:3, complete character and complete shield inside frame, complete head, boots, cape and sword, character fills only 80 to 85 percent of canvas height, 5-8 percent outer margin on all four sides, face in upper portion. Plain uniform mid-gray background for later extraction. No scenery, furniture, floor, ground shadow, floating particles, lighting effects outside the lantern, lettering, logo, watermark, card border or UI. Keep eyes and costume readable at small size."
NEGATIVE="low quality, blurry, muddy colors, photorealistic, 3D render, chibi, extra character, extra limbs, malformed hands, extra fingers, broken wrist, distorted shield, broken lantern, cropped head, cropped feet, cropped weapon, face hidden, dense effects, background scenery, ground shadow, text, logo, watermark, signature, card frame, drawn checkerboard, incomplete clothing construction, bare thighs, thigh-high stockings, fishnet, transparent legwear, bikini armor, nude, child, teen, underage, long hair, witch hat, magic staff"
def main(index):
    global POSITIVE,NEGATIVE
    if index>=2:
        POSITIVE="masterpiece, best quality, solo, exactly one adult 27-year-old female shield knight, ONE full body figure only, isolated character on a completely uniform solid neutral mid-gray background. No second image of the character anywhere. Clean anime linework and precise cel shading. Short dark brown bob hair, clearly gray-green eyes, a small embarrassed warm smile. Silver-gray protective chest armor, shoulder plates and armored gloves, short dark green cape. Fitted waist and armored short skirt; a clearly visible narrow triangular bare-skin cutout on one side of the waist between chest armor and skirt. Small exposed upper shoulder areas. Opaque charcoal matte pantyhose continuously covering both legs with NO bare thigh gaps. Small separate metal knee guards, SHORT ANKLE BOOTS ending below the calves, not tall boots. A broad sturdy silver rectangular shield held firmly by her left armored hand, and one SMALL amber lantern ATTACHED TO THE SHIELD RIM. A sheathed short sword at her opposite hip; other hand resting on its hilt. Stable natural three-quarter standing stance with one foot forward, shoulders gently turned, clear unobscured face. Full complete head, full cape, both complete boots and entire shield visible. Character occupies 80 percent of vertical 2:3 canvas height; generous EMPTY mid-gray margin above her head and below boots and at both sides. Flat uniform mid-gray background, no floor, no cast shadow, no particles, no aura. One coherent full-body character sprite illustration, no character sheet or background portrait, no text or UI."
        NEGATIVE+", reference sheet, character sheet, design sheet, multiple views, inset portrait, large face in background, giant face, ghost portrait, duplicate character, second body, collage, montage, reflections, white background, gradients, floor, cast shadow, sparks, embers, particles, long boots, tall boots, knee-high boots, lantern hanging from belt"
    queue=get('/queue')
    if queue['queue_running'] or queue['queue_pending']:raise RuntimeError('Existing work in queue; no submission or interruption')
    info=get('/object_info');api=json.loads((ROOT/'ArtSource/Workflows/A02_RPG_01_ice_mage_api.json').read_text(encoding='utf-8'))
    api['4']['inputs']['text']=POSITIVE;api['5']['inputs']['text']=NEGATIVE
    api['10']['inputs']['seed']=71011060+index
    api['12']['inputs']['filename_prefix']=f'A07_R003_candidate_{index:02d}_v001'
    choices=info['LoraLoader']['input']['required']['lora_name'][0]
    for nid,filename,prev in [('20','anima_context_detailer_base10.safetensors','17'),('21','reina_v1_epoch14.safetensors','20')]:
        name=next(n for n in choices if n.replace('\\','/').endswith('/'+filename))
        api[nid]={'class_type':'LoraLoader','inputs':{'lora_name':name,'strength_model':.6,'strength_clip':0,'model':[prev,0],'clip':[prev,1]},'_meta':{'title':'A07 user selected '+filename+' model .60 / clip 0'}}
    api['9']['inputs']['model']=['21',0];api['4']['inputs']['clip']=['21',1];api['5']['inputs']['clip']=['21',1]
    if not set(n['class_type'] for n in api.values()).issubset(info):raise RuntimeError('Required nodes missing')
    ui=ui_graph(api,info)
    # ClownsharK's seed widget has its own fixed control after seed.
    for node in ui['nodes']:
        if node['type']=='ClownsharKSampler_Beta':
            keys=[k for k,v in api['10']['inputs'].items() if not isinstance(v,list)]
            node['widgets_values'].insert(keys.index('seed')+1,'fixed')
    graph_path=ROOT/'ArtSource/Workflows'/f'A07_R003_candidate_{index:02d}_api.json';save(graph_path,api);save(graph_path.with_name(graph_path.name.replace('_api','_ui')),ui)
    prompt_source=S/f'candidate_{index:02d}_prompts.json';save(prompt_source,{'positive':POSITIVE,'negative':NEGATIVE,'seed':api['10']['inputs']['seed'],'user_selected_new_lora_strengths':[.6,.6],'new_clip_strengths':[0,0],'style_reference_mode':'visual comparison only; no image conditioning in local t2i graph'})
    outfile=S/f'R003_candidate_{index:02d}_v001.png'
    if outfile.exists():raise FileExistsError('Original candidate already exists; choose a new version')
    started=time.time();r=requests.post(HOST+'/prompt',json={'prompt':api,'client_id':str(uuid.uuid4()),'extra_data':{'extra_pnginfo':{'workflow':ui}}},timeout=30);r.raise_for_status();sub=r.json();pid=sub['prompt_id']
    run={'task':'A07-PILOT','stage':'candidate','index':index,'prompt_id':pid,'submission':sub,'started_unix':started,'api':str(graph_path),'status':'submitted','positive':POSITIVE,'negative':NEGATIVE}
    record=ROOT/'ArtSource/Metadata'/f'A07_R003_candidate_{index:02d}_run.json';save(record,run);print('Submitted R003 '+str(index)+' '+pid,flush=True)
    while True:
        h=get('/history/'+pid)
        if pid in h and (h[pid]['status']['completed'] or h[pid]['status']['status_str']=='error'):h=h[pid];break
        if time.time()-started>1800:raise TimeoutError('No completion within 30min; existing prompt not interrupted')
        time.sleep(3)
    save(ROOT/'ArtSource/Metadata'/f'A07_R003_candidate_{index:02d}_history.json',h)
    images=h.get('outputs',{}).get('12',{}).get('images',[])
    if not images:run.update(status='failed',history_status=h['status']);save(record,run);raise RuntimeError('No generated output; see history')
    r=requests.get(HOST+'/view',params=images[0],timeout=60);r.raise_for_status();outfile.write_bytes(r.content)
    im=Image.open(outfile);run.update(status='candidate_generated_pending_design_review',elapsed_seconds=round(time.time()-started,2),sha256=sha(outfile),dimensions=im.size,mode=im.mode,output=str(outfile),model_hashes_reference='ArtSource/StyleTests/A08_LORA/runtime/model_checks.json',transparency='RGB concept only; not yet extracted or rebuilt master')
    save(record,run);print('Completed R003 candidate '+str(index)+' '+str(run['elapsed_seconds'])+' sec',flush=True)
if __name__=='__main__':main(int(sys.argv[1]))
