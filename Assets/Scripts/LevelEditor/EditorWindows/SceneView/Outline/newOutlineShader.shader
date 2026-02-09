Shader "Custom/InvertedDiagonalLines"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _LineSpacing ("Line Spacing", Float) = 10
        _LineWidth ("Line Width", Range(0, 1)) = 0.5
        _Speed ("Animation Speed", Float) = 1
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _LineSpacing;
            float _LineWidth;
            float _Speed;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;
                // Получаем экранные координаты для стабильности полос при движении
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Получаем исходный цвет и альфу спрайта
                fixed4 texColor = tex2D(_MainTex, i.texcoord) * i.color;
                
                // Рассчитываем положение полос (экранные координаты для стабильности)
                float2 pos = i.screenPos.xy / i.screenPos.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                pos.x *= aspect; 

                // Создаем маску полос
                float lineValue = frac((pos.x + pos.y) * _LineSpacing + _Time.y * _Speed);
                float lineMask = step(lineValue, _LineWidth);

                // Инвертируем исходный цвет спрайта
                fixed3 invertedRGB = fixed3(1.0, 1.0, 1.0) - texColor.rgb;
                
                // Итоговая альфа: она должна быть только там, где есть и спрайт, и полоса
                float finalAlpha = texColor.a * lineMask;

                // Если альфа нулевая, отсекаем пиксель для оптимизации
                if (finalAlpha < 0.01) discard;

                return fixed4(invertedRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}