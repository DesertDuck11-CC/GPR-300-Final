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
            int _KernelRadius, _MinKernelRadius, _AnimateSize, _AnimateOrigin;
            float _SizeAnimationSpeed, _NoiseFrequency;

            // formula for luminance from https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
            float luminance (float3 color) 
            {
                return dot(color, float3(0.299f, 0.587f, 0.114f));
            }

            // returns the average color with a being the variance
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
                float variance = abs(lumVar / n - avg * avg);
                
                return float4(colSum / avg, variance);
            }

            // used to determine the region with the lowest variance
            float hash(uint n) 
            {
                // integer hash copied from Hugo Elias
                n = (n << 13U) ^ n;
                n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
                return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
            }

            float4 frag (v2f i) : SV_Target
            {
                if (_AnimateSize)
                {
                    // idk why this is in two parts, will try without
                    uint seed = i.uv.x + _MainTexTexelSize.z * i.uv.y + _MainTexTexelSize.z * _MainTexTexelSize.w;
                    seed = i.uv.y + _MainTexTexelSize.z * _MainTexTexelSize.w;
                    float kernelRange = (sin(_Time.y * _SizeAnimationSpeed + hash(seed) * _NoiseFrequency) * 0.5f + 0.5f) * _KernelRadius + _MinKernelRadius;
                    int minKSize = floor(kernelRange);
                    int maxKSize = ceil(kernelRange);
                    float t = frac(kernelRange);

                    float windowSize = 2.0f * minKSize + 1;
                    int quadrantSize = int(ceil(windowSize / 2.0f));
                    int numSamples = quadrantSize * quadrantSize;

                    // sample the average color from each surrounding quadrant (counterclockwise from bottom left)
                    float4 q1 = AverageColor(i.uv, -minKSize, 0, -minKSize, 0, numSamples);
                    float4 q2 = AverageColor(i.uv, 0, minKSize, -minKSize, 0, numSamples);
                    float4 q3 = AverageColor(i.uv, 0, minKSize, 0, minKSize, numSamples);
                    float4 q4 = AverageColor(i.uv, -minKSize, 0, 0, minKSize, numSamples);

                    // pick the quadrant with the least variance
                    float minVar = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                    int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minVar;

                    float4 result1 = 0;
                    if (dot(q, 1) > 1)
                    {
                        result1 = saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                    }
                    else
                    {
                        result1 = saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));
                    }

                    windowSize = 2.0f * maxKSize + 1;
                    quadrantSize = int(ceil(windowSize / 2.0f));
                    numSamples = quadrantSize * quadrantSize;

                    // sample the average color from each surrounding quadrant (counterclockwise from bottom left)
                    q1 = AverageColor(i.uv, -maxKSize, 0, -maxKSize, 0, numSamples);
                    q2 = AverageColor(i.uv, 0, maxKSize, -maxKSize, 0, numSamples);
                    q3 = AverageColor(i.uv, 0, maxKSize, 0, maxKSize, numSamples);
                    q4 = AverageColor(i.uv, -maxKSize, 0, 0, maxKSize, numSamples);

                    minVar = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                    q = float4(q1.a, q2.a, q3.a, q4.a) == minVar;

                    float4 result2 = 0;
                    if (dot(q, 1) > 1)
                    {
                        result2 = saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                    }
                    else
                    {
                        result2 = saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));
                    }

                    return lerp(result1, result2, t);
                }
                else
                {
                    float windowSize = 2.0f * _KernelRadius + 1;
                    int quadrantSize = int(ceil(windowSize / 2.0f));
                    int numSamples = quadrantSize * quadrantSize;

                    float4 q1 = AverageColor(i.uv, -_KernelRadius, 0, -_KernelRadius, 0, numSamples);
                    float4 q2 = AverageColor(i.uv, 0, _KernelRadius, -_KernelRadius, 0, numSamples);
                    float4 q3 = AverageColor(i.uv, 0, _KernelRadius, 0, _KernelRadius, numSamples);
                    float4 q4 = AverageColor(i.uv, -_KernelRadius, 0, 0, _KernelRadius, numSamples);

                    float minVar = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                    int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minVar;

                    if (dot(q, 1) > 1)
                    {
                        return saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                    }
                    else
                    {
                        return saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));
                    }

                }
            }
            ENDCG
        }
    }
}
