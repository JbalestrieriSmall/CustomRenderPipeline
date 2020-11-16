Shader "Custom RP/Lit"
{
	Properties
    {
		// Base color
        _BaseMap("Texture", 2D) = "white" {}
		_BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)

		// Cut off
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Toggle(_CLIPPING)] _Clipping("Alpha Clipping", Float) = 0

		// MODS (Metallic in R, Occlusion in G, Detail in B, Smoothness in A)
		[Toggle(_MASK_MAP)] _MaskMapToggle("Mask Map", Float) = 0
		[NoScaleOffset]_MaskMap("Mask (MODS)", 2D) = "white" {}
		_Metallic("Metallic", Range(0, 1)) = 0
		_Occlusion("Occlusion", Range(0, 1)) = 1
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
		_Fresnel("Fresnel", Range(0, 1)) = 1

		// Normals
		[Toggle(_NORMAL_MAP)] _NormalMapToggle("Normal Map", Float) = 0
		[NoScaleOffset] _NormalMap("Normals", 2D) = "bump" {}
		_NormalScale("Normal Scale", Range(0, 1)) = 1

		// Emission
		[NoScaleOffset] _EmissionMap("Emission", 2D) = "white" {}
		[HDR] _EmissionColor("Emission", Color) = (0.0, 0.0, 0.0, 0.0)

		// Details (Albedo in R, Smoothness in B, normal vector XY in AG)
		[Toggle(_DETAIL_MAP)] _DetailMapToggle ("Detail Maps", Float) = 0
		_DetailMap("Details", 2D) = "linearGrey" {}
		[NoScaleOffset] _DetailNormalMap("Detail Normals", 2D) = "bump" {}
		_DetailAlbedo("Detail Albedo", Range(0, 1)) = 1
		_DetailSmoothness("Detail Smoothness", Range(0, 1)) = 1
		_DetailNormalScale("Detail Normal Scale", Range(0, 1)) = 1

		// Transparency
		[Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha("Premultiply Alpha", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1

		// Shadow
		[KeywordEnum(On, Clip, Dither, Off)] _Shadows("Shadows", Float) = 0
		[Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows("Receive Shadows", Float) = 1

		// Lightmaps, allow to bake transparency
		[HideInInspector] _MainTex("Texture for Lightmap", 2D) = "white" {}
		[HideInInspector] _Color("Color for Lightmap", Color) = (0.5, 0.5, 0.5, 1.0)
    }

    SubShader
    {
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include "LitInput.hlsl"
		ENDHLSL

		Pass
        {
			Tags
			{
				"LightMode" = "CustomLit"
			}
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

            HLSLPROGRAM
			#pragma target 3.5
			// Shader_feature allow to enable/disable specific features by creating shader variants
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _PREMULTIPLY_ALPHA
			#pragma shader_feature _RECEIVE_SHADOWS
			#pragma shader_feature _NORMAL_MAP
			#pragma shader_feature _MASK_MAP
			#pragma shader_feature _DETAIL_MAP
			// Create shader variants for each PCF value for directionnal shadows
			#pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
			// Create shader variants for each blend mode
			#pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
			// Create shader variants if shadow mask is enabled
			#pragma multi_compile _ _SHADOW_MASK_ALWAYS _SHADOW_MASK_DISTANCE
			// Create shader variants if lightmap is enabled
			#pragma multi_compile _ LIGHTMAP_ON
			// Create shader variants for LOD
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			// Create shader variants for per object lighting
			#pragma multi_compile _ _LIGHTS_PER_OBJECT
			// Create shader variants for each PCF value for other shadows
			#pragma multi_compile _ _OTHER_PCF3 _OTHER_PCF5 _OTHER_PCF7
			// Generate instancing variants
			#pragma multi_compile_instancing
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
			#include "LitPass.hlsl"
			ENDHLSL
        }

		// Pass for shadow
		Pass
		{
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			ColorMask 0 // Disable color since we only need to render depth for shadows

			HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
			// Create shader variants for LOD
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_instancing
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "ShadowCasterPass.hlsl"
			ENDHLSL
		}

		// Pass for GI
		Pass
		{
			Tags
			{
				"LightMode" = "Meta"
			}

			Cull Off

			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex MetaPassVertex
			#pragma fragment MetaPassFragment
			#include "MetaPass.hlsl"
			ENDHLSL
		}
	}

	CustomEditor "CustomShaderGUI"
}