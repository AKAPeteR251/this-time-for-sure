Shader "StarEcho/UI/RevealArrival"
{
    Properties
    {
        [PerRendererData] _MainTex("UI",2D)="white"{}
        _Phase("Expansion",Float)=0
        _Radius("Radius",Float)=.6
        _Aspect("Aspect",Float)=1.7778
        _Gap("Character light gap",Float)=.075
        _Softness("Soft diffusion",Float)=.14
        _Scatter("Broken light lobes",Float)=.8
        _SignalColor("Light color",Color)=(.2,.6,1,1)
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
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One
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
            struct a2v {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;float4 world:TEXCOORD1;};
            float _Phase,_Radius,_Aspect,_Gap,_Softness,_Scatter;float4 _SignalColor,_ClipRect;
            v2f vert(a2v v){v2f o;o.world=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            float hash(float x){return frac(sin(x*17.13)*4387.23);}
            float4 frag(v2f i):SV_Target
            {
                float2 p=(i.uv-float2(.535,.52))*float2(_Aspect,1);
                float r=length(p/float2(.90,1)),angle=atan2(p.y,p.x)/6.2831853+.5;
                float expansion=smoothstep(0,1,saturate(_Phase));
                float radius=lerp(.48+_Gap,_Radius,expansion);
                // Broad continuous lobes: no sector steps, sharp spokes or uniform ring edge.
                float theta=angle*6.2831853;
                float lobes=.5+.24*sin(theta*5+1.2)+.16*sin(theta*9-2.3)+.10*sin(theta*13+.6);
                float broken=smoothstep(.18,.85,lobes);
                float reach=radius*(.94+.08*sin(theta*3+1.4)+.04*sin(theta*7));
                float width=_Softness*(.8+.45*broken);
                float haze=exp(-pow((r-reach)/width,2));
                float bloom=exp(-pow((r-reach*.97)/(width*1.8),2))*.2;
                float scatter=lerp(.55,broken,_Scatter);
                float clearance=smoothstep(.43+_Gap,.57+_Gap,r);
                float alpha=saturate(haze*.65+bloom)*scatter*clearance*i.color.a;
                float hue=.5+.5*sin(theta*2+1.1);
                float3 cool=_SignalColor.rgb*float3(.68,.78,.90);
                float3 violet=lerp(cool,float3(.36,.19,.68),.48);
                float3 cyan=lerp(cool,float3(.19,.48,.68),.45);
                float3 color=lerp(violet,cyan,hue);
                #ifdef UNITY_UI_CLIP_RECT
                alpha*=UnityGet2DClipping(i.world.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha-.001);
                #endif
                return float4(color*i.color.rgb,alpha);
            }
            ENDCG
        }
    }
}
