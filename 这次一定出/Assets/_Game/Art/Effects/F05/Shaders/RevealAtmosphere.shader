Shader "StarEcho/UI/RevealAtmosphere"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI texture",2D)="white"{}
        _Mode("Nebula 0 / Meteor 1",Float)=0
        _RevealTime("Unscaled evolution",Float)=0
        _Aspect("Screen aspect",Float)=1.7778
        _Strength("Nebula alpha",Float)=.44
        _Threshold("Cloud sparsity",Float)=.46
        _Scale("Cloud scale",Float)=2.8
        _DriftTime("Horizontal drift",Float)=0
        _Filaments("Colored filament density",Float)=.65
        _Billow("Local vertical undulation",Float)=.025
        _Distortion("Local curl and stretch",Float)=.035
        _NebulaFarTex("P09 far dust",2D)="black"{}
        _NebulaNearTex("P09 near dust",2D)="black"{}
        _Warp("Local soft distortion",Float)=.035
        _Grain("Dust grain",Float)=.3
        _CloudSeed("Cloud distribution seed",Float)=0
        _StencilComp("Stencil Comparison",Float)=8
        _Stencil("Stencil ID",Float)=0
        _StencilOp("Stencil Operation",Float)=0
        _StencilWriteMask("Stencil Write Mask",Float)=255
        _StencilReadMask("Stencil Read Mask",Float)=255
        _ColorMask("Color Mask",Float)=15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip",Float)=0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float4 world:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
            float4 _ClipRect;
            float _Mode,_RevealTime,_Aspect,_Strength,_Warp,_Grain,_CloudSeed;
            float _Threshold,_Scale,_DriftTime,_Filaments,_Billow,_Distortion;
            sampler2D _NebulaFarTex,_NebulaNearTex;
            v2f vert(appdata v)
            {
                v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.world=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;
            }
            float hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
            float noise(float3 p)
            {
                float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),
                            lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+float3(1,1,1)),f.x),f.y),f.z);
            }
            float fbm(float3 p)
            {
                float f=noise(p)*.53;
                p=p*2.03+float3(13.1,7.7,9.2);f+=noise(p)*.27;
                p=p*2.01+float3(5.4,19.1,3.2);f+=noise(p)*.13;
                p=p*2.02+float3(11.2,3.4,17.1);f+=noise(p)*.07;
                return f;
            }
            float2 rotateUV(float2 p,float angle){float c=cos(angle),s=sin(angle);return float2(c*p.x-s*p.y,s*p.x+c*p.y);}
            float2 bendUV(float2 uv,float2 center,float t,float phase)
            {
                float2 d=(uv-center)*float2(_Aspect,1);
                float angle=_Distortion*12*exp(-dot(d,d)*5)*sin(t*1.25+phase);
                return center+rotateUV(d,angle)/float2(_Aspect,1);
            }
            float4 nebula(float2 uv)
            {
                float t=_RevealTime,seed=_CloudSeed;
                float2 travel=uv-float2(_DriftTime,0);
                // Local rise/fall bends the source detail without replacing it.
                travel.y+=_Billow*(sin(t*2.3+travel.x*4.2+seed*.31)*.7+sin(t*1.6+travel.y*4.6+seed*.19)*.3);
                float2 p=(travel-.5)*float2(_Aspect,1)*3.2;
                float2 w1=float2(noise(float3(p*.8,t*.5)),noise(float3(p*.8+8.3,t*.43)))-.5;
                float2 w2=float2(noise(float3(p*1.7+13,t*.36)),noise(float3(p*1.7+27,t*.31)))-.5;
                // Three overlapping local bends retain detail; no image cross-fade.
                float2 shaped=bendUV(travel,float2(.20,.58),t,seed*.11);
                shaped=bendUV(shaped,float2(.55,.28),t*.83,seed*.07+2.1);
                shaped=bendUV(shaped,float2(.84,.67),t*1.07,seed*.13+4.2);
                float2 flow=shaped*float2(_Aspect,1)*3;
                float a1=flow.x*1.4+flow.y*2.1+t*.85;
                float a2=flow.x*2.7-flow.y*1.6-t*.65;
                float2 curl=float2(2.1*cos(a1)-.8*cos(a2),-1.4*cos(a1)-1.35*cos(a2))*.35;
                shaped+=curl*_Distortion*.45/float2(_Aspect,1);
                float2 baseUV=(shaped-.5)*float2(_Aspect*.5,1);
                float2 offset=float2(frac(seed*.173),frac(seed*.317));
                float4 f=tex2D(_NebulaFarTex,rotateUV(baseUV,seed*.17+.63)*1.08+.5+offset+(w1+w2*.35)*_Warp);
                float4 n=tex2D(_NebulaNearTex,rotateUV(baseUV,-seed*.11-1.1)*1.2+.5+offset.yx+(w2+w1*.3)*_Warp*.8);
                float4 third=tex2D(_NebulaFarTex,rotateUV(baseUV,seed*.07+2.2)*.84+.5-offset+w2*_Warp);
                // Spatial masks drift with the material; no temporal cloud spawning.
                float m1=smoothstep(.2,.75,noise(float3(p*.7+seed,0)));
                float m2=smoothstep(.2,.75,noise(float3(p*.85+seed+31,0)));
                float m3=smoothstep(.2,.75,noise(float3(p*.6-seed+67,0)));
                float af=f.a*.38*(.25+.75*m1),an=n.a*.34*(.25+.75*m2),ac=third.a*.34*(.25+.75*m3);
                float a=af+an*(1-af)+ac*(1-af)*(1-an);
                float3 col=(f.rgb*af+n.rgb*an*(1-af)+third.rgb*ac*(1-af)*(1-an))/max(a,.0001);
                float grain=noise(float3(p*140+31,t*.15));
                col*=lerp(1,.7+grain*.6,_Grain);
                a*=lerp(1,.65+grain*.65,_Grain);
                float quiet=lerp(.82,1,smoothstep(.18,.45,uv.x));
                float breath=.96+.04*sin(t*1.8+p.x*.85+p.y*1.3);
                return float4(col,saturate(a*(_Strength/.44)*quiet*breath));
            }
            float4 shootingStar(float2 uv)
            {
                float x=uv.x,y=abs(uv.y-.5),head=.85;
                float tail=saturate(x/head);
                float width=lerp(.004,.016,tail);
                float body=exp(-y*y/(width*width))*pow(tail,2)*step(x,head);
                float haze=exp(-y*y/(width*width*16))*pow(tail,3)*step(x,head)*.28;
                float2 d=float2((x-head)*4,y);
                float nucleus=exp(-dot(d,d)*2600);
                float halo=exp(-dot(d,d)*95)*.46;
                float a=saturate(body*.85+haze+nucleus+halo);
                float3 col=lerp(float3(.2,.53,1),float3(.94,.98,1),saturate(body*.5+nucleus));
                return float4(col,a);
            }
            float4 frag(v2f i):SV_Target
            {
                float4 c=_Mode>.5?shootingStar(i.uv):nebula(i.uv);c*=i.color;
                #ifdef UNITY_UI_CLIP_RECT
                c.a*=UnityGet2DClipping(i.world.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a-.001);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
