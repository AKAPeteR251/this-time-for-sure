"""A05-BLENDER v003: crystal fracture, light leaks and restrained impact shake."""
import bpy, math, random, json, sys
from pathlib import Path
from mathutils import Vector
BASE=Path(__file__).resolve().parent
OUT=BASE/'v003'; OUT.mkdir(exist_ok=True)
for name in ['Frames','Review']: (OUT/name).mkdir(exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(BASE/'v002/A05_SummonStage_v002.blend'))
scene=bpy.context.scene
for o in list(bpy.data.objects):
    if o.name.startswith('PortalLightRay_') or o.name=='CentralCrystal': bpy.data.objects.remove(o,do_unlink=True)
def key(o,p,f,v):
    setattr(o,p,v);o.keyframe_insert(data_path=p,frame=f)
def material(name,color,energy,metal=.2):
    m=bpy.data.materials.new(name);m.use_nodes=True
    n=m.node_tree.nodes['Principled BSDF'];n.inputs['Base Color'].default_value=(*color,1)
    n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=.22
    n.inputs['Emission'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=energy
    return m
def emission(m,points):
    s=m.node_tree.nodes['Principled BSDF'].inputs['Emission Strength']
    for f,v in points:s.default_value=v;s.keyframe_insert(data_path='default_value',frame=f)
shell=material('FracturedBlueCrystal',(.10,.39,.66),.24,.45)
inside=material('CrystalInnerFaces',(.23,.63,.8),.6)
crackmat=material('GoldenCrackLeak',(1,.72,.23),0)
emission(crackmat,[(1,0),(60,0),(72,2),(88,5),(99,10),(104,18),(114,0)])
beam_mat=material('ReleasedCrystalLight',(1,.79,.35),0)
emission(beam_mat,[(1,0),(79,0),(90,3),(101,5),(105,9),(112,3),(119,0)])
rig=bpy.data.objects.new('FractureCrystalRig',None);scene.collection.objects.link(rig);rig.location=(0,-.10,4.4)
# Twelve tetrahedra reconstruct a closed six-sided crystal. Interior faces become visible as it ruptures.
ring=[Vector((.46*math.cos(i*math.tau/6),.46*math.sin(i*math.tau/6),0)) for i in range(6)]
rng=random.Random(71008053)
for half,apex in enumerate([Vector((0,0,1.3)),Vector((0,0,-1))]):
    for i in range(6):
        vertices=[apex,ring[i],ring[(i+1)%6],Vector((0,0,0))]
        center=sum(vertices,Vector())/4
        mesh=bpy.data.meshes.new('CrystalShardMesh');mesh.from_pydata([v-center for v in vertices],[],[(0,1,2),(0,3,1),(0,2,3),(1,3,2)]);mesh.update()
        o=bpy.data.objects.new('CrystalShard_%d_%d'%(half,i),mesh);scene.collection.objects.link(o);o.parent=rig
        mesh.materials.append(shell);mesh.materials.append(inside)
        for p in mesh.polygons:p.material_index=0 if p.index==0 else 1
        direction=center.normalized();direction.y-=.35;direction.normalize()
        for f,gap in [(1,0),(62,0),(76,.002),(89,.008),(98,.019),(102,.033),(105,.25),(112,1.75),(122,3.4),(132,4.6)]:
            key(o,'location',f,center+direction*gap)
        spin=Vector((rng.uniform(-3,3),rng.uniform(-3,3),rng.uniform(-3,3)))
        for f,v in [(1,Vector()),(102,Vector()),(112,spin*.45),(132,spin)]:key(o,'rotation_euler',f,v)
        for f,s in [(1,1),(114,1),(124,.15),(132,.001)]:key(o,'scale',f,(s,)*3)
        # Angular golden veins on the original exterior triangular facet.
        a,b,c=vertices[:3];path=[]
        for j in range(7):
            t=.10+j*.12;u=.5+(.10 if j%2 else -.10)
            p=a*(1-t)+(b*(1-u)+c*u)*t
            p+=p.normalized()*.003;path.append(p-center)
        curve=bpy.data.curves.new('CrystalCrackCurve','CURVE');curve.dimensions='3D';curve.bevel_depth=.003;curve.bevel_resolution=1
        spline=curve.splines.new('POLY');spline.points.add(len(path)-1)
        for p,v in zip(spline.points,path):p.co=(*v,1)
        line=bpy.data.objects.new('CrackVein_%d_%d'%(half,i),curve);scene.collection.objects.link(line);line.parent=o;curve.materials.append(crackmat)
        for f,s in [(1,0),(62+i,0),(76+i,.65),(93,1),(108,1),(114,0)]:
            curve.bevel_factor_end=s;curve.keyframe_insert(data_path='bevel_factor_end',frame=f)
for f,s in [(1,.36),(42,.68),(62,.90),(94,.94),(101,.89),(104,1),(132,1)]:key(rig,'scale',f,(s,)*3)
# Growing, tapered beams emerge only during pressure buildup; no permanent corridor spokes.
for i in range(7):
    a=i*math.tau/7+.23+math.sin(i*2.1)*.13
    direction=Vector((math.cos(a),-.72,math.sin(a)*.75)).normalized()
    bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=.002,radius2=.013,depth=1)
    o=bpy.context.object;o.name='CrackLightBeam_%02d'%i;o.parent=rig;o.data.materials.append(beam_mat)
    o.rotation_euler=direction.to_track_quat('Z','Y').to_euler()
    for f,length in [(1,.001),(78+i,.001),(91+i,.55),(101,1.3),(105,3.5),(112,5.5),(119,.001)]:
        key(o,'scale',f,(1,1,length));key(o,'location',f,direction*(.12+length/2))
bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,radius=1)
heart=bpy.context.object;heart.name='ReleasedInnerLight';heart.parent=rig;heart.location=(0,0,0);heart.data.materials.append(beam_mat)
for f,s in [(1,.001),(101,.001),(104,.07),(107,.25),(111,.5),(117,.001)]:key(heart,'scale',f,(s,)*3)
# Frame-sampled vibration, seeded and bounded; one short rupture kick, then rapid settling.
cam=scene.camera;positions={}
for f in range(1,133):scene.frame_set(f);positions[f]=cam.location.copy()
cam.animation_data.action.fcurves
for fc in list(cam.animation_data.action.fcurves):
    if fc.data_path=='location':cam.animation_data.action.fcurves.remove(fc)
for f in range(1,133):
    build=max(0,min(1,(f-64)/38));release=max(0,1-(f-105)/13)
    amp=(.003+.026*build) if 65<=f<=103 else (.10*release if 104<=f<=118 else 0)
    offset=Vector((math.sin(f*2.38)*amp,math.sin(f*1.77)*amp*.25,math.cos(f*2.71)*amp*.7))
    key(cam,'location',f,positions[f]+offset)
    r=.014*build if 65<=f<=103 else (.055*release if 104<=f<=118 else 0)
    key(rig,'location',f,Vector((0,-.1,4.4))+Vector((math.sin(f*2.7)*r,0,math.cos(f*2.1)*r)))
for o in [cam,rig]:
    for fc in o.animation_data.action.fcurves:
        if fc.data_path=='location':
            for k in fc.keyframe_points:k.interpolation='LINEAR'
for o in bpy.data.objects:
    if o.name.startswith('GateShockwave_'):
        for fc in o.animation_data.action.fcurves:
            for k in fc.keyframe_points:k.co.x+=5;k.handle_left.x+=5;k.handle_right.x+=5
scene.timeline_markers.clear()
for name,f in [('CrystalAssembled',62),('CracksGrowing',82),('PressureAndLightLeak',98),('CrystalRupture',105),('TraverseLightCurtain',132)]:scene.timeline_markers.new(name,frame=f)
scene.render.filepath=str(OUT/'Frames/frame_');scene.frame_set(1)
blend=OUT/'A05_SummonStage_v003.blend';bpy.ops.wm.save_as_mainfile(filepath=str(blend))
params=json.loads((BASE/'v002/parameters_v002.json').read_text(encoding='utf-8'))
params.update({'version':'v003','blend_file':str(blend),'review_frames':[82,99,107],'fracture_shards':12,'light_beams':7,'crystal_crack_frames':[62,102],'rupture_frame':105,'shake_camera_peak_units':.10,'shake_crystal_peak_units':.055,'shake_seed':'deterministic sine','status':'candidate_pending_visual_review','changes':'Remove fixed corridor rays; surface crack growth, leaking tapered beams, actual separated crystal shards, pressure vibration and rupture kick.'})
(OUT/'parameters_v003.json').write_text(json.dumps(params,ensure_ascii=False,indent=2),encoding='utf-8')
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if 'preview' in args:
    for name,f in [('01_cracks',82),('02_light_leak',99),('03_rupture',107)]:
        scene.frame_set(f);scene.render.filepath=str(OUT/'Review'/(name+'_v003.png'));bpy.ops.render.render(write_still=True)
elif 'render' in args:bpy.ops.render.render(animation=True)
print('A05 v003 crystal rupture master saved')
