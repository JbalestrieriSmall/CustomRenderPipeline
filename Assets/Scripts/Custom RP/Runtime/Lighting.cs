﻿using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class Lighting
{
    // Shader ids
    static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");
    static int otherLightCountId = Shader.PropertyToID("_OtherLightCount");
    static int otherLightColorsId = Shader.PropertyToID("_OtherLightColors");
    static int otherLightPositionsId = Shader.PropertyToID("_OtherLightPositions");
    static int otherLightDirectionsId = Shader.PropertyToID("_OtherLightDirections");
    static int otherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles");
	static int otherLightShadowDataId = Shader.PropertyToID("_OtherLightShadowData");

    // Keyword
    static string lightsPerObjectKeyword = "_LIGHTS_PER_OBJECT";

    // Directional lights data
    const int maxDirLightCount = 4;
    const int maxOtherLightCount = 4;
    static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
    static Vector4[] dirLightShadowData = new Vector4[maxDirLightCount];

    // Other lights data
    static Vector4[] otherLightColors = new Vector4[maxOtherLightCount];
    static Vector4[] otherLightPositions = new Vector4[maxOtherLightCount];
    static Vector4[] otherLightDirections = new Vector4[maxOtherLightCount];
	static Vector4[] otherLightSpotAngles = new Vector4[maxOtherLightCount];
	static Vector4[] otherLightShadowData = new Vector4[maxOtherLightCount];

    // Buffer (for profilling purpose)
    const string bufferName = "Lighting";
    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    // Culling data
    CullingResults cullingResults;

    // Shadows
    Shadows shadows = new Shadows();

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings, bool useLightsPerObject)
    {
        this.cullingResults = cullingResults;

        buffer.BeginSample(bufferName);

        shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights(useLightsPerObject);
        shadows.Render();

        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupLights(bool useLightsPerObject)
    {
        int otherLightCount = 0;
        int dirLightCount = 0;

        // Indices used to identify lights affecting objects
        NativeArray<int> indexMap = useLightsPerObject ? cullingResults.GetLightIndexMap(Allocator.Temp) : default;
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int i;
        for (i = 0; i < visibleLights.Length; i++)
        {
            int newIndex = -1;
            VisibleLight visibleLight = visibleLights[i];
            switch (visibleLight.lightType)
            {
                case LightType.Directional:
                    if (dirLightCount < maxDirLightCount)
                    {
                        SetupDirectionalLight(dirLightCount++, i, ref visibleLight);
                    }
                    break;
                case LightType.Point:
                    if (otherLightCount < maxOtherLightCount)
                    {
                        newIndex = otherLightCount;
                        SetupPointLight(otherLightCount++, i, ref visibleLight);
                    }
                    break;
				case LightType.Spot:
					if (otherLightCount < maxOtherLightCount)
                    {
                        newIndex = otherLightCount;
						SetupSpotLight(otherLightCount++, i, ref visibleLight);
					}
					break;
            }
			if (useLightsPerObject)
            {
				indexMap[i] = newIndex;
			}
        }
        
        // Cleanup remaining indices
		if (useLightsPerObject)
        {
			for (; i < indexMap.Length; i++)
            {
				indexMap[i] = -1;
			}
			cullingResults.SetLightIndexMap(indexMap);
			indexMap.Dispose();
			Shader.EnableKeyword(lightsPerObjectKeyword);
		}
        else
        {
			Shader.DisableKeyword(lightsPerObjectKeyword);
        }

        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        if (dirLightCount > 0)
        {
            buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
            buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
            buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
        }

        buffer.SetGlobalInt(otherLightCountId, otherLightCount);
        if (otherLightCount > 0)
        {
            buffer.SetGlobalVectorArray(otherLightColorsId, otherLightColors);
            buffer.SetGlobalVectorArray(otherLightPositionsId, otherLightPositions);
            buffer.SetGlobalVectorArray(otherLightDirectionsId, otherLightDirections);
			buffer.SetGlobalVectorArray(otherLightSpotAnglesId, otherLightSpotAngles);
			buffer.SetGlobalVectorArray(otherLightShadowDataId, otherLightShadowData);
        }
    }

    void SetupDirectionalLight(int index, int visibleIndex, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor; // color including intensity
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2); // forward vector
        dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, visibleIndex);
    }

    void SetupPointLight(int index, int visibleIndex, ref VisibleLight visibleLight)
    {
        otherLightColors[index] = visibleLight.finalColor; // color including intensity
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3); // light position
        // Use the light range to hide the light (otherwise it would be visible almost indefinitly)
        position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
        otherLightPositions[index] = position;
		otherLightSpotAngles[index] = new Vector4(0f, 1f);
		Light light = visibleLight.light;
		otherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
    }

    void SetupSpotLight(int index, int visibleIndex, ref VisibleLight visibleLight)
    {
        otherLightColors[index] = visibleLight.finalColor; // color including intensity
        Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3); // light position
        position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
        otherLightPositions[index] = position;
        otherLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2); // forward vector

        Light light = visibleLight.light;
		float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
		float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
		float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
		otherLightSpotAngles[index] = new Vector4(angleRangeInv, -outerCos * angleRangeInv);
		otherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
    }

    public void Cleanup()
    {
        shadows.Cleanup();
    }
}