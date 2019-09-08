using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
	//info

	//chunks
	int m_chunkLength;
	int m_chunkRes;
	List<GameObject> m_chunkObjs;
	List<PlanetChunk> m_chunks;

	public void Initalize(int chunkLength, int chunkRes)
	{
		m_chunkLength = chunkLength;
		m_chunkRes = chunkRes;
		m_chunkObjs = new List<GameObject>(m_chunkLength * m_chunkLength * m_chunkLength);
		m_chunks = new List<PlanetChunk>(m_chunkLength * m_chunkLength * m_chunkLength);
	}
	
	public void AddChunk(GameObject chunk)
	{
		m_chunkObjs.Add(chunk);
		m_chunks.Add(chunk.GetComponent<PlanetChunk>());
	}

	public void AssignChunkNeighboursAndRefresh()
	{
		int chunkLength = m_chunkLength;
		int chunkLength2 = m_chunkLength * m_chunkLength;
		float[] dummyMap = new float[m_chunkRes * m_chunkRes * m_chunkRes];
		for(int z = 0, i = 0; z < chunkLength; z++)
		{
			for(int y = 0; y < chunkLength; y++)
			{
				for(int x = 0; x < chunkLength; x++, i++)
				{
					if(x != chunkLength - 1 && y != chunkLength - 1 && z != chunkLength - 1) m_chunks[i].AssignXYZChunkMap(m_chunks[i + 1 + chunkLength + chunkLength2].m_densityMap);
					else m_chunks[i].AssignXYZChunkMap(dummyMap);
					if(x != chunkLength - 1 && z != chunkLength - 1) m_chunks[i].AssignXZChunkMap(m_chunks[i + 1 + chunkLength2].m_densityMap);
					else m_chunks[i].AssignXZChunkMap(dummyMap);
					if(y != chunkLength - 1 && z != chunkLength - 1) m_chunks[i].AssignYZChunkMap(m_chunks[i + chunkLength + chunkLength2].m_densityMap);
					else m_chunks[i].AssignYZChunkMap(dummyMap);
					if(x != chunkLength - 1 && y != chunkLength - 1) m_chunks[i].AssignXYChunkMap(m_chunks[i + 1 + chunkLength].m_densityMap);
					else m_chunks[i].AssignXYChunkMap(dummyMap);
					if(x != chunkLength - 1) m_chunks[i].AssignXChunkMap(m_chunks[i + 1].m_densityMap);
					else m_chunks[i].AssignXChunkMap(dummyMap);
					if(y != chunkLength - 1) m_chunks[i].AssignYChunkMap(m_chunks[i + chunkLength].m_densityMap);
					else m_chunks[i].AssignYChunkMap(dummyMap);
					if(z != chunkLength - 1) m_chunks[i].AssignZChunkMap(m_chunks[i + chunkLength2].m_densityMap);
					else m_chunks[i].AssignZChunkMap(dummyMap);
					m_chunks[i].Refresh();
				}
			}
		}
	}
}
