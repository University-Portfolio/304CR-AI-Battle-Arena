Shader "Custom/Shield"
{
	Properties
	{
		_Color ("Colour", Color) = (1,1,1,1)
		_Frequency ("Frequency", Float) = 1
		_Width ("Width", Float) = 1
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
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

			fixed4 _Color;
			float _Frequency;
			float _Width;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = mul(UNITY_MATRIX_M, v.vertex).xy;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Calulcate check range
				float check = 1.0f - (((i.uv.y + _Time.w) % _Frequency) / _Frequency);

				// Create band value
				float band = lerp(check * 2, 1.0f - check * 2, check) * _Width;

				fixed4 col = _Color * band;
				return col;
			}
			ENDCG
		}
	}
}
