Shader "Unlit/NewKuwahara" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {

        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            #include "UnityCG.cginc"

            struct VertexData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vp(VertexData v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _KernelRadius;

            // luminance calculation from https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
            float luminance(float3 color) {
                return dot(color, float3(0.299f, 0.587f, 0.114f));
            }

            // Returns average color and variance
            float4 SectorValues(float2 uv, int x1, int x2, int y1, int y2, float n) {
                float lumSum = 0.0f;
                float lumVar = 0.0f;
                float3 colSum = 0.0f;

                [loop]
                for (int x = x1; x <= x2; ++x) {
                    [loop]
                    for (int y = y1; y <= y2; ++y) {
                        float3 sample = tex2D(_MainTex, uv + float2(x, y) * _MainTex_TexelSize.xy).rgb;
                        float lum = luminance(sample);
                        lumSum += lum;
                        lumVar += lum * lum;
                        colSum += saturate(sample);
                    }
                }

                float mean = lumSum / n;
                float variance = abs(lumVar / n - mean * mean);

                return float4(colSum / n, variance);
            }

            float4 fp (v2f i) : SV_Target {
                float windowSize = 2.0f * _KernelRadius + 1;
                int quadrantSize = int(ceil(windowSize / 2.0f));
                int numSamples = quadrantSize * quadrantSize;


                float4 q1 = SectorValues(i.uv, -_KernelRadius, 0, -_KernelRadius, 0, numSamples);
                float4 q2 = SectorValues(i.uv, 0, _KernelRadius, -_KernelRadius, 0, numSamples);
                float4 q3 = SectorValues(i.uv, 0, _KernelRadius, 0, _KernelRadius, numSamples);
                float4 q4 = SectorValues(i.uv, -_KernelRadius, 0, 0, _KernelRadius, numSamples);

                float minVariance = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                // pick the quadrant with the lowest variance
                int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minVariance;
    
                if (dot(q, 1) > 1)
                    return saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                else
                    return saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));
            }
            ENDCG
        }
    }
}