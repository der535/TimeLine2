Shader "Custom/Distortion"
{
    Properties
    {
        _MainTex ("Noise/Flow Map", 2D) = "white" {}
        _Strength ("Distortion Strength", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Обычные объявления текстур и сэмплеров
            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float _Strength;

            // Доступ к _CameraOpaqueTexture (только если включено в URP asset)
            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Считываем смещение из flow/noise-текстуры
                half2 flow = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rg;
                half2 offset = (flow - 0.5) * 2 * _Strength;

                // Преобразуем clip-space позицию в UV экрана
                float2 screenUV = input.positionCS.xy / _ScreenParams.xy;

                // Применяем искажение
                half4 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + offset);

                return sceneColor;
            }
            ENDHLSL
        }
    }
}