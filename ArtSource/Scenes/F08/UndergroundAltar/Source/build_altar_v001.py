"""F08 modular enclosed chamber, actual static FBX export and roundtrip."""
import bpy,bmesh,math,json,hashlib
from pathlib import Path
from mathutils import Vector,Matrix
R=Path(__file__).resolve().parent.parent;T=R/'Textures'
for folder in ['Models','Preview','Source']:(R/folder).mkdir(exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True);s=bpy.context.scene
s.unit_settings.system='METRIC';s.unit_settings.scale_length=1
env=bpy.data.collections.new('F08_ENVIRONMENT');s.collection.children.link(env)
refs=bpy.data.collections.new('F08_REFERENCE_Only');s.collection.children.link(refs)
def move(o,c):
 for old in list(o.users_collection):old.objects.unlink(o)
 c.objects.link(o)
root=bpy.data.objects.new('F08_UndergroundAltar',None);env.objects.link(root)
def pbr(name,prefix):
 m=bpy.data.materials.new(name);m.use_nodes=True;n=m.node_tree.nodes;links=m.node_tree.links;bs=n['Principled BSDF']
 for channel,socket,cs in [('basecolor','Base Color','sRGB'),('roughness','Roughness','Non-Color'),('metallic','Metallic','Non-Color')]:
  tex=n.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(T/(prefix+'_'+channel+'_2048_v001.png')));tex.image.colorspace_settings.name=cs;tex.extension='REPEAT';links.new(tex.outputs['Color'],bs.inputs[socket])
 tex=n.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(T/(prefix+'_normal_2048_v001.png')));tex.image.colorspace_settings.name='Non-Color';norm=n.new('ShaderNodeNormalMap');links.new(tex.outputs['Color'],norm.inputs['Color']);links.new(norm.outputs['Normal'],bs.inputs['Normal'])
 return m
stone=pbr('F08_DeepBlueStone','stone');gold=pbr('F08_AntiqueGold','antique_gold')
def simple(name,c,metal=0,rough=.7,emission=0):
 m=bpy.data.materials.new(name);m.use_nodes=True;n=m.node_tree.nodes['Principled BSDF'];n.inputs['Base Color'].default_value=(*c,1);n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=rough;n.inputs['Emission'].default_value=(*c,1);n.inputs['Emission Strength'].default_value=emission;return m
niche=simple('F08_NicheShadowStone',(.007,.012,.025));glow=simple('F08_AltarInlayEmission',(1,.50,.13),.2,.4,1.5)
def setup(o,name,mat,parent=root,pivot=None):
 o.name=name;move(o,env);o.parent=parent;o.data.materials.append(mat)
 if pivot is not None:
  delta=Vector(pivot)-o.location;o.data.transform(Matrix.Translation(-delta));o.location=Vector(pivot)
 return o
def cube(name,loc,size,mat):
 bpy.ops.mesh.primitive_cube_add(size=2,location=loc);o=bpy.context.object;o.scale=Vector(size)/2;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return setup(o,name,mat)
def cyl(name,radius,depth,base,mat,vertices=64):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=(base[0],base[1],base[2]+depth/2));return setup(bpy.context.object,name,mat,pivot=base)
def torus(name,r,minor,z,mat,x=0,y=0):
 bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=minor,major_segments=96,minor_segments=8,location=(x,y,z));return setup(bpy.context.object,name,mat)
def mesh(name,verts,faces,mat,pivot=(0,0,0)):
 data=bpy.data.meshes.new(name+'_mesh');data.from_pydata(verts,[],faces);data.update();o=bpy.data.objects.new(name,data);env.objects.link(o);o.parent=root;o.location=pivot;o.data.materials.append(mat);return o
def wedge(name,a0,a1,r0,r1,z0,z1,mat):
 points=[]
 for z in [z0,z1]:
  for r,a in [(r0,a0),(r1,a0),(r1,a1),(r0,a1)]:points.append((r*math.cos(a),r*math.sin(a),z))
 pivot=( (r0+r1)/2*math.cos((a0+a1)/2),(r0+r1)/2*math.sin((a0+a1)/2),z0)
 points=[tuple(Vector(p)-Vector(pivot)) for p in points]
 return mesh(name,points,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat,pivot)
