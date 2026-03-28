Shader "Unlit/MovingStripesDiagonalClipped"
{
    Properties
    {
        _Stroke("Stroke", Float) = 0.1
        _Frequency("Stroke", Float) = 1

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
        Blend [_SrcFactor] [_DstFactor]
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

            float _Stroke;
            float _Frequency;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float d = i.uv.y - i.uv.x;
                d = frac(d * _Frequency + _Time); // _Frequency = сколько линий на единицу
                if (d < _Stroke || d > 1.0 - _Stroke)
                    return fixed4(1, 1, 1, 1);
                else
                    return fixed4(0, 0, 0, 1);
            }
            ENDCG
        }
    }
}