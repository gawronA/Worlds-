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
	public ComputeShader m_MarchingCubesShader;
	public ComputeShader m_FillBufferShader;
	public ComputeShader m_FillDensityMapShader;

	float[] m_densityMap;

	MarchingCubes mc;

	void Start ()
	{
		mc = new MarchingCubes();
		mc.m_marchingCubesShader = m_MarchingCubesShader;
		mc.m_clearVerticesShader = m_FillBufferShader;
		//mc.m_fillDensityBufferShader = m_FillDensityMapShader;
		mc.Initalize(m_x_dim, m_y_dim, m_z_dim, m_size);
		m_densityMap = new float[m_x_dim * m_y_dim * m_z_dim];
		
	}
	
	void Update ()
	{
		if(Input.GetButton("Jump"))
		{
			for(int i = 0; i < m_densityMap.Length; i++)
			{
				m_densityMap[i] = Random.value * 2 - 1;
			}
			Mesh mesh = mc.ComputeMesh(m_densityMap);
			GetComponent<MeshFilter>().mesh = mesh;
			GetComponent<MeshCollider>().sharedMesh = mesh;
		}
	}

	private void OnDestroy()
	{
		mc.Release();
	}
}
