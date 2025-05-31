// 2D Top-Down Shadow Shader для Unity URP
// Цей шейдер створює м'які динамічні тіні для 2D top-down ігор

Shader "Custom/2D/TopDownShadow"
{
    Properties
    {
        _MainTex ("Shadow Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.5)
        _FalloffDistance ("Falloff Distance", Range(0, 5)) = 1.0
        _SoftEdge ("Soft Edge", Range(0, 1)) = 0.1
        _DistortAmount ("Distortion Amount", Range(0, 1)) = 0.1
        _DistortSpeed ("Distortion Speed", Range(0, 10)) = 1.0
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType" = "Plane"
        }
        
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 worldPos : TEXCOORD1;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            
            float4 _ShadowColor;
            float _FalloffDistance;
            float _SoftEdge;
            float _DistortAmount;
            float _DistortSpeed;
            float3 _LightPosition;
            float3 _LightDirection;
            float _LightType; // 0: directional, 1: point light
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Отримання позиції вершини
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                
                // Зберігаємо світову позицію для обчислень напрямку до світла
                output.worldPos = mul(unity_ObjectToWorld, input.positionOS);
                
                return output;
            }
            
            // Допоміжна функція для обчислення пом'якшення країв
            float SoftEdgeFalloff(float distance, float radius, float softness)
            {
                float falloffStart = radius * (1.0 - softness);
                if (distance < falloffStart)
                    return 1.0;
                else
                    return 1.0 - smoothstep(falloffStart, radius, distance);
            }
            
            // Функція для створення шуму, що додає живий вигляд тіням
            float2 DistortUV(float2 uv, float time)
            {
                float noiseX = sin(uv.x * 12.0 + time) * 0.1 + sin(uv.y * 7.0 + time * 0.9) * 0.05;
                float noiseY = cos(uv.y * 10.0 + time) * 0.1 + cos(uv.x * 8.0 + time * 1.1) * 0.05;
                
                return float2(noiseX, noiseY) * _DistortAmount;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Отримання текстури тіні з урахуванням базового кольору
                float2 distortedUV = input.uv + DistortUV(input.uv, _Time.y * _DistortSpeed);
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);
                
                // Обчислюємо відстань та напрямок від джерела світла
                float3 worldPos = input.worldPos.xyz;
                float3 lightDir;
                float distanceToLight;
                
                if (_LightType < 0.5) // Directional light
                {
                    lightDir = normalize(_LightDirection);
                    distanceToLight = 1.0; // Для направленого світла використовуємо константу
                }
                else // Point light
                {
                    lightDir = normalize(worldPos - _LightPosition);
                    distanceToLight = length(worldPos - _LightPosition);
                }
                
                // Обчислюємо інтенсивність тіні на основі відстані до джерела світла
                float shadowIntensity = SoftEdgeFalloff(distanceToLight, _FalloffDistance, _SoftEdge);
                
                // Додаємо ефект перспективи для тіней (далі від джерела світла = довша тінь)
                float perspectiveEffect = lerp(1.0, 1.5, saturate(distanceToLight / _FalloffDistance));
                
                // Фінальний колір тіні
                half4 shadowColor = _ShadowColor;
                shadowColor.a *= texColor.a * shadowIntensity * input.color.a * perspectiveEffect;
                
                return shadowColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Sprites/Default"
}