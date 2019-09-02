using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityMapTexture : MonoBehaviour
{
	[Range(0, 128)] public int m_z = 0;
	public bool m_smooth = false;
	//public bool m_Show_samples = false;

	Texture2D m_densityTexture;

	int m_length;//, m_lod;

	float[] m_densityMap;

	void Start()
	{
		m_length = transform.parent.GetComponent<PlanetGenerator>().m_length;
		//m_lod = (int)Mathf.Pow(2, transform.parent.GetComponent<WorldGenerator>().m_LOD);
		m_densityMap = transform.parent.GetComponent<PlanetGenerator>().m_densityMap;

		m_densityTexture = new Texture2D(m_length, m_length, TextureFormat.RGB24, false);
		m_densityTexture.wrapMode = TextureWrapMode.Clamp;
		m_densityTexture.filterMode = FilterMode.Point;
		GetComponent<MeshRenderer>().material.mainTexture = m_densityTexture;
	}
	
	void OnValidate()
	{
		for(int y = 0; y < m_length; y++)
		{
			for(int x = 0; x < m_length; x++)
			{
				if(m_smooth)
				{
					m_densityTexture.SetPixel(x, y, new Color(Mathf.Clamp01(m_densityMap[x + y * m_length +
															(int)Mathf.Clamp(m_z, 0f, (float)m_length - 1f) * m_length * m_length]),
															Mathf.Clamp01(-m_densityMap[x + y * m_length +
															(int)Mathf.Clamp(m_z, 0f, (float)m_length - 1f) * m_length * m_length]),
															1f));
				}
				else
				{
					m_densityTexture.SetPixel(x, y, new Color(Mathf.Ceil(Mathf.Clamp01(m_densityMap[x + y * m_length +
															(int)Mathf.Clamp(m_z, 0f, (float)m_length - 1f) * m_length * m_length])),
															Mathf.Ceil(Mathf.Clamp01(-m_densityMap[x + y * m_length +
															(int)Mathf.Clamp(m_z, 0f, (float)m_length - 1f) * m_length * m_length])),
															1f));
				}

				/*if(m_Show_samples && x % m_lod == 0 && y % m_lod == 0 && m_z % m_lod == 0)
				{
					m_densityTexture.SetPixel(x, y, m_densityTexture.GetPixel(x, y) * Color.yellow);
				}*/
				m_densityTexture.Apply();
			}
		}
	}
}
