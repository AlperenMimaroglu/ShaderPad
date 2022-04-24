Shader "Dream Valley/Shape"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Center ("Center" ,Range(0,1)) = 0.5
        _Radius ("Radius", Range(0,1)) = 0.5
        _Smooth ("Smooth", Range (0.0,0.5)) = 0.1
        [IntRange]_Edge ("EdgeCount" , Range(3,12)) = 3
    }

    SubShader
    {
        Name "ShapeDrawer"
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
            half4 _BaseColor;
            float _Radius;
            float _Center;
            float _Smooth;
            float _Edge;

            #define ROTATION_PER_EDGE 90f;

            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 draw_shape(float2 uv, float edgeCount)
            {
                // float points[edgeCount];
                // for (int i = 0; i < edgeCount; ++i)
                // {
                //     // points
                // }
                half3 sstep = 0;
                sstep = smoothstep((uv.y - _Smooth), (uv.y + _Smooth), _Radius);

                return float4(sstep, 1);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                // uv.x = saturate(uv.x-) 
                float c = length((IN.uv) - _Center);
                float t = length((sqrt(3) / 4) * pow(_Radius, 2));
                return smoothstep(c - _Smooth, c + _Smooth, t);
            }
            ENDHLSL
        }
    }
}