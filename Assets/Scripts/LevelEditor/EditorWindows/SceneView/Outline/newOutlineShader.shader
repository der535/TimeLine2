Shader "Custom/InvertedDiagonalLines_ECS_ScreenInvert"
{
    Properties
    {
        _LineSpacing ("Line Spacing", Float) = 10
        _LineWidth ("Line Width", Range(0, 1)) = 0.5
        _Speed ("Animation Speed", Float) = 1
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Overlay+1" // Чуть позже обычного Overlay
            "RenderType"="Transparent" 
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        ZTest Always // Поверх всего
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // ВАЖНО: Подключаем библиотеку для чтения экрана
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float _LineSpacing;
                float _LineWidth;
                float _Speed;
            CBUFFER_END

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                // 1. Получаем координаты для чтения экрана
                float2 uv = i.screenPos.xy / i.screenPos.w;

                // 2. Читаем цвет фона (то, что под объектом)
                // SampleSceneColor — встроенная функция URP
                half3 sceneColor = SampleSceneColor(uv);

                // 3. Рассчитываем линии (логика та же)
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 pos = uv;
                pos.x *= aspect;

                float time = _Time.y;
                float lineValue = frac((pos.x + pos.y) * _LineSpacing + time * _Speed);
                float lineMask = step(lineValue, _LineWidth);

                // 4. ИНВЕРСИЯ: Если попали в полоску — инвертируем sceneColor, иначе прозрачно
                half3 invertedColor = 1.0 - sceneColor;
                
                // Если lineMask = 1 (внутри линии), рисуем инверсию. Если 0 — прозрачно.
                return half4(invertedColor, lineMask);
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}