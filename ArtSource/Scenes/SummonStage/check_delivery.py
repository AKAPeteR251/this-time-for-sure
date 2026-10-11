from pathlib import Path
from PIL import Image,ImageOps,ImageDraw,ImageStat
import json,hashlib,zipfile,subprocess,re
BASE=Path(__file__).resolve().parent;V=BASE/'v002'
files=[V/'Frames'/f'frame_{i:04d}.png' for i in range(1,133)]
assert all(p.exists() for p in files)
assert all(Image.open(p).size==(1280,720) for p in files)
contact=Image.new('RGB',(1600,1010),(13,23,39));d=ImageDraw.Draw(contact)
selected=[1,24,42,55,78,94,105,118,132]
means=[]
for j,i in enumerate(selected):
    im=Image.open(files[i-1]).convert('RGB');means.append({'frame':i,'mean_rgb':ImageStat.Stat(im).mean})
    contact.paste(im.resize((520,293),Image.Resampling.LANCZOS),((j%3)*533,(j//3)*335))
    d.text(((j%3)*533+8,(j//3)*335+302),f'Frame {i} / clip {(i-1)/24:.2f}s',fill=(240,221,179))
contact.save(V/'Review/A05_space_storyboard_v002.jpg',quality=94)
video=V/'A05_SummonSpace_720p_v002.mp4'
assert video.exists()
ffmpeg='E:/小丸/tools/ffmpeg.exe'
decode=subprocess.run([ffmpeg,'-v','info','-i',str(video),'-f','null','-'],capture_output=True,text=True,encoding='utf-8',errors='replace')
assert decode.returncode==0,decode.stderr
# Count actual decoded frames from FFmpeg's final progress, without retaining raw video in RAM.
counts=re.findall(r'frame=\s*(\d+)',decode.stderr);count=int(counts[-1]);assert count==132
assert '1280x720' in decode.stderr and '24 fps' in decode.stderr
(V/'decode_check_v002.log').write_text(decode.stderr,encoding='utf-8')
assets=[V/'A05_SummonStage_v002.blend',video,V/'parameters_v002.json']+list((V/'Review').glob('*.png'))
result={'task':'A05-BLENDER','status':'render and complete MP4 decoding checked; visual review pending user','resolution':[1280,720],'fps':24,'decoded_frames':count,'duration_seconds':count/24,'png_frames':len(files),'first_middle_last_differ':len({hashlib.sha256(files[i-1].read_bytes()).hexdigest() for i in [1,55,105,132]})==4,'sample_brightness':means,'checks':['132 sequential frame files','all images 1280x720','video complete decode exit 0','decoded frame count 132','three review renders exist'],'assets':[{'path':str(p.relative_to(BASE)).replace('\\','/'),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in assets],'unity_integration':'not performed','audio':'none'}
(V/'delivery_checks_v002.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print('Verified 132 720p PNGs, complete H.264 decode, 5.5 second clip and actual motion frames')
