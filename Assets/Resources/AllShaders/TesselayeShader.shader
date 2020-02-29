Shader "Custom/TesselayeShader"
{
	Properties{
			 _Tess("Tessellation", Range(1,32)) = 4
			 //_MainTex("Base (RGB)", 2D) = "white" {}
			 _Displacement("Displacement", Range(0, 10.0)) = 0.3
			 _Color("Color", color) = (1,1,1,0)
			 _SpecColor("Spec color", color) = (0.5,0.5,0.5,0.5)
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 300

			CGPROGRAM
			#pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tessFixed nolightmap
			#pragma target 4.6
		#include "MapData.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			float _Tess;

			float4 tessFixed()
			{
				return _Tess;
			}

			//sampler2D _DispTex;
			float _Displacement;

			void disp(inout appdata v)
			{
				float d = GetHeightUV (v.texcoord.xy) * _Displacement;
				v.vertex.y +=  d;
			}

			struct Input {
				float2 uv_MainTex;
			};

			//sampler2D _MainTex;
			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutput o) {
				half4 c =  _Color;
				o.Albedo = c.rgb;
				o.Specular = 0.2;
				o.Gloss = 1.0;
				o.Normal = float3(0, 1, 0);
			}
			ENDCG
			 }
				 FallBack "Diffuse"
}
