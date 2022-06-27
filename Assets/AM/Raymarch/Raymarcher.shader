Shader "Unlit/Raymarcher"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _Blend ("Blend Amount", Range(0.1, 2)) = 0.1
        _Cube ("CubePos", Vector) = (0,0,0,1)
        _Sphere ("SpherePos", Vector) = (0,0,0,1)
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

            CBUFFER_START(UnityPerMaterial)
            float4 _Cube;
            float4 _Sphere;
            float _Blend;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.ro = _WorldSpaceCameraPos;
                OUT.hitPos = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            float sdBox(float3 p, float3 b)
            {
                float3 q = abs(p) - b;
                return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
            }

            float sdSphere(float3 p, float s)
            {
                return length(p) - s;
            }

            float smin(float a, float b, float k)
            {
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * k * (1.0 / 4.0);
            }

            float SignedDistanceToScene(float3 p)
            {
                float distToSphere = sdSphere(p - _Sphere, 1);

                float distToBox = sdBox(p - _Cube, float3(1, 1, 1));

                float d = length(float2(length(p.xz) - .5, p.y)) - .1;
                // float3 q = float3(p.x, max(abs(p.y) - .2, 0.0), p.z);
                // return length(float2(length(q.xy) - .2, q.z)) - .1;

                return smin(distToBox, distToSphere, _Blend);
            }

            float Raymarch(float3 ro, float3 rd)
            {
                float dO = 0;
                float dS;

                for (int i = 0; i < MAX_STEPS; ++i)
                {
                    float3 p = ro + dO * rd;
                    dS = SignedDistanceToScene(p);
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

                return normalize(SignedDistanceToScene(p) - float3(
                    SignedDistanceToScene(p - e.xyy),
                    SignedDistanceToScene(p - e.yxy),
                    SignedDistanceToScene(p - e.yyx)
                ));
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

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

                return color;
            }
            ENDHLSL
        }
    }
}