Shader "HoleShader/ZWriteOff"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ColorMask 0
        Pass
        {
            ZWrite Off
        }
    }
}
