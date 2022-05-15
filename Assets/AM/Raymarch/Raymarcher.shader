Shader "Unlit/Raymarcher"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #define MAX_STEPS 100
            #define MAX_DIST 100
            #define SURF_DIST 1e-3

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.ro = TransformWorldToObject(_WorldSpaceCameraPos);
                OUT.hitPos = IN.positionOS;
                return OUT;
            }

            float GetDist(float3 p)
            {
                float d = length(p) - .5;

                d = length(float2(length(p.xz) - .5, p.y)) - .1;
                return d;
            }

            float Raymarch(float3 ro, float3 rd)
            {
                float dO = 0;
                float dS;


                for (int i = 0; i < MAX_STEPS; ++i)
                {
                    float3 p = ro + dO * rd;
                    dS = GetDist(p);
                    dO += dS;

                    if (dS < SURF_DIST || dO > MAX_DIST)
                    {
                        break;
                    }
                }

                return dO;
            }

            float3 GetNormal(float3 p)
            {
                float2 e = float2(1e-3, 0);
                float3 n = GetDist(p) - float3(
                    GetDist(p - e.xyy),
                    GetDist(p - e.yxy),
                    GetDist(p - e.yyx)
                );

                return normalize(n);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv - .5;

                float3 ro = IN.ro;;
                float3 rd = normalize(IN.hitPos - ro);

                float d = Raymarch(ro, rd);

                half4 color = 0;

                if (d >= MAX_DIST)
                {
                    discard;
                }

                half3 p = ro + rd * d;
                half3 n = GetNormal(p);
                n = saturate(n);
                color.rgb = n;

                // color.rgb = rd;
                return color;
            }
            ENDHLSL
        }
    }
}