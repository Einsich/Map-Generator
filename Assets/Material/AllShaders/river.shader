Shader "Custom/river" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal map",2D) = "white"{}
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard alpha
		#pragma target 3.0
		#include "MapData.cginc"

		sampler2D _MainTex,_NormalMap;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};


		void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 c = tex2D (_MainTex, IN.uv_MainTex-float2(_Time.y,0));

			c = ShadedColor(IN.worldPos, c);

			o.Albedo = c.rgb;
			o.Alpha = c.a;

			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
