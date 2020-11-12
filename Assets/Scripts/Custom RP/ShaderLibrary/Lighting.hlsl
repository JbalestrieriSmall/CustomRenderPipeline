#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

float3 GetIncomingLight(Surface surfaceWS, Light light)
{
	return saturate(dot(surfaceWS.normal, light.direction) * light.attenuation) * light.color;
}

float3 GetLighting(Surface surfaceWS, BRDF brdf, Light light)
{
	return GetIncomingLight(surfaceWS, light) * DirectBRDF(surfaceWS, brdf, light);
}

float3 GetLighting(Surface surfaceWS, BRDF brdf, GI gi)
{
	ShadowData shadowData = GetShadowData(surfaceWS);
	shadowData.shadowMask = gi.shadowMask;
	float3 color = gi.diffuse * brdf.diffuse;
	for (int i = 0; i < GetDirectionalLightCount(); i++)
	{
		Light light = GetDirectionalLight(i, surfaceWS, shadowData);
		color += GetLighting(surfaceWS, brdf, light);
	}
	return color;
}

#endif