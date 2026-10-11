import bpy,json,time
from pathlib import Path
base=Path(__file__).resolve().parent
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.eevee.taa_render_samples=8
s.render.resolution_x=320;s.render.resolution_y=180;s.render.resolution_percentage=100
bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,location=(0,0,0))
bpy.ops.object.light_add(type='AREA',location=(1,-2,3));bpy.context.object.data.energy=500
bpy.ops.object.camera_add(location=(0,-4,2));cam=bpy.context.object
from mathutils import Vector
cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();s.camera=cam
base.joinpath('Probe').mkdir(exist_ok=True)
s.render.filepath=str(base/'Probe/eevee_probe_v001.png');s.render.image_settings.file_format='PNG'
t=time.time();bpy.ops.render.render(write_still=True)
(base/'Probe/capability_v001.json').write_text(json.dumps({'blender':bpy.app.version_string,'build_hash':bpy.app.build_hash.decode(),'engine':s.render.engine,'size':[320,180],'samples':8,'elapsed_seconds':round(time.time()-t,3),'status':'actual EEVEE PNG render completed'},indent=2),encoding='utf-8')
