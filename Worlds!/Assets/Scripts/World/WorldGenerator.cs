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
		mc.m_clearBufferShader = m_FillBufferShader;
		mc.m_fillDensityBufferShader = m_FillDensityMapShader;
		mc.Initalize(m_x_dim, m_y_dim, m_z_dim, m_size);
		//mc.m_marchingCubesShader = m_MarchingCubesShader;
		//m_densityMap = new float[m_x_dim * m_y_dim * m_z_dim];
		m_densityMap = new float[m_x_dim * m_y_dim * m_z_dim];
		for(int i = 0; i < m_densityMap.Length; i++)
		{
			m_densityMap[i] = Random.value;
		}
	}
	
	void Update ()
	{
		//mc.Initalize(m_x_dim, m_y_dim, m_z_dim, m_size);
		mc.ComputeMesh(m_densityMap);
		//Mesh mesh = mc.ComputeMesh(m_densityMap);
	}
}
