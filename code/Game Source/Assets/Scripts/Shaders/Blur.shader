// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Blur" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader {
		Tags {
			"RenderType"="Transparant"
			"Queue"="Transparent+100"
			"LightMode"="Always"
		}

		GrabPass { "_GrabTexture2" }

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _GrabTexture2;

			struct vin_vct { float4 vertex : POSITION; };

			struct v2f_vct { float4 vertex : POSITION; float4 uvgrab : TEXCOORD1; };

			v2f_vct vert (vin_vct v) {
				v2f_vct o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				return o;
			}

			half4 frag (v2f_vct i) : COLOR {
				half4 col = 0;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.007, -0.007, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.004, -0.008, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.000, -0.010, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.004, -0.008, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.007, -0.007, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.080, -0.004, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.003, -0.003, 0, 0))) * 2;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.000, -0.005, 0, 0))) * 3;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.003, -0.003, 0, 0))) * 2;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.008, -0.004, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.010,  0.000, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.005,  0.000, 0, 0))) * 3;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.000,  0.000, 0, 0))) * 8;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.005,  0.000, 0, 0))) * 3;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.010,  0.000, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.008,  0.004, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.003,  0.003, 0, 0))) * 2;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.000,  0.005, 0, 0))) * 3;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.003,  0.003, 0, 0))) * 2;
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.008,  0.004, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.007,  0.007, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4(-0.004,  0.008, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.000,  0.010, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.004,  0.008, 0, 0)));
				col += tex2Dproj(_GrabTexture2, UNITY_PROJ_COORD(i.uvgrab + float4( 0.007,  0.007, 0, 0)));
				col /= 44;
				return col;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
