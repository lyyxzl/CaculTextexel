Shader "Hidden/cacultextexel"
{
	Properties
	{
		[MainTexture]_MainTex("Main Texture", 2D) = "black" {}
        //_BaseMap("_BaseMap", 2D) = "black" {}
        Co("c", Color) = (1,0,0,1)
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
			
		Pass
		{
			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x ps3 xbox360 flash xboxone ps4 psp2
			#pragma target 2.0 

			#pragma vertex vert
			#pragma fragment frag


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


			struct Attributes
			{
				float4 positionOS  : POSITION;
				float4 uv          : TEXCOORD0;
			};

			struct Varying
			{
				float4 positionHCS : SV_POSITION;
				float4 uv         : TEXCOORD0;
			};


			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
			CBUFFER_END

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

            TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			Varying vert(Attributes IN)
			{
				Varying OUT = (Varying)0;

				float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
				OUT.positionHCS = TransformWorldToHClip(positionWS);

				OUT.uv.xy = TRANSFORM_TEX(IN.uv.xy, _MainTex);
	
				return OUT;
			}


            float Remap(float value, float oriMin, float oriMax, float dstMin, float dstMax)
            {
                return (value - oriMin) / (oriMax - oriMin) * (dstMax - dstMin) + dstMin;
            }

            float4 frag(Varying IN) : SV_Target
			{
               //return half4(1,0,0,1);
				float4 col = 0;
				col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv.xy);
                half4 col2 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv.xy);

                //return col;
               // return col;
                float2 uv = IN.uv * _MainTex_TexelSize.zw;
                float2 dx = ddx(uv);
                float2 dy = ddy(uv);

                float rho = max(sqrt(dot(dx, dx)), sqrt(dot(dy, dy)));
                float dd = rho;

                half3 color2xUp = half3(0, 1, 0);
                half3 color1xUp = half3(0, 230.0/255.0, 186.0/255.0);
                half3 colorfix = half3(217.0/255.0, 217.0/255.0, 217.0/255.0);
                half3 color1xDown = half3(230.0/255.0, 186.0/255.0, 0);
                half3 color2xDown = half3(255.0/255.0, 0, 0);
                half3 colorNotDefine = half3(38.0/255.0, 38.0/255.0, 38.0/255.0);

        
                if (dd > 1){
                    //1-2xup
                    half3 dst = color2xUp;
                    half3 src = color1xUp;

                    half g = Remap(dd-1, 0, 1, src.g, dst.g);
                    half b = Remap(dd-1, 0, 1, src.b, dst.b);

                    return half4(0, g, b, 1);
                }

                float minD = 0.9;
                float maxD = 1;
                if (dd <= maxD && dd >= minD){
                    //fix-1
                    half3 src= colorfix;
                    half3 dst = color1xUp;

                    half r = Remap(dd, minD, maxD, src.r, dst.r);
                    half g = Remap(dd, minD, maxD, src.g, dst.g);
                    half b = Remap(dd, minD, maxD, src.b, dst.b);

                    return half4(r, g, b, 1);
                }

                maxD = minD;
                minD = 0.5;
                if (dd <= maxD && dd >= minD){
                    
                    half3 src= color1xDown;
                    half3 dst = colorfix;

                    half r = Remap(dd, minD, maxD, src.r, dst.r);
                    half g = Remap(dd, minD, maxD, src.g, dst.g);
                    half b = Remap(dd, minD, maxD, src.b, dst.b);

                    return half4(r, g, b, 1);
                }

                maxD = minD;
                minD = 0;
                if (dd <= maxD && dd >= minD){
                    
                    half3 src= color2xDown;
                    half3 dst = color1xDown;

                    half r = Remap(dd, minD, maxD, src.r, dst.r);
                    half g = Remap(dd, minD, maxD, src.g, dst.g);
                    half b = Remap(dd, minD, maxD, src.b, dst.b);

                    return half4(r, g, b, 1);
                }

                return half4(colorNotDefine,1);
			}
			ENDHLSL
		}
	}
}
