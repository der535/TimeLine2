Shader "Unlit/ChessboardNoPerspective"
{
    Properties
    {
        _Scale ("Scale", Float) = 8.0
        _Color1 ("Color 1", Color) = (1, 1, 1, 1)
        _Color2 ("Color 2", Color) = (0, 0, 0, 1)
        _Speed ("Speed", Float) = 1.0
        _Smoothness ("Smoothness", Range(0, 0.1)) = 0.02
        _StretchX ("Stretch X", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            float _Scale;
            fixed4 _Color1;
            fixed4 _Color2;
            float _Speed;
            float _Smoothness;
            float _StretchX;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;

                float2 uv = i.uv;

                // Горизонтальное растяжение/сжатие
                uv.x = (uv.x - 0.5) * _StretchX + 0.5;

                // Анимация по Y (равномерная, без перспективы)
                uv.y += time;

                // Общий масштаб
                uv *= _Scale;

                // Сглаживание границ
                float2 f = frac(uv);
                float2 distToEdge = min(f, 1.0 - f);
                float minDist = min(distToEdge.x, distToEdge.y);

                float2 cell = floor(uv);
                float isWhite = fmod(cell.x + cell.y, 2.0);

                float edge = _Smoothness * _Scale;
                float blend = smoothstep(0.0, edge, minDist);
                float finalBlend = lerp(0.5, isWhite, blend);

                return lerp(_Color2, _Color1, finalBlend);
            }
            ENDCG
        }
    }
}