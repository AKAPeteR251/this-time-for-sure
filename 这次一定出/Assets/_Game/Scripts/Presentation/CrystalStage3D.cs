using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThisTime.Game
{
    // Isolated real meshes rendered to the existing uGUI stage; no gameplay or scene-wide settings.
    public sealed class CrystalStage3D : MonoBehaviour
    {
        private P04FeelConfig cfg;
        private GameObject rig,model;
        private Camera lens;
        private RawImage output;
        private Material portalMaterial;
        private RenderTexture texture;
        private Transform body;
        private Renderer[] bodyRenderers;
        private readonly List<Transform> rings=new List<Transform>(),floats=new List<Transform>(),shells=new List<Transform>();
        private readonly List<Vector3> floatRest=new List<Vector3>(),shellRest=new List<Vector3>();
        private readonly List<Quaternion> ringRest=new List<Quaternion>();
        private readonly List<RectTransform> movingStars=new List<RectTransform>();
        private readonly List<Vector2> starRest=new List<Vector2>();
        private RawImage starfield;
        private Vector3 bodyRest;
        private Quaternion bodyRotation;
        private Vector3 bodyScale;
        private float born,energy,shake,entry=1,terminal=-1;
        private Color signal;
        private AltarInteriorStage interior;
        private CrystalSheetLight sheetLight;
        public CrystalSheetLight SheetLight {get{return sheetLight;}}
        public bool Indoor {get{return interior!=null;}}
        public int EnvironmentMeshCount {get{return interior==null?0:interior.EnvironmentMeshCount;}}
        public float Age {get{return Time.unscaledTime-born;}}
        public float Entry {get{return entry;}}
        public bool Rendering {get{return rig!=null&&rig.activeSelf&&lens.enabled;}}
        public Vector3 BodyPosition {get{return body.localPosition;}}
        public Quaternion OuterRotation {get{return rings[0].localRotation;}}
        public Vector3 FirstFloatingPosition {get{return floats[0].localPosition;}}
        public int MeshCount {get{return rig.GetComponentsInChildren<MeshRenderer>(true).Length;}}
        public Camera StageCamera {get{return lens;}}
        public Vector2 FirstStarPosition {get{return movingStars[0].anchorMin;}}
        public void Build(P04FeelConfig config)
        {
            cfg=config;
            rig=new GameObject("P19_IsolatedCrystalStage");rig.transform.position=new Vector3(10000,0,10000);
            model=Instantiate(cfg.crystalStageModel,rig.transform,false);model.name="F07_RuntimeStage";
            foreach(var t in model.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer=31;
                if(t.name=="CrystalBody")body=t;
                if(t.name.StartsWith("Ring")){rings.Add(t);ringRest.Add(t.localRotation);}
                if(t.name.StartsWith("Floating")){floats.Add(t);floatRest.Add(t.localPosition);}
                if(t.name.StartsWith("Shard_")){shells.Add(t);shellRest.Add(t.localPosition);}
            }
            // Preserve FBX axis conversion while making the animated roots share Unity world axes.
            body.SetParent(rig.transform,true);
            for(int i=0;i<rings.Count;i++){rings[i].SetParent(rig.transform,true);ringRest[i]=rings[i].localRotation;}
            for(int i=0;i<floats.Count;i++){floats[i].SetParent(rig.transform,true);floatRest[i]=floats[i].localPosition;}
            if(cfg.altarEnvironmentModel!=null)
            {
                body.localPosition+=Vector3.up*cfg.altarStageLift;
                foreach(var ring in rings)ring.localPosition+=Vector3.up*cfg.altarStageLift;
                for(int i=0;i<floats.Count;i++){floats[i].localPosition+=Vector3.up*cfg.altarStageLift;floatRest[i]=floats[i].localPosition;}
            }
            bodyRest=body.localPosition;bodyRotation=body.localRotation;bodyScale=body.localScale;bodyRenderers=body.GetComponentsInChildren<Renderer>();
            var cameraObject=new GameObject("P19_StageCamera");cameraObject.transform.SetParent(rig.transform,false);
            lens=cameraObject.AddComponent<Camera>();lens.cullingMask=1<<31;lens.clearFlags=CameraClearFlags.SolidColor;
            lens.backgroundColor=Color.clear;lens.nearClipPlane=.1f;lens.farClipPlane=80;lens.fieldOfView=37;
            lens.allowHDR=cfg.altarEnvironmentModel!=null;lens.allowMSAA=true;lens.depth=-10;
            LightAt("GoldKey",new Vector3(35,-28,0),new Color(1,.82f,.58f),cfg.altarEnvironmentModel!=null?.45f:1.2f);
            LightAt("BlueRim",new Vector3(20,145,0),new Color(.34f,.60f,1),cfg.altarEnvironmentModel!=null?.4f:.75f);
            if(cfg.altarEnvironmentModel!=null){interior=rig.AddComponent<AltarInteriorStage>();interior.Build(cfg,lens);lens.backgroundColor=new Color(.007f,.012f,.025f,1);}
            if(cfg.crystalSheetLightShader!=null){sheetLight=rig.AddComponent<CrystalSheetLight>();sheetLight.Build(cfg,body,lens);}
            output=gameObject.AddComponent<RawImage>();output.raycastTarget=false;output.color=Color.white;
            if(cfg.crystalPortalCamera&&cfg.crystalPortalShader!=null)
            {portalMaterial=new Material(cfg.crystalPortalShader){name="P24_Portal_Runtime"};output.material=portalMaterial;}
            Resize();rig.SetActive(false);
            var sky=transform.parent.Find("CrystalLivingSky");
            if(sky==null)return;
            foreach(var r in sky.GetComponentsInChildren<RectTransform>())if(r.name.StartsWith("Twinkle")){movingStars.Add(r);starRest.Add(r.anchorMin);}
            starfield=sky.Find("Starfield").GetComponent<RawImage>();
        }
        public void Begin(Color color){signal=color;born=Time.unscaledTime;energy=shake=0;terminal=-1;entry=1;rig.SetActive(true);lens.enabled=true;Tick();}
        public void SetEntry(float value){entry=Mathf.SmoothStep(0,1,Mathf.Clamp01(value));Tick();}
        public void Pose(float value,float shakePixels){energy=value;shake=shakePixels;Tick();}
        public void Terminal(float value){terminal=value;Tick();}
        public void HideGeometry(){if(rig!=null)rig.SetActive(false);}
        private void Update(){if(rig==null||!rig.activeSelf)return;Resize();Tick();}
        private void LateUpdate()
        {
            if(rig==null||!rig.activeSelf)return;float t=Age;
            for(int i=0;i<movingStars.Count;i++)
            {
                var p=starRest[i]+new Vector2(t*cfg.crystalStarDrift*(.6f+i%4*.15f),Mathf.Sin(t*.08f+i)*.002f);
                movingStars[i].anchorMin=movingStars[i].anchorMax=p;
            }
            if(starfield!=null){var uv=starfield.uvRect;uv.x+=t*cfg.crystalStarDrift*.35f;starfield.uvRect=uv;}
        }
        private void Tick()
        {
            if(body==null)return;float t=Age;
            for(int i=0;i<rings.Count;i++)
            {
                string name=rings[i].name;
                Vector3 speed=name=="RingOuter"?cfg.crystalOuterEulerSpeed:name=="RingMiddle"?cfg.crystalMiddleEulerSpeed:cfg.crystalInnerEulerSpeed;
                rings[i].localRotation=(name=="RingFloor"?Quaternion.Euler(0,t*cfg.crystalFloorSpeed,0):Quaternion.Euler(speed*t))*ringRest[i];
            }
            for(int i=0;i<floats.Count;i++)
            {
                Vector3 rest=floatRest[i];Vector3 center=bodyRest;
                Vector3 d=rest-center;
                // Unity Y is up; orbit through X/Z preserves depth and avoids a flat sprite wheel.
                float a=t*cfg.crystalOrbitSpeed*(i%2==0?1:-.75f)*Mathf.Deg2Rad;
                Vector3 moved=new Vector3(d.x*Mathf.Cos(a)-d.z*Mathf.Sin(a),d.y,d.x*Mathf.Sin(a)+d.z*Mathf.Cos(a));
                moved.y+=Mathf.Sin(t*(.55f+i*.035f)+i*1.7f)*cfg.crystalFloatAmplitude;
                floats[i].localPosition=center+moved;
                floats[i].localRotation=Quaternion.Euler(t*(9+i),t*(13+i*.8f),t*(5+i*.5f));
            }
            Vector3 sway=new Vector3(Mathf.Sin(t*.41f)*.035f,Mathf.Sin(t*.85f)*cfg.crystalFloatAmplitude*.5f,0);
            Vector3 hit=new Vector3(Mathf.Sin(t*137),Mathf.Cos(t*113)*.45f,0)*shake*.0035f;
            body.localPosition=bodyRest+sway+hit;
            body.localRotation=Quaternion.Euler(Mathf.Sin(t*.4f)*3,t*cfg.crystalBodySpeed,Mathf.Sin(t*.55f)*2)*bodyRotation;
            float size=cfg.crystalBodyScale*(cfg.crystalPortalCamera?1:Mathf.Lerp(.45f,1,entry))*(1+energy*.055f);
            if(terminal>=0)size*=Mathf.Lerp(1,.86f,Mathf.Clamp01(terminal/cfg.finalChargeSeconds));
            body.localScale=bodyScale*size;
            for(int i=0;i<shells.Count;i++)shells[i].localPosition=shellRest[i]+shellRest[i].normalized*energy*.045f;
            foreach(var renderer in bodyRenderers)
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)if(materials[i]!=null&&materials[i].name.Contains("InnerFaces"))
                {
                    var block=new MaterialPropertyBlock();block.SetColor("_EmissionColor",Color.Lerp(new Color(.08f,.22f,.32f),signal*2.0f,energy));renderer.SetPropertyBlock(block,i);
                }
            }
            Vector3 focus=Vector3.Lerp(new Vector3(0,2.4f,0),new Vector3(0,4.1f,0),entry);
            float distance=Mathf.Lerp(cfg.crystalCameraDistance*1.28f,cfg.crystalCameraDistance,entry);
            // Use the imported Y-up transforms; source reference camera is Blender Z-up.
            lens.transform.localPosition=focus+new Vector3(1.1f,3.0f,-distance);
            lens.transform.LookAt(rig.transform.TransformPoint(focus));
            if(interior!=null)
            {
                lens.transform.localPosition=Vector3.Lerp(cfg.altarCameraWide+(cfg.crystalPortalCamera?cfg.crystalPortalEntryOffset:Vector3.zero),cfg.altarCameraClose,entry);
                lens.transform.LookAt(rig.transform.TransformPoint(Vector3.Lerp(cfg.altarFocusWide,cfg.altarFocusClose,entry)));
                if(cfg.crystalPortalCamera)lens.transform.Rotate(0,0,Mathf.Lerp(cfg.crystalPortalRoll,0,entry),Space.Self);
                lens.fieldOfView=Mathf.Lerp(cfg.altarWideFov,cfg.altarCloseFov,entry);
                interior.Pose(t,energy);
            }
            if(portalMaterial!=null)
            {
                portalMaterial.SetFloat("_Open",Mathf.SmoothStep(0,1,Mathf.InverseLerp(.16f,.90f,entry)));
                portalMaterial.SetFloat("_Aspect",(float)Screen.width/Mathf.Max(1,Screen.height));
            }
            if(sheetLight!=null)sheetLight.Pose(t,energy,shake,terminal,signal);
        }
        private void Resize()
        {
            int w=Mathf.Clamp(Screen.width,640,1920),h=Mathf.Clamp(Screen.height,360,1080);
            if(texture!=null&&texture.width==w&&texture.height==h)return;
            if(texture!=null){lens.targetTexture=null;texture.Release();Destroy(texture);}
            texture=new RenderTexture(w,h,24,interior==null?RenderTextureFormat.ARGB32:RenderTextureFormat.ARGBHalf){name="P19_CrystalStageRT",antiAliasing=2};texture.Create();
            lens.targetTexture=texture;output.texture=texture;
        }
        private void LightAt(string name,Vector3 angles,Color color,float intensity)
        {
            var obj=new GameObject(name);obj.transform.SetParent(rig.transform,false);obj.transform.localRotation=Quaternion.Euler(angles);
            var light=obj.AddComponent<Light>();light.type=LightType.Directional;light.color=color;light.intensity=intensity;light.cullingMask=1<<31;light.shadows=LightShadows.None;
        }
        public bool Contains(Vector2 screen,Camera uiCamera)
        {
            var rect=(RectTransform)transform;Vector2 local;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,screen,uiCamera,out local))return false;
            Vector2 uv=new Vector2((local.x-rect.rect.xMin)/rect.rect.width,(local.y-rect.rect.yMin)/rect.rect.height);
            var ray=lens.ViewportPointToRay(uv);foreach(var renderer in bodyRenderers)if(renderer.bounds.IntersectRay(ray))return true;return false;
        }
        private void OnDisable(){HideGeometry();}
        private void OnDestroy(){if(portalMaterial!=null)Destroy(portalMaterial);if(rig!=null)Destroy(rig);if(texture!=null){texture.Release();Destroy(texture);}}
    }
}
