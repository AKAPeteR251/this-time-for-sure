"""A01-B-SSR v002: use core KSampler for the img2img refinement only."""
import json
from run_style_tests import ROOT, request, execute, save
graph=json.loads((ROOT/'Workflows'/'A01_B_SSR_master_api.json').read_text(encoding='utf-8'))
old=graph['10']['inputs']
graph['10']={'class_type':'KSampler','inputs':{
 'model':old['model'],'positive':old['positive'],'negative':old['negative'],
 'latent_image':old['latent_image'],'seed':old['seed'],'steps':12,'cfg':1.0,
 'sampler_name':'euler','scheduler':'simple','denoise':0.3}}
graph['12']['inputs']['filename_prefix']='A01_B_SSR001_master_v002'
target=ROOT/'Characters'/'SSR001'/'SSR001_card_base_v002.png'
if target.exists(): raise RuntimeError('Retain existing v002; choose another version.')
save(ROOT/'Metadata'/'A01_B_SSR_v002_changes.json',{
 'source':'A01_B_SSR_master_api.json','reason':'v001 has visible noisy artifacts; rejected for delivery',
 'changes':['img2img sampler only: core KSampler, euler/simple, 12 steps, denoise 0.3','output version'],
 'unchanged':'approved source, image scale, prompt, model, LoRA chain and strengths, tiled VAE',
 'note':'User text2image workflow remains preserved; only the added high resolution refinement stage changes.'})
execute('A01_B_SSR_master_v002',graph,request('/object_info'),target)
