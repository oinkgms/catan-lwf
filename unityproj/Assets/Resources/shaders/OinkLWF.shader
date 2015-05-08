Shader "Oink/LWF" {
  Properties {
    _Color ("Color", Color) = (1, 1, 1, 1)
    _AdditionalColor ("AdditionalColor", Color) = (0, 0, 0, 0)
    _MultColor ("MultColor", Color) = (1, 1, 1, 1)
    _AddColor ("AddColor", Color) = (0, 0, 0, 0)
    _LerpColor ("LerpColor", Color) = (1, 1, 1, 0)
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
      fixed4 _MultColor;
      fixed4 _AddColor;
      fixed4 _LerpColor;
      struct appdata {
        float4 vertex: POSITION;
        float2 texcoord: TEXCOORD0;
        fixed4 color: COLOR;
      };
      struct v2f {
        float4 pos: SV_POSITION;
        float2 uv: TEXCOORD0;
        fixed4 color: COLOR0; // by SWF
        fixed4 multColor: COLOR1; // by Script
        fixed4 addColor: COLOR2; // by Script
        fixed4 lerpColor: COLOR3; // by Script
#ifdef ENABLE_ADD_COLOR
        fixed4 additionalColor: COLOR4; // by SWF
#endif
      };
      v2f vert(appdata v) {
        v2f o;
        o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
        o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.color = v.color * _Color;
#ifdef ENABLE_ADD_COLOR
        o.additionalColor = _AdditionalColor;
#endif
        o.lerpColor = _LerpColor;
        return o;
      }
      fixed4 frag(v2f i): COLOR {
        fixed4 o = tex2D(_MainTex, i.uv.xy) * i.color;
#ifdef ENABLE_ADD_COLOR
        o += i.additionalColor;
#endif
        o *= _MultColor;
        o += _AddColor;
        o = lerp(o, fixed4(_LerpColor.xyz, o.w), _LerpColor.w);
        return o;
      }
      ENDCG
    }
  }
}
