using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
	public int m_radius;
	public int m_est_border;
	public float m_scale;
	public int m_xyzResolution = 16;
	public bool m_sharpEdges = false;

	public GameObject m_planetPrefab;
	public GameObject m_planetChunkPrefab;

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
		/*for(int z = 0; z < m_length; z++)
		{
			for(int y = 0; y < m_length; y++)
			{
				for(int x = 0; x < m_length; x++)
				{
					m_densityMap[x + y * m_length + z * m_length2] = x + y * m_length + z * m_length2;
				}
			}
		}*/

		InstantiatePlanet();

		Debug.Log("number of chunks in one direction" + m_chunk_count.ToString());
		Debug.Log("actual length" + m_length.ToString());
		Debug.Log("center" + center.ToString());
	}
	
	void Update ()
	{
		
	}

	void InstantiatePlanet()
	{
		GameObject planet = Instantiate(m_planetPrefab);
		planet.transform.position = Vector3.zero;
		planet.name = "Planet1";
		for(int c_z = 0, id = 0; c_z < m_chunk_count; c_z++)
		{
			for(int c_y = 0; c_y < m_chunk_count; c_y++)
			{
				for(int c_x = 0; c_x < m_chunk_count; c_x++, id++)
				{
					int offset = c_x * (m_xyzResolution - 1) + c_y * (m_xyzResolution - 1) * m_length + c_z * (m_xyzResolution - 1) * m_length2;
					
					GameObject chunkObj = Instantiate(m_planetChunkPrefab, planet.transform, false);
					chunkObj.name = planet.name + "_chunk" + id.ToString();
					PlanetChunk chunk = chunkObj.GetComponent<PlanetChunk>();
					chunk.Initalize(id, m_xyzResolution, m_scale, m_sharpEdges);

					float[] densityMap = new float[m_xyzResolution * m_xyzResolution * m_xyzResolution];
					for(int z = 0; z < m_xyzResolution; z++)
					{
						for(int y = 0; y < m_xyzResolution; y++)
						{
							for(int x = 0; x < m_xyzResolution; x++)
							{
								densityMap[x + y * m_xyzResolution + z * m_xyzResolution * m_xyzResolution] =
									m_densityMap[offset + x + y * m_length + z * m_length2];
							}
						}
					}

					chunk.SetDensityMap(densityMap);
					chunk.Refresh();
				}
			}
		}
	}
}
