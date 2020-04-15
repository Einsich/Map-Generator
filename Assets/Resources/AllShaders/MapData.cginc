sampler2D _SplatMap;
sampler2D _HeightMap;
sampler2D _Noise;
float _MaxHeight;
float _SeaLevel;
float4 _Size;
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
	h = h <= _SeaLevel ? _SeaLevel : h;
	return  h ;
}
float3 GetNoise(float2 uv)
{

	float2 pert = tex2Dlod(_Noise, float4(uv, 0, 0));
	pert.x = (pert.x * 2 - 1);
	pert.y = (pert.y * 2 - 1);
	return float3(pert.x, 0, pert.y);
}
float GetHeightUV(float2 uv)
{
	float h = tex2Dlod(_HeightMap, float4(uv, 0, 0)).r * _MaxHeight;
	h = h <= _SeaLevel ? _SeaLevel : h;
	return  h;
}
float4 Mask(float3 worldPos)
{
	float2 pos = float2(worldPos.x * _Size.x, worldPos.z * _Size.y);

	return tex2Dlod(_SplatMap, float4(pos, 0, 0));
}