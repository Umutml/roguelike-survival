Shader "Custom/GrenadeEmissive"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StartColor ("Flash Start Color", Color) = (1,0.1,0.1,1)
        _EndColor ("Flash End Color", Color) = (0.5,0,0,1)
        _EmissionIntensity ("Emission Intensity", Range(1, 10)) = 5
        _FlashSpeed ("Flash Speed", Range(1, 20)) = 8
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _StartColor;
                half4 _EndColor;
                half _EmissionIntensity;
                half _FlashSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Smooth pulsing effect
                half flash = saturate(sin(_Time.y * _FlashSpeed) * 0.5 + 0.5);
                flash = smoothstep(0.2, 0.8, flash);
                
                // Lerp between start and end colors
                half3 flashColor = lerp(_EndColor.rgb, _StartColor.rgb, flash);
                half3 emissionColor = flashColor * _EmissionIntensity;
                
                half3 finalColor = baseColor.rgb + emissionColor;
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, baseColor.a);
            }
            ENDHLSL
        }
    }
}
