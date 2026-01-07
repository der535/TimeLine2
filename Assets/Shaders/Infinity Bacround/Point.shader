Shader "Custom/RadialThreeZonePoint_Masked"
{
    Properties
    {
        [Header(Colors)]
        _ColorInner ("Inner Color", Color) = (1, 0, 0, 1)
        _ColorMiddle ("Middle Color", Color) = (0, 1, 0, 1)
        _ColorOuter ("Outer Color", Color) = (0, 0, 1, 1)

        [Header(Boundaries)]
        _InnerSize ("Inner Zone Size", Range(0.0, 1.0)) = 0.2
        _MiddleSize ("Middle Zone Size", Range(0.0, 1.0)) = 0.5
        _OuterSize ("Outer Zone Size (Edge)", Range(0.0, 1.0)) = 0.8
        
        [Header(Blending)]
        _Smoothness ("Transition Softness", Range(0.01, 0.5)) = 0.1

        // --- Обязательные поля для поддержки Mask ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // Настройки Stencil для работы с UI Mask
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc" // Подключаем библиотеку для UI Mask

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Цвет из компонента Image
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPosition : TEXCOORD1; // Нужно для RectMask2D
                fixed4 color : COLOR;
            };

            fixed4 _ColorInner;
            fixed4 _ColorMiddle;
            fixed4 _ColorOuter;
            float _InnerSize;
            float _MiddleSize;
            float _OuterSize;
            float _Smoothness;
            float4 _ClipRect; // Для RectMask2D

            v2f vert (appdata v) {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Расстояние от центра
                float dist = distance(i.uv, float2(0.5, 0.5)) * 2.0;

                // 1. Переход от Inner к Middle
                float mask1 = smoothstep(_InnerSize - _Smoothness, _InnerSize + _Smoothness, dist);
                fixed4 col = lerp(_ColorInner, _ColorMiddle, mask1);

                // 2. Переход от Middle к Outer
                float mask2 = smoothstep(_MiddleSize - _Smoothness, _MiddleSize + _Smoothness, dist);
                col = lerp(col, _ColorOuter, mask2);

                // 3. Плавное исчезновение всей точки (Alpha)
                float finalAlpha = 1.0 - smoothstep(_OuterSize - _Smoothness, _OuterSize + _Smoothness, dist);
                col.a *= finalAlpha;

                // Умножаем на цвет из компонента Image (чтобы работал Alpha у Image)
                col *= i.color;

                // --- Применение маскирования ---
                // Поддержка RectMask2D
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}