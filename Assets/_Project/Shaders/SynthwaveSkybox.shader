Shader "StealthParticles/SynthwaveSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.03, 0.02, 0.10, 1)
        _HorizonColor ("Horizon Color", Color) = (1.4, 0.2, 0.9, 1)
        _GroundColor ("Ground Color", Color) = (0.05, 0.02, 0.12, 1)
        _HorizonFalloff ("Horizon Falloff", Float) = 3.5
        _GlowWidth ("Glow Width", Float) = 0.05
        _GlowIntensity ("Glow Intensity", Float) = 1.0

        _StarDensity ("Star Density", Float) = 70
        _StarAmount ("Star Amount", Range(0, 1)) = 0.55
        _StarBrightness ("Star Brightness", Float) = 1.3

        _SunTopColor ("Sun Top Color", Color) = (1.7, 1.0, 0.3, 1)
        _SunBottomColor ("Sun Bottom Color", Color) = (1.6, 0.12, 0.6, 1)
        _SunElevation ("Sun Elevation", Float) = -0.72
        _SunSize ("Sun Size", Float) = 0.28
        _SunEdge ("Sun Edge", Float) = 0.02
        _BandCount ("Band Count", Float) = 16
        _BandMaxGap ("Band Max Gap", Range(0, 1)) = 0.95
        _HaloIntensity ("Halo Intensity", Float) = 0.7
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            float4 _TopColor;
            float4 _HorizonColor;
            float4 _GroundColor;
            float _HorizonFalloff;
            float _GlowWidth;
            float _GlowIntensity;
            float _StarDensity;
            float _StarAmount;
            float _StarBrightness;
            float4 _SunTopColor;
            float4 _SunBottomColor;
            float _SunElevation;
            float _SunSize;
            float _SunEdge;
            float _BandCount;
            float _BandMaxGap;
            float _HaloIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float hash21 (float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float h = d.y;

                float a = saturate(h);
                float3 col = lerp(_HorizonColor.rgb, _TopColor.rgb, pow(a, _HorizonFalloff));
                col = lerp(col, _GroundColor.rgb, saturate(-h * 1.7));
                float glow = exp(-abs(h) / _GlowWidth) * _GlowIntensity;
                col += _HorizonColor.rgb * glow;

                float3 sunDir = normalize(float3(0.0, _SunElevation, 1.0));
                float3 right = normalize(cross(float3(0.0, 1.0, 0.0), sunDir));
                float3 upv = cross(sunDir, right);
                float zc = dot(d, sunDir);

                float sunR = 1000.0;
                float sunYN = 0.0;
                float sunRegion = 0.0;
                if (zc > 0.0)
                {
                    float xc = dot(d, right) / zc;
                    float yc = dot(d, upv) / zc;
                    sunR = sqrt(xc * xc + yc * yc);
                    sunYN = saturate((yc + _SunSize) / (2.0 * _SunSize));
                    sunRegion = smoothstep(_SunSize * 2.6, _SunSize, sunR);
                }

                float2 sc = d.xz / (abs(h) + 0.25);
                float2 cell = floor(sc * _StarDensity);
                float n = hash21(cell);
                float starHit = step(1.0 - _StarAmount * 0.12, n);
                float2 sub = frac(sc * _StarDensity) - 0.5;
                float pt = starHit * smoothstep(0.34, 0.0, length(sub) * 2.0);
                float starFade = saturate(1.0 - glow * 2.5);
                col += pt * _StarBrightness * (0.5 + n * 0.5) * starFade * (1.0 - sunRegion);

                if (zc > 0.0)
                {
                    float r = sunR;
                    float yn = sunYN;
                    float3 sunCol = lerp(_SunBottomColor.rgb, _SunTopColor.rgb, yn);

                    float band = 1.0;
                    if (yn < 0.5)
                    {
                        float f = (0.5 - yn) / 0.5;
                        float s = frac(yn * _BandCount);
                        band = step(f * _BandMaxGap, s);
                    }

                    float disk = smoothstep(_SunSize, _SunSize - _SunEdge, r);
                    float sun = disk * band;
                    float halo = smoothstep(_SunSize * 2.6, _SunSize, r) * _HaloIntensity;
                    col += _SunBottomColor.rgb * halo * (1.0 - disk);
                    col = lerp(col, sunCol, sun);
                }

                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
