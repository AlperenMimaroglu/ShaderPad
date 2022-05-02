Shader "AM/Point To Plane"
{
    Properties
    {
        _Smooth("Smooth", Range(0.0, 0.5)) = 0.1
        _BaseMap("Base Map", 2D) = "white"
        _BumpMap("Normal Map", 2D) = "bump"
        _BlendMap("Blend Map", 2D) = "white"
        _BlendBump("Blend Normal Map", 2D) = "white"
        [HideInInspector]_PlaneNormal("Normal",Vector) = (0,0,0,1)
        [HideInInspector] _PlanePos("PlanePos",Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass
        {
            Name "PointToPlane"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv_blend : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv_blend : TEXCOORD1;
                float3 positionWORLD : NORMAL;
            };

            TEXTURE2D(_BaseMap);
            TEXTURE2D(_BlendMap);
            SAMPLER(sampler_BaseMap);
            SAMPLER(sampler_BlendMap);

            CBUFFER_START(UnityPerMaterial)

            float4 _BaseMap_ST;
            float4 _BlendMap_ST;

            float3 _PlaneNormal;
            float4 _PlanePos;
            float _PlaneHeight;
            float _Smooth;

            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.uv_blend = TRANSFORM_TEX(IN.uv_blend, _BlendMap);
                OUT.positionWORLD = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 blendMap = SAMPLE_TEXTURE2D(_BlendMap, sampler_BlendMap, IN.uv);

                float dist = dot(IN.positionWORLD - _PlanePos, _PlaneNormal);

                half4 colorA = half4(1.0, 0.0, 0.0, 1);
                half4 colorB = half4(0.0, 1.0, 0.0, 1);
                float blend = smoothstep(-.5 - _Smooth, .5 + _Smooth, dist);
                half3 col = lerp(blendMap.rgb, baseMap.rgb, blend);

                half4 color = lerp(colorB, colorA, blend);

                return half4(col.rgb, 1);
            }
            ENDHLSL
        }
    }
}