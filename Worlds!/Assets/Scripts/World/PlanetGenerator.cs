using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
	public int m_radius;
	public int m_est_border;
	public float m_scale;
	public int m_xyzResolution = 16;

	public ComputeShader m_MarchingCubesShader;
	public ComputeShader m_FillBufferShader;
	public ComputeShader m_calculateNormalsShader;

	[HideInInspector] public int m_length;
	int m_length2;
	int m_border;
	int m_chunk_count;
	Vector3Int center;

	[HideInInspector] public float[] m_densityMap;

	void Awake()
	{
		int length = 2 * m_est_border + 2 * m_radius;
		m_chunk_count = Mathf.CeilToInt((length + 1f) / (m_xyzResolution - 1f));
		m_length = m_chunk_count * (m_xyzResolution - 1) + 1;
		m_length2 = m_length * m_length;

		center = new Vector3Int(m_length / 2, m_length / 2, m_length / 2);
		m_densityMap = new float[m_length2 * m_length];
		for(int z = 0; z < m_length; z++)
		{
			for(int y = 0; y < m_length; y++)
			{
				for(int x = 0; x < m_length; x++)
				{
					Vector3 point = center - new Vector3(x, y, z);
					m_densityMap[x + y * m_length + z * m_length2] = Mathf.Clamp((-1.0f / m_radius) * point.magnitude + 1, -1.0f, 1.0f);
				}
			}
		}

		Debug.Log("number of chunks" + m_chunk_count.ToString());
		Debug.Log("actual length" + m_length.ToString());
		Debug.Log("center" + center.ToString());
	}
	
	void Update ()
	{
		
	}
}
