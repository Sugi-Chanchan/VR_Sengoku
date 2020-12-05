Shader "Custom2/Vertex"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		[PowerSlider(5)]_Size("Size",Range(0.0001,0.2)) = 0.05
		[PowerSlider(3)]_Slide("Slide",Range(0,10)) = 1
	}
		SubShader
	{
		Tags {
		"RenderType" = "Opaque" }
		LOD 200

	   Pass{


			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom //ジオメトリーシェーダーを使うことを宣言する
			#include "UnityCG.cginc"

			fixed4 _Color;
			float _Size;
			float _Slide;
			struct appdata
			{
				float4 vertex: POSITION;
				float3 normal :NORMAL;
			};

			struct v2g
			{
				float4 vertex:TEXCOORD0;
			};

			struct g2f
			{
				float4 pos :SV_POSITION;
				float3 normal  :NORMAL;
			};

			v2g vert(appdata v)
			{
				v2g o;
				o.vertex = v.vertex;
				return o;
			}

			float3 random(float3 v) {

				float x, y, z;
				v *= (1 << 10);
				x = sin(v.x + v.y);
				y = sin(v.z + v.y);
				z = sin(v.z + v.x);
				return float3(x, y, z);
			}

			//[maxvertexcount()]は最大出力数今回は一度のループで24個まで頂点を出力できるようにした
			//triangle v2gは他にもpoint, line , triangleadj1などが使える(pointにしてもすべての頂点が出るわけではないっぽい, 多分三角形を構成する最初の点・線だけが出てくる)
			//pointで入力してすべての点を得たい場合はMesh側のTopologyをPointに変えておく必要あり(そうすると頂点の共有は解除されて三角ポリゴン1つにつき3つの頂点がでてくる).

			[maxvertexcount(72)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream) //TriangleStreamは出力されるオブジェクトの形. 他には、PointStream型、LineStream型がある
			{
				float length = _Size;
				float3 v = input[0].vertex;
				float3 slide = random(v)*_Slide;
				float3 center=v+slide;

				float3 pos[8];

				{
					pos[0] = center + float3(-1, 1, 1)*length;
					pos[1] = center + float3(-1, -1, 1)*length;
					pos[2] = center + float3(1, 1, 1)*length;
					pos[3] = center + float3(1, -1, 1)*length;
					pos[4] = center + float3(1, 1, -1)*length;
					pos[5] = center + float3(1, -1, -1)*length;
					pos[6] = center + float3(-1, 1, -1)*length;
					pos[7] = center + float3(-1, -1, -1)*length;



					g2f o;
					//三角ポリゴンは反時計回りに頂点を追加していくと面のむきが正しくなる(はず)
					o.normal = normalize(cross(pos[1] - pos[0], pos[2] - pos[0])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[3] - pos[2], pos[4] - pos[2])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[5] - pos[4], pos[6] - pos[4])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[7] - pos[6], pos[0] - pos[6])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[0] - pos[6], pos[4] - pos[6])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[7] - pos[1], pos[3] - pos[1])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

				}


				v = input[1].vertex;
				slide = random(v)*_Slide;

				center = v + slide;
				{
					pos[0] = center + float3(-1, 1, 1)*length;
					pos[1] = center + float3(-1, -1, 1)*length;
					pos[2] = center + float3(1, 1, 1)*length;
					pos[3] = center + float3(1, -1, 1)*length;
					pos[4] = center + float3(1, 1, -1)*length;
					pos[5] = center + float3(1, -1, -1)*length;
					pos[6] = center + float3(-1, 1, -1)*length;
					pos[7] = center + float3(-1, -1, -1)*length;



					g2f o;
					//三角ポリゴンは反時計回りに頂点を追加していくと面のむきが正しくなる(はず)
					o.normal = normalize(cross(pos[1] - pos[0], pos[2] - pos[0])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[3] - pos[2], pos[4] - pos[2])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[5] - pos[4], pos[6] - pos[4])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[7] - pos[6], pos[0] - pos[6])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[0] - pos[6], pos[4] - pos[6])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[7] - pos[1], pos[3] - pos[1])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

				}


				v = input[2].vertex;
				slide = random(v)*_Slide;

				center = v + slide;
				{
					pos[0] = center + float3(-1, 1, 1)*length;
					pos[1] = center + float3(-1, -1, 1)*length;
					pos[2] = center + float3(1, 1, 1)*length;
					pos[3] = center + float3(1, -1, 1)*length;
					pos[4] = center + float3(1, 1, -1)*length;
					pos[5] = center + float3(1, -1, -1)*length;
					pos[6] = center + float3(-1, 1, -1)*length;
					pos[7] = center + float3(-1, -1, -1)*length;



					g2f o;
					//三角ポリゴンは反時計回りに頂点を追加していくと面のむきが正しくなる(はず)
					o.normal = normalize(cross(pos[1] - pos[0], pos[2] - pos[0])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[3] - pos[2], pos[4] - pos[2])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[5] - pos[4], pos[6] - pos[4])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[7] - pos[6], pos[0] - pos[6])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[0] - pos[6], pos[4] - pos[6])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[6], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[0], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[4], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[2], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

					o.normal = normalize(cross(pos[7] - pos[1], pos[3] - pos[1])); //法線計算

					o.pos = UnityObjectToClipPos(float4(pos[1], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[7], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[3], 1));
					outStream.Append(o);		//頂点の出力
					o.pos = UnityObjectToClipPos(float4(pos[5], 1));
					outStream.Append(o);		//頂点の出力
					outStream.RestartStrip(); //ポリゴンの出力, 三角形1つ出力するごとに書く

				}


			}

			fixed4 frag(g2f i) : COLOR
			{
				fixed4 col;
				col = _Color;

				float3 N = normalize(i.normal);
				float3 L = -_WorldSpaceLightPos0;

				float light = dot(N, -L)*0.5 + 0.5;
				col *= float4(1, 1, 1, 1)*light;
				return col;
			}

			ENDCG
		}
	}
		FallBack "Diffuse"
}