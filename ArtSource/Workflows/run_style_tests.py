"""A00/A01-A: local ComfyUI only; submit one job at a time, retain evidence."""
import copy, hashlib, json, pathlib, struct, time, urllib.request

ROOT = pathlib.Path(__file__).resolve().parents[1]
URL = 'http://127.0.0.1:8188'
MODEL_ROOT = pathlib.Path('E:/ComfyUI-aki-v3/ComfyUI-aki-v3/ComfyUI/models')

def save(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding='utf-8')

def request(route, value=None):
    data = None if value is None else json.dumps(value).encode()
    req = urllib.request.Request(URL + route, data=data, headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req, timeout=30) as response:
        return json.load(response)

SUBJECT = ('masterpiece, best quality, safe, solo, 1girl, adult woman, silver hair, long hair, teal eyes, '
    'looking at viewer, standing, three quarter view, fully clothed, navy blue and ivory tailored uniform, '
    'high collar, long sleeves, brass buttons, dark blue long skirt, '
    'holding a small brass celestial astrolabe in one hand at waist level, other arm relaxed. '
    'An original adult moon harbor archivist with a composed attentive expression. '
    'Her silver gray hair flows gently and her outfit has simple coherent seams. '
    'Vertical collectible illustration, character fills 80 percent of the height, entire head inside frame, '
    'face centered in the upper third, knees visible, spacious quiet lower area. '
    'Moonlit harbor, distant low silhouettes of ships, soft cool blue key light and subtle warm rim light. '
    'No letters, no border, no symbols resembling text, no logo. ')
NEGATIVE = ('worst quality, low quality, blurry, text, watermark, signature, artist name, card frame, '
    'extra person, extra fingers, deformed hands, inconsistent eyes, disconnected limbs, cropped head, '
    'nsfw, explicit, revealing clothes, cluttered foreground')
STYLES = {
 'a': 'Refined anime illustration, clean controlled linework, precise cel shading, subtle gradients, crisp facial features, restrained costume detail. Flat clean color shapes, sharply readable at thumbnail size.',
 'b': 'Soft painterly anime illustration, delicate brush transitions, luminous skin shading, gentle fabric texture, controlled edges, clear facial features. Soft atmospheric light, visible tasteful painted texture, rounded soft shading.',
 'c': 'Decorative fantasy anime illustration, elegant celestial motifs, rich but controlled material rendering, subtle ornamental composition, readable character silhouette. Fine gold star chart arcs in the distant harbor sky, polished decorative details, controlled jewel colors.'
}

def make_graph(style, width, height, prefix):
    graph=json.loads((ROOT/'Workflows'/'user_00348_prompt.json').read_text(encoding='utf-8'))
    graph['2']['inputs'].update(width=width,height=height,batch_size=1)
    graph['4']['inputs']['text']=SUBJECT+STYLES[style]
    graph['5']['inputs']['text']=NEGATIVE
    graph['10']['inputs']['seed']=71007001
    graph['12']['inputs']['filename_prefix']=prefix
    return graph

def ui_graph(graph, info):
    nodes, links = [], []
    for key, node in graph.items():
        nid = int(key)
        schema = info[node['class_type']]
        inputs, widgets = [], []
        for name, spec in {**schema['input'].get('required',{}), **schema['input'].get('optional',{})}.items():
            if name not in node['inputs']: continue
            value = node['inputs'][name]
            if isinstance(value, list) and len(value)==2 and str(value[0]) in graph:
                source, slot = int(value[0]), value[1]
                lid = len(links)+1
                links.append([lid,source,slot,nid,len(inputs),spec[0]])
                inputs.append({'name':name,'type':spec[0],'link':lid})
            else:
                widgets.append(value)
                if name=='seed': widgets.append('fixed')
        nodes.append({'id':nid,'type':node['class_type'],'pos':[((nid-1)%3)*440,((nid-1)//3)*340],
            'size':[400,280],'flags':{},'order':nid-1,'mode':0,'inputs':inputs,
            'outputs':[{'name':name,'type':typ,'links':[]} for name,typ in zip(schema.get('output_name',schema['output']),schema['output'])],
            'properties':{'Node name for S&R':node['class_type']},'widgets_values':widgets})
    for lid,source,slot,_,_,_ in links:
        next(n for n in nodes if n['id']==source)['outputs'][slot]['links'].append(lid)
    return {'last_node_id':max(int(k) for k in graph),'last_link_id':len(links),'nodes':nodes,'links':links,'groups':[], 'config':{},'extra':{},'version':0.4}

def execute(name, graph, info, target):
    save(ROOT/'Workflows'/f'{name}_api.json',graph)
    ui = ui_graph(graph, info)
    save(ROOT/'Workflows'/f'{name}_ui.json',ui)
    started = time.time()
    submission = request('/prompt', {'prompt':graph,'client_id':'zheciyidingchu-art','extra_data':{'extra_pnginfo':{'workflow':ui}}})
    record = {'task':name,'submission':submission,'started_unix':started,'graph_path':f'Workflows/{name}_api.json'}
    save(ROOT/'Metadata'/f'{name}_run.json',record)
    pid = submission['prompt_id']
    print(f'{name}: submitted {pid}',flush=True)
    while True:
        history = request('/history/'+pid)
        if pid in history:
            result = history[pid]
            save(ROOT/'Metadata'/f'{name}_history.json',result)
            if result.get('status',{}).get('status_str')=='error':
                raise RuntimeError(json.dumps(result.get('status'),ensure_ascii=False))
            images = [im for output in result.get('outputs',{}).values() for im in output.get('images',[])]
            if images:
                im = images[0]
                from urllib.parse import urlencode
                with urllib.request.urlopen(URL+'/view?'+urlencode(im),timeout=30) as response:
                    data = response.read()
                if data[:8] != b'\x89PNG\r\n\x1a\n': raise RuntimeError('Output is not PNG')
                target.write_bytes(data)
                record.update(elapsed_seconds=round(time.time()-started,2),output=str(target.relative_to(ROOT)),
                    sha256=hashlib.sha256(data).hexdigest(),dimensions=struct.unpack('>II',data[16:24]),status='generated_pending_visual_review')
                save(ROOT/'Metadata'/f'{name}_run.json',record)
                print(f'{name}: complete {record["elapsed_seconds"]}s -> {target}',flush=True)
                return
        time.sleep(5)

def main():
    info=request('/object_info')
    save(ROOT/'Metadata'/'A00_object_info.json',info)
    save(ROOT/'Metadata'/'A00_system_stats.json',request('/system_stats'))
    for node in make_graph('a',512,768,'A00_test').values():
        if node['class_type'] not in info: raise RuntimeError('Missing node '+node['class_type'])
        schema=info[node['class_type']]['input']['required']
        for name,spec in schema.items():
            value=node['inputs'].get(name)
            if isinstance(spec[0],list) and value not in spec[0]: raise RuntimeError(f'Invalid {name}: {value}')
    execute('A00_smoke',make_graph('a',512,768,'A00_smoke'),info,ROOT/'StyleTests'/'A00_smoke.png')
    for style in STYLES:
        execute(f'A01_{style}',make_graph(style,768,1152,f'A01_SSR001_{style}'),info,ROOT/'StyleTests'/f'SSR001_style_{style}.png')

if __name__=='__main__': main()
