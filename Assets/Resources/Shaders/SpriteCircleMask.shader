Shader "Disney/SledRacer/Sprite Circle Mask"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0
		_RadiusScale ("Radius Scale", Range(0, 1.5)) = 1
		_Feather ("Edge Softness", Range(0.001, 0.25)) = 0.02
		_CenterOffset ("Center Offset", Vector) = (0,0,0,0)

		[PerRendererData][HideInInspector] _MaskCenter ("Mask Center", Vector) = (0,0,0,0)
		[PerRendererData][HideInInspector] _MaskRadius ("Mask Radius", Float) = 0.5
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
			"DisableBatching" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex SpriteCircleVert
			#pragma fragment SpriteCircleFrag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile_local _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#include "UnitySprites.cginc"

			struct v2f_circle
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 localPos : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float _RadiusScale;
			float _Feather;
			float4 _CenterOffset;
			float4 _MaskCenter;
			float _MaskRadius;

			v2f_circle SpriteCircleVert(appdata_t IN)
			{
				v2f_circle OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				float4 vertex = UnityFlipSprite(IN.vertex, _Flip);
				OUT.vertex = UnityObjectToClipPos(vertex);
				OUT.localPos = vertex.xy;
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color * _RendererColor;

				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}

			fixed4 SpriteCircleFrag(v2f_circle IN) : SV_Target
			{
				fixed4 color = SampleSpriteTexture(IN.texcoord) * IN.color;

				float radius = max(_MaskRadius * max(_RadiusScale, 0.0), 0.0001);
				float2 center = _MaskCenter.xy + (_CenterOffset.xy * radius);
				float normalizedDistance = length((IN.localPos - center) / radius);
				float feather = max(_Feather, 0.001);
				fixed mask = 1.0 - smoothstep(1.0 - feather, 1.0, normalizedDistance);

				color.a *= mask;
				color.rgb *= color.a;
				return color;
			}
			ENDCG
		}
	}

	Fallback "Sprites/Default"
}
