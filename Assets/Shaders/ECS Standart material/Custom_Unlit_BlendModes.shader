Shader "Universal Render Pipeline/Custom_Unlit_BlendModes"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        
        [Enum(Multiply, 0, Add, 1, Screen, 2, Overlay, 3, Darken, 4, Lighten, 5, SolidColor, 6)] 
        _ColorBlendMode("Color Blend Mode", Float) = 0

        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5

        // BlendMode Props
        _Surface("__surface", Float) = 0.0
        _Blend("__mode", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [ToggleUI] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "UniversalMaterialType" = "Unlit"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Blend [_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        Cull [_Cull]

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            // --- ДОБАВЛЕНО ДЛЯ ИНСТАНСИНГА И DOTS ---
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            // ----------------------------------------

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Макрос для ID инстанса
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Передача ID во фрагментный шейдер
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Cutoff;
                float _ColorBlendMode;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Инициализация инстансинга
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half3 ApplyBlendMode(half3 base, half3 blend, int mode)
            {
                switch (mode)
                {
                    case 0: return base * blend;
                    case 1: return base + blend;
                    case 2: return 1.0 - (1.0 - base) * (1.0 - blend);
                    case 3: return (base < 0.5) ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend));
                    case 4: return min(base, blend);
                    case 5: return max(base, blend);
                    case 6: return blend;
                    default: return base * blend;
                }
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Настройка ID инстанса во фрагментном шейдере
                UNITY_SETUP_INSTANCE_ID(input);

                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 finalRGB = ApplyBlendMode(texColor.rgb, _BaseColor.rgb, (int)_ColorBlendMode);
                half finalAlpha = texColor.a * _BaseColor.a;

                #if defined(_ALPHATEST_ON)
                    clip(finalAlpha - _Cutoff);
                #endif

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}