using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralTerrain.MarchingCubes;

public class WorldGenerator : MonoBehaviour
{
	public int m_x_dim = 64;
	public int m_y_dim = 64;
	public int m_z_dim = 64;
	public float m_size = 1.0f;
	public ComputeShader m_MarchingCubesShader;

	float[] m_densityMap;

	MarchingCubes mc;

	void Start ()
	{
		mc = new MarchingCubes();
		mc.m_marchingCubesShader = m_MarchingCubesShader;
		mc.Initalize(m_x_dim, m_y_dim, m_z_dim, m_size);
		//mc.m_marchingCubesShader = m_MarchingCubesShader;
		m_densityMap = new float[m_x_dim * m_y_dim * m_z_dim];
	}
	
	void Update ()
	{
		//mc.Initalize(m_x_dim, m_y_dim, m_z_dim, m_size);
		mc.ComputeMesh(m_densityMap);
		//Mesh mesh = mc.ComputeMesh(m_densityMap);
	}
}
