Shader "ColorBlit"
{
        SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ColorBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            int _KernelRadius;

            // luminance calculation from https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
            float luminance(float3 color) {
                return dot(color, float3(0.299f, 0.587f, 0.114f));
            }

            // Returns average color and variance
            float4 SampleQuadrant(float2 uv, int x1, int x2, int y1, int y2, float n) {
                float lumSum = 0.0f;
                float lumVar = 0.0f;
                float3 colSum = 0.0f;

                [loop]
                for (int x = x1; x <= x2; ++x) {
                    [loop]
                    for (int y = y1; y <= y2; ++y) {
                        float3 sample = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv).rgb;
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

            float4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                float windowSize = 2.0f * _KernelRadius + 1;
                int quadrantSize = int(ceil(windowSize / 2.0f));
                int numSamples = quadrantSize * quadrantSize;

                float4 q1 = SampleQuadrant(i.texcoord, -_KernelRadius, 0, -_KernelRadius, 0, numSamples);
                float4 q2 = SampleQuadrant(i.texcoord, 0, _KernelRadius, -_KernelRadius, 0, numSamples);
                float4 q3 = SampleQuadrant(i.texcoord, 0, _KernelRadius, 0, _KernelRadius, numSamples);
                float4 q4 = SampleQuadrant(i.texcoord, -_KernelRadius, 0, 0, _KernelRadius, numSamples);

                float minVariance = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minVariance;
    
                if (dot(q, 1) > 1)
                    return saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                else
                    return saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));
            }
            ENDHLSL
        }
    }
}