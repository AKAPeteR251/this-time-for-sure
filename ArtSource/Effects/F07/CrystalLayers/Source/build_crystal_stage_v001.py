"""F07 static, independently addressable stage. Actual Blender 2.93 FBX export/reimport."""
import bpy,bmesh,json,hashlib,math
from pathlib import Path
from mathutils import Vector,Matrix
BASE=Path(__file__).resolve().parent.parent
SOURCE=BASE.parents[2]/'Scenes/SummonStage/v004_click/A05_ClickCrystal_v004.blend'
for folder in ['Source','Review','Textures']:(BASE/folder).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(SOURCE));scene=bpy.context.scene
scene.frame_set(62)
source_objects=list(bpy.data.objects)
source_collection=bpy.data.collections.new('SOURCE_REFERENCE_NotForExport');scene.collection.children.link(source_collection)
runtime=bpy.data.collections.new('F07_RUNTIME');scene.collection.children.link(runtime)
for o in source_objects:
 for c in list(o.users_collection):c.objects.unlink(o)
 source_collection.objects.link(o)
 o.hide_render=True
source_collection.hide_render=True
for label in ['Giant','Outer','Inner']:
 rig=bpy.data.objects[label+'RingRig'];rig.animation_data_clear();rig.rotation_euler=(math.pi/2,0,0);rig.scale=(1,1,1)
scene.view_layers[0].update()
def empty(name,parent=None,loc=(0,0,0)):
 o=bpy.data.objects.new(name,None);runtime.objects.link(o);o.parent=parent;o.location=loc;return o
root=empty('CrystalStage')
materials={};material_records=[]
def runtime_material(old):
 if old.name in materials:return materials[old.name]
 n=old.node_tree.nodes.get('Principled BSDF') if old.use_nodes else None
 color=tuple(n.inputs['Base Color'].default_value) if n else tuple(old.diffuse_color)
 metal=n.inputs['Metallic'].default_value if n else 0
 rough=n.inputs['Roughness'].default_value if n else .4
 emission=tuple(n.inputs['Emission'].default_value) if n else (0,0,0,1)
 energy=n.inputs['Emission Strength'].default_value if n else 0
 m=bpy.data.materials.new('F07_'+old.name);m.use_nodes=True;q=m.node_tree.nodes['Principled BSDF']
 q.inputs['Base Color'].default_value=color;q.inputs['Metallic'].default_value=metal;q.inputs['Roughness'].default_value=rough
 q.inputs['Emission'].default_value=emission;q.inputs['Emission Strength'].default_value=energy
 m.diffuse_color=color;materials[old.name]=m
 material_records.append({'material':m.name,'source_material':old.name,'base_color_linear_rgba':list(color),'metallic':metal,'roughness':rough,'urp_smoothness_start':1-rough,'emission_linear_rgb':list(emission[:3]),'emission_strength_reference':energy,'textures':[],'shader_mapping':'URP Lit opaque; emission RGB * strength as starting value, tune bloom separately'})
 return m
def clone(o,name,parent,pivot):
 mesh=bpy.data.meshes.new_from_object(o.evaluated_get(bpy.context.evaluated_depsgraph_get()),preserve_all_data_layers=True,depsgraph=bpy.context.evaluated_depsgraph_get())
 mesh.transform(o.matrix_world);mesh.transform(Matrix.Translation(-Vector(pivot)))
 for i,m in enumerate(mesh.materials):mesh.materials[i]=runtime_material(m)
 out=bpy.data.objects.new(name,mesh);runtime.objects.link(out);out.parent=parent
 parentpos=parent.matrix_world.translation if parent else Vector()
 out.location=Vector(pivot)-parentpos;out.hide_render=False
 return out
def merged(name,objects,pivot):
 parts=[clone(o,name+'_part',root,pivot) for o in objects]
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();out=parts[0];out.name=name;return out
for name,label in [('RingOuter','Giant'),('RingMiddle','Outer'),('RingInner','Inner')]:
 rig=bpy.data.objects[label+'RingRig'];merged(name,[o for o in rig.children if o.type=='MESH'],rig.matrix_world.translation)
floor=[o for o in source_objects if o.type=='MESH' and (o.name.startswith('PlatformBrass_') or o.name.startswith('FloorCompass_'))]
merged('RingFloor',floor,(0,0,-.045))
core=bpy.data.objects['FractureCrystalRig'];body=empty('CrystalBody',root,tuple(core.matrix_world.translation));scene.view_layers[0].update()
source_shards=sorted([o for o in core.children if o.name.startswith('CrystalShard_')],key=lambda o:o.name)
assert len(source_shards)==12
for i,o in enumerate(source_shards):clone(o,'Shard_%02d'%i,body,o.matrix_world.translation)
for i,o in enumerate(sorted([o for o in source_objects if o.name.startswith('FloatingCrystal_')],key=lambda o:o.name)):
 gold=i%2==0;variant='B' if i in [3,7] else 'A'
 name='FloatingGold_%02d'%(i//2) if gold else 'FloatingIce_%s_%02d'%(variant,i//2)
 new=clone(o,name,root,o.matrix_world.translation)
 if not gold and variant=='B':
  # Original family, newly widened asymmetric faceted contour. Bake into mesh, not nonuniform object scale.
  for v in new.data.vertices:
   v.co.x*=1.5 if v.co.x>0 else 1.2;v.co.z*=.78
   if v.co.z>0:v.co.x+=.075
  new.data.update()
# Correct normals and generate actual UVs on every exported mesh.
for o in runtime.objects:
 if o.type!='MESH':continue
 bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(o.data);bm.free()
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.15192,island_margin=.02);bpy.ops.object.mode_set(mode='OBJECT')
source_collection.hide_viewport=True
scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
# New reference camera and lights; not part of runtime FBX.
refs=bpy.data.collections.new('F07_REVIEW_Only');scene.collection.children.link(refs)
def move_ref(o):
 for c in list(o.users_collection):c.objects.unlink(o)
 refs.objects.link(o)
