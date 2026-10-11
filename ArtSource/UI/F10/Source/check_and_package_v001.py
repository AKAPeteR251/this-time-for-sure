from pathlib import Path
from PIL import Image
import numpy as np, json, zipfile, hashlib, io, xml.etree.ElementTree as ET

root=Path(__file__).resolve().parent.parent
checks=[]
def check(label, condition):
    if not condition: raise AssertionError(label)
    checks.append({'check':label,'result':'PASS'})
spec={}
for rank in ['R','SR','SSR','UR']:
    spec[f'card_frame_{rank}_v001.png']=(1024,1536)
    spec[f'card_rarity_{rank}_v001.png']=(512,512)
spec.update({'card_new_border_mask_v001.png':(512,192),'card_new_fill_v001.png':(512,192),'card_name_shade_v001.png':(1024,256),'card_name_ornament_v001.png':(1024,256)})
check('Exactly 12 expected runtime textures',set(p.name for p in (root/'Textures').glob('*.png'))==set(spec))
arrays={}
for name,size in spec.items():
    im=Image.open(root/'Textures'/name); a=np.array(im); arrays[name]=a
    check(name+' RGBA and correct size', im.mode=='RGBA' and im.size==size)
    check(name+' transparent outside and nonempty layer',a[:,:,3].min()==0 and a[:,:,3].max()>0 and not np.any(a[0,:,3]) and not np.any(a[-1,:,3]) and not np.any(a[:,0,3]) and not np.any(a[:,-1,3]))
    check(name+' transparent pixels contain no hidden RGB',not np.any(a[:,:,:3][a[:,:,3]==0]))
base=arrays['card_frame_R_v001.png'][:,:,3]
for rank in ['R','SR','SSR','UR']:
    a=arrays[f'card_frame_{rank}_v001.png']
    check(rank+' exact empty inner window',not np.any(a[64:1472,48:976]))
    check(rank+' identical frame alpha geometry',np.array_equal(base,a[:,:,3]))
mask=arrays['card_new_border_mask_v001.png']
check('NEW mask grayscale RGB',np.array_equal(mask[:,:,0],mask[:,:,1]) and np.array_equal(mask[:,:,0],mask[:,:,2]))
check('NEW mask and fill same canvas',mask.shape==arrays['card_new_fill_v001.png'].shape)
check('NEW fill translucent',arrays['card_new_fill_v001.png'][:,:,3].max()<255)
check('Name ornament leaves mapped character-name area empty',not np.any(arrays['card_name_ornament_v001.png'][118:256,239:963,3]))
check('Name shade fades continuously from transparent top',not np.any(arrays['card_name_shade_v001.png'][:8,:,3]) and arrays['card_name_shade_v001.png'][210,512,3]>180)
for rank in ['R','SR','SSR','UR']:
    with zipfile.ZipFile(root/'Source'/f'card_{rank}_layers_v001.ora') as z:
        check(rank+' ORA archive integrity',z.testzip() is None)
        stack=ET.fromstring(z.read('stack.xml'));layers=stack.findall('./stack/layer')
        check(rank+' ten independent editable layers',len(layers)==10)
        im=Image.new('RGBA',(1024,1536))
        for layer in reversed(layers):
            layer_im=Image.open(io.BytesIO(z.read(layer.attrib['src']))).convert('RGBA')
            check(rank+' layer '+layer.attrib['name']+' aligned to full canvas',layer_im.size==(1024,1536))
            im.alpha_composite(layer_im)
        expected=np.array(Image.open(root/'Preview'/f'card_{rank}_full_v001.png'))
        check(rank+' ORA recomposition exactly matches delivered preview',np.array_equal(np.array(im),expected))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
workspace=root.parents[2]
check('Preview character identical to existing original',sha(root/'Source/PreviewInputs/thunder_miko_original.png')==sha(workspace/'ArtSource/StyleTests/RPG_Professions/05_thunder_miko_v001.png'))
check('Preview icon identical to existing runtime icon',sha(root/'Source/PreviewInputs/element_thunder_existing.png')==sha(workspace/'这次一定出/Assets/_Game/Art/Effects/UIAssets/element_thunder_v001.png'))
report={'task':'F10-CARD-UI','status':'Delivered; visual and Unity integration acceptance pending','checks':checks,'manual_visual_review':{'small_cards':'Viewed actual 180x270 and 300x450 comparisons; rank, NEW and character name distinguishable; no red sticker or solid black badge','runtime_content':'Procedural geometry and exact fixed rank glyphs only; character/name/NEW text and icon confined to preview-only ORA layers'},'limits':['No Unity import or Main runtime validation performed','ORA verified by archive parsing and exact layer recomposition; external editor opening not tested','UR and NEW animation require later Unity shader implementation']}
(root/'checks.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
files=sorted(p for p in root.rglob('*') if p.is_file() and p.suffix!='.zip' and p.name!='SHA256SUMS.txt')
(root/'SHA256SUMS.txt').write_text('\n'.join(sha(p)+'  '+p.relative_to(root).as_posix() for p in files)+'\n',encoding='utf-8')
package=root/'F10_CharacterCardUI_v001.zip'
with zipfile.ZipFile(package,'w',zipfile.ZIP_DEFLATED) as z:
    for p in sorted(root.rglob('*')):
        if p.is_file() and p.suffix!='.zip': z.write(p,p.relative_to(root).as_posix())
with zipfile.ZipFile(package) as z:check('Delivery ZIP integrity',z.testzip() is None)
print(json.dumps({'checks_passed':len(checks),'zip':str(package),'zip_bytes':package.stat().st_size,'sha256':sha(package)},ensure_ascii=False))
