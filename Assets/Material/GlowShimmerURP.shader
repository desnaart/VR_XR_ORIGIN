Shader "Custom/GlowShimmerURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.8, 0.95, 1, 1)
        [HDR]_GlowColor ("Glow Color", Color) = (0.0, 0.55, 3.0, 1)
        _GlowStrength ("Glow Strength", Range(0, 20)) = 4
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 2
        _ShimmerSpeed ("Shimmer Speed", Range(0, 10)) = 1
        _NoiseScale ("Noise Scale", Range(1, 80)) = 20
        _Smoothness ("Smoothness", Range(0,1)) = 0.4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _GlowColor;
                half _GlowStrength;
                half _FresnelPower;
                half _ShimmerSpeed;
                half _NoiseScale;
                half _Smoothness;
            CBUFFER_END

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                OUT.uv = IN.uv;
                OUT.fogCoord = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 normalWS = normalize(IN.normalWS);
                half3 viewDirWS = normalize(GetWorldSpaceViewDir(IN.positionWS));

                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 litColor = _BaseColor.rgb * (0.35 + ndotl * mainLight.color.rgb);

                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                float shimmerUV = noise(IN.uv * _NoiseScale + _Time.y * _ShimmerSpeed);
                half shimmer = lerp(0.65h, 1.35h, shimmerUV);
                half3 emission = _GlowColor.rgb * fresnel * _GlowStrength * shimmer;

                half3 finalColor = litColor + emission;
                finalColor = MixFog(finalColor, IN.fogCoord);
                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
