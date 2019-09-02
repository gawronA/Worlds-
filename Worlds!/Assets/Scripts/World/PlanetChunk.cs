using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralTerrain.MarchingCubes;

public class PlanetChunk : MonoBehaviour
{
	int m_res;
	float m_scale;

	float[] m_densityMap;

	MeshRenderer m_meshRenderer;

	MarchingCubes mc;

	void Start ()
	{
		m_res = transform.parent.GetComponent<Planet>().m_chunk_res;
		m_scale = transform.parent.GetComponent<Planet>().m_chunk_scale;
		m_meshRenderer = GetComponent<MeshRenderer>();
		mc = new MarchingCubes();
		mc.Initalize(m_res, m_res, m_res, m_scale, 0);
	}
	
	void Update ()
	{
		
	}

	public void SetDensityMap(float[] map)
	{
		m_densityMap = map;
	}

	public void Refresh()
	{

	}
}
