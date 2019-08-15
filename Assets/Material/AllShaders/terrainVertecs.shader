Shader "Custom/terrainVertecs" {
	Properties {
		_Color("Water Color", Color) = (1,1,1,1)
		_MainTex("Color Map", 2D) = "white" {}
		_TerrainTex("Terrain Map", 2DArray) = "white" {}
		_TerrainNormTex("Normal Map", 2DArray) = "white" {}
		_OccupeTex("Occupe Texture", 2D) = "white" {}
		_OccupeMap("Occupe Map", 2D) = "white" {}
		_ProvincesMap("Provinces Map",2D) = "white"{}
		_TerrainMode("Terrain Mode",Range(0, 1)) = 0
		_TerrainSource("Terrain source", 2DArray) = "white" {}
		_TerrainNormSource("Terrain normals sourse", 2DArray) = "white" {}
		_BorderMod("Border mode",Float) = 1
		_Select("Color of select province", Color) = (1,1,1,1)
		_SelectTime("Time of select",Float) = 0
		_FogNoise("Fog noise",2D) = "white"{}
		_MapBackgroung("Map background",2D) = "white"{}

		_MainWaveTex("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _FlowMap("Flow (RG, A noise)", 2D) = "black" {}
		[NoScaleOffset] _DerivHeightMap("Deriv (AG) Height (B)", 2D) = "black" {}
		_UJump("U jump per phase", Range(-0.25, 0.25)) = 0.25
		_VJump("V jump per phase", Range(-0.25, 0.25)) = 0.25
		_Tiling("Tiling", Float) = 1
		_Speed("Speed", Float) = 1
		_FlowStrength("Flow Strength", Float) = 1
		_FlowOffset("Flow Offset", Float) = 0
		_HeightScale("Height Scale, Constant", Float) = 0.25
		_HeightScaleModulated("Height Scale, Modulated", Float) = 0.75

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard vertex:vert

		#pragma target 3.0
		#include "MapData.cginc"

		sampler2D _MainTex,_MainTexNorm,_OccupeTex,_OccupeMap;
		sampler2D _FogNoise, _MapBackgroung;
	sampler2D _MainWaveTex, _FlowMap, _DerivHeightMap, _ProvincesMap;
	float _UJump, _VJump, _Tiling, _Speed, _FlowStrength, _FlowOffset;
	float _HeightScale, _HeightScaleModulated,_TerrainMode;
	UNITY_DECLARE_TEX2DARRAY(_TerrainTex);
	UNITY_DECLARE_TEX2DARRAY(_TerrainNormTex);
	UNITY_DECLARE_TEX2DARRAY(_TerrainSource);
	UNITY_DECLARE_TEX2DARRAY(_TerrainNormSource);
	float4 _Color;
	float4 _Select;
	float _SelectTime, _BorderMod;
		struct Input {
			float2 uv_MainTex;
			float2 ocuv;
			float3 worldPos;
		};


		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

			void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.ocuv = v.texcoord1.xy;
		}

		float3 UnpackDerivativeHeight(float4 textureData) {
			float3 dh = textureData.agb;
			dh.xy = dh.xy * 2 - 1;
			return dh;
		}


		float3 FlowUVW(
			float2 uv, float2 flowVector, float2 jump,
			float flowOffset, float tiling, float time, bool flowB
		) {
			float phaseOffset = flowB ? 0.5 : 0;
			float progress = frac(time + phaseOffset);
			float3 uvw;
			uvw.xy = uv - flowVector * (progress + flowOffset);
			uvw.xy *= tiling;
			uvw.xy += phaseOffset;
			uvw.xy += (time - progress) * jump;
			uvw.z = 1 - abs(1 - 2 * progress);
			return uvw;
		}

		float4 Terrain(float2 uv,float2 foguv)
		{
			float4 c;
			for (int i = 0; i < 4; i++)
			{
				float4 l = UNITY_SAMPLE_TEX2DARRAY(_TerrainTex, float3(uv,i));
				
					for (int j = 0; j < 4; j++)if (i * 4 + j < 14)
						c += UNITY_SAMPLE_TEX2DARRAY(_TerrainSource, float3(foguv*0.3, i * 4 + j))*l[j];
			}
			return c;
		}
		float4 Normals(float2 uv, float2 foguv)
		{
			float4 c;
			for (int i = 0; i < 4; i++)
			{
				float4 l = UNITY_SAMPLE_TEX2DARRAY(_TerrainNormTex, float3(uv, i));

				for (int j = 0; j < 4; j++)if (i * 4 + j < 14)
					c += UNITY_SAMPLE_TEX2DARRAY(_TerrainNormSource, float3(foguv*0.1, i * 4 + j))*l[j];
			}
			return normalize(c)*2;
		}
		float4 Water(out float3 norm,float2 ocuv)
		{
			float2 wateruv = ocuv / 10;
			float3 flow = tex2D(_FlowMap, wateruv).rgb;
			flow.xy = flow.xy * 2 - 1;
			flow *= _FlowStrength;
			float noise = tex2D(_FlowMap, wateruv).a;
			float time = _Time.y * _Speed + noise;
			float2 jump = float2(_UJump, _VJump);

			float3 uvwA = FlowUVW(
				wateruv, flow.xy, jump,
				_FlowOffset, _Tiling, time, false
			);
			float3 uvwB = FlowUVW(
				wateruv, flow.xy, jump,
				_FlowOffset, _Tiling, time, true
			);

			float finalHeightScale =
				flow.z * _HeightScaleModulated + _HeightScale;

			float3 dhA =
				UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwA.xy)) *
				(uvwA.z * finalHeightScale);
			float3 dhB =
				UnpackDerivativeHeight(tex2D(_DerivHeightMap, uvwB.xy)) *
				(uvwB.z * finalHeightScale);

			norm =  normalize(float3(-(dhA.xy + dhB.xy), 1));

			float4 texA = tex2D(_MainWaveTex, uvwA.xy) * uvwA.z;
			float4 texB = tex2D(_MainWaveTex, uvwB.xy) * uvwB.z;

			return ((texA + texB) * _Color);
		}

		bool equals(float4 a, float4 b)
		{
			a -= b;
			a *= a;
			float eps = 0.000001;
			return  a.x< eps && a.y<eps&&a.z<eps;
		}
		float4 BorderColor(float2 t)
		{
			if (t.y == 0)
				return float4(0,0,0,0);
			float w;
			if (t.y == 1.)
				w = (((47.22*t.x - 94.44)*t.x + 57.03)*t.x - 9.81)*t.x + 1.0;
			else
				w = ((1.6*t.x - 2.27)*t.x + 0.27)*t.x + 1.0;
			return float4(w, w, w, 1);
		}
		float lengthState=0.15, lengthProv=0.05;
		float2 uvmid;
		float4 p0;
		float2 BordDirection(float4 p, float4 c, float2 uv, float2 d,float boost)
		{
			float4 p1 = tex2D(_ProvincesMap, uvmid + d);
			float2 ans = 0;
			if (equals(p, p1))
				return ans;
			p1 = tex2D(_MainTex, uvmid + d);
			bool equal = equals(c, p1);
			if ((c.a == 0 || p1.a == 0) && !equal)
				equal = true;
			if (equal && _BorderMod == 1.)
				return ans;
			uv.x *= _Size.z;
			uv.y *= _Size.w;
			d.x *= _Size.z;
			d.y *= _Size.w;
			float2 v = float2((uv.x - (int)uv.x), (uv.y - (int)uv.y));
		
			
			v.x = (1 + d.x)*0.5 - d.x * v.x;
			v.y = (1 + d.y)*0.5 - d.y * v.y;

			
			float l = (v.x*abs(d.x) +v.y*abs(d.y));
			if (!equal)
			{
				
				if (l < lengthState)
				{
					if(boost == 2.)
					l = min(v.x, v.y)*boost;
					ans = float2((lengthState - l) / lengthState, 1);
					//t = ;
					
				}
			}
			else
				if (l < lengthProv)
				{
					if (boost == 2.)
						l = min(v.x, v.y)*boost;
					ans = float2((lengthProv - l) / lengthProv, 2);
				}
			return ans;
		}
		float4 Border(float2 uv,float2 world)
		{
			float2 ans = 0,buf;
			float2 dx = float2(_Size.x, 0);
			float2 dy = float2(0, _Size.y);
			

			float4 c0 = tex2D(_MainTex, uvmid);
			float2 d = dx;
			int cS = 0,cP = 0;
			lengthProv = 0.1;
			lengthState = 0.15;
			for (int i = 0; i < 4; i++)
			{
				buf = BordDirection(p0, c0, uv, d,1);
				if (buf.y > 0)
				{
					if (ans.y != 1.)
					ans = buf;
					if (buf.y == 1.)
					cS++;
					if (buf.y == 2.)
						cP++;
				}
				d = float2(d.y, -d.x);
			}
			if (cS == 1 )
				return BorderColor(ans);
			d = dx +dy;
			float boost = (cS == 2 ||cP == 2) ? 2. : 1;
			lengthProv *= boost;
			lengthState *= boost;
			for (int i = 0; i<4; i++)
			{
				buf = BordDirection(p0, c0, uv, d,boost);
				if (buf.y > 0)
				{
					if (buf.y == 1.||(ans.y!=1.&& cP != 1))
					{
						ans = buf;
						break;
					}
				}
				d = float2(d.y, -d.x);
			}

			return BorderColor(ans);
		}
		float4 FogAndIncognito(float3 pos)
		{
			float4 splat = Mask(pos);
			float2 foguv = float2(pos.x,pos.z) * 0.025;
			float2 foguv1 = foguv;
			foguv1.y -= _Time.y*0.04;
			foguv.x += sin(_Time.x)*sin(_Time.x)*0.05 + _Time.x;


			float4 color1 = (tex2D(_FogNoise, foguv) + tex2D(_FogNoise, foguv1));
			color1.rgb = color1.rgb * 0.2 - float3(0.1, 0.1, 0.1);
			color1.a = 0.65;
			color1 *= splat.r;
			float4 color2 = tex2D(_MapBackgroung, float2(pos.x, pos.z) * _Size.xy)*splat.g;
			float4 color3 = splat.b*float4(0, 0, 0, 0);
			return  color1 + color2 + color3;
		}
		void surf (Input IN, inout SurfaceOutputStandard o) {	
			
			float2 uv = IN.uv_MainTex;
			float4 c = tex2D(_MainTex, uv);
			
			float4 d = tex2D(_OccupeTex, IN.ocuv*0.5f) * tex2D(_OccupeMap, uv);			
			c = c*(1 - d.a) + d;

			float3 norm;
			float4 water = Water(norm, IN.ocuv)*(1 - c.a);

			o.Normal = norm*(1 - c.a)+ Normals(uv, IN.ocuv)*(c.a);

			if (_TerrainMode > 0)
			{
				c = Terrain(uv, IN.ocuv);
			}
			c = c * (c.a) + water * (1 - c.a);
			float2 world = float2(uv.x* _Size.z, uv.y* _Size.w);
			uvmid = float2(((int)world.x + 0.5)* _Size.x, ((int)world.y + 0.5)* _Size.y);
			p0 = tex2D(_ProvincesMap, uvmid);

			if (equals(p0, _Select))
			{
				c *= (0.5*sin((_Time.y - _SelectTime)*4) + 1.5);
			}
			float4 border =  Border(uv, IN.ocuv);
			if (border.a > 0)
				c *= border;
			float4 fog = FogAndIncognito(IN.worldPos);
			//c = ShadedColor(IN.worldPos, c);
			
			c.rgb = c.rgb * (1 - fog.a) + fog * fog.a;
			o.Normal = o.Normal* (1 - fog.a) + float3(0, 1, 0)*fog.a;
			o.Metallic = 0;
			o.Smoothness = 0.5;
			o.Albedo = c.rgb;

		}
		ENDCG
	}
	FallBack "Diffuse"
}
