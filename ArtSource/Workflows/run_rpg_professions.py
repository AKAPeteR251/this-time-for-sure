"""A02-RPG: six original adult profession/element concept cards, local workflow."""
import json
from run_style_tests import ROOT, request, execute, save

COMMON=(
 'masterpiece, best quality, safe, solo, 1girl, original adult female JRPG character, '
 'grown adult proportions, attractive expressive face, looking at viewer, '
 'premium anime collectible illustration, clear elegant anime linework, precise cel shading, '
 'soft gradients within clean color shapes, coherent costume construction, '
 'dynamic diagonal composition, believable weight balance, natural anatomy, '
 'hair and loose fabric following the motion, strong readable silhouette, '
 'tasteful glamour and carefully designed skin exposure, no nudity. '
 'Vertical 2:3 canvas, character fills approximately 80 percent of height, '
 'entire head and main prop inside frame, face in upper third, clear readable eyes, '
 'face is the main contrast focus, legs visible, a quiet lower area for later UI. '
 'No lettering, no logo, no watermark, no card border. '
)
NEGATIVE=(
 'worst quality, low quality, blurry, text, watermark, logo, signature, artist name, card border, '
 'extra character, extra limbs, malformed hands, extra fingers, broken wrists, twisted torso, '
 'impossible spine, cropped head, face hidden by effects, stiff symmetrical standing pose, '
 'nude, exposed nipples, exposed genitals, underwear exposure, explicit sex, '
 'child, teen, underage, childlike proportions, chibi, school uniform, excessive particles'
)
PROFILES=[
 {'key':'01_ice_mage','title':'冰法师','age':24,'element':'冰','seed':71008031,
  'identity':'白金长发、冰蓝眼、宽檐尖帽、雪花晶体法杖',
  'design':(
   'An adult 24-year-old ice sorceress with platinum blonde long hair, icy blue eyes, '
   'a confident mischievous smile, a large asymmetrical midnight blue pointed witch hat '
   'decorated with one prominent snowflake crystal and silver clasps. '
   'She wears a tailored midnight blue and icy white off-shoulder spellcaster dress, '
   'fitted bodice with a subtle neckline, a side slit revealing one thigh, long gloves, '
   'thigh high dark boots and flowing translucent-looking ice blue cape panels. '
   'She pivots while casting, one bent knee stepping forward, one hand firmly holding '
   'a long straight silver staff topped by a large six-point snowflake crystal, '
   'the other open palm projecting a few sharp floating ice shards outward. '
   'Cold blue magic curls behind her shoulders, not across her face. '
   'Background: ancient snow-covered stone tower ruins under a winter night sky, '
   'soft blue moonlight, restrained luminous ice reflections.')},
 {'key':'02_light_priest','title':'光牧师','age':20,'element':'光','seed':71008032,
  'identity':'浅金长发、金绿眼、百合发饰、日轮权杖',
  'design':(
   'An adult 20-year-old female holy priestess with long pale golden hair, warm gold-green eyes, '
   'a bright gentle reassuring smile, a white lily hair ornament and a small golden sun emblem. '
   'Youthful adult facial features and slim grown-woman proportions. '
   'She wears an ivory white and gold ceremonial healer gown, open shoulders, elegant modest neckline, '
   'fitted waist, asymmetrical layered skirt with one side slit, flowing detached long sleeves, '
   'white high boots and a long ivory scarf with gold trim. '
   'She is turning and reaching a hand toward the viewer as if offering healing, '
   'one knee softly bent, her long hair and scarf sweeping to the opposite side, '
   'the other hand holding a clear slender golden staff with a circular sun-shaped head. '
   'A few warm golden healing motes hover around the open palm. '
   'Background: a luminous stone cathedral garden, tall distant arches, morning golden light, '
   'white lilies and restrained soft atmosphere, face clearly separated from the light background.')},
 {'key':'03_wind_elf_archer','title':'风精灵弓箭手','age':22,'element':'风','seed':71008033,
  'identity':'长尖耳、浅绿长发、翡翠眼、叶片弓',
  'design':(
   'An adult 22-year-old female woodland elf archer with clearly visible long pointed elf ears, '
   'long pale mint green hair partly tied into a high ponytail, emerald green eyes, '
   'an alert playful confident expression and a leaf-shaped gold hair clasp. '
   'She wears a forest green leather fitted ranger bodice with tasteful shoulder and waist exposure, '
   'short layered leaf-shaped skirt panels over opaque fitted shorts, a slim utility belt, '
   'one long fingerless arm guard, high leather boots and a small flowing hooded cape. '
   'A dynamic three quarter shooting pose: torso turning toward the viewer, front knee bent, '
   'one hand holding a visibly curved leaf-carved longbow, other hand drawing a single arrow '
   'and its straight taut bowstring back near her cheek, with clear believable arm and hand positions. '
   'The arrow and bow remain away from the eyes, no overlapping bow limbs. '
   'Her ponytail and green cape stream sideways in a wind gust, a few leaves tracing the motion. '
   'Background: a sun-dappled ancient forest with distant ruins and mist, emerald wind light, '
   'clean foreground, bright skin contrasting against the deeper green woodland.')},
 {'key':'04_fire_warrior','title':'火女战士','age':25,'element':'火','seed':71008034,
  'identity':'赤红高马尾、琥珀眼、黑金轻甲、宽刃大剑',
  'design':(
   'An adult 25-year-old female fire swordswoman with long vivid crimson hair in a high ponytail, '
   'amber eyes, a bold challenging smile and athletic adult feminine proportions. '
   'She wears well-constructed obsidian and brass fantasy light armor, a fitted breastplate '
   'with a modest neckline, uncovered shoulders and strong arms, red cloth waist panels, '
   'a short asymmetrical armored skirt over fitted opaque battle shorts, thigh armor on one leg, '
   'sturdy high boots and a torn scarlet cape flowing backward. '
   'She lunges forward from a three quarter angle with one knee bent, '
   'holding one large broad straight sword by its clearly defined hilt across her upper body '
   'and diagonally beside the shoulder, blade fully visible and not blocking the face, '
   'the other hand balancing her movement, expressive dynamic hips and shoulders. '
   'Fire glows along the sword edge with a few sparks trailing away from the face. '
   'Background: dark volcanic basalt ruins and a restrained red lava horizon, '
   'warm orange rim light, clear face illumination, no clutter or excessive flames.')},
 {'key':'05_thunder_miko','title':'雷巫女','age':19,'element':'雷','seed':71008035,
  'identity':'黑紫长发、紫瞳、朱红结绳、御币与符札',
  'design':(
   'An adult 19-year-old Japanese-style shrine maiden with long black hair with dark violet highlights, '
   'violet eyes, youthful adult facial features, a lively confident smile, '
   'a large crimson ribbon and tiny fox mask pinned near one temple. '
   'She wears an original fantasy miko outfit clearly based on a white kimono top and crimson hakama, '
   'fashioned into a fitted off-shoulder white wrap bodice with a tasteful neckline, '
   'very wide flowing detached sleeves, red waist knot, asymmetrical crimson pleated skirt '
   'with one thigh slit, opaque fitted undershorts, white tabi-style high boots. '
   'She is mid-step in a sweeping ceremonial dance, one leg lifting naturally, '
   'torso and shoulders turning, hair and long sleeves spiraling around the movement. '
   'One hand holds a gohei ritual wand with crisp white folded paper streamers, '
   'the other hand spreads a few rectangular blank talisman papers without readable writing. '
   'A small purple lightning arc connects the talismans away from her face. '
   'Background: a moonlit Japanese shrine with a distant red torii gate, wet stone steps, '
   'violet storm clouds and restrained electric light, clearly readable silhouette.')},
 {'key':'06_star_magical_girl','title':'星魔法少女','age':19,'element':'星','seed':71008036,
  'identity':'粉色双马尾、蓝绿眼、星形胸针、星杖与飘带',
  'design':(
   'An adult 19-year-old magical heroine with pink long twin ponytails, turquoise eyes, '
   'a cheerful confident smile, youthful adult face and slim fully grown woman proportions. '
   'She wears an original pink, ivory and lavender magical heroine costume, '
   'a fitted bodice with a large gold star brooch, open shoulders, tasteful small waist cutouts, '
   'short layered flared skirt with opaque fitted safety shorts, long gloves, knee high white boots, '
   'large ribbons and flowing translucent-looking ribbon tails; no school uniform. '
   'A lively airborne twirling pose with believable adult proportions, one knee bent, '
   'other leg extended diagonally, hair and skirt ribbons following the spin. '
   'One hand raises a slender wand with a large five-point star head at the side of the picture, '
   'the other arm reaches openly toward the viewer, face unobstructed. '
   'A controlled swirl of pink and violet starlight follows the wand motion. '
   'Background: distant fantasy town rooftops under a deep twilight sky, '
   'gentle cosmic sparkle and soft luminous star glow, uncluttered lower area.')}
]

def main():
    folder=ROOT/'StyleTests'/'RPG_Professions'
    folder.mkdir(parents=True,exist_ok=True)
    info=request('/object_info')
    save(ROOT/'Metadata'/'A02_RPG_profiles.json',PROFILES)
    for profile in PROFILES:
        key=profile['key']
        target=folder/(key+'_v001.png')
        if target.exists():
            print('Preserved existing candidate:',key,flush=True)
            continue
        graph=json.loads((ROOT/'Workflows'/'A01_R01_dynamic_api.json').read_text(encoding='utf-8'))
        graph['4']['inputs']['text']=COMMON+profile['design']
        graph['5']['inputs']['text']=NEGATIVE
        graph['10']['inputs']['seed']=profile['seed']
        graph['12']['inputs']['filename_prefix']='A02_RPG_'+key+'_v001'
        graph['8']={'class_type':'VAEDecodeTiled','inputs':{'samples':['10',0],'vae':['1',0],'tile_size':512,'overlap':64,'temporal_size':64,'temporal_overlap':8}}
        execute('A02_RPG_'+key,graph,info,target)

if __name__=='__main__': main()
