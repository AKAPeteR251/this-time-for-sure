Shader "ThisTime/UI/CrystalPortal"
{
    Properties
    {
        [PerRendererData] _MainTex("Stage",2D)="white"{}
        _Open("Opening",Range(0,1))=0
        _Aspect("Screen aspect",Float)=1.77778
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            sampler2D _MainTex;float _Open,_Aspect;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)*i.color;
                float d=length((i.uv-.5)*float2(_Aspect,1));
                float radius=lerp(-.10,.5*sqrt(_Aspect*_Aspect+1)+.13,_Open);
                float mask=1-smoothstep(radius-.085,radius+.035,d);
                c.a*=mask;
                return c;
            }
            ENDCG
        }
    }
}
