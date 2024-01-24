// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Geometry/FlatShading"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Ambient("Ambient", Color) = (1,1,1,1)
		_TestColor("TestColor", Color) = (1,1,1,1)
		_Shininess("Shininess", Integer) = 32
		_SpecularStrength("Specular Strength", Float) = 1
		_MainTex("Albedo", 2D) = "white" {}
	}

		SubShader
	{

		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }

		Pass
		{
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			float4 _Color;
			sampler2D _MainTex;
			float4 _Ambient;
			float4 _TestColor;
			int _Shininess;
			float _SpecularStrength;

			fixed4 _LightColor0;

			struct v2g
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 vertex : TEXCOORD1;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal: TEXCOORD1;
				//float3 random: TEXCOORD2;
			};

			v2g vert(appdata_full v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
			{
				g2f o;

				// Compute the normal
				float3 vecA = IN[1].vertex - IN[0].vertex;
				float3 vecB = IN[2].vertex - IN[0].vertex;
				float3 normal = cross(vecA, vecB);
				o.normal = normalize(mul(normal, (float3x3) unity_WorldToObject));
				//o.random = sin(sin(19494949 * IN[0].pos.x + 19233434) * 89765 + cos(19494949 * IN[0].pos.y + 19233434) * 4343476356 + sin(756564534 * IN[0].pos.z + 676545) * 4545);

				// Compute barycentric uv
				o.uv = (IN[0].uv + IN[1].uv + IN[2].uv) / 3;

				for (int i = 0; i < 3; i++)
				{
					o.pos = IN[i].pos;
					triStream.Append(o);
				}
			}

			half4 frag(g2f i) : COLOR
			{
				//float3 lightColor = unity_LightColor[0].rgb;
				float3 lightColor = _LightColor0.rgb;
				float4 col = tex2D(_MainTex, i.uv);

				// Compute diffuse light
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuse = max(0, dot(i.normal, lightDir)) * lightColor;

				//specular
				float3 viewDir = normalize(WorldSpaceViewDir(i.pos));
				float3 reflectDir = reflect(-lightDir, i.normal);

				float spec = pow(max(dot(viewDir, reflectDir), 0.0), _Shininess);
				float3 specular = _SpecularStrength * spec * lightColor;

				//float t = clamp((i.pos.y+2) / -10.0, 0, 1.0);
				//float3 color = _Color + t * (_TestColor - _Color);
				col.rgb *= (_Ambient.rgb + diffuse + specular) * _Color;
				//col.rgb = lightColor; 
				return col;
			}

			ENDCG
		}
	}
		Fallback "Diffuse"
}