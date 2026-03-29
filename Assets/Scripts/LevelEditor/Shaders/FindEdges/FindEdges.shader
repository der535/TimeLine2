Shader "Custom/FindEdges_DOTS"
{
    Properties
    {
        [MainTexture] _BaseMap ("Source Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
        _BackgroundColor ("Background Color", Color) = (1,1,1,1)
        _Threshold ("Sensitivity", Range(0, 1)) = 0.2
        _Thickness ("Line Thickness", Range(0.5, 3.0)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Transparent" 
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseMap_TexelSize; // Автоматически заполняется Unity (x=1/w, y=1/h, z=w, w=h)
                half4 _EdgeColor;
                half4 _BackgroundColor;
                float _Threshold;
                float _Thickness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            // Функция для получения яркости пикселя
            float GetLuminance(float2 uv)
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                return (col.r * 0.3 + col.g * 0.59 + col.b * 0.11) * col.a;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 texel = _BaseMap_TexelSize.xy * _Thickness;

                // Оператор Собеля для поиска градиента
                // Выборка соседних пикселей
                float tleft = GetLuminance(input.uv + float2(-texel.x, texel.y));
                float left = GetLuminance(input.uv + float2(-texel.x, 0));
                float bleft = GetLuminance(input.uv + float2(-texel.x, -texel.y));
                float top = GetLuminance(input.uv + float2(0, texel.y));
                float bottom = GetLuminance(input.uv + float2(0, -texel.y));
                float tright = GetLuminance(input.uv + float2(texel.x, texel.y));
                float right = GetLuminance(input.uv + float2(texel.x, 0));
                float bright = GetLuminance(input.uv + float2(texel.x, -texel.y));

                // Горизонтальный и вертикальный градиенты
                float gx = tleft + 2.0 * left + bleft - tright - 2.0 * right - bright;
                float gy = tleft + 2.0 * top + tright - bleft - 2.0 * bottom - bright;

                // Мощность края
                float edge = sqrt(gx * gx + gy * gy);

                // Инвертируем и применяем порог
                float edgeMask = smoothstep(_Threshold, _Threshold + 0.1, edge);

                // Смешиваем цвета: если край - берем EdgeColor, иначе BackgroundColor
                half4 finalColor = lerp(_BackgroundColor, _EdgeColor, edgeMask);
                
                // Учитываем общую прозрачность исходной текстуры в центре
                half centerAlpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a;
                finalColor.a *= centerAlpha;

                return finalColor;
            }
            ENDHLSL
        }
    }
}