Shader "Custom/CircleWithHole"
{
    Properties
    {
        _MainColor ("Circle Color", Color) = (1, 1, 1, 1)
        _Radius ("Radius", Range(0, 0.5)) = 0.4
        _Thickness ("Thickness", Range(0, 0.5)) = 0.1
        _Smoothness ("Smoothness", Range(0, 0.1)) = 0.01
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

            fixed4 _MainColor;
            float _Radius;
            float _Thickness;
            float _Smoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; // UV координаты от 0 до 1
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Смещаем координаты, чтобы (0,0) был в центре
                float2 centeredUV = i.uv - 0.5;
                
                // Считаем расстояние от текущего пикселя до центра
                float dist = length(centeredUV);

                // Вычисляем внешний и внутренний радиусы
                float outerRadius = _Radius;
                float innerRadius = _Radius - _Thickness;

                // Создаем маску для круга с мягкими краями (anti-aliasing)
                float outerCircle = smoothstep(outerRadius, outerRadius - _Smoothness, dist);
                float innerCircle = smoothstep(innerRadius, innerRadius - _Smoothness, dist);

                // Вычитаем внутренний круг из внешнего, чтобы получить кольцо
                float mask = outerCircle - innerCircle;

                // Применяем цвет и прозрачность
                fixed4 col = _MainColor;
                col.a *= mask;

                return col;
            }
            ENDCG
        }
    }
}