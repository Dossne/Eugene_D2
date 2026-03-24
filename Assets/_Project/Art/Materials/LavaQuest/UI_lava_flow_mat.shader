Shader "UI/LavaFlow_Mini"
{
    Properties
    {
        _BaseColor ("Base Color (Tint)", Color) = (1,1,1,1)
        _MainTex ("Base (RGBA)", 2D) = "white" {}
        _FlowTex ("Flow Map (Grayscale)", 2D) = "gray" {}
        _Tiling ("Tiling", Float) = 1
        _FlowSpeedX ("Flow Speed X", Float) = 0.03
        _Intensity ("Flow Intensity", Range(0,2)) = 1
        _Contrast ("Flow Contrast", Range(0.2,4)) = 1

        // UI Stencil
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // ClipRect (если используешь ScrollView/Mask с м€гкостью)
        _UseClipRect ("Use Clip Rect", Float) = 0
        _UIMaskSoftnessX ("Softness X", Float) = 0
        _UIMaskSoftnessY ("Softness Y", Float) = 0
    }

    SubShader
    {
        Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One One
ColorMask RGB
        ColorMask [_ColorMask]

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

            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _FlowTex; float4 _FlowTex_ST;

            fixed4 _BaseColor;
            float _Tiling, _FlowSpeedX, _Intensity, _Contrast;

            float4 _ClipRect;
            float  _UseClipRect;
            float  _UIMaskSoftnessX, _UIMaskSoftnessY;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float2 uvF:TEXCOORD1; fixed4 col:COLOR; float4 worldPos:TEXCOORD2; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.col = v.color * _BaseColor;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float2 uvf = TRANSFORM_TEX(v.uv, _FlowTex) * _Tiling;
                uvf.y -= _Time.y * _FlowSpeedX; 
                o.uvF = uvf;

                o.worldPos = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                baseCol *= i.col;

                half flow = tex2D(_FlowTex, i.uvF).r;
                flow = saturate(pow(flow, _Contrast) * _Intensity);

                // осветление (screen)
                fixed3 result = 1 - (1 - baseCol.rgb) * (1 - flow);
                fixed4 col = fixed4(result, baseCol.a);

          
                if (_UseClipRect > 0.5)
{
    col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
}


                return col; // <-- без UnityGetColorMask()
            }
            ENDCG
        }
    }

    Fallback "UI/Default"
}
