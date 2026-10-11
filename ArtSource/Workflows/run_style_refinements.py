"""A01-A v002: keep user's graph; adjust style strength, preserve v001 files."""
from run_style_tests import ROOT, request, execute, make_graph, SUBJECT, STYLES, save

info=request('/object_info')
changes={
 'b':(0.15,'Soft painterly anime illustration, smooth soft shading, subtle watercolor-like transitions, delicate painted fabric texture, thin understated outlines. '),
 'c':(0.3,'Decorative celestial fantasy illustration, fine gold engraved circular star charts, tasteful art nouveau-inspired celestial ornaments surrounding the distant harbor, jewel toned moonlit atmosphere. ')
}
for style,(factor,lead) in changes.items():
    graph=make_graph(style,768,1152,f'A01_SSR001_{style}_v002')
    graph['4']['inputs']['text']=lead+SUBJECT+STYLES[style]
    for key in ('6','16','17'):
        for field in ('strength_model','strength_clip'):
            graph[key]['inputs'][field]*=factor
    save(ROOT/'Metadata'/f'A01_{style}_v002_changes.json',{
        'source':'Workflows/user_00348_prompt.json','changes':{
        'positive_prompt':'original SSR001 plus style emphasis','negative_prompt':'project negative prompt',
        'size':[768,1152],'seed':71007001,'lora_strength_multiplier':factor,
        'output_prefix':graph['12']['inputs']['filename_prefix']},
        'unchanged':'graph structure, model, text encoder, VAE, sampler and replay settings',
        'reason':'v001 style LoRA dominates brushwork; maintain original files and try reduced strength'})
    execute(f'A01_{style}_v002',graph,info,ROOT/'StyleTests'/f'SSR001_style_{style}_v002.png')
