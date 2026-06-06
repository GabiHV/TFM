Shader "Custom/Aura"
{
    Properties
    {
        _Color ("Color", Color) = (0,1,1,1)
        _Intensity ("Intensity", Float) = 1
        _Scale ("Scale", Float) = 1.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _Scale;
            float4 _Color;
            float _Intensity;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 scaled = v.vertex.xyz + v.normal * (_Scale - 1);
                o.pos = UnityObjectToClipPos(float4(scaled, 1));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pulse = (sin(_Time.y * 2) + 1) * 0.5;
                return _Color * pulse * _Intensity;
            }
            ENDCG
        }
    }
}