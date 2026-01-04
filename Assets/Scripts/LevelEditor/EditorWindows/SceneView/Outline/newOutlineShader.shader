Shader "Custom/URP_SpriteOutlineSmooth_Hollow"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness ("Max Thickness", Float) = 5
        _Softness ("Softness", Range(0.1, 10)) = 1.0
        _InnerCutoff ("Inner Cutoff", Range(0, 1)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline"
        }

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
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _Softness;
            float _InnerCutoff;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float InterleavedGradientNoise(float2 position_screen)
            {
                float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
                return frac(magic.z * frac(dot(position_screen, magic.xy)));
            }

            float4 frag(Varyings input) : SV_Target
            {
                // 1. Проверяем альфу текущего пикселя.
                // Если тут есть спрайт — СРАЗУ выходим, рисуя прозрачность.
                float4 baseTex = tex2D(_MainTex, input.uv);
                if (baseTex.a > _InnerCutoff)
                {
                    discard; // Вырезаем внутренность полностью
                }

                // 2. Если мы здесь, значит пиксель прозрачный, и тут может быть обводка.
                float totalAlpha = 0;
                float maxAlpha = 0;
                const int samples = 32; // Увеличил для плавности
                const float goldenAngle = 2.39996323;

                float noise = InterleavedGradientNoise(input.positionCS.xy);
                float randomRotation = noise * 6.28;

                float2 texelSize = _MainTex_TexelSize.xy;
                float radiusScale = _OutlineThickness * _Softness;

                for (int i = 0; i < samples; i++)
                {
                    float r = sqrt((float)i + 0.5) / sqrt((float)samples);
                    float theta = i * goldenAngle + randomRotation;
                    float2 offset = float2(cos(theta), sin(theta)) * r * radiusScale * texelSize;

                    float a = tex2D(_MainTex, input.uv + offset).a;

                    // Ослабляем влияние сэмплов по мере удаления от края
                    float weight = smoothstep(1.0, 0.0, r);
                    totalAlpha += a * weight;
                    maxAlpha += weight;
                }

                float glow = totalAlpha / maxAlpha;

                // Настройка интенсивности
                glow = saturate(glow * _OutlineColor.a * 5.0);

                // Плавное затухание внешнего края
                glow = pow(glow, 1.5);

                return float4(_OutlineColor.rgb, glow);
            }
            ENDHLSL
        }
    }
}