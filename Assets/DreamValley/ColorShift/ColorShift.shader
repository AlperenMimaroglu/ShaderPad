Shader "Dream Valley/Color Shift"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        _ColorA("Color A", Color) = (1,1,1,1)
        _ColorB("Color B", Color) = (1,1,1,1)
    }

    SubShader
    {
        Name "Color Shift"
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
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // To make the Unity shader SRP Batcher compatible, declare all
            // properties related to a Material in a a single CBUFFER block with 
            // the name UnityPerMaterial.
            CBUFFER_START(UnityPerMaterial)
            // The following line declares the _BaseColor variable, so that you
            // can use it in the fragment shader.

            float4 _BaseMap_ST;
            half4 _ColorA;
            half4 _ColorB;

            #define ROTATION_PER_EDGE 90f;

            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float x = frac(IN.uv.x + _Time.y);
                x = abs(x * 2. - 1);
                float4 col = lerp(_ColorB, _ColorA, x);

                return col;
            }
            ENDHLSL
        }
    }
}