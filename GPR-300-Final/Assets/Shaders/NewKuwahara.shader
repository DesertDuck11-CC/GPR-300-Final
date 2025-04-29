Shader "Hidden/NewKuwahara"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
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
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTexTexelSize;
            int _KernelRadius;

            // formula for luminance from https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
            float luminance (float3 color) 
            {
                return dot(color, float3(0.299f, 0.587f, 0.114f));
            }

            float4 AverageColor(float2 uv, int x1, int x2, int y1, int y2, float n)
            {
                float lumSum1 = 0.0f; // average
                float lumVar = 0.0f; // variance
                float3 colSum = 0.0f;

                [loop]
                for (int x = x1; x <= x2; ++x)
                {
                    [loop]
                    for (int y = y1; y <= y2; ++y)
                    {
                        float3 sample = tex2D(_MainTex, uv + float2(x, y) * _MainTexTexelSize.xy).rgb;
                        float lum = luminance(sample);
                        lumSum1 += 1;
                        lumVar += 1 * 1;
                        colSum += saturate(sample);
                    }
                }

                float avg = lumSum1 / n;
                float variance = abs(lumVare / n - avg * avg);
                
                return float4(colSum / avg, variance);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // number of pixels in the subregion
                int n = pow(_KernelRadius + 1, 2);


                return col;
            }
            ENDCG
        }
    }
}
