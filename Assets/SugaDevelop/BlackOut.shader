Shader "Custom/BlackOut"
{
    Properties
    {
		_Parameter("Parameter",Range(0,1))=0.5
		_StartTime("StartTime",Float)=0
    }
    SubShader
    {
		Tags {
		"Queue" = "Transparent"
		"RenderType" = "Opaque" }
        LOD 100
		GrabPass{}

        Pass
        {
			Cull Front
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
				float4 grabPos :TEXCOOD0;
                float4 vertex : SV_POSITION;
            };

			sampler2D _GrabTexture;
			float _Parameter;
			float _StartTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = tex2D(_GrabTexture, i.grabPos);
				fixed mono = (col.x + col.y + col.z) / 3 ;
				fixed4 monocol = fixed4(mono, mono, mono, 1);
				//float parameter = (_Time - _StartTime)*10;
				float parameter = _Parameter;
				col = lerp(col, monocol, (parameter>1)?1:parameter);
                return col;
            }
            ENDCG
        }
    }
}
