using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralTerrain.MarchingCubes;

public class WorldGenerator : MonoBehaviour
{
	public int m_x_dim = 8;
	public int m_y_dim = 8;
	public int m_z_dim = 8;
	public float m_size = 1.0f;
	public float m_radius = 5.0f;
	public int m_LOD = 0;
	public ComputeShader m_MarchingCubesShader;
	public ComputeShader m_FillBufferShader;
	public ComputeShader m_calculateNormalsShader;

	float[] m_densityMap;

	MarchingCubes mc;

	Mesh mesh;

	void Start ()
	{
		mesh = new Mesh();
		mc = new MarchingCubes();
		mc.m_marchingCubesShader = m_MarchingCubesShader;
		mc.m_clearVerticesShader = m_FillBufferShader;
		mc.m_calculateNormalsShader = m_calculateNormalsShader;
		mc.Initalize(m_x_dim, m_y_dim, m_z_dim, m_size, m_LOD);
		m_densityMap = new float[m_x_dim * m_y_dim * m_z_dim];
		//Random.InitState(20);
		//for(int i = 0; i < m_densityMap.Length; i++) m_densityMap[i] = Random.Range(-1f, 1f);
		//float radius = 7.0f;
		Vector3 center = new Vector3(m_x_dim / 2, m_y_dim / 2, m_z_dim / 2);
		for(int z = 0; z < m_z_dim; z++)
		{
			for(int y = 0; y < m_y_dim; y++)
			{
				for(int x = 0; x < m_x_dim; x++)
				{
					Vector3 point = center - new Vector3(x, y, z);
					m_densityMap[x + y * m_x_dim + z * m_x_dim * m_y_dim] = Mathf.Clamp((-1.0f / m_radius) * point.magnitude + 1, -1.0f, 1.0f);
				}
			}
		}
		/*
		for(int z = 0; z < m_z_dim; z++)
		{
			for(int y = 0; y < m_y_dim; y++)
			{
				for(int x = 0; x < m_x_dim; x++)
				{
					m_densityMap[x + y * m_x_dim + z * m_x_dim * m_y_dim] = -y / (m_y_dim / 2) + 1;
				}
			}
		}*/
	}
	
	void Update ()
	{
		if(Input.GetButton("Jump"))
		{
			/*for(int i = 0; i < m_densityMap.Length; i++)
			{
				m_densityMap[i] = Random.value * 2 - 1;
			}*/
			mesh = mc.ComputeMesh(m_densityMap);
			GetComponent<MeshFilter>().mesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
			DrawNormals(mesh);
		}
	}

	private void OnDestroy()
	{
		mc.Release();
	}

	void DrawNormals(Mesh mesh)
	{
		for(int i = 0; i < mesh.vertexCount; i++)
		{
			Debug.DrawLine(mesh.vertices[i], mesh.vertices[i] + mesh.normals[i], Color.green, 120.0f);
		}
	}
}
