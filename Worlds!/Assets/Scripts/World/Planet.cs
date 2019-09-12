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
		for(int z = 0, i = 0; z < chunkLength; z++)
		{
			for(int y = 0; y < chunkLength; y++)
			{
				for(int x = 0; x < chunkLength; x++, i++)
				{
					if(x != chunkLength - 1 && y != chunkLength - 1 && z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i + 1 + chunkLength + chunkLength2], 26); //+x+y+z                                                                                     
                    if(y != chunkLength - 1 && z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i + chunkLength + chunkLength2], 25);                                
                    if(x - 1 >= 0 && y != chunkLength - 1 && z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i - 1 + chunkLength + chunkLength2], 24);

                    if(x != chunkLength - 1 && y != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i + 1 + chunkLength], 23);
                    if(y != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i + chunkLength], 22);
                    if(x - 1 >= 0 && y != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i - 1 + chunkLength], 21);

                    if(x != chunkLength - 1 && y != chunkLength - 1 && z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i + 1 + chunkLength - chunkLength2], 20);
                    if(y != chunkLength - 1 && z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i + chunkLength - chunkLength2], 19);
                    if(x - 1 >= 0 && y != chunkLength - 1 && z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i - 1 + chunkLength - chunkLength2], 18);
                    
                    if(x != chunkLength - 1 && z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i + 1 + chunkLength2], 17);
                    if(z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i + chunkLength2], 16);
                    if(x - 1 >= 0 && z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i - 1 + chunkLength2], 15);

                    if(x != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i + 1], 14);
                    if(x - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i - 1], 12);

                    if(x != chunkLength - 1 && z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i + 1 - chunkLength2], 11);
                    if(z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i - chunkLength2], 10);
                    if(x - 1 >= 0 && z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i - 1 - chunkLength2], 9);

                    if(x != chunkLength - 1 && y - 1 >= 0 && z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i + 1 - chunkLength + chunkLength2], 8);
                    if(y - 1 >= 0 && z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i - chunkLength + chunkLength2], 7);
                    if(x - 1 >= 0 && y - 1 >= 0 && z != chunkLength - 1) m_chunks[i].AssignNeighbour(m_chunks[i - 1 - chunkLength + chunkLength2], 6);

                    if(x != chunkLength - 1 && y - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i + 1 - chunkLength], 5);
                    if(y - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i - chunkLength], 4);
                    if(x - 1 >= 0 && y - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i - 1 - chunkLength], 3);

                    if(x != chunkLength - 1 && y - 1 >= 0 && z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i + 1 - chunkLength - chunkLength2], 2);
                    if(y - 1 >= 0 && z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i - chunkLength - chunkLength2], 1);
                    if(x - 1 >= 0 && y - 1 >= 0 && z - 1 >= 0) m_chunks[i].AssignNeighbour(m_chunks[i - 1 - chunkLength - chunkLength2], 0);
                    m_chunks[i].RefreshCollider();
				}
			}
		}
	}
    
}
