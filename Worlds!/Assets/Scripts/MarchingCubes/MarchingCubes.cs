using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
	public class MarchingCubes : MonoBehaviour
	{
		public int m_x_dimension = 64;
		public int m_y_dimension = 64;
		public int m_z_dimension = 64;
		public ComputeShader m_marchingCubesShader;
		public float m_offset = 0.0f;

		float[] m_densityMap;
		int m_maxVertices;

		ComputeBuffer m_cubeEdgeFlags, m_traingleConnectionTable;
		ComputeBuffer m_meshBuffer, m_densityBuffer;

		MeshFilter m_meshFilter;
		MeshRenderer m_meshRenderer;

		int m_case = 0;

		struct Vertex
		{
			public Vector4 position;
			public Vector3 normal;
		};

		void Start()
		{
			//each dimension must be divisable by 8
			//if(m_x_dimension % 8 != 0 || m_y_dimension % 8 != 0 || m_z_dimension % 8 != 0) throw new System.ArgumentException("N must be divisible be 8");
			m_densityMap = new float[m_x_dimension * m_y_dimension * m_z_dimension];
			m_maxVertices = (m_x_dimension - 1) * (m_y_dimension - 1) * (m_z_dimension - 1) * 5 * 3; //3 voxels * 5 tris
			for(int z = 0; z < m_x_dimension; z++)
			{
				for(int y = 0; y < m_y_dimension; y++)
				{
					for(int x = 0; x < m_x_dimension; x++)
					{
						m_densityMap[x + y * m_x_dimension + z * m_x_dimension * m_y_dimension] = -1.0f;
						//if(x == 0) m_densityMap[x + y * m_x_dimension + z * m_x_dimension * m_y_dimension] = 1.0f;
						//else m_densityMap[x + y * m_x_dimension + z * m_x_dimension * m_y_dimension] = 1.0f - (float)(x + 1) / m_x_dimension;
					}
				}
			}
			m_densityMap[0] = 1f;
			m_densityMap[3] = 1f;
			m_densityMap[6] = 1f;
			m_densityBuffer = new ComputeBuffer(m_x_dimension * m_y_dimension * m_z_dimension, sizeof(float));
			m_densityBuffer.SetData(m_densityMap);

			m_meshFilter = GetComponent<MeshFilter>();
			m_meshRenderer = GetComponent<MeshRenderer>();

			m_cubeEdgeFlags = new ComputeBuffer(256, sizeof(int));
			m_cubeEdgeFlags.SetData(MarchingCubesTables.CubeEdgeFlags);
			m_traingleConnectionTable = new ComputeBuffer(256 * 16, sizeof(int));
			m_traingleConnectionTable.SetData(MarchingCubesTables.TriangleConnectionTable);

			float[] val = new float[m_maxVertices * 7];	//float4 + float3
			for(int i = 0; i < m_maxVertices * 7; i++) val[i] = -1.0f;
			m_meshBuffer = new ComputeBuffer(m_maxVertices, sizeof(float) * 7);
			m_meshBuffer.SetData(val);


			m_marchingCubesShader.SetInt("_DensityMap_sizex", m_x_dimension);
			m_marchingCubesShader.SetInt("_DensityMap_sizey", m_y_dimension);
			m_marchingCubesShader.SetInt("_DensityMap_sizez", m_z_dimension);
			m_marchingCubesShader.SetFloat("_DensityOffset", m_offset);
			m_marchingCubesShader.SetFloat("_Size", 1.0f);
			m_marchingCubesShader.SetBuffer(0, "_CubeEdgeFlags", m_cubeEdgeFlags);
			m_marchingCubesShader.SetBuffer(0, "_TriangleConnectionTable", m_traingleConnectionTable);
			m_marchingCubesShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
			m_marchingCubesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
			m_marchingCubesShader.Dispatch(0, m_x_dimension / 2, m_y_dimension / 2, m_z_dimension / 2);

			Vertex[] verts = new Vertex[m_maxVertices];
			m_meshBuffer.GetData(verts);

			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			Mesh mesh = new Mesh();
			for(int i = 0; i < verts.Length && verts[i].position.w != -1.0f; i++)
			{
				vertices.Add(new Vector3(verts[i].position.x, verts[i].position.y, verts[i].position.z));
				triangles.Add(i);
			}
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles.ToArray(), 0);
			mesh.RecalculateNormals();
			m_meshFilter.mesh = mesh;

			Debug.Log("Case: " + m_case.ToString());
		}

		void Update()
		{
			if(Input.GetButtonDown("Jump"))
			{
				m_case++;
				if(m_case > 255) m_case = 0;
				SetDensityMap(m_case, ref m_densityMap);
				Triangulate(m_densityMap);
				Debug.Log("Case: " + m_case.ToString());
			}
		}

		void SetDensityMap(int _case, ref float[] densityMap)
		{
			for(int i = 0; i < 8; i++)
			{
				densityMap[i] = ((_case & 1 << i) > 0) ? 0.5f : -0.5f;
			}
		}

		void Triangulate(float[] densityMap)
		{
			m_meshFilter.mesh.Clear();
			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			Mesh mesh = new Mesh();

			m_densityBuffer.SetData(densityMap);

			float[] val = new float[m_maxVertices * 7]; //float4 + float3
			for(int i = 0; i < m_maxVertices * 7; i++) val[i] = -1.0f;
			m_meshBuffer = new ComputeBuffer(m_maxVertices, sizeof(float) * 7);
			m_meshBuffer.SetData(val);

			m_marchingCubesShader.SetBuffer(0, "_DensityMap", m_densityBuffer);
			m_marchingCubesShader.SetBuffer(0, "_Vertices", m_meshBuffer);
			m_marchingCubesShader.Dispatch(0, m_x_dimension / 2, m_y_dimension / 2, m_z_dimension / 2);

			Vertex[] verts = new Vertex[m_maxVertices];
			m_meshBuffer.GetData(verts);

			for(int i = 0; i < verts.Length && verts[i].position.w != -1.0f; i++)
			{
				vertices.Add(new Vector3(verts[i].position.x, verts[i].position.y, verts[i].position.z));
				triangles.Add(i);
			}
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles.ToArray(), 0);
			mesh.RecalculateNormals();
			m_meshFilter.mesh = mesh;
		}
	}

}
