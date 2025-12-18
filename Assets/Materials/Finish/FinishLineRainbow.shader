Shader "Custom/FinishLineRainbow"
{
    Properties
    {
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 2.0
        _WaveFrequency ("Wave Frequency", Range(0, 20)) = 5.0
        _WaveAmplitude ("Wave Amplitude", Range(0, 1)) = 0.3
        _RainbowSpeed ("Rainbow Speed", Range(0, 5)) = 1.0
        _Brightness ("Brightness", Range(0, 3)) = 1.5
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.0
        _RimPower ("Rim Power", Range(0, 10)) = 4.0
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
                half _WaveSpeed;
                half _WaveFrequency;
                half _WaveAmplitude;
                half _RainbowSpeed;
                half _Brightness;
                half _GlowIntensity;
                half _RimPower;
            CBUFFER_END
            
            // HSV to RGB conversion
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Vertex wave animation
                float wave = sin((input.positionOS.y + _Time.y * _WaveSpeed) * _WaveFrequency) * _WaveAmplitude;
                input.positionOS.xyz += input.normalOS * wave * 0.1;
                
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
                
                // Animated rainbow colors
                half hue = frac(input.positionWS.y * 0.2 + input.positionWS.x * 0.1 + _Time.y * _RainbowSpeed);
                half3 rainbowColor = hsv2rgb(float3(hue, 0.8, 1.0));
                
                // Wave pattern overlay
                half wavePattern = sin((input.positionWS.y + _Time.y * _WaveSpeed * 0.5) * _WaveFrequency * 0.5);
                wavePattern = wavePattern * 0.5 + 0.5; // Remap to 0-1
                
                // Fresnel rim effect
                half fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _RimPower);
                half3 rimGlow = rainbowColor * fresnel * _GlowIntensity;
                
                // Pulsing brightness
                half pulse = sin(_Time.y * 2.0) * 0.3 + 0.7;
                
                // Combine effects
                half3 baseColor = rainbowColor * wavePattern * _Brightness * pulse;
                half3 finalColor = baseColor + rimGlow;
                
                // Dynamic transparency
                half alpha = 0.5 + wavePattern * 0.3 + fresnel * 0.2;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