bpy.ops.object.camera_add(location=(3,-22,10));cam=bpy.context.object;cam.name='F07_ReviewCamera';move_ref(cam)
cam.rotation_euler=(Vector((0,0,3.5))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='PERSP';cam.data.lens=28;scene.camera=cam
lights=[]
for name,loc,power,color,size in [('GoldKey',(-4,-5,9),1700,(1,.68,.28),7),('BlueRim',(4,2,8),2300,(.25,.48,1),6),('FrontSoft',(0,-8,6),1100,(.7,.8,1),5)]:
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name='F07_'+name;move_ref(o);o.data.energy=power;o.data.color=color;o.data.size=size;o.rotation_euler=(Vector((0,0,4.4))-o.location).to_track_quat('-Z','Y').to_euler()
 lights.append({'name':o.name,'type':'AREA','position':list(loc),'energy_watts':power,'color_linear_rgb':list(color),'size_m':size})
scene.frame_start=1;scene.frame_end=1;scene.frame_set(1)
scene.render.resolution_x=1280;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.render.image_settings.color_mode='RGB';scene.render.film_transparent=False;scene.eevee.taa_render_samples=64;scene.eevee.use_bloom=True;scene.eevee.bloom_intensity=.025
scene.render.filepath=str(BASE/'Review/F07_stage_blender_v001.png');bpy.ops.render.render(write_still=True)
# Static export only: no reference objects, no actions, no UI or camera.
bpy.ops.object.select_all(action='DESELECT')
for o in runtime.objects:o.select_set(True)
bpy.context.view_layer.objects.active=root
blend=BASE/'Source/F07_CrystalStage_v001.blend';bpy.ops.wm.save_as_mainfile(filepath=str(blend))
fbx=BASE/'F07_CrystalStage_v001.fbx'
bpy.ops.export_scene.fbx(filepath=str(fbx),use_selection=True,object_types={'EMPTY','MESH'},global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',use_space_transform=True,bake_space_transform=False,use_mesh_modifiers=True,mesh_smooth_type='FACE',use_tspace=False,add_leaf_bones=False,bake_anim=False,path_mode='AUTO')
def inventory(objects):
 result=[]
 for o in objects:
  if o.type not in ['MESH','EMPTY']:continue
  item={'name':o.name,'parent':o.parent.name if o.parent else None,'type':o.type,'position_local':list(o.location),'scale':list(o.scale)}
  if o.type=='MESH':
   item.update({'vertices':len(o.data.vertices),'polygons':len(o.data.polygons),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'uv_layers':len(o.data.uv_layers),'material_slots':[m.name if m else None for m in o.data.materials],'world_bbox':[list(o.matrix_world@Vector(c)) for c in o.bound_box]})
  result.append(item)
 return sorted(result,key=lambda x:x['name'])
original=inventory(runtime.objects)
report={'task':'F07-ART-CRYSTAL-LAYERS','source_blend':str(SOURCE),'blender':bpy.app.version_string,'units':'meter; 1 Blender unit = 1m','export_axis_forward':'-Z','export_axis_up':'Y','global_scale':1,'bake_animation':False,'source_reference_kept':True,'static_frame_reference':62,'camera':{'position':list(cam.location),'rotation_euler_radians':list(cam.rotation_euler),'lens_mm':28,'sensor_width_mm':cam.data.sensor_width},'lights':lights,'materials':material_records,'export_inventory':original}
# Actual independent round-trip, compare geometry extents and hierarchy after FBX import.
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(fbx),use_anim=False)
imported=inventory(bpy.context.scene.objects);a={i['name']:i for i in original};b={i['name']:i for i in imported}
assert set(a)==set(b),(set(a)-set(b),set(b)-set(a))
errors=[]
for name,x in a.items():
 y=b[name];assert x['parent']==y['parent'],name;assert x['type']==y['type'],name
 assert all(abs(v-1)<1e-5 for v in y['scale']), (name,y['scale'])
 if x['type']=='MESH':
  assert y['uv_layers']>0 and y['vertices']==x['vertices'],name
  assert y['material_slots']==x['material_slots'],name
  distance=max((Vector(u)-Vector(v)).length for u,v in zip(x['world_bbox'],y['world_bbox']));errors.append(distance);assert distance<.001,(name,distance)
report['roundtrip']={'passed':True,'object_count':len(imported),'mesh_count':sum(i['type']=='MESH' for i in imported),'shard_count':len(source_shards),'max_world_bbox_corner_error_m':max(errors),'name_hierarchy_uv_material_slots_scale_checked':True,'unity_import':'not performed'}
report['import_inventory']=imported
report['sha256']={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in [fbx,blend]}
(BASE/'Source/export_checks_v001.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('F07 FBX exported and actually reimported, checked names/hierarchy/UV/material slots/scale/world bounds')
