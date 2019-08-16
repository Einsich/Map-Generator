// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FlagAnimation"
{
	Properties{
		[PerRendererData]
		_MainTex("Main Texture", 2D) = "white" {}
		_NormalMap("Normal Map", 2D) = "white" {}
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma vertex vert
		#pragma target 3.0
		#include "MapData.cginc"

		sampler2D _MainTex,_NormalMap;

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
	};
	UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		struct appdata {
		float4 vertex : POSITION;
		float4 tangent: TANGENT;
		float3 normal : NORMAL;
		float2 texcoord : TEXCOORD0;
	};

	void vert(inout appdata v)
	{
		float3 p = UnityObjectToClipPos(v.vertex);
		float k = p.x + p.z+p.y;
		v.vertex.z += sin(k+_Time.z) * v.texcoord.x*0.15f;
	}
		void surf(Input IN, inout SurfaceOutputStandard o) {
		float4 c = tex2D(_MainTex, IN.uv_MainTex);
		
		c = ShadedColor(IN.worldPos, c);

		o.Albedo = c.rgb;
		o.Alpha = c.a;
		o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
	}
	ENDCG
	}
		FallBack "Diffuse"
}
