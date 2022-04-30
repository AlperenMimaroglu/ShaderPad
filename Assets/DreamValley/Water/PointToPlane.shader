Shader "Dream Valley/Point To Plane"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _BaseMap("Base Map", 2D) = "white"
        _Normal("Normal",Vector) = (0,0,0,1)
        _Height("Height",float) = 0
        _PlanePos("PlanePos",Vector) = (0, 0, 0, 0)
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
                float3 positionWORLD : TEXCOORD1;
            };

            // This macro declares _BaseMap as a Texture2D object.
            TEXTURE2D(_BaseMap);
            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
            // The following line declares the _BaseMap_ST variable, so that you
            // can use the _BaseMap variable in the fragment shader. The _ST 
            // suffix is necessary for the tiling and offset function to work.
            float4 _BaseMap_ST;
            float3 _Normal;
            float _Height;
            float4 _PlanePos;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.positionWORLD = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                float x = dot(IN.positionWORLD - _PlanePos.xyz, _Normal);

                half4 col = 1;
                if (x >= 0.0)
                {
                    col = half4(1.0, 0.0, 0.0, 1);
                }
                else
                {
                    col = half4(0.0, 1.0, 0.0, 1);
                }

                return col;
            }
            ENDHLSL
        }
    }
}