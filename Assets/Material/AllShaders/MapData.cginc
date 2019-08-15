sampler2D _SplatMap;
float4 _Size;

float4 ShadedColor(float3 worldPos, float4 c)
{
	float2 pos = float2(worldPos.x * _Size.x, worldPos.z * _Size.y);

	float4 mask = tex2Dlod(_SplatMap,float4( pos,0,0));
	mask.r += mask.g;
	if (mask.r>0)
		c.rgb *= 0.6 + (1 - mask.r)*0.4;
	return c * (1-mask.g);
}
float4 Mask(float3 worldPos)
{
	float2 pos = float2(worldPos.x * _Size.x, worldPos.z * _Size.y);

	return tex2Dlod(_SplatMap, float4(pos, 0, 0));
}