Shader "Custom/URP 2D/Blueprint Paper Sprite"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1, 1, 1, 1)
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        _PaperColor("Paper Color", Color) = (0.032, 0.173, 0.361, 1)
        _PaperHighlight("Paper Highlight", Color) = (0.073, 0.267, 0.518, 1)
        _GridColor("Minor Grid Color", Color) = (0.239, 0.733, 0.980, 1)
        _MajorGridColor("Major Grid Color", Color) = (0.745, 0.941, 1.000, 1)
        _InkColor("Ink Color", Color) = (0.918, 0.980, 1.000, 1)

        _MinorGridSize("Minor Grid Size", Float) = 0.2
        _MajorGridEvery("Major Grid Every", Float) = 4
        _LineWidth("Grid Line Width", Range(0.001, 0.1)) = 0.014
        _GridIntensity("Grid Intensity", Range(0, 2)) = 1

        _NoiseScale("Paper Noise Scale", Float) = 18
        _NoiseAmount("Paper Noise Amount", Range(0, 0.5)) = 0.08
        _FiberStrength("Fiber Strength", Range(0, 0.5)) = 0.05
        _DiagonalSheen("Diagonal Sheen", Range(0, 1)) = 0.12

        _InkStrength("Ink Strength", Range(0, 2)) = 1
        _InkFromTexture("Ink From Texture", Range(0, 1)) = 0
        _TextureLumaInfluence("Texture Luma Influence", Range(0, 1)) = 0
        _Opacity("Opacity", Range(0, 1)) = 1

        // Legacy sprite properties kept for SpriteRenderer compatibility.
        [HideInInspector] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex BlueprintVertex
            #pragma fragment BlueprintFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _PaperColor;
                half4 _PaperHighlight;
                half4 _GridColor;
                half4 _MajorGridColor;
                half4 _InkColor;
                float4 _MainTex_ST;
                float _MinorGridSize;
                float _MajorGridEvery;
                float _LineWidth;
                float _GridIntensity;
                float _NoiseScale;
                float _NoiseAmount;
                float _FiberStrength;
                float _DiagonalSheen;
                float _InkStrength;
                float _InkFromTexture;
                float _TextureLumaInfluence;
                float _Opacity;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float GridMask(float2 uv, float cellSize, float lineWidth)
            {
                float safeCellSize = max(cellSize, 0.0001);
                float2 distToLine = min(frac(uv / safeCellSize), 1.0 - frac(uv / safeCellSize)) * safeCellSize;
                float2 aa = max(fwidth(uv), float2(0.0001, 0.0001));
                float2 lineMask = 1.0 - smoothstep(lineWidth, lineWidth + aa * 1.5, distToLine);
                return saturate(max(lineMask.x, lineMask.y));
            }

            Varyings BlueprintVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 BlueprintFragment(Varyings input) : SV_Target
            {
                float2 uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                half4 spriteSample = input.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                float2 paperPos = uv;
                float minorGrid = GridMask(paperPos, _MinorGridSize, _LineWidth);
                float majorGrid = GridMask(paperPos, _MinorGridSize * max(_MajorGridEvery, 1.0), _LineWidth * 1.8);

                float noiseA = ValueNoise(paperPos * _NoiseScale);
                float noiseB = ValueNoise(paperPos * (_NoiseScale * 0.43) + 17.0);
                float grain = (noiseA * 0.65 + noiseB * 0.35) - 0.5;

                float fiber = sin((paperPos.x * 1.7 + paperPos.y * 32.0) * (_NoiseScale * 0.15) + noiseB * 6.28318);
                fiber = fiber * 0.5 + 0.5;

                float sheen = sin((paperPos.x + paperPos.y) * (_NoiseScale * 0.08));
                sheen = sheen * 0.5 + 0.5;

                float3 paperColor = lerp(_PaperColor.rgb, _PaperHighlight.rgb, saturate(0.35 + grain * _NoiseAmount * 4.0 + sheen * _DiagonalSheen));
                paperColor += (noiseA - 0.5) * _NoiseAmount;
                paperColor += (fiber - 0.5) * _FiberStrength;

                float gridBlend = saturate(minorGrid * 0.45 * _GridIntensity);
                float majorBlend = saturate(majorGrid * 0.9 * _GridIntensity);

                paperColor = lerp(paperColor, _GridColor.rgb, gridBlend);
                paperColor = lerp(paperColor, _MajorGridColor.rgb, majorBlend);

                float luminance = dot(spriteSample.rgb, float3(0.2126, 0.7152, 0.0722));
                float inkMask = lerp(spriteSample.a, spriteSample.a * (1.0 - luminance), _TextureLumaInfluence);
                inkMask = saturate(inkMask * _InkStrength);
                inkMask *= _InkFromTexture;

                float3 finalColor = lerp(paperColor, _InkColor.rgb, inkMask);
                return half4(finalColor, spriteSample.a * _Opacity);
            }
            ENDHLSL
        }
    }
}
