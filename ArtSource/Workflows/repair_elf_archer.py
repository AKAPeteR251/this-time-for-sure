"""A02-RPG: simplify the failed bow interaction; keep the same adult design chain."""
import json
from run_style_tests import ROOT,request,execute,save
from run_rpg_professions import COMMON,NEGATIVE
graph=json.loads((ROOT/'Workflows'/'A02_RPG_03_wind_elf_archer_api.json').read_text(encoding='utf-8'))
graph['4']['inputs']['text']=COMMON+(
 'An adult 22-year-old female woodland elf ranger, long sharply pointed elf ears, '
 'pale mint green hair in a flowing high ponytail, emerald green eyes, '
 'an alert playful smile, a golden leaf hair clasp. '
 'Forest green fitted leather ranger bodice, bare shoulders, a small waist opening, '
 'leaf-shaped short skirt panels over opaque fitted shorts, brown utility belt, '
 'fingerless leather arm guards, high brown boots, short flowing green hooded cape. '
 'She is dashing forward through the forest while looking back toward the viewer, '
 'one knee raised in a natural running stride, torso in a dynamic three-quarter turn, '
 'ponytail and cape swept far sideways by the wind. '
 'She carries ONE SIMPLE FULLY VISIBLE wooden longbow in one hand beside her body. '
 'The bow is relaxed, NOT being drawn or fired. The bow has a single clean curved wooden arch '
 'and ONE straight string connecting its two tips, both tips fit inside the picture. '
 'Her fingers clearly wrap around the central grip, the entire bow is angled diagonally '
 'from upper left to lower center, away from the face and legs. '
 'Her other free hand reaches naturally backward for balance, not toward her mouth. '
 'A leather quiver with a few visible arrow feathers is strapped to her back, no loose arrow being held. '
 'A few emerald wind wisps and leaves follow her motion. '
 'Ancient forest ruins, sunlit canopy and deep green shadows, skin clearly separated from the background, '
 'readable fantasy adventure character with a clear elf-ranger identity.'
)
graph['5']['inputs']['text']=NEGATIVE+', drawn bow, firing arrow, hand at mouth, disconnected arrow, multiple bowstrings, tangled bow limbs'
graph['10']['inputs']['seed']=71008043
graph['12']['inputs']['filename_prefix']='A02_RPG_03_wind_elf_archer_v002'
target=ROOT/'StyleTests'/'RPG_Professions'/'03_wind_elf_archer_v002.png'
if target.exists(): raise RuntimeError('Retain existing v002.')
save(ROOT/'Metadata'/'A02_RPG_elf_repair_changes.json',{
 'task':'A02-RPG','source':'A02_RPG_03_wind_elf_archer_api.json',
 'reason':'v001 arrow/bowstring interaction is structurally incoherent; preserve only as failed draft',
 'changes':['replace shooting interaction with running and carrying a relaxed bow','seed 71008043','new version prefix'],
 'unchanged':'adult character identity, model, LoRA, sampler, size, style chain'})
execute('A02_RPG_03_wind_elf_archer_v002',graph,request('/object_info'),target)
