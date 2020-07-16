sampler2D _SplatMap;
sampler2D _HeightMap;
sampler2D _Noise;
sampler2D _StateColor;
sampler2D _ProvincesColor;
float _MaxHeight;
float _SeaLevel;
float4 _Size;
float4 _NoiseConst;//(Scale, Strength);
bool equals(float4 a, float4 b)
{
	a -= b;
	a *= a;
	float eps = 0.000001;
	return  a.x < eps && a.y < eps && a.z < eps;
}
float2 ToUVScale(float3 worldPos)
{
	return float2(worldPos.x * _Size.x, worldPos.z * _Size.y);
}
float4 ShadedColor(float3 worldPos, float4 c)
{
	float2 pos = float2(worldPos.x * _Size.x, worldPos.z * _Size.y);

	float4 mask = tex2Dlod(_SplatMap,float4( pos,0,0));
	mask.r += mask.g;
	if (mask.r>0)
		c.rgb *= 0.6 + (1 - mask.r)*0.4;
	return c * (1-mask.g);
}
float GetHeight(float2 uv)
{
	float h = tex2Dlod(_HeightMap, float4(uv, 0, 0)).r * _MaxHeight;
	return  h <= _SeaLevel ? _SeaLevel : h;
}
float3 GetNoise(float2 uv)
{

	float2 pert = tex2Dlod(_Noise, float4(uv, 0, 0));
	pert.x = (pert.x * 2 - 1);
	pert.y = (pert.y * 2 - 1);
	return float3(pert.x, 0, pert.y);
}
float4 Mask(float3 worldPos)
{
	float2 pos = float2(worldPos.x * _Size.x, worldPos.z * _Size.y);

	return tex2Dlod(_SplatMap, float4(pos, 0, 0));
}
float4 GetStateColor(float2 uv)
{
	return tex2Dlod(_StateColor, float4(uv, 0, 0));
}
float4 GetProvincesColor(float2 uv)
{
	return tex2Dlod(_ProvincesColor, float4(uv, 0, 0));
}
int GetProvincesIndex(float2 uv)
{
	float4 c = tex2Dlod(_ProvincesColor, float4(uv, 0, 0));
	return (int)((c.r + c.g * 256) * 255);
}
float2 MidUV(float2 uv)
{
	return (float2((int)(uv.x * _Size.z), (int)(uv.y * _Size.w)) + float2(0.5, 0.5)) * _Size.xy;
}
float2 GetAngle(float2 uv, float2 dir)
{
	int a, b, c, d;
	float dx = dir.x * _Size.x;
	float dy = dir.y * _Size.y;
	float y0 = min(uv.y, uv.y + dy);
	float y1 = max(uv.y, uv.y + dy);
	float x0 = min(uv.x, uv.x + dx);
	float x1 = max(uv.x, uv.x + dx);
	a = GetProvincesIndex(float2(x0, y0));
	b = GetProvincesIndex(float2(x1, y0));
	c = GetProvincesIndex(float2(x1, y1));
	d = GetProvincesIndex(float2(x0, y1));
	float2 angle = 0;
	if (a == b && a == d && a != c)
		angle = float2(1, 1);
	else if (b == a && b == c && b != d)
		angle = float2(-1, 1);
	else if (c == b && c == d && c != a)
		angle = float2(-1, -1);
	else if (d == a && d == c && d != b)
		angle = float2(1, -1);
	else if (a == b && a != d && b != c)
		angle = float2(1, 0);
	else if (b == c && b != a && c != d)
		angle = float2(0, 1);
	else if (c == d && c != b && d != a)
		angle = float2(-1, 0);
	else if (d == a && d != c && a != b)
		angle = float2(0, -1);
	return angle;
}
//dir = (+-1,+-1)
float2 Perturb(float2 uv, float2 dir)
{
	float2 angle = GetAngle(uv, dir);
	float2 uvcorner = (float2((int)(uv.x * _Size.z), (int)(uv.y * _Size.w)) + float2(0.5, 0.5) + dir * 0.5)* _Size.xy;
	float2 noise = tex2Dlod(_Noise, float4(uvcorner * 1230, 0, 0)).xz;
	float2 perturb;
	if (length(angle) < 1.1)
	{
		perturb = noise * 2 - float2(1, 1);
	}
	else
	{
		perturb = (noise + float2(0.3, 0.3)) * angle * 0.5;
	}
	return perturb* _NoiseConst.y;
}
bool SimilarProvince(float2 uv1, float2 uv2)
{
	return GetProvincesIndex(uv1) == GetProvincesIndex(uv2);
}
bool SimilarState(float2 uv1, float2 uv2)
{
	return equals(GetStateColor(uv1), GetStateColor(uv2));
}

float2 testingWater(float2 uv1, float2 uv2)
{
	float a1 = GetStateColor(uv1).w;
	float a2 = GetStateColor(uv2).w;
	return float2(a1 + a2, a1 * a2);
}