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

float3 GetLighting(Surface surfaceWS, BRDF brdf)
{
	ShadowData shadowData = GetShadowData(surfaceWS);
	float3 color = 0.0;
	for (int i = 0; i < GetDirectionalLightCount(); i++)
	{
		Light light = GetDirectionalLight(i, surfaceWS, shadowData);
		color += GetLighting(surfaceWS, brdf, light);
	}
	return color;
}

#endif