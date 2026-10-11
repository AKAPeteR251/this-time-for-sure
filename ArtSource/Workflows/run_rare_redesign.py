"""A01-R01: adult SSR allure and active pose, based on user's exact graph."""
from run_style_tests import ROOT, request, execute, make_graph, save

PROMPT = (
 'masterpiece, best quality, safe, solo, 1girl, mature adult woman, 25 years old, silver gray long hair, '
 'teal blue eyes, confident inviting smile, looking at viewer, crescent moon brass hair ornament, '
 'elegant hourglass figure, off shoulder dress, bare shoulders, moderate cleavage, fitted waist, '
 'navy blue and ivory asymmetric fantasy evening dress, gold fasteners, high side slit, '
 'one exposed thigh, long gloves, high boots, flowing long skirt panels. '
 'An original adult moon harbor archivist performing a graceful dramatic turning motion, '
 'a lively twisting three quarter pose with clear weight balance, one bent knee stepping forward, '
 'her chest and hips turned in different directions in a natural dance-like contrapposto, '
 'long silver hair and silk ribbons swept sideways by the motion, seductive confident eye contact. '
 'She holds an OPEN brass armillary astrolabe with three clearly visible intersecting metal rings '
 'and a small glowing blue star suspended inside, away from her face; not a glass ball or lantern. '
 'One gloved hand gestures outward, tracing a faint curved starlight trail. '
 'Premium anime collectible SSR splash illustration, adult feminine glamour, '
 'elegant costume design with tasteful skin exposure, dynamic diagonal composition. '
 'Character fills 80 percent of a vertical 2:3 canvas, entire head inside frame, '
 'face in the upper third, face has the sharpest contrast, legs visible, lower area uncluttered. '
 'Deep midnight blue moonlit harbor, full moon, dark silhouettes of distant sailing ships, '
 'faint luminous celestial navigation arcs, blue tide reflections, cool face light, gentle warm rim light. '
 'Refined anime linework, rich fabric rendering, controlled luminous color. '
 'No text, no logo, no card frame, no watermark.'
)
NEGATIVE = (
 'worst quality, low quality, blurry, text, watermark, signature, artist name, card frame, '
 'extra person, extra limbs, deformed hands, extra fingers, broken wrists, disconnected limbs, '
 'cropped head, twisted torso, impossible spine, obscured face, stiff symmetrical standing pose, '
 'plain work uniform, high closed collar, long straight conservative skirt, '
 'glass bottle, glass lantern, bright daylight, school uniform, child, teen, childlike proportions, '
 'nude, nipples, exposed genitals, underwear exposure, explicit sex'
)

info=request('/object_info')
graph=make_graph('c',768,1152,'A01_R01_SSR001_dynamic_v001')
graph['4']['inputs']['text']=PROMPT
graph['5']['inputs']['text']=NEGATIVE
graph['10']['inputs']['seed']=71007011
save(ROOT/'Metadata'/'A01_R01_changes.json',{
 'source':'Workflows/user_00348_prompt.json',
 'changes':['original adult SSR character and glamorous active pose prompts','negative prompt adjusted for moderate skin exposure','768x1152','seed 71007011','task output prefix'],
 'unchanged':'model, VAE, encoder, three LoRA strengths, replay node, sampler settings',
 'task':'A01-R01','user_direction':'SSR/UR allure, motion, distinct traits; plain ordinary cards'
})
execute('A01_R01_dynamic',graph,info,ROOT/'StyleTests'/'SSR001_dynamic_v001.png')
