﻿Shader "Custom/Texture"
{
	Properties
	{
		_Texture("Texture",2D) = "white"{}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass{

			Cull Off
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _Texture;

			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 pos: SV_POSITION;
				float2 uv:TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col;
				col = tex2D(_Texture, i.uv);

				clip(col.a-0.1);
				return col;
			}

			ENDCG
		}
	}
}
