using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

	ScriptableRenderContext context;

	Camera camera;

	const string bufferName = "Custom Render Camera";
	CommandBuffer buffer = new CommandBuffer { name = bufferName };

	CullingResults cullingResults;

	Lighting lighting = new Lighting();

	public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
    {
		this.context = context;
		this.camera = camera;

        // Do it before culling because it can add geometry to the scene
        PrepareBuffer();
        PrepareForSceneWindow();

		if (!Cull())
        {
			return;
		}

		BeginSample();

		lighting.Setup(context, cullingResults);
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
	    DrawUnsupportedShaders();
	    DrawGizmos();

		EndSample();
	}

	void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        // Draw opaque
        SortingSettings sortingSettings = new SortingSettings(this.camera) {
            criteria = SortingCriteria.CommonOpaque
        };
	
        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) {
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing
		};
		drawingSettings.SetShaderPassName(1, litShaderTagId);
	
		FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        // Draw skybox
		context.DrawSkybox(camera);

        // Draw transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawingSettings.sortingSettings = sortingSettings;
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;
		context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
	}

	void BeginSample()
    {
		context.SetupCameraProperties(camera);
        bool shouldClearDepth = camera.clearFlags <= CameraClearFlags.Depth;
        bool shouldClearColor = camera.clearFlags == CameraClearFlags.Color;
        Color clearColor = shouldClearColor ? camera.backgroundColor.linear : Color.clear;
		buffer.ClearRenderTarget(shouldClearDepth, shouldClearColor, clearColor);
		buffer.BeginSample(SampleName);
		ExecuteBuffer();
	}

	void EndSample()
    {
		buffer.EndSample(SampleName);
		ExecuteBuffer();
		context.Submit();
	}

    bool Cull()
    {
		if (camera.TryGetCullingParameters(out ScriptableCullingParameters parameters))
        {
			cullingResults = context.Cull(ref parameters);
			return true;
		}
		return false;
	}

	void ExecuteBuffer()
    {
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}
}