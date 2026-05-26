Shader "Custom/Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,1,1,1)
        _OutlineSize ("Outline Size", Range(0, 0.05)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // PASADA DE CONTORNO
        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _OutlineSize;
            float4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float3 norm = normalize(v.normal);
                v.vertex.xyz += norm * _OutlineSize;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}