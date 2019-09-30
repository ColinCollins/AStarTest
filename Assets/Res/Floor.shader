﻿Shader "Custom / Floor" {
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) =  (1, 1, 1, 1)
    }
    SubShader
    {
		Tags{"RenderType" = "Always"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "UnityCG.cginc"

			struct a2v {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;

			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				// sample the texture
				fixed4 texColor = tex2D(_MainTex, i.uv);

				return texColor * _Color;
			}

            ENDCG
        }
    }
}
