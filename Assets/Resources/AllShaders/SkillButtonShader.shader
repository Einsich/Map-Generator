Shader "Custom/SkillButtonShader"
{

	Properties
	{
		 [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

			//_HealthColor("_HealthColor", Color) = (0,.72,.12,1)
			//_HitColor("_HitColor", Color) = (1,.8,.8,1)
			//_LoseColor("_LoseColor", Color) = (.7,0,0,1)
			//_VoidColor("_VoidColor", Color) = (0,0,0,1)
			//_StripeColor("_StripeColor", Color) = (.5,0.5,0.3,1)
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
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				Name "Default"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					float4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
					float4 worldPosition : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				float4 _Color;
				//float4 _HealthColor, _HitColor, _LoseColor, _StripeColor,_Color, _VoidColor;


				v2f vert(appdata_t v)
				{
					v2f OUT;
					OUT.worldPosition = v.vertex;
					OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
					OUT.texcoord = v.texcoord;
					OUT.color = v.color * _Color;
					return OUT;
				}
				float4 Grey(float4 color)
				{
					float t = (color.x + color.y + color.z) / 3.0;
					color = float4(t,t,t,1);
					return color;
				}
				
				fixed4 frag(v2f IN) : SV_Target
				{
					float4 color = tex2D(_MainTex, IN.texcoord);// *IN.color;
					//return color;

					bool Research = IN.color.x <= 0.1;
					bool Active = IN.color.x-0.2 <= 0.01;
					float CullDown = IN.color.y;
					float2 uv0 = float2(IN.color.z, IN.color.w);
					float2 uv = (IN.texcoord - uv0);
					uv *= 1024. / 48.;
					uv = (uv - float2(0.5,0.5)) * 2;
					if (Research)
					{
						color = Grey(color);
					}
					else
					{
						if (Active)
						{

							float d = 0.6;
							float t = max(abs(uv.x), abs(uv.y));
							if (t > d)
								t = (1 - t) / (1. - d);							
							else
								t = 1;
							//t *= t;
							color *= t;
							color.w = 1;
						}
						else
						{
						 float pi = 3.141592653589793238462;
							float a = atan2(uv.y, uv.x);
							if (a < 0)
								a += 2 * pi;
							a = a / (2 * pi);
							if (CullDown < a)
								color -= float4(0.3, 0.3, 0.3, 0);
						}
					}
					/*if (LevelUp > 0)
					{
						return color;
						float d = 0.5;
						float t = max(abs(uv.x), abs(uv.y));
						if (t > d)
							t = (1 - t) / (1. - d);
						else
							t = 1;
						t = 1 - t;
						float4 y = float4(1, 1, 0, 1);
						color = color + y * t*LevelUp;
					}*/
					return color;
				}
					
			ENDCG
			}
		}
}
