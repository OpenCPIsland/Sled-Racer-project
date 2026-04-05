Shader "Disney/SledRacer/Sprite Edge Trim"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0
		_TrimXPixels ("Horizontal Trim (Pixels)", Range(0, 4)) = 1
		_TrimYPixels ("Vertical Trim (Pixels)", Range(0, 4)) = 1
		_Cutoff ("Mask Threshold", Range(0, 1)) = 0.01
		_Softness ("Mask Softness", Range(0.001, 0.25)) = 0.02

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
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment SpriteEdgeTrimFrag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile_local _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#include "UnitySprites.cginc"

			float4 _MainTex_TexelSize;
			float _TrimXPixels;
			float _TrimYPixels;
			float _Cutoff;
			float _Softness;

			fixed GetTrimmedAlpha(float2 uv)
			{
				float2 texelOffset = _MainTex_TexelSize.xy * float2(max(_TrimXPixels, 0.0), max(_TrimYPixels, 0.0));

				fixed minAlpha = SampleSpriteTexture(uv).a;
				minAlpha = min(minAlpha, SampleSpriteTexture(uv + float2(texelOffset.x, 0.0)).a);
				minAlpha = min(minAlpha, SampleSpriteTexture(uv - float2(texelOffset.x, 0.0)).a);
				minAlpha = min(minAlpha, SampleSpriteTexture(uv + float2(0.0, texelOffset.y)).a);
				minAlpha = min(minAlpha, SampleSpriteTexture(uv - float2(0.0, texelOffset.y)).a);

				return minAlpha;
			}

			fixed4 SpriteEdgeTrimFrag(v2f IN) : SV_Target
			{
				fixed4 color = SampleSpriteTexture(IN.texcoord) * IN.color;
				fixed trimmedAlpha = GetTrimmedAlpha(IN.texcoord);
				fixed mask = smoothstep(_Cutoff, _Cutoff + max(_Softness, 0.001), trimmedAlpha);

				color.a *= mask;
				color.rgb *= color.a;
				return color;
			}
			ENDCG
		}
	}

	Fallback "Sprites/Default"
}
