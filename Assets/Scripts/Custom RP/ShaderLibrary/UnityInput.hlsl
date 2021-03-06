#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED

// Most of these values are required to avoid compilation error, they are not all used though
CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;
	float4 unity_LODFade;
    real4 unity_WorldTransformParams;

	// Per object light indices
	real4 unity_LightData;
	real4 unity_LightIndices[2];

	// Occlusion Probes
	float4 unity_ProbesOcclusion;

	// Reflection Probes
	float4 unity_SpecCube0_HDR;

    // Lightmaps
	float4 unity_LightmapST;
	float4 unity_DynamicLightmapST;

    // Light Probes
	float4 unity_SHAr;
	float4 unity_SHAg;
	float4 unity_SHAb;
	float4 unity_SHBr;
	float4 unity_SHBg;
	float4 unity_SHBb;
	float4 unity_SHC;

    // Light Probe Proxy Volume (LLPV)
	float4 unity_ProbeVolumeParams;
	float4x4 unity_ProbeVolumeWorldToObject;
	float4 unity_ProbeVolumeSizeInv;
	float4 unity_ProbeVolumeMin;
CBUFFER_END

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;
float3 _WorldSpaceCameraPos;

#endif