using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    public sealed class RevealSky : MonoBehaviour
    {
        private P04FeelConfig cfg;private RawImage ground,far,near;
        private Image[] stars,shards;private RawImage meteor,proceduralCloud;
        private Material cloudMaterial,meteorMaterial;
        private float elapsed,progress,skyAge;private bool cloudInitialized;
        private readonly System.Random visualRandom=new System.Random();
        private float nextMeteor,cloudSeed;
        private int meteorRegion;
        private MeteorFlight flight;
        private readonly System.Collections.Generic.List<MeteorFlight> flights=new System.Collections.Generic.List<MeteorFlight>();
        [System.Serializable] public sealed class MeteorFlight {public Vector2 start,end;public float at,seconds,fade,peak,width;}
        public MeteorFlight[] MeteorFlights {get{return flights.ToArray();}}
        public float CloudSeed {get{return cloudSeed;}}
        public float Elapsed {get{return elapsed;}}
        public Vector2 CloudOffset {get{return new Vector2(far.uvRect.x,near.uvRect.x);}}
        public float FirstStarAlpha {get{return stars[0].color.a;}}
        public float MeteorAlpha {get{return meteor.color.a;}}
        public bool UsesProcedural {get{return cloudMaterial!=null;}}
        public float NebulaTime {get{return cloudMaterial==null?0:cloudMaterial.GetFloat("_RevealTime");}}
        public void Build(P04FeelConfig config)
        {
            cfg=config;cloudSeed=cfg.nebulaComposition;cloudInitialized=true;ground=Raw("Starfield",cfg.revealStarfield);far=Raw("FarNebula",cfg.nebulaFar);near=Raw("NearNebula",cfg.nebulaNear);
            if(cfg.proceduralRevealSky&&cfg.revealAtmosphereShader!=null)
            {
                far.enabled=near.enabled=false;
                proceduralCloud=Raw("ProceduralNebula",Texture2D.whiteTexture);
                cloudMaterial=new Material(cfg.revealAtmosphereShader);cloudMaterial.SetFloat("_Mode",0);proceduralCloud.material=cloudMaterial;cloudMaterial.SetTexture("_NebulaFarTex",cfg.nebulaFar);cloudMaterial.SetTexture("_NebulaNearTex",cfg.nebulaNear);

            }
            stars=new Image[Mathf.Clamp(cfg.revealStarCount,8,64)];
            for(int i=0;i<stars.Length;i++)
            {
                stars[i]=Img("Twinkle"+i,i%5==0?cfg.starCross:cfg.starSoft);
                Place(stars[i].rectTransform,new Vector2(.025f+Hash(i,2)*.95f,.04f+Hash(i,5)*.92f),Vector2.one*(i%5==0?16:8));
            }
            meteor=Raw("Meteor",cfg.meteorTrail==null?Texture2D.whiteTexture:cfg.meteorTrail.texture);
            if(cloudMaterial!=null){meteor.texture=Texture2D.whiteTexture;meteorMaterial=new Material(cfg.revealAtmosphereShader);meteorMaterial.SetFloat("_Mode",1);meteor.material=meteorMaterial;}
            meteor.rectTransform.localRotation=Quaternion.Euler(0,0,-28);
            shards=new Image[cfg.revealArrivalShader!=null?0:22];for(int i=0;i<shards.Length;i++)shards[i]=Img("OutwardIce"+i,cfg.revealShard);
            Animate(0);
        }
        public void Animate(float value){progress=value;}
        private void OnEnable(){elapsed=0;if(!cloudInitialized){cloudSeed=Range(0,100);cloudInitialized=true;}meteorRegion=visualRandom.Next(6);nextMeteor=Range(1.2f,2.8f);flight=null;flights.Clear();}
        private void Update()
        {
            if(cfg==null)return;elapsed+=Time.unscaledDeltaTime;skyAge+=Time.unscaledDeltaTime;
            var rect=(RectTransform)transform;float aspect=Mathf.Max(1,rect.rect.width)/Mathf.Max(1,rect.rect.height);
            float textureAspect=cfg.revealStarfield.width/(float)cfg.revealStarfield.height;
            float u=Mathf.Min(1,aspect/textureAspect),v=Mathf.Min(1,textureAspect/aspect);ground.uvRect=new Rect((1-u)/2,(1-v)/2,u,v);
            far.uvRect=new Rect(Mathf.Repeat(elapsed*cfg.nebulaFarSpeed,1),0,aspect/2,1);far.color=new Color(1,1,1,cfg.nebulaFarAlpha);
            near.uvRect=new Rect(Mathf.Repeat(elapsed*cfg.nebulaNearSpeed,1),0,aspect/2,1);near.color=new Color(1,1,1,cfg.nebulaNearAlpha);
            if(cloudMaterial!=null){cloudMaterial.SetFloat("_RevealTime",skyAge*cfg.nebulaEvolution);cloudMaterial.SetFloat("_Aspect",aspect);cloudMaterial.SetFloat("_Strength",cfg.proceduralNebulaAlpha*Mathf.SmoothStep(0,1,elapsed/1.6f));cloudMaterial.SetFloat("_Warp",cfg.nebulaWarp);cloudMaterial.SetFloat("_Grain",cfg.nebulaGrain);cloudMaterial.SetFloat("_CloudSeed",cloudSeed);cloudMaterial.SetFloat("_Threshold",cfg.nebulaThreshold);cloudMaterial.SetFloat("_Scale",cfg.nebulaScale);cloudMaterial.SetFloat("_Filaments",cfg.nebulaFilaments);cloudMaterial.SetFloat("_Billow",cfg.nebulaBillow);cloudMaterial.SetFloat("_Distortion",cfg.nebulaDistortion);cloudMaterial.SetFloat("_DriftTime",skyAge*cfg.nebulaDrift);}
            for(int i=0;i<stars.Length;i++)stars[i].color=new Color(.72f,.85f,1,.12f+.6f*Mathf.Pow((Mathf.Sin(elapsed*(.6f+Hash(i,8)) + i*2.4f)+1)*.5f,2));
            TickMeteor(rect.rect.size);
            float launch=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.05f,.48f,progress));float a=(1-Mathf.InverseLerp(.28f,.68f,progress))*launch*.65f;
            for(int i=0;i<shards.Length;i++)
            {
                float angle=i*Mathf.PI*2/shards.Length;float radius=Mathf.Lerp(.02f,.5f+Hash(i,10)*.22f,launch);
                Place(shards[i].rectTransform,new Vector2(.535f+Mathf.Cos(angle)*radius,.52f+Mathf.Sin(angle)*radius),new Vector2(16+Hash(i,11)*18,50+Hash(i,12)*60));
                shards[i].rectTransform.localRotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg-90+launch*25);shards[i].color=new Color(1,1,1,a);
            }
        }
        private void TickMeteor(Vector2 size)
        {
            if(elapsed>=nextMeteor)
            {
                Vector2 start=Vector2.zero;
                int col=(meteorRegion%3+flights.Count*2)%3,row=(meteorRegion/3+flights.Count)%2;
                for(int i=0;i<24;i++){start=new Vector2(Range(.12f+col*.26f,.36f+col*.26f),row==0?Range(.22f,.40f):Range(.62f,.90f));if(flights.Count==0||Vector2.Distance(start,flights[flights.Count-1].start)>.28f)break;}
                float dir=start.x<.32f?1:start.x>.68f?-1:visualRandom.Next(2)==0?1:-1;
                flight=new MeteorFlight{start=start,end=new Vector2(Mathf.Clamp(start.x+dir*Range(.18f,.35f),.04f,.96f),Mathf.Max(.04f,start.y-Range(.08f,.26f))),
                    at=elapsed,seconds=Mathf.Max(.5f,cfg.meteorSeconds)*Range(.88f,1.12f),fade=Mathf.Max(.5f,cfg.meteorFadeSeconds),peak=cfg.meteorOpacity*Range(.8f,1.1f),width=Range(170,270)};
                flights.Add(flight);nextMeteor=elapsed+flight.seconds+flight.fade+Mathf.Max(.6f,cfg.meteorInterval-flight.seconds-flight.fade)*Range(.8f,1.3f);
            }
            if(flight==null){meteor.color=Color.clear;return;}
            float age=elapsed-flight.at;
            float fadeAge=Mathf.Max(0,age-flight.seconds);
            float alpha=flight.peak*Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.7f))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01(fadeAge/flight.fade)));
            float travel=Mathf.Min(1,age/flight.seconds)+Mathf.Min(.16f,fadeAge/flight.fade*.16f);
            Vector2 delta=flight.end-flight.start;
            meteor.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y*size.y,delta.x*size.x)*Mathf.Rad2Deg);
            Place(meteor.rectTransform,flight.start+delta*travel,new Vector2(flight.width,flight.width/4));meteor.color=new Color(.82f,.9f,1,alpha);
        }
        private float Range(float a,float b){return Mathf.Lerp(a,b,(float)visualRandom.NextDouble());}
        private void OnDestroy(){if(cloudMaterial!=null)Destroy(cloudMaterial);if(meteorMaterial!=null)Destroy(meteorMaterial);}
        private RawImage Raw(string name,Texture tex){var r=Rect(name).gameObject.AddComponent<RawImage>();r.texture=tex;r.raycastTarget=false;return r;}
        private Image Img(string name,Sprite sprite){var i=Rect(name).gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}
        private RectTransform Rect(string name){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        private static void Place(RectTransform r,Vector2 anchor,Vector2 size){r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=Vector2.zero;r.sizeDelta=size;}
        private static float Hash(int i,int salt){return Mathf.Repeat(Mathf.Sin(i*17.13f+salt*31.7f)*4387.23f,1);}
    }
}
