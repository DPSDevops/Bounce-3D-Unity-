Shader "Custom/CheckpointInactive"
{
    Properties
    {
        _Color1 ("Stripe Color 1", Color) = (1, 0, 0, 1)
        _Color2 ("Stripe Color 2", Color) = (1, 1, 1, 1)
        _StripeFrequency ("Stripe Frequency", Range(1, 50)) = 10
        _StripeAngle ("Stripe Angle", Range(0, 360)) = 45
        _ScrollSpeed ("Scroll Speed", Range(-5, 5)) = -1.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
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
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color1;
                half4 _Color2;
                half _StripeFrequency;
                half _StripeAngle;
                half _ScrollSpeed;
                half _Smoothness;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = positionInputs.positionCS;
                output.normalWS = normalInputs.normalWS;
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Get main light
                Light mainLight = GetMainLight();
                
                // Rotate UV coordinates for diagonal stripes
                half angleRad = radians(_StripeAngle);
                half2 rotatedUV;
                rotatedUV.x = input.uv.x * cos(angleRad) - input.uv.y * sin(angleRad);
                rotatedUV.y = input.uv.x * sin(angleRad) + input.uv.y * cos(angleRad);
                
                // Animate stripes (scroll right to left)
                half scrollOffset = _Time.y * _ScrollSpeed;
                half stripePattern = frac((rotatedUV.x + scrollOffset) * _StripeFrequency);
                
                // Smooth step for crisp but anti-aliased stripes
                half mask = smoothstep(0.45, 0.55, stripePattern);
                
                // Blend between two colors
                half3 stripeColor = lerp(_Color1.rgb, _Color2.rgb, mask);
                
                // Calculate lighting
                half3 normal = normalize(input.normalWS);
                half NdotL = saturate(dot(normal, mainLight.direction));
                half3 lighting = mainLight.color * (NdotL * 0.7 + 0.3);
                half3 ambient = half3(0.2, 0.2, 0.2);
                
                // Apply lighting to stripe pattern
                half3 finalColor = stripeColor * (lighting + ambient);
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}

