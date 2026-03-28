Shader "Custom/SimpleColor_DOTS"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 1. Обязательная директива для поддержки DOTS Instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                // Макрос для ID инстанса в атрибутах
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                // Макрос для передачи ID в фрагментный шейдер
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 2. Все свойства из Properties ДОЛЖНЫ быть внутри этого CBUFFER для DOTS
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // 3. Инициализация инстансинга
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 4. Доступ к данным инстанса во фрагментном шейдере
                UNITY_SETUP_INSTANCE_ID(input);

                return _BaseColor;
            }
            ENDHLSL
        }
    }
}