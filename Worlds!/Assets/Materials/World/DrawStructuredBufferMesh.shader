// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Materials/World/DrawMeshShaderBetter" 
{
	SubShader 
	{
		Pass 
		{
			//Cull back

			CGPROGRAM

			#include "UnityCG.cginc"
			//#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			struct Vertex
			{
				float4 position;
				float3 normal;
			};

			uniform StructuredBuffer<Vertex> _MeshBuffer;
			
			struct a2v
			{
				float4 position : POSITION;
				float4 normal : NORMAL;
				uint id : SV_VertexID;
			};
			
			struct v2f 
			{
				float4 pixel_position : SV_POSITION;
				float3 color : COLOR;
			};

			v2f vert(a2v v)
			{
				v.position.xyz = _MeshBuffer[v.id].position.xyz;
				v.normal.xyz = _MeshBuffer[v.id].normal.xyz;

				v2f p;
				p.pixel_position = UnityObjectToClipPos(v.position);
				p.color = dot(float3(0,1,0), v.normal) * 0.5 + 0.5;
				
				return p;
			}

			float4 frag(v2f p) : COLOR
			{
				return float4(p.color,1);
			}

			ENDCG

		}
	}
}