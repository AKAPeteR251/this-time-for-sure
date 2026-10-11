using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThisTime.Game
{
    // Environment shares the isolated F07 camera/layer and follows its lifetime.
    public sealed class AltarInteriorStage : MonoBehaviour
    {
        private P04FeelConfig cfg;
        private Camera lens;
        private GameObject room;
        private Light crystalLight;
        private readonly List<Transform> flames=new List<Transform>(),mist=new List<Transform>();
        private readonly List<Light> fireLights=new List<Light>();
        private readonly List<Vector3> mistRest=new List<Vector3>();
        private readonly List<Material> owned=new List<Material>();
        private Renderer[] inlays;
        private VolumeProfile profile;
        private MaterialPropertyBlock block;
        public int EnvironmentMeshCount {get{return room.GetComponentsInChildren<MeshRenderer>().Length;}}
        public int FlameCount {get{return flames.Count;}}
        public void Build(P04FeelConfig config,Camera camera)
        {
            cfg=config;lens=camera;block=new MaterialPropertyBlock();
            var cameraData=lens.GetUniversalAdditionalCameraData();cameraData.renderPostProcessing=true;cameraData.volumeLayerMask=1<<31;cameraData.volumeTrigger=lens.transform;
            var volumeObject=new GameObject("AltarLocalVolume");volumeObject.layer=31;volumeObject.transform.SetParent(transform,false);volumeObject.transform.localPosition=new Vector3(0,7,0);
            var box=volumeObject.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(38,22,38);
            var volume=volumeObject.AddComponent<Volume>();volume.isGlobal=false;volume.priority=20;volume.blendDistance=0;profile=ScriptableObject.CreateInstance<VolumeProfile>();volume.sharedProfile=profile;
            var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(cfg.altarBloom);bloom.threshold.Override(.8f);bloom.scatter.Override(.65f);
            profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.ACES);var adjust=profile.Add<ColorAdjustments>(true);adjust.postExposure.Override(cfg.altarExposure);adjust.contrast.Override(8);
            room=Instantiate(cfg.altarEnvironmentModel,transform,false);room.name="F08_RuntimeInterior";
            foreach(var t in room.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
            var glow=new List<Renderer>();
            foreach(var r in room.GetComponentsInChildren<Renderer>())
            {
                if(r.name.StartsWith("Altar_Inlay_Emission"))glow.Add(r);
                if(!r.name.StartsWith("Brazier_Bowl"))continue;
                Vector3 position=transform.InverseTransformPoint(new Vector3(r.bounds.center.x,r.bounds.max.y,r.bounds.center.z));
                var flame=Quad("AltarFlame_"+flames.Count,position+Vector3.up*.72f,new Vector3(1.4f,1.7f,1),cfg.altarFlame,Color.white,4);
                flames.Add(flame);
                fireLights.Add(Point("BrazierLight_"+fireLights.Count,position+Vector3.up*.6f,new Color(1,.38f,.09f),cfg.altarWarmLight,9));
            }
            inlays=glow.ToArray();
            crystalLight=Point("CrystalColdLight",new Vector3(0,5.8f,-1),new Color(.18f,.5f,1),cfg.altarCoolLight,16);
            Point("RearStoneRead",new Vector3(0,7,8),new Color(.15f,.29f,.48f),3,14);
            for(int i=0;i<5;i++)
            {
                Vector3 p=new Vector3((i-2)*4.2f,.75f,(i%2==0?-3.8f:2.6f));
                mistRest.Add(p);mist.Add(Quad("AltarLowMist_"+i,p,new Vector3(9,1.7f,1),cfg.altarMist,new Color(.28f,.42f,.58f,cfg.altarMistAlpha),1));
            }
            var dustObject=new GameObject("AltarDust");dustObject.layer=31;dustObject.transform.SetParent(transform,false);dustObject.transform.localPosition=new Vector3(0,4,0);
            var dust=dustObject.AddComponent<ParticleSystem>();var main=dust.main;main.startLifetime=12;main.startSpeed=.035f;main.startSize=.025f;
            main.startColor=new Color(.5f,.65f,.8f,.2f);main.maxParticles=100;main.simulationSpace=ParticleSystemSimulationSpace.Local;
            var emission=dust.emission;emission.rateOverTime=5;var shape=dust.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(20,8,18);
            var color=dust.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.2f),new GradientAlphaKey(1,.8f),new GradientAlphaKey(0,1)});color.color=gradient;
            dust.GetComponent<ParticleSystemRenderer>().sharedMaterial=Material(cfg.altarDust,new Color(.5f,.65f,.8f,.3f),1);dust.Play();
        }
        private Light Point(string name,Vector3 p,Color c,float strength,float range)
        {
            var obj=new GameObject(name);obj.transform.SetParent(transform,false);obj.transform.localPosition=p;obj.layer=31;
            var light=obj.AddComponent<Light>();light.type=LightType.Point;light.color=c;light.intensity=strength;light.range=range;light.cullingMask=1<<31;light.shadows=LightShadows.None;return light;
        }
        private Material Material(Texture2D texture,Color c,int tiles)
        {
            var mat=new Material(cfg.altarBillboardShader);mat.SetTexture("_MainTex",texture);mat.SetColor("_Color",c);mat.SetFloat("_Tiles",tiles);owned.Add(mat);return mat;
        }
        private Transform Quad(string name,Vector3 p,Vector3 scale,Texture2D texture,Color c,int tiles)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Quad);obj.name=name;obj.layer=31;Destroy(obj.GetComponent<Collider>());
            obj.transform.SetParent(transform,false);obj.transform.localPosition=p;obj.transform.localScale=scale;
            var renderer=obj.GetComponent<Renderer>();renderer.sharedMaterial=Material(texture,c,tiles);renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;return obj.transform;
        }
        public void Pose(float age,float energy)
        {
            for(int i=0;i<flames.Count;i++)
            {
                flames[i].rotation=Quaternion.LookRotation(flames[i].position-lens.transform.position,Vector3.up);
                block.Clear();block.SetFloat("_Frame",Mathf.Floor(age*cfg.altarFlameFps+i*3)%16);block.SetColor("_Color",Color.white*(1.1f+Mathf.Sin(age*7+i)*.06f));
                flames[i].GetComponent<Renderer>().SetPropertyBlock(block);
                fireLights[i].intensity=cfg.altarWarmLight*(.94f+Mathf.Sin(age*5+i)*.06f);
            }
            for(int i=0;i<mist.Count;i++)
            {
                mist[i].localPosition=mistRest[i]+new Vector3(Mathf.Sin(age*.06f+i)*1.5f,Mathf.Sin(age*.13f+i)*.08f,0);
                mist[i].rotation=Quaternion.LookRotation(mist[i].position-lens.transform.position,Vector3.up);
            }
            crystalLight.intensity=cfg.altarCoolLight*(1+energy*.4f);
            foreach(var renderer in inlays){block.Clear();block.SetColor("_EmissionColor",new Color(1,.5f,.13f)*(.6f+energy*.6f+Mathf.Sin(age*.75f)*.08f));renderer.SetPropertyBlock(block);}
        }
        private void OnDestroy(){foreach(var mat in owned)Destroy(mat);if(profile!=null){foreach(var component in profile.components)Destroy(component);Destroy(profile);}}
    }
}
