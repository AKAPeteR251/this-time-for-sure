from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
from run_style_tests import ROOT
from run_rpg_professions import PROFILES
folder=ROOT/'StyleTests'/'RPG_Professions'
sheet=Image.new('RGB',(1200,1376),'#111723')
draw=ImageDraw.Draw(sheet)
font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',24)
small=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',16)
draw.text((16,12),'传统 RPG 职业试制 · 动态 / 魅力 / 属性识别',font=font,fill='#eae5d9')
for index,profile in enumerate(PROFILES):
    repaired=folder/(profile['key']+'_v002.png')
    path=repaired if repaired.exists() else folder/(profile['key']+'_v001.png')
    if not path.exists(): raise RuntimeError('Missing candidate '+str(path))
    with Image.open(path) as source:
        assert source.size==(768,1152),source.size
        preview=source.convert('RGB').resize((384,576),Image.Resampling.LANCZOS)
    x=16+(index%3)*400
    y=56+(index//3)*640
    sheet.paste(preview,(x,y))
    draw.text((x,y+586),str(index+1)+' '+profile['title']+' · '+str(profile['age'])+'岁',font=font,fill='#eae5d9')
    draw.text((x,y+619),profile['identity'],font=small,fill='#99adc4')
draw.text((16,1344),'768×1152 职业选型稿；尚未分层或接入 Unity；普通卡的朴素版本后续另定。',font=small,fill='#99adc4')
sheet.save(folder/'RPG_professions_contact_v001.png')
print(folder/'RPG_professions_contact_v001.png')
