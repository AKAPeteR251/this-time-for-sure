Shader "StarEcho/AltarBillboard"
{
    Properties { _MainTex("Texture",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Tiles("Atlas tiles",Float)=1 _Frame("Frame",Float)=0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;float _Tiles;float _Frame;
            CBUFFER_END
            Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color;return o;}
            half4 frag(Varyings i):SV_Target
            {
                float tiles=max(1,_Tiles),frame=floor(_Frame);
                float2 cell=float2(fmod(frame,tiles),tiles-1-floor(frame/tiles));
                half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,(i.uv+cell)/tiles);
                return c*_Color*i.color;
            }
            ENDHLSL
        }
    }
}
