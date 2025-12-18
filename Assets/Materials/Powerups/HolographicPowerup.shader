Shader "Custom/HolographicPowerup"
{
    Properties
    {
        _Color ("Hologram Color", Color) = (0.2, 0.8, 1.0, 0.7)
        _RimColor ("Rim Color", Color) = (0.5, 1.0, 1.0, 1)
        _RimPower ("Rim Power", Range(0.1, 8.0)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 2.0
        
        _ScanlineColor ("Scanline Color", Color) = (0.3, 0.9, 1.0, 1)
        _ScanlineSpeed ("Scanline Speed", Range(-5, 5)) = 2.0
        _ScanlineFrequency ("Scanline Frequency", Range(1, 100)) = 20
        _ScanlineWidth ("Scanline Width", Range(0, 1)) = 0.1
        
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0.1
        _GlitchSpeed ("Glitch Speed", Range(0, 10)) = 5.0
        
        _Alpha ("Transparency", Range(0, 1)) = 0.6
        _Brightness ("Brightness", Range(0, 3)) = 1.5
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
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;
                half4 _ScanlineColor;
                half _ScanlineSpeed;
                half _ScanlineFrequency;
                half _ScanlineWidth;
                half _GlitchAmount;
                half _GlitchSpeed;
                half _Alpha;
                half _Brightness;
            CBUFFER_END
            
            // Noise function for glitch effect
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Small glitch displacement
                float glitchNoise = random(float2(_Time.y * _GlitchSpeed, input.positionOS.y));
                float glitch = (glitchNoise - 0.5) * _GlitchAmount * 0.01;
                input.positionOS.x += glitch;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = positionInputs.positionCS;
                output.normalWS = normalInputs.normalWS;
                output.positionWS = positionInputs.positionWS;
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Normalize vectors
                half3 normal = normalize(input.normalWS);
                half3 viewDir = normalize(input.viewDirWS);
                
                // Fresnel rim lighting
                half fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _RimPower);
                half3 rimLight = _RimColor.rgb * fresnel * _RimIntensity;
                
                // Animated scanlines
                half scanlineOffset = _Time.y * _ScanlineSpeed;
                half scanlinePattern = frac((input.positionWS.y + scanlineOffset) * _ScanlineFrequency);
                half scanline = step(1.0 - _ScanlineWidth, scanlinePattern);
                half3 scanlineEffect = _ScanlineColor.rgb * scanline * 2.0;
                
                // Pulsing effect
                half pulse = sin(_Time.y * 3.0) * 0.2 + 0.8;
                
                // Combine effects
                half3 baseColor = _Color.rgb * _Brightness * pulse;
                half3 finalColor = baseColor + rimLight + scanlineEffect;
                
                // Dynamic alpha with fresnel
                half alpha = _Alpha + fresnel * 0.3;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
