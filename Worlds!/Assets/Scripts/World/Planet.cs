using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
	//info
	string m_planet_name;

	//chunks
	[HideInInspector] public int m_chunk_res { get; protected set; }
	[HideInInspector] public float m_chunk_scale { get; protected set; }
	List<GameObject> m_chunks;

	void Initalize(string planet_name)
	{
		
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
}
