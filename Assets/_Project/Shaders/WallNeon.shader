Shader "StealthParticles/WallNeon"
{
    Properties
    {
        [HDR] _FillColor ("Fill Color", Color) = (0.1, 0.04, 0.16, 1)
        [HDR] _BorderColor ("Border Color", Color) = (5.657, 0.0, 4.437, 1)
        _BorderWidth ("Border Width (world units)", Float) = 0.04
        _BorderSoftness ("Border Softness", Range(0.05, 1.0)) = 0.5
        _ShadowStrength ("Bottom Shadow Strength", Range(0.0, 1.0)) = 0.75
        _ShadowHeight ("Bottom Shadow Height", Range(0.01, 1.0)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "WallNeonUnlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _FillColor;
                float4 _BorderColor;
                float _BorderWidth;
                float _BorderSoftness;
                float _ShadowStrength;
                float _ShadowHeight;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 op = IN.positionOS;
                float3 scl = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22));
                float3 dist = (0.5 - abs(op)) * scl;
                float mn = min(dist.x, min(dist.y, dist.z));
                float mx = max(dist.x, max(dist.y, dist.z));
                float mid = dist.x + dist.y + dist.z - mn - mx;
                float inner = _BorderWidth * (1.0 - _BorderSoftness);
                float border = 1.0 - smoothstep(inner, _BorderWidth, mid);
                float keepBottom = (op.y >= 0.0 || dist.y >= mx) ? 1.0 : 0.0;
                border *= keepBottom;
                float t = saturate((op.y + 0.5) / _ShadowHeight);
                float shade = lerp(1.0 - _ShadowStrength, 1.0, t);
                float3 col = lerp(_FillColor.rgb * shade, _BorderColor.rgb, border);
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
