Shader "Custom/DiagonalLinesAlphaMask_DOTS"
{
    Properties
    {
        _MainTex ("Mask Texture", 2D) = "white" {} // текстура, из которой берётся альфа для маски
        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.1 // порог отсечения

        [Header(Diagonal Lines)]
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _LineCount ("Line Count", Float) = 10
        _LineThickness ("Line Thickness", Range(0,1)) = 0.1
        _LineSpeed ("Line Speed", Float) = 1.0

        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactor("Src factor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactor("Dst factor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendOp)]
        _Opp("Operation", Float) = 5
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Overlay+100" "RenderPipeline"="UniversalPipeline"
        }
        BlendOp [_Opp]
        Blend [_SrcFactor] [_DstFactor]
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _AlphaThreshold;
                half4 _LineColor;
                float _LineCount;
                float _LineThickness;
                float _LineSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                // 1. Проверка маски (оставляем как есть)
                half maskAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a;
                if (maskAlpha < _AlphaThreshold)
                {
                    discard;
                }

                // 2. Расчет линий
                float d = IN.uv.x + IN.uv.y;
                float scaledD = d * _LineCount + _Time.y * _LineSpeed;
                float fracD = frac(scaledD);
                float halfThick = _LineThickness * 0.5;
                float lineMask = smoothstep(0.5 - halfThick, 0.5 - halfThick + 0.01, fracD) -
                    smoothstep(0.5 + halfThick - 0.01, 0.5 + halfThick, fracD);

                // 3. ИСПРАВЛЕНИЕ:
                // Если lineMask меньше маленького порога (почти ноль), полностью отбрасываем пиксель.
                // Это гарантирует, что "пустое" место станет прозрачным для GPU.
                clip(lineMask - 0.001);

                half4 finalColor = _LineColor;
                finalColor.a *= lineMask;

                return finalColor;
            }
            ENDHLSL
        }
    }
}