def join(objs,name,pivot):
 bpy.ops.object.select_all(action='DESELECT')
 for o in objs:o.select_set(True)
 bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();o=objs[0];o.name=name
 world=o.matrix_world.copy();o.data.transform(world);o.data.transform(Matrix.Translation(-Vector(pivot)));o.matrix_world=Matrix.Translation(Vector(pivot));o.parent=root;return o
# Three shallow circular layers. No new crystal pedestal.
cyl('Altar_Steps_Lower',8,.18,(0,0,0),stone,96)
cyl('Altar_Steps_Upper',7.65,.18,(0,0,.18),stone,96)
cyl('Altar_Base',7.30,.18,(0,0,.36),stone,96)
for i,(r,z) in enumerate([(7.96,.16),(7.61,.34),(7.25,.52),(8.45,.012),(10.7,.012)]):torus('Altar_Inlay_Metal_%02d'%i,r,.022,z,gold)
for i,r in enumerate([2.3,4.3,6.85]):torus('Altar_Inlay_Emission_%02d'%i,r,.010,.552,glow)
for i in range(16):
 a=i*math.tau/16;o=cube('Altar_Inlay_MetalTick_%02d'%i,(6.2*math.cos(a),6.2*math.sin(a),.55),(.28,.035,.015),gold);o.rotation_euler.z=a
for i in range(24):wedge('Floor_Module_%02d'%i,i*math.tau/24,(i+1)*math.tau/24,.01,15.5,-.3,0,stone)
# All 24 full wall wedges: no missing rear wall or star-sky holes.
for i in range(24):wedge('Wall_Module_%02d'%i,i*math.tau/24,(i+1)*math.tau/24,15,15.65,0,10,stone)
for r,z in [(14.98,.28),(14.98,9.6)]:torus('Wall_Cornice_%s'%z,r,.13,z,stone);torus('Wall_GoldBorder_%s'%z,r-.03,.016,z-.1,gold)
# Reusable thick pillars with separate gold collar mesh.
for i in range(8):
 a=(i+.5)*math.tau/8;x=12.9*math.cos(a);y=12.9*math.sin(a)
 parts=[cyl('pillarpart',.83,.32,(x,y,0),stone,12),cyl('pillarpart',.61,8.85,(x,y,.32),stone,12),cyl('pillarpart',.82,.35,(x,y,9.17),stone,12)]
 join(parts,'Pillar_Module_%02d'%i,(x,y,0))
 goldparts=[torus('collar',.64,.045,.47,gold,x,y),torus('collar',.68,.045,8.96,gold,x,y)]
 join(goldparts,'Pillar_GoldTrim_%02d'%i,(x,y,0))
# Niches are recessed dark panels on a fully sealed wall, with raised arch geometry.
for i in range(6):
 a=(i+.5)*math.tau/6;r=14.84;x=r*math.cos(a);y=r*math.sin(a);rot=a-math.pi/2
 o=cube('Wall_Alcove_Back_%02d'%i,(x,y,4.35),(2.5,.07,4.6),niche);o.rotation_euler.z=rot
 pieces=[]
 for side in [-1,1]:
  px=x+side*1.35*math.cos(rot);py=y+side*1.35*math.sin(rot)
  c=cube('archleg',(px,py,3.7),(.22,.22,3.35),stone);c.rotation_euler.z=rot;pieces.append(c)
 for j in range(12):
  t=(j+.5)*math.pi/12;lx=1.35*math.cos(t);z=5.4+1.35*math.sin(t)
  c=cube('archstone',(x+lx*math.cos(rot),y+lx*math.sin(rot),z),(.37,.25,.22),stone);c.rotation_euler=(math.sin(rot)*(t-math.pi/2),-math.cos(rot)*(t-math.pi/2),rot);pieces.append(c)
 join(pieces,'Wall_Alcove_Arch_%02d'%i,(x,y,2))
 # One geometric emblem style, repeated, no letters.
 em=[]
 for rr in [.36,.65]:
  e=torus('emblemring',rr,.024,4.75,gold,x*.99,y*.99);e.rotation_euler=(math.pi/2,0,rot);em.append(e)
 for side in [-1,1]:
  e=cube('emblemspike',(x*.99+side*.35*math.cos(rot),y*.99+side*.35*math.sin(rot),4.1),(.025,.025,1.4),gold);em.append(e)
 join(em,'Wall_Emblem_%02d'%i,(x,y,4.75))
