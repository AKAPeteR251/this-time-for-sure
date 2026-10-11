"""A05-BLENDER original common portal stage. Blender 2.93 EEVEE, no UI/text/characters."""
import bpy,math,random,json,time,sys
from pathlib import Path
from mathutils import Vector
BASE=Path(__file__).resolve().parent;ROOT=BASE.parents[2]
OUT=BASE/'v001';OUT.mkdir(exist_ok=True);(OUT/'Frames').mkdir(exist_ok=True);(OUT/'Review').mkdir(exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1280;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.render.fps=24;scene.frame_start=1;scene.frame_end=132
scene.eevee.taa_render_samples=24;scene.eevee.use_gtao=True;scene.eevee.gtao_distance=3;scene.eevee.gtao_factor=1.2
scene.eevee.use_bloom=True;scene.eevee.bloom_intensity=.055;scene.eevee.bloom_radius=6;scene.eevee.bloom_threshold=1
scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGB'
scene.view_settings.view_transform='Filmic';scene.view_settings.look='Medium High Contrast';scene.view_settings.exposure=-.3
world=bpy.data.worlds.new('DeepNavyWorld');world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.008,.016,.043,1);world.node_tree.nodes['Background'].inputs[1].default_value=.4;scene.world=world

def mat(name,color,metal=0,emission=0):
    m=bpy.data.materials.new(name);m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF')
    n.inputs['Base Color'].default_value=(*color,1);n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=.28
    n.inputs['Emission'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=emission
    return m
gold=mat('AntiqueBrass',(.62,.32,.075),.8,.08);lit=mat('GoldLuminous', (1,.59,.17),.3,4)
ice=mat('CrystalBlue',(.12,.58,1),.5,2.5);ivory=mat('IvoryCore',(1,.84,.48),.1,8)
dark=mat('ObsidianStage',(.013,.025,.049),.55);dustmat=mat('DistantGoldDust',(.61,.43,.17),0,2)

def parent_empty(name,location=(0,0,0),rotation=(0,0,0)):
    o=bpy.data.objects.new(name,None);scene.collection.objects.link(o);o.location=location;o.rotation_euler=rotation;return o
def mesh_assign(o,name,material,parent=None):
    o.name=name;o.data.materials.append(material)
    if parent:o.parent=parent
    return o
def torus(name,r,thickness,material,parent=None,location=(0,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=thickness,major_segments=128,minor_segments=8,location=location)
    return mesh_assign(bpy.context.object,name,material,parent)
def cube(name,loc,scale,material,parent=None):
    bpy.ops.mesh.primitive_cube_add(size=2,location=loc);o=mesh_assign(bpy.context.object,name,material,parent);o.scale=scale;return o
def gem(name,location,size,material,parent=None):
    verts=[(0,0,1.3),(0,0,-1)]+[(.46*math.cos(i*math.tau/6),.46*math.sin(i*math.tau/6),0) for i in range(6)]
    faces=[(0,2+i,2+(i+1)%6) for i in range(6)]+[(1,2+(i+1)%6,2+i) for i in range(6)]
    mesh=bpy.data.meshes.new(name+'_mesh');mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new(name,mesh);scene.collection.objects.link(o);o.location=location;o.scale=(size,)*3;o.data.materials.append(material)
    if parent:o.parent=parent
    return o
def key(o,prop,f,value):
    setattr(o,prop,value);o.keyframe_insert(data_path=prop,frame=f)
def polish_keys(o):
    if o.animation_data and o.animation_data.action:
        for curve in o.animation_data.action.fcurves:
            for k in curve.keyframe_points:k.interpolation='BEZIER';k.handle_left_type='AUTO_CLAMPED';k.handle_right_type='AUTO_CLAMPED'

# Compact platform only; no palace/city/character modeling.
bpy.ops.mesh.primitive_cylinder_add(vertices=96,radius=6.5,depth=.28,location=(0,0,-.2));mesh_assign(bpy.context.object,'SummonPlatform',dark)
for r in [6.3,5.9,4.7,4.5,2.0]:torus('PlatformBrass_%s'%r,r,.018,lit,location=(0,0,-.045))
for i in range(24):
    t=i*math.tau/24;o=cube('FloorCompass_%02d'%i,(5.45*math.cos(t),5.45*math.sin(t),-.04),(.20,.035,.012),gold);o.rotation_euler.z=t

rigs=[]
for label,r,y,spin in [('Giant',4.35,1.05,.9),('Outer',3.6,0,-1.3),('Inner',2.45,-.8,1.8)]:
    root=parent_empty(label+'RingRig',(0,y,4.4),(math.pi/2,0,0));rigs.append(root)
    for j,offset in enumerate([0,-.16,-.29]):torus(label+'Band_%d'%j,r+offset,.025 if j==0 else .012,lit if j==1 else gold,root)
    count=64 if label=='Giant' else 48
    for i in range(count):
        t=i*math.tau/count;long=.105 if i%4==0 else .045
        o=cube(label+'Tick_%02d'%i,((r-.08)*math.cos(t),(r-.08)*math.sin(t),0),(long,.015,.023),lit if i%4==0 else gold,root);o.rotation_euler.z=t
    for i in range(8):
        t=i*math.tau/8;g=gem(label+'Crest_%d'%i,(r*math.cos(t),r*math.sin(t),0),.20,ivory if i%2==0 else gold,root)
        g.rotation_euler=(math.pi/2,0,t)
    key(root,'rotation_euler',1,(math.pi/2,0,0));key(root,'rotation_euler',74,(math.pi/2,spin*1.4,0));key(root,'rotation_euler',112,(math.pi/2,spin*2.6,0));key(root,'rotation_euler',132,(math.pi/2,spin*3.5,0))
    key(root,'scale',1,(.68,)*3);key(root,'scale',36,(1,)*3);key(root,'scale',90,(1,)*3);key(root,'scale',108,(1.12,)*3);key(root,'scale',132,(1.45,)*3);polish_keys(root)

# Eight faceted crystal columns assemble around the inner aperture.
crystal_root=parent_empty('CrystalArray',(0,-.15,4.4))
for i in range(8):
    t=i*math.tau/8;loc=(3.0*math.cos(t),-.3+.35*math.sin(t*2),3.0*math.sin(t))
    g=gem('FloatingCrystal_%02d'%i,loc,.50,ice if i%2 else ivory,crystal_root)
    g.rotation_euler=(.25,t*.4,t+.3)
    key(g,'scale',1,(.06,)*3);key(g,'scale',42+i,(.50,)*3);key(g,'scale',95,(.56,)*3);key(g,'scale',118,(.16,)*3)
    key(g,'rotation_euler',1,(.25,t*.4,t));key(g,'rotation_euler',132,(.5,t*.4+2,t+3));polish_keys(g)
key(crystal_root,'rotation_euler',1,(0,0,0));key(crystal_root,'rotation_euler',108,(0,.28,0));key(crystal_root,'rotation_euler',132,(0,.72,0));polish_keys(crystal_root)
core=gem('CentralCrystal',(0,-.10,4.4),.55,ivory)
key(core,'scale',1,(.28,)*3);key(core,'scale',70,(.65,)*3);key(core,'scale',90,(.28,)*3);key(core,'scale',108,(1.9,)*3);key(core,'scale',119,(.001,)*3);polish_keys(core)
key(core,'rotation_euler',1,(0,0,0));key(core,'rotation_euler',119,(.3,0,3.8));polish_keys(core)

# Light column/corridor is reusable geometric light, not any player signature.
for i in range(6):
    t=i*math.tau/6;o=cube('PortalLightRay_%d'%i,(.22*math.cos(t),2.7,4.4+.22*math.sin(t)),(.009,4.6,.009),lit)
    key(o,'scale',1,(.001,.001,.001));key(o,'scale',56,(.009,4.6,.009));key(o,'scale',99,(.023,7,.023));key(o,'scale',120,(.005,7,.005));polish_keys(o)

random.seed(71008052)
for i in range(110):
    x=random.uniform(-11,11);y=random.uniform(2,16);z=random.uniform(.4,12)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=random.uniform(.014,.038),location=(x,y,z));o=mesh_assign(bpy.context.object,'StarDust_%03d'%i,dustmat)
    key(o,'location',1,(x,y,z));key(o,'location',132,(x,y-1,z+.28));polish_keys(o)

for name,loc,power,color,size in [('GoldKey',(-4,-5,9),1700,(1,.68,.28),7),('BlueRim',(4,2,8),2300,(.25,.48,1),6),('FrontSoft',(0,-8,6),1100,(.7,.8,1),5)]:
    bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name=name;o.data.energy=power;o.data.color=color;o.data.size=size;o.rotation_euler=(Vector((0,0,4))-o.location).to_track_quat('-Z','Y').to_euler()

target=parent_empty('CameraAim',(0,0,4.4));bpy.ops.object.camera_add(location=(2.7,-16,6));cam=bpy.context.object;cam.name='PortalTraverseCamera';scene.camera=cam;cam.data.lens=34;cam.data.clip_start=.07;cam.data.clip_end=120
con=cam.constraints.new('TRACK_TO');con.target=target;con.track_axis='TRACK_NEGATIVE_Z';con.up_axis='UP_Y'
for f,loc,aim in [(1,(2.7,-16,6),(0,0,4.4)),(40,(1.2,-11.2,5.1),(0,.5,4.4)),(78,(0,-7,4.4),(0,2,4.4)),(94,(0,-6.4,4.4),(0,3,4.4)),(112,(0,-2.7,4.4),(0,6,4.4)),(124,(0,2.0,4.4),(0,12,4.4)),(132,(0,6,4.4),(0,16,4.4))]:
    key(cam,'location',f,loc);key(target,'location',f,aim)
polish_keys(cam);polish_keys(target)

# Final white-gold curtain for Unity transition, no UI or names baked into it.
flash=bpy.data.materials.new('FinalLightCurtain');flash.use_nodes=True;nodes=flash.node_tree.nodes;nodes.clear();out=nodes.new('ShaderNodeOutputMaterial');mix=nodes.new('ShaderNodeMixShader');trans=nodes.new('ShaderNodeBsdfTransparent');emit=nodes.new('ShaderNodeEmission');emit.inputs[0].default_value=(1,.90,.67,1);emit.inputs[1].default_value=5
flash.node_tree.links.new(trans.outputs[0],mix.inputs[1]);flash.node_tree.links.new(emit.outputs[0],mix.inputs[2]);flash.node_tree.links.new(mix.outputs[0],out.inputs[0]);flash.blend_method='BLEND';flash.use_screen_refraction=False
for f,v in [(1,0),(102,0),(113,.06),(120,.45),(128,1),(132,1)]:mix.inputs[0].default_value=v;mix.inputs[0].keyframe_insert(data_path='default_value',frame=f)
bpy.ops.mesh.primitive_plane_add(size=2);plane=bpy.context.object;plane.name='FinalCurtain';plane.parent=cam;plane.location=(0,0,-1);plane.scale=(3,3,3);plane.data.materials.append(flash)

scene.timeline_markers.new('Establish_UnityTime1.15',frame=1);scene.timeline_markers.new('RingsLocked_UnityTime3.4',frame=55);scene.timeline_markers.new('BriefRecoil_UnityTime4.95',frame=92);scene.timeline_markers.new('GateBurst_UnityTime5.4',frame=103);scene.timeline_markers.new('UnityLightCurtain',frame=132)
scene.render.filepath=str(OUT/'Frames/frame_');scene.frame_set(1)
blend=OUT/'A05_SummonStage_v001.blend';bpy.ops.wm.save_as_mainfile(filepath=str(blend))
parameters={'task':'A05-BLENDER','blender':bpy.app.version_string,'engine':scene.render.engine,'resolution':[1280,720],'fps':24,'frames':[1,132],'seconds':5.5,'whole_ceremony_with_Unity_front_seconds':6.65,'samples':24,'geometry_seed':71008052,'rings':3,'floating_crystals':8,'camera_lens_mm':34,'bloom_intensity':.055,'exposure':-.3,'audio':'none; P04 separate voiceover','text_or_username_or_character_baked':False,'review_frames':[1,70,112],'blend_file':str(blend),'status':'candidate_pending_render_review'}
(OUT/'parameters_v001.json').write_text(json.dumps(parameters,ensure_ascii=False,indent=2),encoding='utf-8')
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if 'preview' in args:
    for label,frame in [('01_establish',1),('02_giant_ring',70),('03_gate_burst',112)]:
        scene.frame_set(frame);scene.render.filepath=str(OUT/'Review'/f'{label}_v001.png');bpy.ops.render.render(write_still=True)
elif 'render' in args:
    bpy.ops.render.render(animation=True)
print('A05 Blender stage saved; task mode '+str(args))
