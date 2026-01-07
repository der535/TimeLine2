Shader "UI/CustomDirectionalGradient"
{
    Properties
    {
        _ColorTop ("Color 1", Color) = (1,1,1,1)
        _ColorBottom ("Color 2", Color) = (0,0,0,1)
        _Angle ("Angle (Degrees)", Range(0, 360)) = 0
        _Bias ("Color Bias", Range(0.01, 0.99)) = 0.5
        
        // Обязательные параметры для интеграции с UI
        [HideInInspector] _MainTex ("Sprite Texture", 2D) = "white" {}
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
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // Настройки для прозрачности и UI слоев
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
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv : TEXCOORD0;
                float4 color    : COLOR; // Цвет из компонента Image
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 color    : COLOR;
            };

            fixed4 _ColorTop;
            fixed4 _ColorBottom;
            float _Angle;
            float _Bias;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // Пробрасываем цвет Image (включая прозрачность)
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float rad = _Angle * 0.0174533;
                float2 direction = float2(cos(rad), sin(rad));
                float2 centeredUV = i.uv - 0.5;
                
                float t = dot(centeredUV, direction) + 0.5;
                t = saturate(t);

                // Применяем Bias
                t = smoothstep(0.0, 1.0, (t - _Bias + 0.5));

                // Смешиваем цвета
                fixed4 col = lerp(_ColorBottom, _ColorTop, t);
                
                // Умножаем на цвет самого компонента Image (чтобы работал ползунок Alpha в инспекторе)
                col *= i.color;

                return col;
            }
            ENDCG
        }
    }
}