Shader "Oink/LWF" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_AdditionalColor ("AdditionalColor", Color) = (0, 0, 0, 0)
		_HogeColor ("HogeColor", Color) = (0, 0, 0, 0)
		_MainTex ("Texture", 2D) = "white" {}
		BlendModeSrc ("BlendModeSrc", Float) = 0
		BlendModeDst ("BlendModeDst", Float) = 0
		BlendEquation ("BlendEquation", Float) = 0
	}

	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
		}
		Cull Off
		ZWrite Off
		Blend [BlendModeSrc] [BlendModeDst], One One
		BlendOp [BlendEquation], Add
		Pass {
			CGPROGRAM
			#pragma multi_compile DISABLE_ADD_COLOR ENABLE_ADD_COLOR
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			half4 _MainTex_ST;
			fixed4 _Color;
			#ifdef ENABLE_ADD_COLOR
			fixed4 _AdditionalColor;
			#endif
			fixed4 _HogeColor;
			struct appdata {
				float4 vertex: POSITION;
				float2 texcoord: TEXCOORD0;
				fixed4 color: COLOR;
			};
			struct v2f {
				float4 pos: SV_POSITION;
				float2 uv: TEXCOORD0;
			#ifdef ENABLE_ADD_COLOR
				fixed4 color: COLOR0;
				fixed4 additionalColor: COLOR1;
			#else
				fixed4 color: COLOR;
			#endif
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;
			#ifdef ENABLE_ADD_COLOR
				o.additionalColor = _AdditionalColor;
			#endif
				return o;
			}
			fixed4 frag(v2f i): COLOR
			{
				fixed4 o = tex2D(_MainTex, i.uv.xy) * i.color;
			#ifdef ENABLE_ADD_COLOR
				o += i.additionalColor;
			#endif
				return o;// * fixed4(_HogeColor.xyz, 1);
			}
			ENDCG
		}
	}
}
