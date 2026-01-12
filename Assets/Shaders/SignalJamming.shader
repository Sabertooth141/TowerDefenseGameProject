Shader "Custom/SignalJamming"
{
    Properties
    {
        _Intensity("Intensity", Range(0, 1)) = 1
        _NoiseStrength("Noise Strength", Range(0, 1)) = 0.5
        _ScanlineStrength("Scanline Strength",Range(0, 1)) = 0.5
        _Pixelation("Pixelation", Range(1, 300)) = 160
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "SignalJamPass"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;
            float _NoiseStrength;
            float _ScanlineStrength;
            float _Pixelation;

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                
                // Sample original - _BlitTexture is automatically set by RenderGraph
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                
                // If intensity is very low, just return original
                if (_Intensity <= 0.01)
                {
                    return col;
                }

                float t = _Time.y * 8.0;
                float2 distortedUV = uv;
                
                // 1. Pixelation
                if (_Pixelation > 1.0)
                {
                    float2 pixelUV = floor(uv * _Pixelation) / _Pixelation;
                    distortedUV = lerp(uv, pixelUV, _Intensity * 0.5);
                }

                // 2. Horizontal jitter
                float lineIndex = floor(distortedUV.y * 180.0);
                float jitterSeed = floor(t * 2.0);
                float jitter = (rand(float2(lineIndex, jitterSeed)) - 0.5) * 0.015 * _Intensity;
                distortedUV.x += jitter;

                // 3. Vertical wobble
                distortedUV.y += sin(t * 3.0 + distortedUV.x * 30.0) * 0.003 * _Intensity;
                
                // Clamp UV
                distortedUV = saturate(distortedUV);
                
                // 4. RGB chromatic aberration
                float shift = 0.003 * _Intensity;
                half r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, saturate(distortedUV + float2(shift, 0))).r;
                half g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, distortedUV).g;
                half b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, saturate(distortedUV - float2(shift, 0))).b;
                
                col = half4(r, g, b, 1.0);
                
                // 5. Static noise
                float noiseSeed = t * 10.0;
                float noise = (rand(uv * noiseSeed) - 0.5) * _NoiseStrength * _Intensity * 0.15;
                col.rgb += noise;
                
                // 6. Scanlines
                float scanline = sin(uv.y * 800.0 + t * 5.0) * 0.04;
                col.rgb *= (1.0 - scanline * _ScanlineStrength * _Intensity);
                
                // 7. Random horizontal bars
                float barNoise = rand(float2(floor(uv.y * 50.0), floor(t * 5.0)));
                if (barNoise > 0.97)
                {
                    col.rgb += (barNoise - 0.97) * 10.0 * _Intensity;
                }
                
                // 8. Signal spikes
                float spike = step(0.996, rand(float2(floor(t * 3.0), 0.5)));
                col.rgb += spike * _Intensity * 0.4;
                
                return saturate(col);
            }
            ENDHLSL
        }
    }
}