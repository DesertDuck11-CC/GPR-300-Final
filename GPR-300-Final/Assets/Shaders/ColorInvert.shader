Shader "Unlit/ColorInvert"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _CameraOpaqueTexture;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = o.pos;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Convert screen position to UV
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                screenUV = screenUV * 0.5 + 0.5;
                screenUV.y = 1.0f - screenUV.y;

                // Sample the scene color
                fixed3 sceneColor = tex2D(_CameraOpaqueTexture, screenUV).rgb;

                // Subtract white (1,1,1)
                fixed3 result = 1.0f - sceneColor;

                // Output the color with full alpha
                return fixed4(result, 1.0);
            }
            ENDCG
        }
    }
}