import csv,hashlib,json
from pathlib import Path
from PIL import Image
from run_style_tests import ROOT,save
from run_rpg_professions import PROFILES

observations={
 '01_ice_mage':{'strengths':['尖帽、雪花杖和冰晶使职业/元素清楚','露肩、开衩和屈膝姿态保留稀有卡魅力'],
  'defects':['法杖握持和右侧张开手需要正式图细检','帽沿与法杖距画幅边缘较近']},
 '02_light_priest':{'strengths':['白金服饰、百合与日轮杖形成光牧师身份','偏温柔的表情和施法手势与其他职业区分'],
  'defects':['实际姿态更像坐/跪而非转身伸手','权杖的实际握持关系不明确','背景较亮，正式页应检查脸部对比']},
 '03_wind_elf_archer':{'strengths':['长尖耳、浅绿发、皮甲和箭袋易于认出精灵游侠','v002消除v001不成立的拉弦/箭杆交互'],
  'defects':['长弓仍有画幅裁切，正式母版需留出完整道具空间','实际是持弓展示而非明确的奔跑动作']},
 '04_fire_warrior':{'strengths':['红马尾、轻甲、宽刃剑与熔岩形成明确火战士特征','屈膝前冲和持剑姿态较有动作感'],
  'defects':['剑刃末端被画幅裁切','双手交叠握剑需高分辨率复核']},
 '05_thunder_miko':{'strengths':['红白服饰、御币、鸟居和紫雷让职业题材清楚','明快表情和抬腿舞步呈现年轻成人气质'],
  'defects':['符札出现伪文字，正式图应去除或换为原创非文字纹样','狐狸面饰与发结未充分实现','符札手指需精修']},
 '06_star_magical_girl':{'strengths':['粉色双马尾、星胸针、飘带和星杖形成清楚魔法少女风格','伸手与倾斜构图有亲和力和动势'],
  'defects':['星杖实际握持不明确','伸向镜头的手部与近大远小透视需正式图检查']}
}
folder=ROOT/'StyleTests'/'RPG_Professions'
rows=[]
for profile in PROFILES:
    key=profile['key']
    version='v002' if key=='03_wind_elf_archer' else 'v001'
    task='A02_RPG_'+key+('_v002' if version=='v002' else '')
    path=folder/(key+'_'+version+'.png')
    with Image.open(path) as picture:
        assert picture.size==(768,1152)
        mode=picture.mode
    record_path=ROOT/'Metadata'/(task+'_run.json')
    record=json.loads(record_path.read_text(encoding='utf-8'))
    digest=hashlib.sha256(path.read_bytes()).hexdigest()
    assert digest==record['sha256']
    record.update(status='concept_pending_user_selection',profession=profile['title'],
        element=profile['element'],declared_character_age=profile['age'],
        visual_review=observations[key],layers='complete_image_only',unity_validation='not_integrated',
        model_hashes='Metadata/A01_B_model_hashes.json',
        style_chain='User PNG workflow; same model/LoRA/sampler settings; profession prompt and seed changed; tiled decode')
    save(record_path,record)
    thumb=folder/(key+'_'+version+'_thumb.png')
    with Image.open(path) as picture:
        picture.convert('RGB').resize((256,384),Image.Resampling.LANCZOS).save(thumb)
    rows.append({'assetId':'style_rpg_'+key,'profession':profile['title'],'element':profile['element'],
        'declaredAge':profile['age'],'image':str(path.relative_to(ROOT.parent)),
        'thumbnail':str(thumb.relative_to(ROOT.parent)),'width':768,'height':1152,'alpha':'false',
        'status':'concept_pending_user_selection','prompt_id':record['submission']['prompt_id'],
        'elapsed_seconds':record['elapsed_seconds'],'sha256':digest,'source_mode':mode})
rejected=ROOT/'Metadata'/'A02_RPG_03_wind_elf_archer_run.json'
record=json.loads(rejected.read_text(encoding='utf-8'))
record.update(status='superseded_prop_structure',visual_review={'defects':['箭杆与弓弦关系不成立'],'replacement':'03_wind_elf_archer_v002.png'})
save(rejected,record)
save(ROOT/'Metadata'/'A02_RPG_asset_manifest.json',rows)
with (ROOT/'Metadata'/'A02_RPG_asset_manifest.csv').open('w',encoding='utf-8-sig',newline='') as stream:
    writer=csv.DictWriter(stream,fieldnames=list(rows[0]))
    writer.writeheader()
    writer.writerows(rows)
print('Verified six PNG hashes and dimensions; exported six derived thumbnails and concept manifest.')
