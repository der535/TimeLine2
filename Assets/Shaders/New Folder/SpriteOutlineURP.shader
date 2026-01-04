Shader "Custom/SpriteOutlineSmooth"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // Явно объявляем TexelSize — URP не создаёт его автоматически
            float4 _MainTex_TexelSize; // ← это решает ошибку

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half alpha = base.a;

                if (alpha > 0.5)
                    return base * _Color;

                // Размер одного пикселя текстуры в UV-координатах
                float2 texelSize = _MainTex_TexelSize.xy; // ← теперь безопасно

                float sampleDist = _OutlineWidth;
                float outline = 0;

                // 8-directional sample
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-sampleDist, -sampleDist) * texelSize).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( 0,          -sampleDist) * texelSize).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( sampleDist, -sampleDist) * texelSize).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-sampleDist,  0         ) * texelSize).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( sampleDist,  0         ) * texelSize).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-sampleDist,  sampleDist) * texelSize).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( 0,           sampleDist) * texelSize).a;
                outline += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( sampleDist,  sampleDist) * texelSize).a;

                if (outline > 0.001)
                    return _OutlineColor;

                return 0;
            }
            ENDHLSL
        }
    }
}