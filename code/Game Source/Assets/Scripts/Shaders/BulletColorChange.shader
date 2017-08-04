// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ColorChange" //Basically took Unity's default sprite shader and modified it for my needs. Any added line will be suffixed with "//Added".
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		//_Color("Tint", Color) = (1,1,1,1) //Removed
		[PerRendererData] _Color1("Replace Red With", Color) = (1,1,1,1) //Added
		[PerRendererData] _Color2("Replace Green With", Color) = (1,1,1,1) //Added
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile _ PIXELSNAP_ON
		#include "UnityCG.cginc"

	struct appdata_t
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex   : SV_POSITION;
		fixed4 color : COLOR;
		float2 texcoord  : TEXCOORD0;
	};

	//fixed4 _Color; //Removed

	v2f vert(appdata_t IN)
	{
		v2f OUT;
		OUT.vertex = UnityObjectToClipPos(IN.vertex);
		OUT.texcoord = IN.texcoord;
		//OUT.color = IN.color * _Color; //Removed
		OUT.color = IN.color; //Addded
		#ifdef PIXELSNAP_ON
		OUT.vertex = UnityPixelSnap(OUT.vertex);
		#endif

		return OUT;
	}

	sampler2D _MainTex;
	sampler2D _AlphaTex;
	float _AlphaSplitEnabled;
	float4 _Color1; //Added
	float4 _Color2; //Added

	fixed4 SampleSpriteTexture(float2 uv)
	{
		fixed4 color = tex2D(_MainTex, uv);

		#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
		if (_AlphaSplitEnabled)
			color.a = tex2D(_AlphaTex, uv).r;
		#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

		if (color.r > 0 || color.g > 0) { //Added
			float denominator = color.r + color.g; //Added
			color.a *= _Color1.a * color.r/(denominator) + _Color2.a * color.g/(denominator); //Added; if this is at the bottom of this if-block, it fails for some reason
			float redPart = color.r*color.r/denominator; //Added
			float greenPart = color.g*color.g/denominator; //Added
			color.r = _Color1.r * redPart + _Color2.r * greenPart; //Added
			color.g = _Color1.g * redPart + _Color2.g * greenPart; //Added
			color.b = _Color1.b * redPart + _Color2.b * greenPart; //Added
		}

		return color;
	}

	fixed4 frag(v2f IN) : SV_Target
	{
		fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
		c.rgb *= c.a;
		return c;
	}
		ENDCG
	}
	}
}
