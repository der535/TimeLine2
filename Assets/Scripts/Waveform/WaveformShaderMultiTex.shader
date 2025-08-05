Shader "Custom/WaveformShaderMultiTex"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _DataTex ("Data Texture", 2D) = "white" {}
        [HDR] _Color ("Wave Color", Color) = (1,1,1,1) // Добавлен HDR атрибут
        _AmpScale ("Amplitude Scale", Float) = 1.0
        _Background ("Background", Color) = (0,0,0,0)
        [Toggle] _Antialias("Antialias", Float) = 1
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _DataTex;
            float4 _DataTex_ST;
            fixed4 _Color; // Используем объявленный в Properties цвет
            fixed4 _Background;
            float _AmpScale;
            float _Antialias;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _DataTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float sample = tex2D(_DataTex, float2(i.uv.x, 0.5)).r;
                float amp = sample * _AmpScale;
                float dist = abs(i.uv.y * 2 - 1) - amp;
                
                float wave = 1.0;
                if(_Antialias > 0.5)
                    wave = 1.0 - smoothstep(0, 0.015 * _AmpScale, dist);
                else
                    wave = dist < 0;
                
                // Используем _Color вместо фиксированного цвета
                return lerp(_Background, _Color, wave);
            }
            ENDCG
        }
    }
}