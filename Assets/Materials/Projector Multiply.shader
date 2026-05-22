// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "Projector/Multiply"
{
    Properties
    {
        _ShadowTex ("Cookie", 2D) = "gray" {}
        _FalloffTex ("FallOff", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "QUEUE"="Transparent" }

        Pass
        {
            Tags { "QUEUE"="Transparent" }

            ZWrite Off

            Fog
            {
                Color (1,1,1,1)
            }

            Blend DstColor Zero
            AlphaTest Greater 0
            ColorMask RGB
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _ShadowTex;
            sampler2D _FalloffTex;

            float4x4 unity_Projector;
            float4x4 unity_ProjectorClip;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uvShadow : TEXCOORD0;
                float4 uvFalloff : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.uvShadow = mul(unity_Projector, v.vertex);
                o.uvFalloff = mul(unity_ProjectorClip, v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 proj = i.uvShadow.xyz / i.uvShadow.w;
                float2 uv = proj.xy;

                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    discard;

                fixed4 texS = tex2D(_ShadowTex, uv);

                texS.a = 1.0 - texS.a;

                fixed4 falloff = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));

                return lerp(
                    fixed4(1,1,1,0),
                    texS,
                    falloff.a
                );
            }

            ENDCG
        }
    }
}