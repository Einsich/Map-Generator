Shader "Custom/hpBarShader"
{
	Properties
	{
		 [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		//[PerRendererData] _Health("Select", Float) = 0.4
		//[PerRendererData] _Hit("Hit", Float) = .6
		//[PerRendererData] _Count("Count", Int) = 1

		_HealthColor("_HealthColor", Color) = (0,.72,.12,1)
		_HitColor("_HitColor", Color) = (1,.8,.8,1)
		_LoseColor("_LoseColor", Color) = (.7,0,0,1)
		_VoidColor("_VoidColor", Color) = (0,0,0,1)
		_StripeColor("_StripeColor", Color) = (.5,0.5,0.3,1)
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
				float4 _HealthColor, _HitColor, _LoseColor, _StripeColor,_Color, _VoidColor;
				

				v2f vert(appdata_t v)
				{
					v2f OUT;
					OUT.worldPosition = v.vertex;
					OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
					OUT.texcoord = v.texcoord;
					OUT.color = v.color *_Color;
					return OUT;
				}
				float Stripe(float t, float d)
				{
					float a = 0.02;
					while (t - a >= 0)
						t -= d;
					if (t <= -a)
						return 0;
					return -t * t / (a * a) + 1;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
				float4 color;
				float Health = IN.color.x;
				float Hit = IN.color.y;
				float Count = IN.color.z;
				if (Count == 0)
					return _VoidColor;
					float t = IN.texcoord.x;
					if (t < Health)
						color = _HealthColor;
					else
						if (t < Hit)
							color = _HitColor;
						else
							return _LoseColor * tex2D(_MainTex, IN.texcoord);
					float q = Stripe(t,  Count);
					color = color * (1 - q) + _StripeColor * q;
					return color * tex2D(_MainTex, IN.texcoord);
				}
			ENDCG
			}
		}
}
