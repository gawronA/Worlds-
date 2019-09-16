// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Materials/World/DrawMeshShader" 
{
	SubShader 
	{
		Pass 
		{
			Tags { "RenderType"="Opaque" }
			Cull off

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			struct Vert
			{
				float4 position;
				float3 normal;
			};

			uniform StructuredBuffer<Vert> _MeshBuffer;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float3 col : COLOR;
			};

			v2f vert(uint id : SV_VertexID)
			{
				Vert vert = _MeshBuffer[id];

				v2f OUT;
				OUT.pos = UnityObjectToClipPos(float4(vert.position.xyz, 1));
				//OUT.pos = float4(vert.position.xyz, 1);
				
				OUT.col = dot(float3(0,1,0), vert.normal) * 0.5 + 0.5;
				
				return OUT;
			}

			float4 frag(v2f IN) : COLOR
			{
				return float4(IN.col,1);
			}

			ENDCG

		}
	}
}