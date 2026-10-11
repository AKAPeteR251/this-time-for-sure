"""Click-driven A05 assets. Static matching alpha states and one-shot rupture sequence."""
import bpy,json
from pathlib import Path
from mathutils import Vector
BASE=Path(__file__).resolve().parent;OUT=BASE/'v004_click'
OUT.mkdir(exist_ok=True)
for folder in ['States','Effects','Rupture','Review']:(OUT/folder).mkdir(exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(BASE/'v003/A05_SummonStage_v003.blend'))
scene=bpy.context.scene;cam=scene.camera;rig=bpy.data.objects['FractureCrystalRig']
scene.frame_set(62)
def descendants(o):
    result=[]
    for child in o.children:result.append(child);result.extend(descendants(child))
    return result
crystal=set([rig]+descendants(rig))
for o in crystal:o.hide_render=True
for o in bpy.data.objects:
    if o.name.startswith('GateShockwave_') or o.name=='FinalCurtain':o.hide_render=True
scene.render.filepath=str(OUT/'background_giant_rings_720p_v004.png');scene.render.image_settings.color_mode='RGB';scene.render.film_transparent=False
bpy.ops.render.render(write_still=True)
# All states share camera, placement, scale and 1024-square canvas. User interaction owns time.
for o in bpy.data.objects:o.hide_render=o not in crystal and o.type not in ['LIGHT','CAMERA']
cam.animation_data_clear();cam.data.animation_data_clear()
for c in list(cam.constraints):cam.constraints.remove(c)
cam.location=(0,-6,4.4);cam.rotation_euler=(Vector((0,0,4.4))-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.type='ORTHO';cam.data.ortho_scale=3.5
scene.render.resolution_x=1024;scene.render.resolution_y=1024
scene.render.image_settings.color_mode='RGBA';scene.render.film_transparent=True
rig.animation_data_clear();rig.location=(0,-.1,4.4);rig.scale=(.94,)*3
lights=[o for o in crystal if o.name.startswith('CrackLightBeam_') or o.name=='ReleasedInnerLight']
for o in lights:o.hide_render=True
for name,frame in [('00_intact',62),('01_first_cracks',82),('02_deep_cracks',99)]:
    scene.frame_set(frame);scene.render.filepath=str(OUT/'States'/(name+'_v004.png'));bpy.ops.render.render(write_still=True)
for o in crystal:o.hide_render=o not in lights
for o in lights:o.hide_render=False
scene.frame_set(99);scene.render.filepath=str(OUT/'Effects/light_leak_v004.png');bpy.ops.render.render(write_still=True)
for o in crystal:o.hide_render=False
scene.frame_start=103;scene.frame_end=126
scene.render.filepath=str(OUT/'Rupture/crystal_')
scene.frame_set(103)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'A05_ClickCrystal_v004.blend'))
for index,frame in enumerate(range(103,127)):
    scene.frame_set(frame);scene.render.filepath=str(OUT/'Rupture'/('crystal_%03d.png'%index));bpy.ops.render.render(write_still=True)
manifest={'task':'A05-BLENDER','version':'v004_click','status':'assets_generated_pending_visual_review_and_Unity','source_master':'v003/A05_SummonStage_v003.blend','canvas':[1024,1024],'alpha':'RGBA transparent film','pivot':[.5,.5],'states':['00_intact','01_first_cracks','02_deep_cracks'],'clicks':['First click: intact -> first cracks','Second click: first cracks -> deep cracks + optional independent light leak','Third click: play rupture sequence once -> Unity portal/reveal'],'rupture_frames':24,'rupture_fps_reference':24,'rupture_reference_seconds':1,'idle_and_wait':'Unity indefinitely; no forced timeout','vibration':'Unity transform offset; static states contain no vibration','username_signature_UI_character':'not baked','background':[1280,720],'integration':'not performed','geometry':'12 actual tetrahedral crystal fragments','scale_changes_between_static_states':False}
(OUT/'parameters_v004.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
print('Click assets completed: 3 alpha states, separate leak, 24 rupture frames, backdrop and master')
