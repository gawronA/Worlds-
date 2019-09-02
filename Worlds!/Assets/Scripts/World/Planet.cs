using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{

	//chunks
	[HideInInspector] public int m_chunk_res { get; protected set; }
	[HideInInspector] public float m_chunk_scale { get; protected set; }
	List<PlanetChunk> m_chunks;

	void Start()
	{
		m_chunk_res = 2;
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
}
