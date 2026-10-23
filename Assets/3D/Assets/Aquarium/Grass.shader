
Shader "Custom/GrassWind"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Grass Color", Color) = (1,1,1,1)

        [Header(Wind)]
        _WindStrength ("Wind Strength", Float) = 0.25
        _WindSpeed ("Wind Speed", Float) = 2
        _WindVariation ("Wind Variation", Float) = 2
        _WindDirection ("Wind Direction XZ", Vector) = (1,0,0.3,0)

        [Header(Random Movement)]
        _Randomness ("Randomness", Float) = 0.35
        _RandomScale ("Random Scale", Float) = 8

        [Header(Flutter)]
        _FlutterStrength ("Flutter Strength", Float) = 0.04
        _FlutterSpeed ("Flutter Speed", Float) = 9

        [Header(Height Mask)]
        _UseHeightMask ("Use Height Mask (0 Off 1 On)", Float) = 0
        _HeightAxis ("Height Axis (0 X 1 Y 2 Z)", Float) = 1
        _RootPosition ("Root Position", Float) = 0
        _GrassHeight ("Grass Height", Float) = 1
        _BendPower ("Bend Power", Float) = 2

        [Header(Transparency)]
        _Opacity ("Opacity", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "GrassForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

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
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float _Opacity;
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _WindDirection;

                float _WindStrength;
                float _WindSpeed;
                float _WindVariation;

                float _Randomness;
                float _RandomScale;

                float _FlutterStrength;
                float _FlutterSpeed;

                float _UseHeightMask;
                float _HeightAxis;
                float _RootPosition;
                float _GrassHeight;
                float _BendPower;

            CBUFFER_END

            // RANDOM HASH
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);

                return frac(p.x * p.y);
            }

            // SMOOTH RANDOM NOISE
            float Noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 localPos = IN.positionOS.xyz;

                float3 worldPos =
                    TransformObjectToWorld(localPos);

                float t = _Time.y;

                // HEIGHT MASK
                float heightCoordinate = localPos.y;

                if (_HeightAxis < 0.5)
                    heightCoordinate = localPos.x;
                else if (_HeightAxis >= 1.5)
                    heightCoordinate = localPos.z;

                float heightSize =
                    abs(_GrassHeight) < 0.00001
                    ? 1.0
                    : _GrassHeight;

                float height = saturate(
                    (heightCoordinate - _RootPosition)
                    / heightSize
                );

                float heightMask = pow(
                    height,
                    max(_BendPower, 0.001)
                );

                float mask = lerp(
                    1.0,
                    heightMask,
                    saturate(_UseHeightMask)
                );

                // RANDOMNESS
                // Stable spatial noise gives nearby
                // vertices smoothly varying motion.

                float2 randomUV =
                    worldPos.xz * _RandomScale;

                float random1 = Noise2D(
                    randomUV + float2(12.4, 31.7)
                ) * 2.0 - 1.0;

                float random2 = Noise2D(
                    randomUV * 1.73 + float2(45.1, 8.3)
                ) * 2.0 - 1.0;

                // Different phase for different areas
                float randomPhase =
                    random1 * _Randomness * 3.0;

                // Slightly different rotation speeds
                float randomSpeed =
                    1.0 + random2 * _Randomness * 0.3;

                // Slightly different strengths
                float randomStrength =
                    1.0 + random1 * _Randomness * 0.35;

                // WIND PATTERN
                float phase = dot(
                    worldPos.xz,
                    float2(0.73, 1.21)
                ) * _WindVariation;

                phase += randomPhase;

                float windTime =
                    t * _WindSpeed * randomSpeed;

                float wave1 = sin(
                    windTime + phase
                );

                float wave2 = sin(
                    windTime * 0.57 +
                    phase * 1.37
                ) * 0.35;

                float mainWind =
                    (wave1 + wave2) *
                    _WindStrength *
                    randomStrength;

                // RANDOM FLUTTER
                float flutterSpeed =
                    _FlutterSpeed *
                    (1.0 + random1 * _Randomness * 0.4);

                float flutter = sin(
                    t * flutterSpeed +
                    phase * 2.4 +
                    random2 * _Randomness * 3.0
                ) * _FlutterStrength;

                // WIND DIRECTION
                float2 direction = _WindDirection.xz;

                float directionLength = length(direction);

                if (directionLength < 0.0001)
                    direction = float2(1, 0);
                else
                    direction /= directionLength;

                float2 sideDirection =
                    float2(-direction.y, direction.x);

                // APPLY MOVEMENT
                float2 offset =
                    direction * mainWind +
                    sideDirection * flutter;

                worldPos.xz += offset * mask;

                OUT.positionCS =
                    TransformWorldToHClip(worldPos);

                OUT.normalWS =
                    TransformObjectToWorldNormal(IN.normalOS);

                OUT.uv = TRANSFORM_TEX(
                    IN.uv,
                    _BaseMap
                );

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    IN.uv
                );

                half4 color = tex * _BaseColor;

                float3 normalWS =
                    normalize(IN.normalWS);

                Light mainLight = GetMainLight();

                float diffuse = abs(
                    dot(normalWS, mainLight.direction)
                );

                half3 ambient = max(
                    SampleSH(normalWS),
                    half3(0.25, 0.25, 0.25)
                );

                half3 lighting =
                    ambient +
                    mainLight.color * diffuse * 0.7;

return half4(
    color.rgb * lighting,
    color.a * _Opacity
);
            }

            ENDHLSL
        }
    }
}
