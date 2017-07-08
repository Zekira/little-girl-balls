// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Invert" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader {
		Tags {
			"RenderType"="Transparant"
			"Queue"="Transparent"
		}

		GrabPass { "_GrabTexture" }

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _GrabTexture;

			struct vin_vct { float4 vertex : POSITION; };

			struct v2f_vct { float4 vertex : POSITION; float4 uvgrab : TEXCOORD1; };

			v2f_vct vert (vin_vct v) {
				v2f_vct o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				return o;
			}

			half4 frag (v2f_vct i) : COLOR {
				half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				col.r = 1 - col.r;
				col.g = 1 - col.g;
				col.b = 1 - col.b;
				return col;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
