Shader "Tzar/VerticalFog"
{
    Properties
    {
       _Color("Main Color", Color) = (1, 1, 1, .5)
       _Color2("Secondary Color", Color) = (0.5, 0.5, 1, .3)
       _Intensity("Intensity", float) = 1
       _GradientPower("Gradient Power", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent"  }
  
        Pass
        {
           Blend SrcAlpha OneMinusSrcAlpha
           ZWrite Off
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma multi_compile_fog
           #include "UnityCG.cginc"
  
           struct appdata
           {
               float4 vertex : POSITION;
           };
  
           struct v2f
           {
               float4 scrPos : TEXCOORD0;
               UNITY_FOG_COORDS(1)
               float4 vertex : SV_POSITION;
           };

		 
  
           sampler2D _CameraDepthTexture;
           float4 _Color;
           float4 _Color2;
           float4 _IntersectionColor;
           float _Intensity;
           float _GradientPower;
  
           v2f vert(appdata v)
           {
               v2f o;
               o.vertex = UnityObjectToClipPos(v.vertex);
               o.scrPos = ComputeScreenPos(o.vertex);
               UNITY_TRANSFER_FOG(o,o.vertex);
               return o;   
           }
  
  
            half4 frag(v2f i) : SV_TARGET
            {
               float depth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));
               float diff = saturate(_Intensity * (depth - i.scrPos.w));
               
               // Создаем градиент на основе глубины
               float gradientFactor = pow(diff, _GradientPower);
               
               // Интерполируем между двумя цветами
               fixed4 gradientColor = lerp(_Color2, _Color, gradientFactor);
               
               // Применяем сглаживание для более плавного перехода
               float smoothFactor = diff * diff * diff * (diff * (6 * diff - 15) + 10);
               fixed4 col = lerp(fixed4(gradientColor.rgb, 0.0), gradientColor, smoothFactor);

               UNITY_APPLY_FOG(i.fogCoord, col);
               return col;
            }
  
            ENDCG
        }
    }
}
