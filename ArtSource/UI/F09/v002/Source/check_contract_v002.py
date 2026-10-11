from pathlib import Path
from PIL import Image
import numpy as np,json,zipfile,hashlib,xml.etree.ElementTree as ET
R=Path(__file__).resolve().parent.parent;checks=[]
expected=['Star Echo','Covenant of the Stars','By this name, I awaken the echoes','that slumber among the stars.','Let this covenant be sealed,','with the stars as witness.','Sign your name']
j=json.loads((R/'Source/layout_v002.json').read_text(encoding='utf-8'));assert [x['text'] for x in j['all_text']]==expected
xml=ET.fromstring((R/'Source/contract_english_editable_v002.svg').read_text(encoding='utf-8'));assert [x.text for x in xml.iter() if x.tag.endswith('text')]==expected
for p in sorted((R/'Textures').glob('*.png')):
 im=Image.open(p);a=np.array(im)[:,:,3];size=(1024,1024) if 'wax_' in p.name else (2048,1536)
 assert im.mode=='RGBA' and im.size==size
 assert a.max()>0 and not(a[0].any() or a[-1].any() or a[:,0].any() or a[:,-1].any())
 bbox=im.getchannel('A').getbbox()
 if 'seal_red' in p.name:assert min(bbox[0],bbox[1],1024-bbox[2],1024-bbox[3])>=102
 if 'parchment' in p.name:assert a[240:1350,210:1820].min()==255
 if 'lettering' in p.name:assert not a[922:1275,205:1475].any()
 checks.append({'file':p.name,'size':list(im.size),'mode':im.mode,'alpha_bbox':bbox,'four_borders_zero':True,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
assert len(checks)==4
preview=Image.open(R/'Preview/contract_complete_preview_v002.png');assert preview.size==(2048,1536)
with zipfile.ZipFile(R/'Source/F09_ContractParchmentWax_v002.ora') as z:
 assert z.testzip() is None;stack=ET.fromstring(z.read('stack.xml'));layers=stack.findall('stack/layer');assert len(layers)==7
 for layer in layers:
  assert z.getinfo(layer.attrib['src']).file_size>0
review=Image.new('RGBA',(1280,720),(24,29,39,255));review.alpha_composite(preview.resize((880,660),Image.Resampling.LANCZOS),(200,30));review.convert('RGB').save(R/'Preview/contract_1280x720_readability_v002.jpg',quality=95)
result={'task':'F09-CONTRACT','checks':checks,'text_exact_against_brief':True,'svg_text_exact':True,'signature_region_no_lettering':True,'paper_core_opaque':True,'wax_safe_margin_10_percent':True,'ora_editable_layers':7,'status':'file checks passed; user visual review and Unity integration pending','readability_preview':'Preview/contract_1280x720_readability_v002.jpg'}
(R/'Source/delivery_checks_v002.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print('F09: 4 runtime RGBA layers + full preview checked; exact wording, empty signature, opaque paper core, 7-layer ORA')
