using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using LightType = UnityEngine.LightType;

public partial class CustomRenderPipeline
{
    partial void InitializeForEditor();

#if UNITY_EDITOR

	partial void InitializeForEditor()
    {
		Lightmapping.SetDelegate(lightsDelegate);
	}
    
	protected override void Dispose(bool disposing)
    {
		base.Dispose(disposing);
		Lightmapping.ResetDelegate();
	}

	static Lightmapping.RequestLightsDelegate lightsDelegate = (Light[] lights, NativeArray<LightDataGI> output) =>
    {
        LightDataGI lightData = new LightDataGI();
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            switch (light.type)
            {
                case LightType.Directional:
                    DirectionalLight directionalLight = new DirectionalLight();
                    LightmapperUtils.Extract(light, ref directionalLight);
                    lightData.Init(ref directionalLight);
                    break;
                case LightType.Point:
                    PointLight pointLight = new PointLight();
                    LightmapperUtils.Extract(light, ref pointLight);
                    lightData.Init(ref pointLight);
                    break;
                case LightType.Spot:
                    SpotLight spotLight = new SpotLight();
                    LightmapperUtils.Extract(light, ref spotLight);
                    spotLight.innerConeAngle = light.innerSpotAngle * Mathf.Deg2Rad;
                    spotLight.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
                    lightData.Init(ref spotLight);
                    break;
                case LightType.Area:
                    RectangleLight rectangleLight = new RectangleLight();
                    LightmapperUtils.Extract(light, ref rectangleLight);
                    rectangleLight.mode = LightMode.Baked;
                    lightData.Init(ref rectangleLight);
                    break;
                default:
                    lightData.InitNoBake(light.GetInstanceID());
                    break;
            }
            lightData.falloff = FalloffType.InverseSquared;
            output[i] = lightData;
        }
    };

#endif
}