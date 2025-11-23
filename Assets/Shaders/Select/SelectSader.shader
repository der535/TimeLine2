Shader "Unlit/MovingStripesDiagonalClipped"
{
    Properties
    {
        _MainTex ("Mask Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Density ("Lines Per World Unit", Float) = 5.0
        _Width ("Line Width (World)", Float) = 0.05
        _Speed ("Speed (World Units/sec)", Float) = 0.5
        _Angle ("Angle (Radians)", Float) = 0.7854 // 45°
        
        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactor("Src Factor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactor("Dst Factor", Float) = 10
        [Enum(UnityEngine.Rendering.BlendOp)]
        _Opp("Dst Factor", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Density;
            float _Width;
            float _Speed;
            float _Angle;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // учитываем tiling/offset


                
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                
                // === 1. Маска по текстуре ===
                fixed4 texColor = tex2D(_MainTex, i.uv);
                if (texColor.a < 0.5) // прозрачные части — не рисуем
                    return fixed4(0,0,0,0);

                // === 2. Диагональный узор в мировых координатах ===
                float sina = sin(_Angle);
                float cosa = cos(_Angle);
                float coord = -sina * i.worldPos.x + cosa * i.worldPos.y;
                float timeOffset = (_Time.y + 100000) * _Speed;
                float patternCoord = coord + timeOffset;

                float spacing = 1.0 / _Density;
                float cycle = spacing;
                float local = fmod(patternCoord, cycle);
                if (local < 0.0) local += cycle;

                // === 3. Рисуем только если в полосе ===
                if (local < _Width)
                    return _Color * texColor.a; // можно умножить на альфу маски
                else
                    return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}