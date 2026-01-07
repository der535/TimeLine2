Shader "Custom/ChessboardFullControlScroll"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Icon Tint", Color) = (1,1,1,1)
        _BgColor ("Background Color", Color) = (0,0,0,0)
        
        _Scale ("Total Count", Float) = 5.0
        
        [Header(Movement Settings)]
        _ScrollDirection ("Direction (X, Y)", Vector) = (1, 0, 0, 0)
        _Speed ("Scroll Speed", Float) = 0.5
        
        [Header(Spacing Settings)]
        _GapX ("Horizontal Gap", Range(0, 1)) = 0.1
        _GapY ("Vertical Gap", Range(0, 1)) = 0.1
        _OffsetAmount ("Row Offset", Range(0, 1)) = 0.5
        
        [Header(Icon Settings)]
        _IconSizeX ("Icon Scale X", Range(0.1, 2)) = 1.0
        _IconSizeY ("Icon Scale Y", Range(0.1, 2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _BgColor;
            float _Scale;
            float _GapX, _GapY;
            float _IconSizeX, _IconSizeY;
            float _OffsetAmount;
            
            // Новые переменные для движения
            float4 _ScrollDirection;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Рассчитываем смещение на основе времени и направления
                // _Time.y — это стандартное время в Unity (аналог Time.time)
                float2 offset = _ScrollDirection.xy * _Time.y * _Speed;
                
                // Применяем смещение к UV перед масштабированием сетки
                o.uv = (v.uv + offset) * _Scale;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 gridUV = i.uv;
                
                // 1. Шахматный сдвиг рядов
                float row = floor(gridUV.y);
                if (fmod(abs(row), 2.0) >= 1.0)
                {
                    gridUV.x += _OffsetAmount;
                }

                float2 localUV = frac(gridUV);

                // 2. Учет зазоров и масштаба иконки
                float2 finalScale = float2(_IconSizeX, _IconSizeY) * (1.0 - float2(_GapX, _GapY));
                
                // 3. Центрирование текстуры в ячейке
                float2 centeredUV = (localUV - 0.5) / finalScale + 0.5;

                // 4. Отрисовка
                if (any(centeredUV < 0.0) || any(centeredUV > 1.0))
                {
                    return _BgColor;
                }

                fixed4 texCol = tex2D(_MainTex, centeredUV) * _Color;
                return lerp(_BgColor, texCol, texCol.a);
            }
            ENDCG
        }
    }
}
