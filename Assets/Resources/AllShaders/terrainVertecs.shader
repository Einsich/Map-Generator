Shader "Custom/terrainVertecs" {
	Properties {
		_Color("Water Color", Color) = (1,1,1,1)
		_TerrainTex("Terrain Map", 2DArray) = "white" {}
		_TerrainNormTex("Normal Map", 2DArray) = "white" {}
		_OccupeTex("Occupe Texture", 2D) = "white" {}
		_OccupeMap("Occupe Map", 2D) = "white" {}
		_TerrainMode("Terrain Mode",Range(0, 1)) = 0
		_TerrainSource("Terrain source", 2DArray) = "white" {}
		_TerrainNormSource("Terrain normals sourse", 2DArray) = "white" {}
		//[PerRendererData]
		_BorderMod("Border mode",Float) = 1
		//	[PerRendererData]
		_Select("Color of select province", Color) = (1,1,1,1)
		//	[PerRendererData]
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
		#pragma surface surf Standard addshadow fullforwardshadows vertex:vert nolightmap 
		#pragma target 4.6
		#include "MapData.cginc"
#include "Tessellation.cginc"

		sampler2D _MainTexNorm,_OccupeTex,_OccupeMap;
		sampler2D _FogNoise, _MapBackgroung;
	sampler2D _MainWaveTex, _FlowMap, _DerivHeightMap;
	float _UJump, _VJump, _Tiling, _Speed, _FlowStrength, _FlowOffset;
	float _HeightScale, _HeightScaleModulated,_TerrainMode;
	UNITY_DECLARE_TEX2DARRAY(_TerrainTex);
	//UNITY_DECLARE_TEX2DARRAY(_TerrainNormTex);
	UNITY_DECLARE_TEX2DARRAY(_TerrainSource);
	UNITY_DECLARE_TEX2DARRAY(_TerrainNormSource);
	float4 _Color;
	float4 _Select;
	float _SelectTime, _BorderMod;
	#define BorderTreshold 0.45
	#define StateBordLen 0.25
	#define ProvBordLen 0.1
		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};


		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		Input vert(inout appdata_full v) {
			Input o;
			v.vertex.y = GetHeight(v.texcoord);
			v.vertex.xz += Perturb(v.texcoord - _Size.xy * 0.5, float2(1, 1));
			return o;
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
				float4 l = UNITY_SAMPLE_TEX2DARRAY(_TerrainTex, float3(uv, i));

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

		float4 BorderColor(float2 t)
		{
			if (t.y == 0)
				return float4(0,0,0,0);
			float w;
			if (t.y == 2)
				w = (((47.22*t.x - 94.44)*t.x + 57.03)*t.x - 9.81)*t.x + 1.0;
			else
				w = ((1.6*t.x - 2.27)*t.x + 0.27)*t.x + 1.0;
			return float4(w, w, w, 1);
		}
		float lengthState, lengthProv;
		float2 uvmid;
		int provInd;
		float2 BordDirection(int prInd, float4 c, float2 uv, float2 d,float boost)
		{
			int prInd1 = GetProvincesIndex(uvmid + d);
			float2 ans = 0;
			if (prInd == prInd1)
				return ans;
			float4 p1 = GetStateColor(uvmid + d);
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
		float4 Border(float2 uv)
		{

			float3 delta = GetNoise(uv ) ;
			//uv.x += delta.x * _Size.x;
			//uv.y += delta.z * _Size.y;
			float2 ans = 0,buf;
			float2 dx = float2(_Size.x, 0);
			float2 dy = float2(0, _Size.y);
			

			float4 c0 = GetStateColor(uvmid);
			float2 d = dx;
			int cS = 0,cP = 0;
			lengthProv = 0.1;
			lengthState = 0.25;
			uint i;
			for (i = 0; i < 4; i++)
			{
				buf = BordDirection(provInd, c0, uv, d,1);
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
			for (i = 0; i<4; i++)
			{
				buf = BordDirection(provInd, c0, uv, d,boost);
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
		float2 GetNormal(float2 fixed_uv)
		{
			float2 angle = GetAngle(fixed_uv - _Size.xy * 0.5, float2(1, 1));
			if (angle.x == 0 || angle.y == 0)
				return angle;
			else
				return float2(angle.y, -angle.x)*0.5;
		}
		float2 BorderDist(float2 uvmid, float2 world, float2 ang1, float2 ang2)
		{
			float2 p0, p1, p2, b0, b1, c0, c1;
			p0 = (uvmid * _Size.zw + ang1 * 0.5);
			p2 = (uvmid * _Size.zw + ang2 * 0.5);
			p1 = (p0 + p2) * 0.5;
			b0 = GetNormal(p0 * _Size.xy);
			b1 = GetNormal(p2 * _Size.xy);

			p0 += Perturb(uvmid, ang1);
			p2 += Perturb(uvmid, ang2);
			p1 += GetNoise(p1 * _Size.xy).xz * 0.2;

			if (p0.x < p2.x)
			{
				float2 temp = p0;
				p0 = p2;
				p2 = temp;
				temp = b0;
				b0 = b1;
				b1 = temp;
			}
			if (dot(p2 - p0, b0) < 0)
				b0 = -b0;
			if (dot(p2 - p0, b1) < 0)
				b1 = -b1;
			float2 s1 = p2 - p0, s2 = world - p0;
			float signMid = s1.x * s2.y - s1.y * s2.x; 
			float sighPoint = 0;
			float2 r1 = p0, r2;
			const float a = 0.5, b = 0.5;
			const int N = 5;
			const float dt = 1.0 / N;
			float len = min(length(world - p0), length(world - p2));
			for (int i = 1; i <= N; i++)
			{
				float t = i * dt;
				r2 = lerp(lerp(p0, p1, t), lerp(p1, p2, t), t);
				float tau = t;
				if (tau < a)
					r2 = r2 * sqrt(tau / a) + (1 - sqrt(tau / a)) * (p0 + t * b0);
				if (t > b)
					r2 = r2 * (1 - sqrt((tau - b) / (1 - b))) + sqrt((tau - b) / (1 - b)) * (p2 + (t - 1) * b1);
				float2 a = world - r1, b = world - r2, c = r2 - r1;
				float dist = (a.x * b.y - a.y * b.x) / length(c);
				if (dot(a, c) < 0 || dot(b, -c) < 0)
					dist = min(length(r1 - world), length(r2 - world));
				else
					dist = abs(dist);
				if (dist < len)
				{
					len = dist;
					sighPoint = c.x * a.y - c.y * a.x;
				}
				r1 = r2;
			}
			return float2(len, sighPoint * signMid);
		}
		float2 BestBordTypeLen(float2 b1, float2 b2)
		{
			if (b1.x < b2.x)
			{
				return b2;
			}
			else if (b1.x > b2.x)
			{
				return b1;
			}
			else
			{
				return b1.y < b2.y ? b1 : b2;
			}
				
		}
		float2 AnalizeBordType(bool similarState, float dist)
		{
			float type = 0;
			if (similarState)
			{
				if (dist < ProvBordLen)
				{
					type = 1;
				}
			}
			else
			{
				if (dist < StateBordLen)
				{
					type = 2;
				}
			}
			return float2(type, dist);
		}
		float4 ImproveBorder(float2 uvmid, float2 world)
		{
			float2 ang[4] = { float2(1, 1), float2(-1, 1), float2(-1, -1), float2(1, -1) };
			float2 dir[4] = { float2(0, 1), float2(-1, 0), float2(0, -1), float2(1, 0) };
			float2 dir_corn[4] = { float2(1, 1), float2(-1, 1), float2(-1, -1), float2(1, -1) };
			float2 ang_corn[8] = { float2(1, -1), float2(-1, 1), float2(-1, -1), float2(1, 1), float2(-1, 1),float2(1, -1),  float2(1, 1), float2(-1, -1) };
			float2 typeLen = float2(0, 100);//1 - provbord, 2 - statebord
			float2 correctUV = uvmid;
			for (int i = 0; i < 4; i++)
			{
				if (!SimilarProvince(uvmid, uvmid + dir[i] * _Size.xy))
				{
					float2 dist = BorderDist(uvmid, world, ang[i], ang[(i + 1) & 3]);
					float2 r = AnalizeBordType(SimilarState(uvmid, uvmid + dir[i] * _Size.xy), dist.x);
					if (dist.y < 0)
						correctUV = uvmid + dir[i] * _Size.xy;
					float2 test = testingWater(uvmid, uvmid + dir[i] * _Size.xy);
					if (test.x != 0 && test.y == 0)
						r = float2(0, 100);
					typeLen = BestBordTypeLen(typeLen, r);
				}
			}
			for (int i = 0; i < 4; i++)
			{
				float2 corn = (uvmid * _Size.zw + dir_corn[i] * 0.5) + Perturb(uvmid, dir_corn[i]);
				if (length(world - corn) < BorderTreshold && !SimilarProvince(uvmid, uvmid + dir_corn[i] * _Size.xy))
				{
					float2 r1 = float2(0, 100), r2 = float2(0, 100);
					if (SimilarState(uvmid, uvmid + float2(dir_corn[i].x, 0) * _Size.xy))
					{
						float2 dist = BorderDist(uvmid + dir_corn[i] * _Size.xy, world, -dir_corn[i], ang_corn[i * 2]);
						r1 = AnalizeBordType(SimilarState(uvmid + dir_corn[i] * _Size.xy, uvmid + float2(dir_corn[i].x, 0) * _Size.xy), dist.x);
						float2 test = testingWater(uvmid + dir_corn[i] * _Size.xy, uvmid + float2(dir_corn[i].x, 0) * _Size.xy);
						if (test.x != 0 && test.y == 0)
							r1 = float2(0, 100);
					}

					if (SimilarState(uvmid, uvmid + float2(0, dir_corn[i].y) * _Size.xy))
					{
						float2 dist = BorderDist(uvmid + dir_corn[i] * _Size.xy, world, -dir_corn[i], ang_corn[i * 2 + 1]);
						r2 = AnalizeBordType(SimilarState(uvmid + dir_corn[i] * _Size.xy, uvmid + float2(0, dir_corn[i].y) * _Size.xy), dist.x);
						float2 test = testingWater(uvmid + dir_corn[i] * _Size.xy, uvmid + float2(0, dir_corn[i].y) * _Size.xy);
						if (test.x != 0 && test.y == 0)
							r2 = float2(0, 100);
					}
					typeLen = BestBordTypeLen(typeLen, BestBordTypeLen(r1, r2));
				}
			}
			float norma = typeLen.x == 1 ? ProvBordLen : StateBordLen;
			typeLen.y = typeLen.y < norma ? typeLen.y / norma : 1;
			return float4(correctUV, typeLen);
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
			float2 world =  uv * _Size.zw;
			uvmid = float2(((int)world.x + 0.5)* _Size.x, ((int)world.y + 0.5)* _Size.y);
			float4 impr = ImproveBorder(uvmid, IN.worldPos.xz);
			float4 c = GetStateColor(impr.xy);
			float2 ocuv = float2(IN.worldPos.x, IN.worldPos.z);
			float4 d = tex2D(_OccupeTex, ocuv*0.5f) * tex2D(_OccupeMap, uv);			
			c = c*(1 - d.a) + d;

			float3 norm;
			float4 water = Water(norm, ocuv)*(1 - c.a);

			o.Normal = norm*(1 - c.a)+ Normals(uv, ocuv)*(c.a);

			if (_TerrainMode > 0)
			{
				c = Terrain(uv,ocuv);
			}
			c = c * (c.a) + water * (1 - c.a);
			provInd = GetProvincesIndex(uvmid);
			if (equals(GetProvincesColor(impr.xy), _Select))
			{
				c *= (0.2*sin((_Time.y - _SelectTime)*4) + 1.2);
			}
			if(impr.b != 0)
			c *= BorderColor(float2(impr.a, impr.b ));
			//float4 border =  Border(uv);
			//if (border.a > 0)
				//c *= border;
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
