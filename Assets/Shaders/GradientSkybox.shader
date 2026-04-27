Shader "Custom/GradientSkybox"
{
    Properties
    {
        _TopColor    ("Top Color",    Color) = (0.02, 0.02, 0.05, 1)
        _BottomColor ("Bottom Color", Color) = (0.0,  0.0,  0.0,  1)
        _Exponent    ("Gradient Exponent", Float) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _BottomColor;
                float  _Exponent;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dir        : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.dir        = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = saturate(pow(saturate(normalize(IN.dir).y * 0.5 + 0.5), _Exponent));
                return lerp(_BottomColor, _TopColor, t);
            }
            ENDHLSL
        }
    }
}
