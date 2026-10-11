using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThisTime.Game
{
    // Soft widening sheets in the same depth-buffered 3D stage, without input colliders.
    public sealed class CrystalSheetLight : MonoBehaviour
    {
        private P04FeelConfig cfg;private Transform crystal;private Camera lens;
        private Material material;private MaterialPropertyBlock block;private Mesh mesh;
        private readonly List<MeshRenderer> sheets=new List<MeshRenderer>();
        private static readonly float[] angles={25,145,225,285,75,335,185};
        public int VisibleSheets {get;private set;}
        public float Strength {get;private set;}
        public void Build(P04FeelConfig config,Transform body,Camera camera)
        {
            cfg=config;crystal=body;lens=camera;block=new MaterialPropertyBlock();
            material=new Material(cfg.crystalSheetLightShader);mesh=SheetMesh();
            for(int i=0;i<angles.Length;i++)
            {
                var obj=new GameObject("CrackLightSheet_"+i);obj.layer=31;obj.transform.SetParent(transform,false);
                obj.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
                renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.enabled=false;sheets.Add(renderer);
            }
        }
        private static Mesh SheetMesh()
        {
            const int steps=12;var vertices=new Vector3[(steps+1)*2];var uv=new Vector2[vertices.Length];var indices=new int[steps*6];
            for(int i=0;i<=steps;i++)
            {
                float x=i/(float)steps;float width=Mathf.Lerp(.018f,.5f,Mathf.Pow(x,.8f));
                // A shallow bent fan has real depth rather than a screen-space stroke.
                float z=Mathf.Sin(x*Mathf.PI)*.14f;
                vertices[i*2]=new Vector3(x,-width,z);vertices[i*2+1]=new Vector3(x,width,z);uv[i*2]=new Vector2(x,0);uv[i*2+1]=new Vector2(x,1);
                if(i==steps)continue;int b=i*6,v=i*2;indices[b]=v;indices[b+1]=v+1;indices[b+2]=v+2;indices[b+3]=v+1;indices[b+4]=v+3;indices[b+5]=v+2;
            }
            var result=new Mesh{name="P22_TaperedLightSheet"};result.vertices=vertices;result.uv=uv;result.triangles=indices;result.RecalculateBounds();result.RecalculateNormals();return result;
        }
        public void Pose(float age,float energy,float shake,float terminal,Color signal)
        {
            VisibleSheets=0;Strength=Mathf.SmoothStep(0,1,Mathf.InverseLerp(cfg.crystalSheetStart,1,energy));
            float finale=terminal<0?0:Mathf.Clamp01(terminal/cfg.finalChargeSeconds);
            for(int i=0;i<sheets.Count;i++)
            {
                float reveal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(cfg.crystalSheetStart+i*.065f,cfg.crystalSheetStart+.18f+i*.065f,energy));
                var renderer=sheets[i];renderer.enabled=reveal>.001f;if(!renderer.enabled)continue;VisibleSheets++;
                float angle=(angles[i]+Mathf.Sin(age*.35f+i)*2)*Mathf.Deg2Rad;
                Vector3 direction=lens.transform.right*Mathf.Cos(angle)+lens.transform.up*Mathf.Sin(angle);
                Vector3 front=(lens.transform.position-crystal.position).normalized;
                renderer.transform.position=crystal.position+direction*(.36f+i%3*.08f)+front*.82f;
                renderer.transform.rotation=Quaternion.LookRotation(lens.transform.forward,lens.transform.up)*Quaternion.Euler(0,(i%2==0?1:-1)*12,angle*Mathf.Rad2Deg);
                float length=cfg.crystalSheetLength*(.75f+i%3*.12f)*Mathf.Lerp(.45f,1.15f,reveal)*(1+finale*.25f);
                renderer.transform.localScale=new Vector3(length,cfg.crystalSheetWidth*Mathf.Lerp(.5f,1.2f,reveal)*(1+finale*.3f),1);
                block.Clear();block.SetColor("_Tint",signal);block.SetFloat("_Strength",cfg.crystalSheetIntensity*reveal*(.88f+Mathf.Sin(age*2.7f+i)*.06f+shake*.006f+finale*.4f));block.SetFloat("_Age",age);block.SetFloat("_Seed",i*1.73f);renderer.SetPropertyBlock(block);
            }
        }
        private void OnDisable(){VisibleSheets=0;Strength=0;foreach(var sheet in sheets)if(sheet!=null)sheet.enabled=false;}
        private void OnDestroy(){if(material!=null)Destroy(material);if(mesh!=null)Destroy(mesh);}
    }
}
