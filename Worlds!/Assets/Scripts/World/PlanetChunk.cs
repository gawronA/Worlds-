using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralTerrain.MarchingCubes;

public class PlanetChunk : MonoBehaviour
{
	Planet m_planet;

	//info
	int m_id;

	int m_res;
	float m_scale;

	float[] m_densityMap;

	MeshFilter m_meshFilter;

	MarchingCubes mc;

	void Start ()
	{
		m_planet = transform.parent.GetComponent<Planet>();
		m_res = m_planet.m_chunk_res;
		m_scale = m_planet.m_chunk_scale;
		
		m_meshFilter = GetComponent<MeshFilter>();
		mc = new MarchingCubes();
		mc.Initalize(m_res, m_res, m_res, m_scale, 0);
	}
	
	void Update ()
	{
		
	}

	public void Initalize(int id, int res, float scale)
	{
		m_id = id;
		m_res = res;
		m_scale = scale;
	}
	public void SetDensityMap(float[] map)
	{
		m_densityMap = map;
	}

	public void Refresh()
	{
		Mesh mesh = mc.ComputeMesh(m_densityMap);
		mesh.name = m_planet.name + "_" + m_id.ToString();
		m_meshFilter.mesh.Clear();
	}

}
