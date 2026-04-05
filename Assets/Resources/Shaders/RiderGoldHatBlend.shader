Shader "Disney/SledRacer/RiderGoldHatBlend"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_GoldHatTex ("Gold Hat (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _GoldHatTex;
		fixed4 _Color;

		struct Input
		{
			float2 uv_MainTex;
		};

		inline fixed IsGoldHatPixel(fixed3 color)
		{
			return (color.r > 0.45 && color.g > 0.35 && color.b < 0.35 && color.r - color.b > 0.15 && color.g - color.b > 0.08) ? 1.0 : 0.0;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 goldHatColor = tex2D(_GoldHatTex, IN.uv_MainTex);
			baseColor.rgb = lerp(baseColor.rgb, goldHatColor.rgb, IsGoldHatPixel(goldHatColor.rgb));
			o.Albedo = baseColor.rgb;
			o.Alpha = baseColor.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
