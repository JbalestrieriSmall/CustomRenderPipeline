Shader "Custom RP/Unlit"
{
	Properties
    {
		// Base color
        _BaseMap("Texture", 2D) = "white" {}
		_BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)

		// Transparency
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

		// Shadow
		[KeywordEnum(On, Clip, Dither, Off)] _Shadows("Shadows", Float) = 0
    }

    SubShader
    {
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include "UnlitInput.hlsl"
		ENDHLSL

		Pass
        {
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

            HLSLPROGRAM
			#pragma target 3.5
			 // shader_feature allow to enable/disable specific features by creating shader variants
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnlitPass.hlsl"
			ENDHLSL
        }

		// Pass for shadow, even if it's unlit, object can still cast shadow
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