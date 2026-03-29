Shader "Custom/SunburstTwist_DOTS"
{
    Properties
    {
        _MainTex ("Mask Texture", 2D) = "white" {} // текстура, из которой берётся альфа для маски
        [MainColor] _BaseColor ("Background Color", Color) = (1,1,1,1)
        _LineColor ("Line Color", Color) = (0,0,0,1)
        _LineCount ("Line Count", Float) = 36
        _RotationOffset ("Rotation Offset", Float) = 0.0
        _Twist ("Twist Factor", Float) = 0.0 // Новое поле для закрутки
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _LineColor;
                float _LineCount;
                float _RotationOffset;
                float _Twist; // Добавлено в CBUFFER для ECS
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 1. Вектор от центра
                float2 fromCenter = input.uv - 0.5;
                
                // 2. Расстояние от центра (нужно для закрутки)
                float dist = length(fromCenter);
                
                // 3. Расчет базового угла
                float angle = atan2(fromCenter.y, fromCenter.x);
                
                // 4. ПРИМЕНЕНИЕ ЗАКРУТКИ:
                // Мы смещаем угол пропорционально расстоянию и значению _Twist
                float twistedAngle = angle + (dist * _Twist);
                
                // 5. Нормализация угла в диапазон [0, 1] с учетом вращения
                float normAngle = (twistedAngle + PI + _RotationOffset * 2.0 * PI) / (2.0 * PI);
                
                // 6. Генерация лучей
                float rays = frac(normAngle * _LineCount);
                
                // Сглаживание
                float feather = 0.02 / _LineCount; 
                float lineMask = smoothstep(0.5 - feather, 0.5 + feather, rays);

                return lerp(_LineColor, _BaseColor, lineMask);
            }
            ENDHLSL
        }
    }
}