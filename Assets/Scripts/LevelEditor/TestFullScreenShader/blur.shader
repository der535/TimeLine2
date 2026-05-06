Shader "PostProcess/TwoPassGaussian"
{
    Properties { _Intensity("Intensity", Float) = 1 }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZTest Always ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float _Intensity;
        // Веса Гаусса для ядра 5x5
        static const float weights[5] = { 0.06136, 0.24477, 0.38774, 0.24477, 0.06136 };
        static const float offsets[5] = { -2, -1, 0, 1, 2 };

        float4 Blur(Varyings input, float2 direction)
        {
            float2 uv = input.texcoord;
            float2 texelSize = 1.0 / _ScreenSize.xy;
            float4 color = 0;

            for (int i = 0; i < 5; i++)
            {
                float2 offset = direction * offsets[i] * _Intensity * texelSize;
                color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset) * weights[i];
            }
            return color;
        }
        ENDHLSL

        Pass
        {
            Name "HorizontalBlur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            float4 frag(Varyings i) : SV_Target { return Blur(i, float2(1, 0)); }
            ENDHLSL
        }

        Pass
        {
            Name "VerticalBlur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            float4 frag(Varyings i) : SV_Target { return Blur(i, float2(0, 1)); }
            ENDHLSL
        }
    }
}