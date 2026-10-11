"""Review refinement: readable crystal facets, expanding blast rings, wider lock shot."""
from pathlib import Path
src=Path(__file__).with_name('build_stage_v001.py').read_text(encoding='utf-8')
src=src.replace("BASE/'v001'","BASE/'v002'").replace('Stage_v001.blend','Stage_v002.blend').replace('parameters_v001.json','parameters_v002.json').replace('{label}_v001.png','{label}_v002.png')
src=src.replace("(.12,.58,1),.5,2.5","(.12,.58,1),.35,.55").replace("(1,.84,.48),.1,8","(1,.84,.48),.2,1.1")
src=src.replace("core=gem('CentralCrystal',(0,-.10,4.4),.55,ivory)","core_mat=mat('BlueGoldCrystalCore',(.32,.63,1),.25,.65)\ncore=gem('CentralCrystal',(0,-.10,4.4),.55,core_mat)\nfor f,v in [(1,.65),(78,1.3),(94,.25),(104,12),(114,24),(119,0)]:\n    core_mat.node_tree.nodes['Principled BSDF'].inputs['Emission Strength'].default_value=v\n    core_mat.node_tree.nodes['Principled BSDF'].inputs['Emission Strength'].keyframe_insert(data_path='default_value',frame=f)")
src=src.replace("target=parent_empty('CameraAim'", "# Three blast waves expand into the camera at the gate opening.\nfor i in range(3):\n    wave=torus('GateShockwave_%d'%i,.80,.018,lit,location=(0,-.3-i*.12,4.4));wave.rotation_euler.x=math.pi/2\n    key(wave,'scale',1,(.001,)*3);key(wave,'scale',98+i*2,(.001,)*3);key(wave,'scale',106+i*2,(3.3,)*3);key(wave,'scale',117+i*2,(8,)*3);polish_keys(wave)\n\ntarget=parent_empty('CameraAim'")
src=src.replace("polish_keys(cam);polish_keys(target)","polish_keys(cam);polish_keys(target)\nfor f,v in [(1,28),(55,25),(78,31),(112,34),(132,30)]:\n    cam.data.lens=v;cam.data.keyframe_insert(data_path='lens',frame=f)")
src=src.replace("'review_frames':[1,70,112]","'review_frames':[1,55,105]").replace("'camera_lens_mm':34","'camera_lens_mm_keyframes':[[1,28],[55,25],[78,31],[112,34],[132,30]]")
src=src.replace("('02_giant_ring',70),('03_gate_burst',112)","('02_giant_ring',55),('03_gate_burst',105)")
exec(compile(src,str(Path(__file__).with_name('build_stage_v001.py')),'exec'))
