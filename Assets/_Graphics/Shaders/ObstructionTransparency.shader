// Shader Name: Custom/ObstructionTransparencyURP_SimpleLit
// Description: Makes objects transparent between the camera and a target position (_PlayerPosition).
//              Uses URP SimpleLit lighting model otherwise. Optimized for performance.
//              No Surrender 2025

Shader "Custom/ObstructionTransparencyURP_SimpleLit" {
    Properties {
        [Header(Appearance)]
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        
        [Header(Transparency Settings)]
        _TransparencyRadius ("Transparency Radius", Range(0.1, 10)) = 1.5
        _MinTransparency ("Minimum Transparency", Range(0.01, 1)) = 0.2 // How transparent it becomes inside the radius
        _TransparencySmoothness ("Transparency Edge Smoothness", Range(0.01, 1.0)) = 0.5 // Controls the softness of the hole edge
        _AbsoluteMinAlpha ("Absolute Minimum Alpha", Range(0.01, 0.2)) = 0.05 // Prevents complete disappearance
        
        [Header(Rendering Settings)]
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 0 // 0 = Off (render both sides), 1 = Front, 2 = Back
        [Toggle] _ZWrite ("Z Write", Float) = 1 // Write to depth buffer (helps with complex meshes)
        
        [HideInInspector] _Surface("__surface", Float) = 1.0 // Opaque = 0, Transparent = 1
        [HideInInspector] _Blend("__blend", Float) = 0.0 // Alpha = 0, Premultiply = 1, Additive = 2, Multiply = 3
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0 // Off = 0, On = 1
        [HideInInspector][ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0 // On = 1, Off = 0
    }

    SubShader {
        Tags {
            "RenderPipeline"="UniversalPipeline"  
            "RenderType"="Transparent"        
            "Queue"="Transparent"                
            "IgnoreProjector"="True"              
        }
        LOD 100
        
        Pass {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull [_CullMode]
            
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes {
                float4 positionOS : POSITION;
            };
            
            struct Varyings {
                float4 positionHCS : SV_POSITION;
            };
            
            Varyings DepthVert(Attributes input) {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 DepthFrag() : SV_TARGET {
                return 0;
            }
            ENDHLSL
        }
        
        Pass {
            Name "ForwardLit" 
            Tags { "LightMode"="UniversalForward" } 
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]                
            Cull [_CullMode]                

            HLSLPROGRAM
           
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _TransparencyRadius;
                half _MinTransparency;
                float _TransparencySmoothness;
                half _AbsoluteMinAlpha;
                float4 _PlayerPosition;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            
            struct Attributes {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                float4 tangentOS    : TANGENT;
            };

            struct Varyings {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                half3 normalWS      : TEXCOORD2;
            };
            
            Varyings vert(Attributes IN) {
                Varyings OUT;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                
                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }
            
            half4 frag(Varyings IN, bool isFrontFace : SV_IsFrontFace) : SV_Target {
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 baseColor = texColor * _BaseColor;
                half initialAlpha = baseColor.a;
                
                half3 normalWS = normalize(IN.normalWS);
                if (!isFrontFace) {
                    normalWS = -normalWS;
                }
                
                Light mainLight = GetMainLight();
                
                half3 ambient = SampleSH(normalWS);
                
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = NdotL * mainLight.color;
                
                half3 litColor = baseColor.rgb * (ambient + diffuse);
                
                float3 cameraPosWS = GetCameraPositionWS();
                float3 playerPosWS = _PlayerPosition.xyz;
                
                float3 cameraToPlayerVec = playerPosWS - cameraPosWS;
                float distanceCameraToPlayer = length(cameraToPlayerVec);
                float3 cameraToPlayerDir = SafeNormalize(cameraToPlayerVec);
                
                float3 cameraToFragVec = IN.positionWS - cameraPosWS;
                float distanceCameraToFrag = length(cameraToFragVec);
                float3 cameraToFragDir = SafeNormalize(cameraToFragVec);
                
                // Calculate if fragment is between camera and player
                float dotAlignment = dot(cameraToFragDir, cameraToPlayerDir);
                bool isCloserThanPlayer = distanceCameraToFrag < (distanceCameraToPlayer - 0.1);
                
                bool isInDirection = dotAlignment > 0.5;
                bool isBetween = isInDirection && isCloserThanPlayer;
                
                half finalAlpha = initialAlpha;
                
                if (isBetween) {
                    float projLength = dot(cameraToFragVec, cameraToPlayerDir);
                    float3 pointOnLine = cameraPosWS + cameraToPlayerDir * projLength;
                    float perpDistance = distance(IN.positionWS, pointOnLine);
                    
                    float edgeStart = _TransparencyRadius * (1.0 - _TransparencySmoothness);
                    float transparencyFactor = 1.0 - smoothstep(edgeStart, _TransparencyRadius, perpDistance);
                    
                    float targetAlpha = max(_MinTransparency * initialAlpha, _AbsoluteMinAlpha);
                    finalAlpha = lerp(initialAlpha, targetAlpha, transparencyFactor);
                }
                
                finalAlpha = max(finalAlpha, _AbsoluteMinAlpha);
                
                return half4(litColor, finalAlpha);
            }
            ENDHLSL
        }
        
        UsePass "Universal Render Pipeline/Simple Lit/ShadowCaster"
    }

    Fallback "Universal Render Pipeline/Simple Lit"
}