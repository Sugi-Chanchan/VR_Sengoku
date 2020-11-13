Shader "Custom/Unlit"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
	}
		SubShader
	{

		LOD 100


		CGINCLUDE

		#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			
			
				
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);    //これを使うと勝手にoffsetとかやってくれる
				o.uv = v.uv;
				return o;
			}



		ENDCG
		Pass
		{
			
			CGPROGRAM
			sampler2D _MainTex;
			float4 _MainTex_ST; //o.uv = TRANSFORM_TEX(v.uv, _MainTex); で使うか自分で使う. このシェーダーでは使っていない

			fixed4 frag(v2f i) : SV_Target
			{

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv* _MainTex_ST.xy + _MainTex_ST.zw);
				return col;
			}
			ENDCG
		}
		Pass
		{
				Cull Front   
				CGPROGRAM

				fixed4 _Color;
				fixed4 frag(v2f i) : SV_Target
				{
					return _Color;
				}
				ENDCG
			}

	}
}