# Closed dome shell, 0.5m thick, correctly oriented inner and outer surfaces.
verts=[];segments=96;rings=14
for outer in [False,True]:
 for k in range(rings):
  r=.2+(15.65-.2)*k/(rings-1);z=10+3.2*(1-(r/15.65)**2)+(.5 if outer else 0)
  for j in range(segments):a=j*math.tau/segments;verts.append((r*math.cos(a),r*math.sin(a),z))
faces=[];offset=rings*segments
for layer in range(2):
 base=layer*offset
 for k in range(rings-1):
  for j in range(segments):
   a=base+k*segments+j;b=base+k*segments+(j+1)%segments;c=b+segments;d=a+segments
   faces.append((a,d,c,b) if layer==0 else (a,b,c,d))
 faces.append(tuple(base+j for j in (range(segments-1,-1,-1) if layer==0 else range(segments))))
for j in range(segments):
 k=(rings-1)*segments;faces.append((k+j,k+(j+1)%segments,offset+k+(j+1)%segments,offset+k+j))
mesh('Ceiling_Dome',verts,faces,stone)
ribs=[]
for r in [3.8,7.4,10.7,14.7]:
 z=10+3.2*(1-(r/15.65)**2)-.09;ribs.append(torus('rib',r,.14,z,stone));torus('Ceiling_GoldInlay_%s'%r,r,.016,z-.135,gold)
join(ribs,'Ceiling_Ribs',(0,0,10))
# Four braziers, reusable base and hollow metal bowl, no baked flame mesh in FBX.
braziers=[]
for i,(x,y) in enumerate([(-9,-3.8),(9,-3.8),(-10.5,6),(10.5,6)]):
 baseparts=[cyl('base',.60,.25,(x,y,0),stone,8),cyl('base',.39,1.48,(x,y,.25),stone,8),cyl('base',.57,.22,(x,y,1.73),gold,8)]
 join(baseparts,'Brazier_Base_%02d'%i,(x,y,0))
 bowlverts=[];profile=[(.20,1.89),(.64,2.12),(.68,2.20),(.60,2.20),(.18,1.98)]
 for rr,z in profile:
  for j in range(32):a=j*math.tau/32;bowlverts.append((rr*math.cos(a),rr*math.sin(a),z-1.89))
 bowlfaces=[]
 for k in range(len(profile)-1):
  for j in range(32):bowlfaces.append((k*32+j,k*32+(j+1)%32,(k+1)*32+(j+1)%32,(k+1)*32+j))
 bowlfaces.append(tuple(range(31,-1,-1)));bowlfaces.append(tuple((len(profile)-1)*32+j for j in range(32)))
 mesh('Brazier_Bowl_%02d'%i,bowlverts,bowlfaces,gold,(x,y,1.89));braziers.append((x,y,2.2))
# Valid normals, applied object rotation/scales, tiling UV0; UV1 lightmaps left for Unity.
for o in list(env.objects):
 if o.type!='MESH':continue
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(o.data);bm.free()
 uv=o.data.uv_layers.new(name='UV0_Tiling')
 for face in o.data.polygons:
  normal=face.normal;axis=max(range(3),key=lambda a:abs(normal[a]));axes=[a for a in range(3) if a!=axis]
  for li in face.loop_indices:
   v=o.matrix_world@o.data.vertices[o.data.loops[li].vertex_index].co;uv.data[li].uv=(v[axes[0]]/2.8,v[axes[1]]/2.8)
