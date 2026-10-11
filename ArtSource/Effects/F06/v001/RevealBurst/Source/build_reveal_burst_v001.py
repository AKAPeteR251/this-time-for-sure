"""Original F06 single-element geometry and procedural light, Blender 2.93."""
import bpy,math,json
from pathlib import Path
from mathutils import Vector
from mathutils.geometry import tessellate_polygon
ROOT=Path(__file__).resolve().parent.parent
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.eevee.taa_render_samples=64
s.eevee.use_bloom=False;s.render.film_transparent=True
s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGBA'
s.view_settings.view_transform='Standard';s.view_settings.look='Medium High Contrast'
world=bpy.data.worlds.new('NeutralWorld');world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.8,.85,.9,1);world.node_tree.nodes['Background'].inputs[1].default_value=.6;s.world=world
bpy.ops.object.camera_add(location=(0,0,5));cam=bpy.context.object;cam.name='AssetCamera';cam.data.type='ORTHO';cam.data.ortho_scale=2;s.camera=cam
bpy.ops.object.light_add(type='AREA',location=(-2,2,4));light=bpy.context.object;light.data.energy=250;light.data.size=5
palette=[]
for i,c in enumerate([(.64,.72,.78),(.82,.88,.91),(.37,.46,.53),(.58,.66,.72),(.95,.97,.98),(.44,.54,.62)]):
 m=bpy.data.materials.new('NeutralFacet_%d'%i);m.use_nodes=True;n=m.node_tree.nodes['Principled BSDF'];n.inputs['Base Color'].default_value=(*c,1);n.inputs['Roughness'].default_value=.32;n.inputs['Metallic'].default_value=.12;palette.append(m)
shapes={
 'A':[(-.03,.83),(.17,.28),(.10,.12),(.20,-.16),(.02,-.82),(-.07,-.46),(-.18,-.28),(-.12,.13),(-.19,.26)],
 'B':[(.08,.82),(.30,.32),(.23,.18),(.38,.05),(.24,-.26),(.04,-.43),(-.11,-.82),(-.18,-.39),(-.36,-.10),(-.25,.05),(-.32,.24)],
 'C':[(.02,.83),(.13,.51),(.05,.18),(.24,.42),(.17,.04),(.05,-.12),(.16,-.38),(.03,-.83),(-.06,-.51),(-.14,-.29),(-.07,-.05),(-.16,.18),(-.08,.53)]}
objects=[]
for label,points in shapes.items():
 verts=[Vector((x,y,0)) for x,y in points]
 if label!='C':
  verts.append(Vector((.015,.04,.115)));faces=[(i,(i+1)%len(points),len(points)) for i in range(len(points))]
 else:
  tris=tessellate_polygon([verts]);faces=[tuple(p if isinstance(p,int) else next(i for i,v in enumerate(verts) if (v-p).length<1e-6) for p in t) for t in tris]
  verts=[Vector((v.x,v.y,.025 if i%3==0 else 0)) for i,v in enumerate(verts)]
 mesh=bpy.data.meshes.new('Shard_%s_facets'%label);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('Shard_'+label,mesh);s.collection.objects.link(o);objects.append(o)
 for m in palette:mesh.materials.append(m)
 for p in mesh.polygons:p.material_index=[1,4,0,3,2,1,5,0,4,2,3,1,5][p.index%13]
# One plane with a soft tapered procedural alpha; shader supplies no embedded shard.
bpy.ops.mesh.primitive_plane_add(size=2);streak=bpy.context.object;streak.name='SingleStreak';streak.scale=(.18,.84,1);objects.append(streak)
m=bpy.data.materials.new('SoftNeutralStreak');m.use_nodes=True;m.blend_method='BLEND';m.use_nodes=True
nodes=m.node_tree.nodes;nodes.clear();links=m.node_tree.links
uv=nodes.new('ShaderNodeTexCoord');sep=nodes.new('ShaderNodeSeparateXYZ');links.new(uv.outputs['UV'],sep.inputs[0])
def op(kind,a,b=None):
 n=nodes.new('ShaderNodeMath');n.operation=kind
 if isinstance(a,(float,int)):n.inputs[0].default_value=a
 else:links.new(a,n.inputs[0])
 if b is not None:
  if isinstance(b,(float,int)):n.inputs[1].default_value=b
  else:links.new(b,n.inputs[1])
 return n.outputs[0]
y=sep.outputs['Y'];x=sep.outputs['X'];width=op('ADD',.013,op('MULTIPLY',op('POWER',y,3),.10))
cross=op('EXPONENT',op('MULTIPLY',op('POWER',op('DIVIDE',op('SUBTRACT',x,.5),width),2),-2))
end=op('POWER',op('MAXIMUM',op('SINE',op('MULTIPLY',y,math.pi)),0),.9)
alpha=op('MULTIPLY',cross,op('MULTIPLY',end,op('ADD',.15,op('MULTIPLY',y,.85))))
trans=nodes.new('ShaderNodeBsdfTransparent');emit=nodes.new('ShaderNodeEmission');emit.inputs[0].default_value=(.9,.95,1,1);emit.inputs[1].default_value=1
mix=nodes.new('ShaderNodeMixShader');links.new(alpha,mix.inputs[0]);links.new(trans.outputs[0],mix.inputs[1]);links.new(emit.outputs[0],mix.inputs[2]);out=nodes.new('ShaderNodeOutputMaterial');links.new(mix.outputs[0],out.inputs[0]);streak.data.materials.append(m)
for o in objects:o.hide_render=True
s.render.resolution_y=1024;s.render.resolution_percentage=100
for o in objects:
 for other in objects:other.hide_render=other!=o
 is_streak=o==streak;s.render.resolution_x=256 if is_streak else 512
 filename='reveal_burst_streak_v001.png' if is_streak else 'reveal_burst_shard_'+o.name[-1]+'_v001.png'
 s.render.filepath=str(ROOT/filename);bpy.ops.render.render(write_still=True)
for o in objects:o.hide_render=o==streak
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Source/F06_RevealBurst_v001.blend'))
(ROOT/'Source/parameters_v001.json').write_text(json.dumps({'task':'F06-ART-REVEAL-BURST','blender':bpy.app.version_string,'engine':s.render.engine,'samples':64,'shape_vertices':shapes,'camera_ortho_scale':2,'alpha':'transparent film RGBA','bloom':False,'source':'original procedural meshes / material; no AI model or extracted game art','seed':'not applicable; deterministic'},ensure_ascii=False,indent=2),encoding='utf-8')
print('F06: 4 actual PNG assets rendered and editable master saved')
