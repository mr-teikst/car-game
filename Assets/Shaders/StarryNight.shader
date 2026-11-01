Shader "NightDrive/Starry Night"
{
    Properties
    {
        _Zenith ("Zenith Color", Color) = (0.006, 0.012, 0.045, 1)
        _Horizon ("Horizon Color", Color) = (0.035, 0.055, 0.13, 1)
        _StarBrightness ("Star Brightness", Range(0, 5)) = 1.7
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "RenderPipeline"="UniversalPipeline" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Zenith, _Horizon;
                half _StarBrightness;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; };
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }
            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.direction);
                float altitude = saturate(dir.y);
                half3 sky = lerp(_Horizon.rgb, _Zenith.rgb, pow(altitude, .42));
                float2 uv = float2(atan2(dir.z, dir.x) / 6.2831853 + .5, asin(clamp(dir.y,-1,1)) / 3.14159265 + .5);
                float2 grid = uv * float2(300, 150);
                float2 cell = floor(grid);
                float seed = hash21(cell);
                float2 offset = float2(hash21(cell + 19.7), hash21(cell + 47.1)) * .7 + .15;
                float radius = length(frac(grid) - offset);
                float star = (1.0 - smoothstep(.025, .085, radius)) * step(.982, seed);
                float visible = smoothstep(.015, .16, dir.y);
                sky += star * visible * _StarBrightness * lerp(float3(.65,.78,1), float3(1,.93,.82), hash21(cell + 3.5));
                return half4(sky, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