# F07 unchanged mesh instances solely for reference, moved vertically as one root onto altar.
f07=R.parents[1]/'Effects/F07/CrystalLayers/Source/F07_CrystalStage_v001.blend'
with bpy.data.libraries.load(str(f07),link=False) as (src,dst):dst.objects=[n for n in src.objects if n in ['CrystalStage','CrystalBody','RingOuter','RingMiddle','RingInner','RingFloor'] or n.startswith('Shard_') or n.startswith('FloatingIce_') or n.startswith('FloatingGold_')]
for o in dst.objects:
 if o is not None:refs.objects.link(o);o.hide_render=False
froot=bpy.data.objects['CrystalStage'];froot.location.z+=.585
# Preview-only flames from actual supplied frame and cool/warm lights.
fm=bpy.data.materials.new('REFERENCE_Flame_Only');fm.use_nodes=True;fm.blend_method='BLEND';n=fm.node_tree.nodes;n.clear();tex=n.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(R/'Source/flame_frame00_reference_v001.png'));emit=n.new('ShaderNodeEmission');emit.inputs[1].default_value=2;trans=n.new('ShaderNodeBsdfTransparent');mix=n.new('ShaderNodeMixShader');out=n.new('ShaderNodeOutputMaterial');L=fm.node_tree.links;L.new(tex.outputs['Color'],emit.inputs[0]);L.new(tex.outputs['Alpha'],mix.inputs[0]);L.new(trans.outputs[0],mix.inputs[1]);L.new(emit.outputs[0],mix.inputs[2]);L.new(mix.outputs[0],out.inputs[0])
for i,(x,y,z) in enumerate(braziers):
 bpy.ops.mesh.primitive_plane_add(size=2,location=(x,y,z+.65),rotation=(math.pi/2,0,0));o=bpy.context.object;o.name='REFERENCE_Flame_%02d'%i;move(o,refs);o.scale=(.7,.9,1);o.data.materials.append(fm)
 bpy.ops.object.light_add(type='POINT',location=(x,y,z+.7));o=bpy.context.object;o.name='REF_Amber_%02d'%i;move(o,refs);o.data.color=(1,.38,.08);o.data.energy=600;o.data.shadow_soft_size=1
for name,loc,power,color,size in [('CoolCrystal',(0,-1,6),1500,(.12,.5,1),5),('BlueRoom',(0,-5,10),1800,(.20,.37,.65),10),('WarmFront',(0,-10,8),800,(.65,.46,.25),9),('BackRead',(0,8,8),1000,(.18,.32,.52),8)]:
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name='REF_'+name;move(o,refs);o.data.energy=power;o.data.color=color;o.data.size=size;o.rotation_euler=(Vector((0,0,3))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(0,-13.7,7.2));cam=bpy.context.object;cam.name='REF_GameCamera';move(cam,refs);s.camera=cam
def aim(loc,target,lens):cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=lens
aim((0,-13.7,7.2),(0,0,5.6),17)
world=bpy.data.worlds.new('F08_InteriorWorld');world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.007,.012,.025,1);world.node_tree.nodes['Background'].inputs[1].default_value=.18;s.world=world
s.render.engine='BLENDER_EEVEE';s.eevee.taa_render_samples=48;s.eevee.use_gtao=True;s.eevee.gtao_distance=2;s.eevee.use_bloom=True;s.eevee.bloom_intensity=.045;s.eevee.use_ssr=True
s.render.resolution_x=1280;s.render.resolution_y=720;s.render.resolution_percentage=100;s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGB'
s.view_settings.view_transform='Filmic';s.view_settings.look='Medium High Contrast';s.view_settings.exposure=.35
s.render.filepath=str(R/'Preview/front_gameview.png');bpy.ops.render.render(write_still=True)
aim((0,-9,5.3),(0,0,4.8),26);s.render.filepath=str(R/'Preview/crystal_closeup.png');bpy.ops.render.render(write_still=True)
# Side section is an explicit diagnostic view; source/export retains all wall/dome pieces.
for o in env.objects:
 if o.name.startswith('Wall_Module_') and o.location.x>0:o.hide_render=True
 if o.name.startswith('Wall_Alcove_') or o.name.startswith('Wall_Emblem_'):o.hide_render=o.location.x>0
 if o.name in ['Ceiling_Dome','Ceiling_Ribs'] or o.name.startswith('Ceiling_GoldInlay'):o.hide_render=True
