Shader "Unlit/MovingStripesDiagonalClipped"
{
    Properties
    {
        _Radius("Radius", Float) = 0.1
        _Smoothness("Edge Smoothness", Range(0, 0.1)) = 0.01

        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactor("Src Factor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactor("Dst Factor", Float) = 10
        [Enum(UnityEngine.Rendering.BlendOp)]
        _Opp("Dst Factor", Float) = 10
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent"
        }
        LOD 100
        Blend [_SrcFactor] [_DstFactor] // или SrcAlpha OneMinusDstAlpha — оба работают
        BlendOp [_Opp]
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
                float4 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float _Radius;
            float _Smoothness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pos = float2(0.5, 0.5);
                float dist = length(i.uv - pos);

                // Ручное управление шириной сглаживания
                float edge = _Smoothness;

                // smoothstep(нижняя_граница, верхняя_граница, значение)
                // → 1.0 внутри круга, 0.0 снаружи, плавный переход между
                float alpha = 1.0 - smoothstep(_Radius - edge, _Radius + edge, dist);

                return fixed4(0, 0, 0, alpha); // чёрный круг с прозрачным краем
            }
            ENDCG
        }
    }
}