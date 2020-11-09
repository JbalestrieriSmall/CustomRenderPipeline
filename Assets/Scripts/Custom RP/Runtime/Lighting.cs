using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class Lighting
{
	// Shader ids
    static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");

	// Directional lights data
	const int maxDirLightCount = 4;
	static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
	static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

	// Buffer for profilling purpose
	const string bufferName = "Lighting";
	CommandBuffer buffer = new CommandBuffer {
		name = bufferName
	};

	// Culling data
	CullingResults cullingResults;
	
	public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
    {
		this.cullingResults = cullingResults;

		buffer.BeginSample(bufferName);
		SetupLights();
		buffer.EndSample(bufferName);
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	void SetupLights()
	{
		int dirLightCount = 0;
		NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
		for (int i = 0; i < visibleLights.Length; i++)
		{
			VisibleLight visibleLight = visibleLights[i];
			if (visibleLight.lightType == LightType.Directional)
			{
				SetupDirectionalLight(dirLightCount, ref visibleLight);
				dirLightCount++;
				if (dirLightCount >= maxDirLightCount)
				{
					break;
				}
			}
		}

		buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
		buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
		buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
	}
	
	void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
	{
		dirLightColors[index] = visibleLight.finalColor; // color including intensity
		dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2); // forward vector
    }
}