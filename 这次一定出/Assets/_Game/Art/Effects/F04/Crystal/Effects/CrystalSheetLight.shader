Shader "StarEcho/CrystalSheetLight"
{
    Properties { _Tint("Rarity color",Color)=(1,.8,.4,1) _Strength("Strength",Float)=1 _Age("Age",Float)=0 _Seed("Seed",Float)=0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent" }
        Pass
        {
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION;float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            float4 _Tint;float _Strength;float _Age;float _Seed;
            CBUFFER_END
            Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;return o;}
            half4 frag(Varyings i):SV_Target
            {
                float x=i.uv.x,y=i.uv.y*2-1;
                float wave=sin(x*9-_Age*1.3+_Seed)*sin(x*3.14159)*.08;
                float edge=pow(saturate(1-abs(y-wave)),2.3);
                float tip=1-smoothstep(.45,1,x);
                float source=smoothstep(0,.04,x);
                float folds=.78+.13*sin(y*14+x*8-_Age*.65+_Seed)+.09*sin(x*19+_Age*.4+_Seed);
                float a=edge*tip*source*folds;
                float3 color=lerp(_Tint.rgb,float3(1,1,.96),.38*(1-x));
                return half4(color*a*_Strength,0);
            }
            ENDHLSL
        }
    }
}
