from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib,zipfile,shutil
ROOT=Path(__file__).resolve().parents[4];R=Path(__file__).resolve().parent.parent;Q=R/'QA';Q.mkdir(exist_ok=True)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',25)
old=Image.open(R/'source/R003_candidate_01_v001.png').convert('RGB');new=Image.open(R/'source/R003_candidate_04_v001.png').convert('RGB')
assert old.size==new.size==(768,1152)
sheet=Image.new('RGB',(1024,658),(27,31,41));d=ImageDraw.Draw(sheet)
for x,label,im in [(64,'Earlier trial / 01',old),(576,'Flat cel trial / 04',new)]:
    d.text((x,14),label,font=font,fill=(236,240,247));sheet.paste(im.resize((384,576),Image.Resampling.LANCZOS),(x,64))
sheet.save(Q/'R003_flat_before_after_v002.png');new.resize((512,768),Image.Resampling.LANCZOS).save(Q/'R003_flat_preview_v002.png')
new.crop((294,74,550,330)).save(Q/'R003_flat_face_100_v002.png')
api=ROOT/'ArtSource/Workflows/A07_R003_candidate_04_api.json';graph=json.loads(api.read_text(encoding='utf-8'))
run=json.loads((ROOT/'ArtSource/Metadata/A07_R003_candidate_04_run.json').read_text(encoding='utf-8'));history=json.loads((ROOT/'ArtSource/Metadata/A07_R003_candidate_04_history.json').read_text(encoding='utf-8'))
assert history['status']['status_str']=='success' and history['status']['completed']
assert history['prompt'][2]==graph
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assert sha(R/'source/R003_candidate_04_v001.png')==run['sha256']
params={'task':'A07-PILOT','subtask':'Milo flat cel style revision','style_request':'User rejected oily rendering; now flat colors, hard-edged cel shadows, minimal highlights','status':'candidate ready for visual review; no formal master, alpha extraction or Unity integration','selected_preview_candidate':4,'positive':graph['4']['inputs']['text'],'negative':graph['5']['inputs']['text'],'seed':graph['10']['inputs']['seed'],'sampler':graph['10']['inputs'],'lora_chain':[graph[n]['inputs'] for n in ['6','16','17','20','21']],'generated_dimensions':[768,1152],'generated_mode':Image.open(R/'source/R003_candidate_04_v001.png').mode,'prompt_id':run['prompt_id'],'elapsed_seconds':run['elapsed_seconds'],'source_sha256':run['sha256'],'face_review_crop_top_left':[294,74,550,330],'source':'Local ComfyUI Anima Turbo; no built-in image tool; no portrait reference-conditioning node','style_review':'Hair, cloth and legwear use broad flat fills and hard shadows; metal retains small graphic light shapes. Reduced oily specular appearance relative to trial01.','remaining_design_issues':['Shield nearly touches left canvas boundary; needs safe margin','Exposed upper-thigh gap means continuous pantyhose requirement not yet met','No clearly resolved side-waist skin opening','Source background still has shadow and stray specks; RGB is not a transparent sprite'],'sources_retained':['candidate01','candidate02 recovered from real history after interruption','candidate03 first flat attempt','candidate04 second flat attempt'],'master_rule':'Confirm style/design before formal rebuild; exact observed gaps require correction before alpha delivery'}
(R/'production_flat_v002.json').write_text(json.dumps(params,ensure_ascii=False,indent=2),encoding='utf-8')
package=R/'A07_R003_FlatStyleReview_v002.zip'
with zipfile.ZipFile(package,'w',zipfile.ZIP_DEFLATED) as z:
    for p in sorted(R.rglob('*')):
        if p.is_file() and p.suffix!='.zip':z.write(p,p.relative_to(R).as_posix())
    for i in [3,4]:
        for kind in ['api','ui']:
            p=ROOT/'ArtSource/Workflows'/f'A07_R003_candidate_{i:02d}_{kind}.json';z.write(p,'Workflows/'+p.name)
        for kind in ['run','history']:
            p=ROOT/'ArtSource/Metadata'/f'A07_R003_candidate_{i:02d}_{kind}.json';z.write(p,'Metadata/'+p.name)
with zipfile.ZipFile(package) as z:assert z.testzip() is None
print('Flat-style review exported; original/hash/history verified; package CRC passed.')
