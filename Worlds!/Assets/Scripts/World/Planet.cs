using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
	//info
	//string m_planet_name;

	//chunks
	//[HideInInspector] public int m_chunk_res { get; protected set; }
	//[HideInInspector] public float m_chunk_scale { get; protected set; }
	int m_chunkLength;
	List<GameObject> m_chunkObjs;
	List<PlanetChunk> m_chunks;

	public void Initalize(int chunkLength)
	{
		m_chunkLength = chunkLength;
		m_chunkObjs = new List<GameObject>(m_chunkLength * m_chunkLength * m_chunkLength);
		m_chunks = new List<PlanetChunk>(m_chunkLength * m_chunkLength * m_chunkLength);
	}
	
	public void AddChunk(GameObject chunk)
	{
		m_chunkObjs.Add(chunk);
		m_chunks.Add(chunk.GetComponent<PlanetChunk>());
	}

	public void AssignChunkNeighbours()
	{
		for(int z = 0, i = 0; z < m_chunkLength - 1; z++)
		{
			for(int y = 0; y < m_chunkLength - 1; y++)
			{
				for(int x = 0; x < m_chunkLength - 1; x++, i++)
				{
					m_chunks[i].AssignXChunk(m_chunks[(x + 1) + y * m_chunkLength + z * m_chunkLength * m_chunkLength]);
					m_chunks[i].AssignYChunk(m_chunks[x + (y + 1) * m_chunkLength + z * m_chunkLength * m_chunkLength]);
					m_chunks[i].AssignZChunk(m_chunks[x + y * m_chunkLength + (z + 1) * m_chunkLength * m_chunkLength]);
				}
			}
		}
	}
}
