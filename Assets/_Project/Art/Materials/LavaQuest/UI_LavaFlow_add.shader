Shader "UI/LavaFlow_Add"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} // NEW: чтобы UI Image не ругался

        _FlowTex ("Flow Map (Grayscale)", 2D) = "gray" {} // sRGB Off, Repeat, None
        _Tiling  ("Tiling", Vector) = (0.8, 0.4, 0, 0)
        _Speed   ("Flow Speed (x,y)", Vector) = (0.0, -0.02, 0, 0) // вниз по Y<0

        _GlowColor    ("Glow Color", Color) = (1,0.85,0,1) // жёлтый
        _GlowStrength ("Glow Strength", Range(0,3)) = 1.0
        _Contrast     ("Flow Contrast",  Range(0.2,4)) = 1.5
        _Intensity    ("Flow Intensity", Range(0,4)) = 1.2

        _EdgeFeather  ("Edge Feather (0..0.5)", Range(0,0.5)) = 0.1
        _CornerRadius ("Corner Radius (0..0.5)", Range(0,0.5)) = 0.0

        // UI Stencil для совместимости с Canvas/Maskable
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        // ЧИСТО АДДИТИВНЫЙ СВЕТ, без альфы
        Blend One One
        ColorMask RGB

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex; float4 _MainTex_ST;           // NEW: объявление, чтобы Canvas был счастлив :)
            sampler2D _FlowTex; float4 _FlowTex_ST;
            float4 _Tiling, _Speed;
            fixed4 _GlowColor; float _GlowStrength, _Contrast, _Intensity;
            float _EdgeFeather, _CornerRadius;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f {
                float4 pos:SV_POSITION;
                float2 uvFlow:TEXCOORD0;
                float2 uvRect:TEXCOORD1;
                fixed3 tint:TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uvRect = v.uv; // 0..1 прямоугольник

                // flow UV
                float2 uvf = TRANSFORM_TEX(v.uv, _FlowTex);
                uvf *= _Tiling.xy;
                uvf += _Speed.xy * _Time.y;
                o.uvFlow = uvf;

                o.tint = v.color.rgb; // цвет от Image->Color если надо
                return o;
            }

            // SDF скруглённого прямоугольника
            float rectRoundMask(float2 uv, float radius, float feather)
            {
                float2 p = uv * 2.0 - 1.0;
                float2 b = 1.0 - 2.0 * radius;
                float2 d = abs(p) - b;
                float dist = length(max(d,0.0)) + min(max(d.x,d.y), 0.0);
                return saturate(1.0 - smoothstep(0.0, feather, dist));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // маска «света» из flow-текстуры
                half flow = tex2D(_FlowTex, i.uvFlow).r;
                flow = saturate(pow(flow, _Contrast) * _Intensity);
                half addMask = max(0, flow - 0.5) * 2.0; // берём только светлую часть

                // цветное свечение
                fixed3 glow = _GlowColor.rgb * (_GlowStrength * addMask);

                // мягкая рамка по прямоугольнику (перо + опц. радиус)
                float m = rectRoundMask(i.uvRect, _CornerRadius, max(1e-4,_EdgeFeather));

                // ОТДАЁМ ТОЛЬКО RGB (аддитив), без альфы
                return fixed4(glow * m, 0);
            }
            ENDCG
        }
    }
    Fallback Off
}
