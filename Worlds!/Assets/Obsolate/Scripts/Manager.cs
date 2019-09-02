using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
	public Transform prefab;
	public int m_planetsCount;

	void Start ()
	{
		CreatePlanets(m_planetsCount);
	}
	
	public void CreatePlanets(int planetsCount)
	{
		int planetIndex = 0;
		for(int i=0; i< planetsCount; i++)
		{
			Transform tmp;
			tmp = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
			tmp.name = "EarthPlanet" + (planetIndex++).ToString();
			tmp.transform.localScale *= 100;
		}
	}
}
