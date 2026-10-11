exec(open(__file__.replace('a05_sam_refine.py','a05_sam_extract.py'),encoding='utf-8').read().split('for i,mask in enumerate(masks):')[0])
import cv2
out=BASE/'SAM_v002';out.mkdir(exist_ok=True)
mask=masks[0].astype('uint8')
parts=[('left_knee',[10,750,303,1035],[[76,923],[126,950],[164,836]]),('right_leg',[409,753,708,1152],[[556,934],[600,1094]]),('rear_boot',[266,864,398,1069],[[330,990],[332,938]]),('body',[231,396,514,771],[[299,503],[374,629],[333,699]]),('hair',[258,12,768,510],[[413,128],[551,142],[688,285]])]
part_meta=[]
for name,box,pts in parts:
    ms,sc,_=predictor.predict(point_coords=np.array(pts),point_labels=np.ones(len(pts),dtype=int),box=np.array(box),multimask_output=True)
    chosen=int(np.argmax(sc));part=ms[chosen].astype('uint8')
    clipped=np.zeros_like(part);x0,y0,x1,y1=box;clipped[y0:y1,x0:x1]=part[y0:y1,x0:x1]
    mask=np.maximum(mask,clipped);part_meta.append({'name':name,'box':box,'points':pts,'scores':sc.tolist(),'chosen':chosen})
# Remove detached background sparks; preserve the single connected body/weapon silhouette.
count,cc,stats,_=cv2.connectedComponentsWithStats(mask,8)
main=1+int(np.argmax(stats[1:,cv2.CC_STAT_AREA]));mask=(cc==main).astype('uint8')
a=Image.fromarray(mask*255);a.save(out/'mask_v002.png')
rgba=im.convert('RGBA');rgba.putalpha(a);rgba.save(out/'cutout_v002.png')
bg=Image.new('RGBA',im.size,(36,67,88,255));bg.alpha_composite(rgba);bg.convert('RGB').save(out/'preview_v002.jpg',quality=94)
meta={'method':'local SAM vit_b part-box refinement union; largest connected component, no repainting','parts':part_meta,'weight':str(WEIGHT),'source':str(SOURCE),'source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'original_size':im.size,'status':'pending_visual_review','elapsed_seconds':round(time.time()-start,3)}
(out/'run_metadata.json').write_text(json.dumps(meta,ensure_ascii=False,indent=2),encoding='utf-8');print('SAM refinement complete')
