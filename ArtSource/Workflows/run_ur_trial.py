"""A01-B-UR: one original adult UR candidate using the approved local style chain."""
import json
from run_style_tests import ROOT, request, execute, save

target=ROOT/'Characters'/'UR001'/'UR001_card_base_v001.png'
target.parent.mkdir(parents=True,exist_ok=True)
if target.exists(): raise RuntimeError('UR v001 already exists; retain it and choose a new version.')
graph=json.loads((ROOT/'Workflows'/'A01_R01_dynamic_api.json').read_text(encoding='utf-8'))
graph['2']['inputs'].update(width=1024,height=1536,batch_size=1)
graph['4']['inputs']['text']=(
 'masterpiece, best quality, safe, solo, 1girl, mature adult woman, 27 years old, tall elegant woman, '
 'long flowing black hair, amber golden eyes, commanding confident expression, looking at viewer, '
 'distinct broken gold eclipse hair ornament at one temple, long gold star earring, '
 'elegant hourglass figure, bare shoulders, moderate cleavage, fitted waist, '
 'obsidian black and gold asymmetric fantasy ceremonial gown, structured fitted bodice, '
 'one high slit revealing a thigh, long black gloves, high heeled black boots, '
 'long crimson lined cape sweeping widely behind her. '
 'Original adult celestial guardian Jin Xing, the Last Night Watcher, '
 'captured in a majestic active turning stride, one leg advancing, '
 'hips at a three quarter angle and upper body turning naturally toward the viewer, '
 'a low but controlled cinematic angle, strong diagonal movement and believable anatomy. '
 'One open gloved palm holds a small levitating fractured gold star core made of three sharp crystal shards '
 'with a dark gap between them; the other arm spreads her flowing cape away from the face. '
 'Her black hair and crimson cape stream sideways in the same wind direction. '
 'Premium anime UR collectible splash illustration, adult feminine glamour, confident presence, '
 'refined clear anime linework, luminous golden rim light, controlled rich fabric shading, '
 'same crisp face and eye rendering as the approved moon harbor archivist style. '
 'A huge distant eclipse with a thin burning golden corona hangs behind her upper body, '
 'deep black navy celestial night, sparse fractured constellation paths, muted crimson horizon, '
 'few glowing embers kept away from her face, layered distant atmosphere. '
 'The character is the main focus, no other people, lower area visually quiet, '
 'vertical 2:3 composition, character fills 82 percent of canvas height, '
 'entire head inside the frame, face placed in the upper third, visible legs and clear silhouette. '
 'No letters, no logo, no card border, no watermark.'
)
graph['5']['inputs']['text']=(
 'worst quality, low quality, blurry, text, watermark, signature, artist name, card frame, '
 'extra person, extra limbs, bad hands, extra fingers, broken wrists, impossible spine, '
 'cropped head, face covered by particles, stiff symmetrical standing pose, '
 'silver hair, blue eyes, lantern, astrolabe, plain work uniform, school uniform, '
 'child, teen, childlike proportions, nude, nipples, exposed genitals, underwear exposure, explicit sex'
)
graph['10']['inputs']['seed']=71007021
graph['12']['inputs']['filename_prefix']='A01_B_UR001_trial_v001'
graph['8']={'class_type':'VAEDecodeTiled','inputs':{'samples':['10',0],'vae':['1',0],'tile_size':512,'overlap':64,'temporal_size':64,'temporal_overlap':8}}
save(ROOT/'Metadata'/'A01_B_UR_changes.json',{
 'task':'A01-B-UR','style_source':'A01_R01_dynamic_api.json',
 'changes':['original adult UR001 design prompt','1024x1536','seed 71007021','tiled VAE decode','output prefix'],
 'unchanged':'model, encoder, VAE, three LoRA strengths, custom sampler settings, replay node',
 'reference_constraint':'No image-reference conditioning used; consistency relies on same style chain and prompt design.',
 'production_limit':'one UR candidate; no remaining characters'})
execute('A01_B_UR_trial',graph,request('/object_info'),target)
