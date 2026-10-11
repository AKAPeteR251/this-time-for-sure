"""Arrange generated samples without editing their visual content."""
from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',24)
small=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',17)
sheet=Image.new('RGB',(1248,720),'#111925')
draw=ImageDraw.Draw(sheet)
draw.text((24,14),'SSR001 雾璃 · 同一工作流的三种画法候选',font=font,fill='#e9e4d8')
labels=['A 清晰平涂','B 细线柔影尝试','C 装饰星图']
for i,style in enumerate('abc'):
    refined=ROOT/'StyleTests'/f'SSR001_style_{style}_v002.png'
    path=refined if style=='b' and refined.exists() else ROOT/'StyleTests'/f'SSR001_style_{style}.png'
    with Image.open(path) as source:
        assert source.size==(768,1152),source.size
        preview=source.convert('RGB').resize((384,576),Image.Resampling.LANCZOS)
    sheet.paste(preview,(24+i*408,60))
    draw.text((24+i*408,647),labels[i],font=font,fill='#e9e4d8')
draw.text((24,689),'选型稿 768×1152；未分层、未接入 Unity；选择后制作正式图。',font=small,fill='#99adc4')
sheet.save(ROOT/'StyleTests'/'SSR001_contact_sheet.png')
print(ROOT/'StyleTests'/'SSR001_contact_sheet.png')
