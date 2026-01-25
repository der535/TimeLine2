Shader "Custom/URP_SpriteOutline_WorldSpace"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness ("Thickness (World Units)", Float) = 0.05
        _InnerCutoff ("Inner Cutoff", Range(0, 1)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _InnerCutoff;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 baseTex = tex2D(_MainTex, input.uv);
                
                // Рисуем только там, где прозрачно (внутри вырезаем)
                if (baseTex.a > _InnerCutoff) discard;

                // Векторы осей объекта в мировом пространстве
                // Позволяют перевести "мировое смещение" обратно в локальные UV
                float3 worldRight = normalize(float3(GetObjectToWorldMatrix()[0].xyz));
                float3 worldUp = normalize(float3(GetObjectToWorldMatrix()[1].xyz));
                
                // Масштаб объекта (чтобы понять размер 1 юнита в UV)
                float scaleX = length(float3(GetObjectToWorldMatrix()[0].xyz));
                float scaleY = length(float3(GetObjectToWorldMatrix()[1].xyz));

                float totalAlpha = 0;
                const int samples = 12; // Для мирового пространства 12 точек обычно хватает
                const float PI2 = 6.283185;

                for (int i = 0; i < samples; i++)
                {
                    float angle = (float)i / samples * PI2;
                    float2 dir = float2(cos(angle), sin(angle));
                    
                    // Вычисляем смещение в UV координатах
                    // Толщина делится на масштаб: если объект в 2 раза больше, 
                    // нужно пройти в 2 раза меньше "пути" в UV, чтобы получить тот же метр.
                    float2 uvOffset = float2(dir.x / scaleX, dir.y / scaleY) * _OutlineThickness;
                    
                    totalAlpha += tex2D(_MainTex, input.uv + uvOffset).a;
                }

                float alpha = saturate(totalAlpha / samples * _OutlineColor.a * 10.0);

                return float4(_OutlineColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}