Shader "Custom/Sample_TextureAndTransparent"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SubTex("Texture", 2D) = "white" {}

	}
		SubShader
	{

		LOD 100


		Pass
		{
			//Cull Front   //先に内側だけ処理してる
			//ZTest Always    //まず深度バッファ無視して後ろにあるものも全て描画(これだけだと前後関係がおかしくなる)
							//そのあと深度バッファに応じてもう一度描画(これだけだと透過部分が先に処理された場合その後ろに描画されなくなる)
			CGPROGRAM
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

			sampler2D _MainTex;
			float4 _MainTex_ST; //o.uv = TRANSFORM_TEX(v.uv, _MainTex); で使うか自分で使う. このシェーダーでは使っていない
			sampler2D _SubTex;
			float4 _SubTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);    //これを使うと勝手にoffsetとかやってくれる
				o.uv = v.uv;
				return o;
			}





			fixed4 frag(v2f i) : SV_Target
			{
				
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv* _MainTex_ST.xy + _MainTex_ST.zw);
				return col;
			}
			ENDCG
		}

	}
}
