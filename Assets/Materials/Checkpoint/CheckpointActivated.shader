Shader "Custom/CheckpointActivated"
{
    Properties
    {
        _GlassColor ("Glass Tint", Color) = (0.7, 0.9, 1.0, 0.3)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95
        _RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.1
        _FresnelPower ("Fresnel Power", Range(0, 10)) = 3.0
        _RimColor ("Rim Color", Color) = (0.5, 0.8, 1.0, 1)
        _RimIntensity ("Rim Intensity", Range(0, 3)) = 1.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
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
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _GlassColor;
                half _Smoothness;
                half _RefractionStrength;
                half _FresnelPower;
                half4 _RimColor;
                half _RimIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = positionInputs.positionCS;
                output.normalWS = normalInputs.normalWS;
                output.positionWS = positionInputs.positionWS;
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Normalize vectors
                half3 normal = normalize(input.normalWS);
                half3 viewDir = normalize(input.viewDirWS);
                
                // Get main light
                Light mainLight = GetMainLight();
                
                // Fresnel effect (rim lighting)
                half fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _FresnelPower);
                
                // Rim lighting
                half3 rimLight = _RimColor.rgb * fresnel * _RimIntensity;
                
                // Specular highlight (glass reflection)
                half3 halfDir = normalize(mainLight.direction + viewDir);
                half spec = pow(saturate(dot(normal, halfDir)), 50.0 * _Smoothness);
                half3 specular = mainLight.color * spec * 0.8;
                
                // Simple refraction approximation (offset based on normal)
                half2 screenUV = input.screenPos.xy / input.screenPos.w;
                half2 refractionOffset = normal.xy * _RefractionStrength * 0.05;
                
                // Base glass color with lighting
                half NdotL = saturate(dot(normal, mainLight.direction));
                half3 lighting = mainLight.color * (NdotL * 0.3 + 0.7);
                
                // Combine effects
                half3 glassColor = _GlassColor.rgb * lighting;
                half3 finalColor = glassColor + rimLight + specular;
                
                // Glass transparency with fresnel-based alpha
                half alpha = _GlassColor.a + fresnel * 0.3;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}

