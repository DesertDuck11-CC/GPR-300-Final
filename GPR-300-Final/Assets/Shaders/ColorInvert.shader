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
            int _KernelRadius, _MinKernelSize, _AnimateSize, _AnimateOrigin;
            float _SizeAnimationSpeed, _NoiseFrequency;

            // luminance calculation from https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
            float luminance(float3 color) {
                return dot(color, float3(0.299f, 0.587f, 0.114f));
            }

            float hash(uint n) {
                // integer hash copied from Hugo Elias
                n = (n << 13U) ^ n;
                n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
                return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
            }

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

            float4 SampleQuadrant(float2 uv, int x1, int x2, int y1, int y2, float n) {
                float lumSum = 0.0f;
                float lumVar = 0.0f;
                float3 colSum = 0.0f;
                float2 _MainTex_TexelSize = _ScreenParams.zw;

                [loop]
                for (int x = x1; x <= x2; ++x) {
                    [loop]
                    for (int y = y1; y <= y2; ++y) {
                        float3 sample = tex2D(_CameraOpaqueTexture, uv + float2(x, y) * _MainTex_TexelSize).rgb;
                        float lum = luminance(sample);
                        lumSum += lum;
                        lumVar += lum * lum;
                        colSum += saturate(sample);
                    }
                }

                float mean = lumSum / n;
                float std = abs(lumVar / n - mean * mean);

                return float4(colSum / n, std);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                screenUV = screenUV * 0.5 + 0.5;
                screenUV.y = 1.0f - screenUV.y;
                float windowSize = 2.0f * _KernelRadius + 1;
                int quadrantSize = int(ceil(windowSize / 2.0f));
                int numSamples = quadrantSize * quadrantSize;


                float4 q1 = SampleQuadrant(screenUV, -_KernelRadius, 0, -_KernelRadius, 0, numSamples);
                float4 q2 = SampleQuadrant(screenUV, 0, _KernelRadius, -_KernelRadius, 0, numSamples);
                float4 q3 = SampleQuadrant(screenUV, 0, _KernelRadius, 0, _KernelRadius, numSamples);
                float4 q4 = SampleQuadrant(screenUV, -_KernelRadius, 0, 0, _KernelRadius, numSamples);

                float minstd = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minstd;
    
                if (dot(q, 1) > 1)
                    return saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                else
                    return saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));
                // Convert screen position to UV
                //float2 screenUV = i.screenPos.xy / i.screenPos.w;
                

                // Sample the scene color
                fixed3 sceneColor = tex2D(_CameraOpaqueTexture, screenUV).rgb;

                // Subtract white (1,1,1)
                fixed3 result = 1.0f - sceneColor;

                // Output the color with full alpha
                return fixed4(result, 1.0);
            }

            float4 fp (v2f i) : SV_Target {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float windowSize = 2.0f * _KernelRadius + 1;
                    int quadrantSize = int(ceil(windowSize / 2.0f));
                    int numSamples = quadrantSize * quadrantSize;


                    float4 q1 = SampleQuadrant(screenUV, -_KernelRadius, 0, -_KernelRadius, 0, numSamples);
                    float4 q2 = SampleQuadrant(screenUV, 0, _KernelRadius, -_KernelRadius, 0, numSamples);
                    float4 q3 = SampleQuadrant(screenUV, 0, _KernelRadius, 0, _KernelRadius, numSamples);
                    float4 q4 = SampleQuadrant(screenUV, -_KernelRadius, 0, 0, _KernelRadius, numSamples);

                    float minstd = min(q1.a, min(q2.a, min(q3.a, q4.a)));
                    int4 q = float4(q1.a, q2.a, q3.a, q4.a) == minstd;
    
                    if (dot(q, 1) > 1)
                        return saturate(float4((q1.rgb + q2.rgb + q3.rgb + q4.rgb) / 4.0f, 1.0f));
                    else
                        return saturate(float4(q1.rgb * q.x + q2.rgb * q.y + q3.rgb * q.z + q4.rgb * q.w, 1.0f));
            }
            ENDCG
        }
    }
}