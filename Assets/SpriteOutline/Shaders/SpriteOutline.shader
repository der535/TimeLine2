Shader "Sprites/Outline_GradientAlpha"
{
    Properties
    {
       [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
       _Color ("Main texture Tint", Color) = (1,1,1,1)

       [Header(General Settings)]
       [MaterialToggle] _OutlineEnabled ("Outline Enabled", Float) = 1
       _Thickness ("Glow Width", float) = 10
       _Falloff ("Glow Falloff", Range(0.1, 2)) = 1.0
       
       [Header(Outline Color)]
       _SolidOutline ("Outline Color", Color) = (1,1,1,1)
    }

    SubShader
    {
       Tags
       { 
          "Queue"="Transparent" 
          "IgnoreProjector"="True" 
          "RenderType"="Transparent" 
          "PreviewType"="Plane"
          "CanUseSpriteAtlas"="True"
       }

       Cull Off Lighting Off ZWrite Off
       Blend One OneMinusSrcAlpha

       Pass
       {
       CGPROGRAM
          #pragma vertex vert
          #pragma fragment frag
          #include "UnityCG.cginc"
          
          struct appdata_t {
             float4 vertex   : POSITION;
             float4 color    : COLOR;
             float2 texcoord : TEXCOORD0;
          };

          struct v2f {
             float4 vertex   : SV_POSITION;
             fixed4 color    : COLOR;
             float2 texcoord  : TEXCOORD0;
          };

          fixed4 _Color;
          fixed _Thickness;
          fixed _OutlineEnabled;
          fixed4 _SolidOutline;
          float _Falloff;

          sampler2D _MainTex;
          float4 _MainTex_TexelSize;

          v2f vert(appdata_t IN) {
             v2f OUT;
             OUT.vertex = UnityObjectToClipPos(IN.vertex);
             OUT.texcoord = IN.texcoord;
             OUT.color = IN.color * _Color;
             return OUT;
          }

          // Функция вычисляет максимальную альфу в радиусе, создавая мягкий край
          float GetMaxAlpha(float2 uv) {
             float2 texelSize = _MainTex_TexelSize.xy;
             float maxAlpha = 0;
             int iterations = 8; // Количество выборок для сглаживания
             
             // Проверяем соседние пиксели по кругу
             for(int i = 0; i < iterations; i++) {
                float angle = i * (6.28318 / iterations);
                float2 offset = float2(cos(angle), sin(angle)) * _Thickness * texelSize;
                float sampleAlpha = tex2D(_MainTex, uv + offset).a;
                maxAlpha = max(maxAlpha, sampleAlpha);
             }
             
             // Дополнительная выборка чуть ближе для плотности
             for(int j = 0; j < iterations; j++) {
                float angle = j * (6.28318 / iterations);
                float2 offset = float2(cos(angle), sin(angle)) * (_Thickness * 0.5) * texelSize;
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + offset).a);
             }

             return maxAlpha;
          }

          fixed4 frag(v2f IN) : SV_Target {
             // 1. Получаем основной цвет спрайта
             fixed4 mainCol = tex2D(_MainTex, IN.texcoord) * IN.color;
             
             if(_OutlineEnabled <= 0) return mainCol;

             // 2. Вычисляем "силу" обводки (дистанцию до спрайта)
             // Ищем максимальную альфу вокруг текущего пустого пикселя
             float alphaDist = GetMaxAlpha(IN.texcoord);
             
             // Применяем спад (falloff), чтобы градиент был мягче
             float outlineFactor = pow(alphaDist, _Falloff);

             // 3. Формируем цвет обводки
             fixed4 outlineCol = _SolidOutline;
             outlineCol.a *= outlineFactor;
             outlineCol.rgb *= outlineCol.a; // Premultiplied alpha

             // 4. Смешивание: если есть основной спрайт, рисуем его. 
             // Если нет — рисуем градиентную обводку.
             fixed4 finalCol = mainCol;
             
             // Мягкое наложение обводки под спрайт
             finalCol.rgb = mainCol.rgb + outlineCol.rgb * (1.0 - mainCol.a);
             finalCol.a = max(mainCol.a, outlineCol.a);

             return finalCol;
          }
       ENDCG
       }
    }
}