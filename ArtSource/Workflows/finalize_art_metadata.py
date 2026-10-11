import csv,json
from pathlib import Path
from run_style_tests import ROOT,save
reviews={
 'A01_B_SSR_master_v002':{
  'result':'art_candidate_ready_for_integration',
  'strengths':['保留认可稿的构图、角色身份、服装与月港主题','v001噪点已消除，线条与肤色干净'],
  'remaining':['抬腿支撑与星图仪持握仍需实际欣赏页细检']},
 'A01_B_UR_trial':{
  'result':'style_consistency_trial_pending_user_review',
  'strengths':['同一LoRA链，面部和色块画法接近SSR','黑发金眼、黑金红披风与日蚀主题明确','无图内文字边框或明显噪点'],
  'remaining':['道具生成成小日蚀环，未实现原设定的三片碎星核','动态比SSR克制，UR设计等待用户确认']}
}
for task,review in reviews.items():
    path=ROOT/'Metadata'/f'{task}_run.json'
    record=json.loads(path.read_text(encoding='utf-8'))
    record.update(status='pending_integration_review',visual_review=review,
        model_hashes='Metadata/A01_B_model_hashes.json',
        layers='complete_image_only',unity_validation='pending',
        unity_version='2022.3.53f1c1 target; actual import not verified')
    save(path,record)
path=ROOT/'Metadata'/'A01_R01_dynamic_run.json'
record=json.loads(path.read_text(encoding='utf-8'))
record['visual_review']['status']='user_approved_art_direction'
record['user_feedback']='这种可以'
save(path,record)

manifest=ROOT.parent/'Docs'/'Production'/'asset_manifest.csv'
rows=list(csv.DictReader(manifest.open(encoding='utf-8-sig',newline='')))
fields=list(rows[0])
files={
 'char_ssr001_card':('SSR001','SSR001_card_base_v002.png','完整图图生图细化；v001噪点未交付；待Unity接入验收'),
 'char_ssr001_thumb':('SSR001','SSR001_thumb_v002.png','由v002完整母版派生；待Unity接入验收'),
 'char_ur001_card':('UR001','UR001_card_base_v001.png','同风格UR试制完整图；日蚀环道具；待用户设计确认与Unity验收'),
 'char_ur001_thumb':('UR001','UR001_thumb_v001.png','由UR试制母版派生；待验收')}
for row in rows:
    if row['assetId'] in files:
        cid,name,notes=files[row['assetId']]
        row.update(relativeTarget=f'这次一定出/Assets/_Game/Art/Characters/{cid}/{name}',
            status='pending_review',notes=notes)
    elif row['assetId']=='style_ssr001_dynamic_r01':
        row.update(status='direction_approved',notes='用户：这种可以；仅造型方向确认，非Unity运行验收')
with manifest.open('w',encoding='utf-8',newline='') as stream:
    writer=csv.DictWriter(stream,fieldnames=fields)
    writer.writeheader()
    writer.writerows(rows)
print('Recorded real outputs, user art-direction approval, and four asset rows.')
