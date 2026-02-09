Shader "Custom/RadialSunburst_Twist"
{
    Properties
    {
        _ColorA ("Color 1", Color) = (1,1,1,1)
        _ColorB ("Color 2", Color) = (0,0,0,1)
        _Segments ("Segments Count", Range(2, 50)) = 10
        _Angle ("Rotation Angle", Range(0, 360)) = 0.0
        _Twist ("Twist Intensity", Float) = 0.0
        _Center ("Center Offset", Vector) = (0.5, 0.5, 0, 0)
        _Smoothness ("Edge Smoothness", Range(0, 0.5)) = 0.01
        
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
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

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            fixed4 _ColorA;
            fixed4 _ColorB;
            float _Segments;
            float _Angle;
            float _Twist;
            float2 _Center;
            float _Smoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Центрируем UV
                float2 uv = i.uv - _Center;
                
                // Вычисляем расстояние до центра
                float dist = length(uv);
                
                // Базовый угол
                float angle = atan2(uv.y, uv.x);

                // Добавляем скручивание
                angle += dist * _Twist;

                // Добавляем вращение через параметр _Angle (переводим градусы в радианы)
                angle += radians(_Angle);

                // Нормализуем угол в диапазон [0, 1]
                float normalizedAngle = (angle / UNITY_TWO_PI) + 0.5;
                
                // Создаем паттерн сегментов
                float pattern = frac(normalizedAngle * _Segments);
                
                // Плавные переходы между цветами
                float edge = _Smoothness * 0.5;
                float transition = smoothstep(0.5 - edge, 0.5 + edge, pattern);
                
                // Интерполяция цветов
                fixed4 finalCol = lerp(_ColorA, _ColorB, transition);
                
                return finalCol * i.color;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}