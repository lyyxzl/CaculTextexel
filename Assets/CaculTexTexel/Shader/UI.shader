Shader "Hidden/CacTexUI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderQueue" = "Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                half n = -1;
                #if UNITY_UV_STARTS_AT_TOP
                    n = 1;
                #endif

                if (v.vertex.y > 0) {
                    o.vertex.y = 0.7 * n;
                }
                else {
                    o.vertex.y = n;
                }
                o.vertex.x = v.vertex.x;
                o.vertex.z = n;
                o.vertex.w = 1;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //  #if UNITY_UV_STARTS_AT_TOP
                //     half s = 0.1;
                // #else
                //  half s = 0.5;
                //     return half4(s,0,0,1);
                //  #endif
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                //col.a = 0.1;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