aim((24,-18,20),(0,0,4),26);s.render.filepath=str(R/'Preview/side_structure.png');bpy.ops.render.render(write_still=True)
for o in env.objects:o.hide_render=False
aim((0,-13.7,7.2),(0,0,5.6),17)
# Portable blend: pack images, also ship separate maps for Unity.
for image in bpy.data.images:
 if image.source=='FILE':image.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(R/'Source/F08_UndergroundAltar_v001.blend'))
bpy.ops.object.select_all(action='DESELECT')
for o in env.objects:o.select_set(True)
bpy.context.view_layer.objects.active=root
fbx=R/'Models/F08_UndergroundAltar_v001.fbx'
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'EMPTY','MESH'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',use_space_transform=True,bake_space_transform=False,mesh_smooth_type='FACE',bake_anim=False,path_mode='AUTO',embed_textures=False)
def inventory(obs):
 result=[]
 for o in obs:
  if o.type not in ['EMPTY','MESH']:continue
  j={'name':o.name,'parent':o.parent.name if o.parent else None,'scale':list(o.scale),'type':o.type}
  if o.type=='MESH':j.update({'vertices':len(o.data.vertices),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'uv_count':len(o.data.uv_layers),'materials':[m.name if m else None for m in o.data.materials],'bounds':[list(o.matrix_world@Vector(v)) for v in o.bound_box]})
  result.append(j)
 return result
original=inventory(env.objects)
params={'task':'F08-ALTAR','blender':bpy.app.version_string,'room_inner_radius_m':15,'wall_height_m':10,'dome_inner_apex_m':13.2,'dome_thickness_m':.5,'altar_radii_m':[8,7.65,7.3],'altar_top_m':.54,'F07_reference_root_offset_z_m':.585,'F07_original_mesh_unchanged':True,'F07_exported_in_environment_fbx':False,'floor_z_m':0,'root_scale':[1,1,1],'export_forward':'-Z','export_up':'Y','lightmap_uv1':'not authored; Unity Generate Lightmap UVs if baking','camera_front':{'position':list(cam.location),'rotation_radians':list(cam.rotation_euler),'lens_mm':17},'side_preview':'section diagnostic; ceiling and near side hidden ONLY for that preview','export_inventory':original}
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(fbx),use_anim=False)
imported=inventory(bpy.context.scene.objects);a={x['name']:x for x in original};b={x['name']:x for x in imported};assert set(a)==set(b)
error=0
for name,x in a.items():
 y=b[name];assert x['parent']==y['parent'] and all(abs(v-1)<1e-4 for v in y['scale'])
 if x['type']=='MESH':
  assert x['vertices']==y['vertices'] and y['uv_count']>0 and x['materials']==y['materials'],name
  d=max((Vector(u)-Vector(v)).length for u,v in zip(x['bounds'],y['bounds']));error=max(error,d);assert d<.001,(name,d)
params['roundtrip']={'passed':True,'mesh_count':sum(x['type']=='MESH' for x in imported),'node_count':len(imported),'triangles':sum(x.get('triangles',0) for x in imported),'max_bbox_corner_error_m':error,'Unity_validation':'not performed'}
params['import_inventory']=imported
(R/'Source/model_checks_v001.json').write_text(json.dumps(params,ensure_ascii=False,indent=2),encoding='utf-8')
print('F08 actual environment FBX export/reimport checks passed')
