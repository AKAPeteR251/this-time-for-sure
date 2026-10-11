Shader "ThisTime/UI/ContractBurn"
{
    Properties
    {
        [PerRendererData] _MainTex("UI layer",2D)="white"{}
        _Burn("Burn progress",Range(0,1))=0
        _BurnTime("Burn seconds",Float)=0
        _Edge("Hot edge width",Float)=.06
        _Glow("Paper fire",Float)=0
        _StencilComp("Stencil comparison",Float)=8
        _Stencil("Stencil ID",Float)=0
        _StencilOp("Stencil operation",Float)=0
        _StencilWriteMask("Stencil write",Float)=255
        _StencilReadMask("Stencil read",Float)=255
        _ColorMask("Color mask",Float)=15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Alpha clip",Float)=0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True"}
        Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;float4 screen:TEXCOORD1;float4 local:TEXCOORD2;};
            sampler2D _MainTex;fixed4 _TextureSampleAdd;float4 _ClipRect,_PaperScreenRect,_PaperScreenAxisX,_PaperScreenAxisY;
            float _Burn,_BurnTime,_Edge,_Glow;
            float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise(float2 p){float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(q),hash(q+float2(1,0)),f.x),lerp(hash(q+float2(0,1)),hash(q+1),f.x),f.y);}
            v2f vert(appdata v)
            {
                v2f o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;
                o.screen=ComputeScreenPos(o.vertex);return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 original=(tex2D(_MainTex,i.uv)+_TextureSampleAdd)*i.color;
                float2 delta=i.screen.xy/i.screen.w-_PaperScreenRect.xy;
                float2 paper=float2(dot(delta,_PaperScreenAxisX.xy),dot(delta,_PaperScreenAxisY.xy));
                float irregular=(noise(paper*float2(8,10))-.5)*.13+(noise(paper*float2(31,35))-.5)*.035;
                float front=lerp(-.18,1.22,_Burn);
                float d=paper.y+irregular-front;
                float remaining=smoothstep(-.006,.007,d);
                float charred=1-smoothstep(_Edge*.5,_Edge*1.3,d);
                float hot=exp(-abs(d)/max(.005,_Edge*.32));
                float active=smoothstep(0,.04,_Burn)*(1-smoothstep(.94,1,_Burn));
                float flicker=noise(float2(paper.x*44,paper.y*21-_BurnTime*5));
                float tongue=exp(-abs(d-(.012+flicker*.08))/max(.008,_Edge*.28))*flicker*_Glow;
                original.rgb=lerp(original.rgb,original.rgb*.08,charred*active);
                float fire=(hot*.9+tongue*.8)*active;
                original.rgb=lerp(original.rgb,float3(1,.22+.5*flicker,.025)*1.3,saturate(fire));
                original.a*=max(remaining,tongue*active*.9);
                if(_Burn>=1)original.a=0;
                #ifdef UNITY_UI_CLIP_RECT
                original.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(original.a-.001);
                #endif
                return original;
            }
            ENDCG
        }
    }
}
