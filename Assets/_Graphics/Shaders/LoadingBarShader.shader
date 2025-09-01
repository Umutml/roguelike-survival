Shader "Unlit/LoadingBarShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0, 0, 0)
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // Saydamlık için kaynak ve hedef alfa blend ayarı
        Blend SrcAlpha OneMinusSrcAlpha 
        ZWrite Off // Transparan nesnelerin derinlik yazma kapalı
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _ScrollSpeed;
            float4 _Color;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv + _ScrollSpeed.xy * _Time.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;
                clip(texColor.a - 0.1); // Transparan bölgeler için alpha clip
                return texColor;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
