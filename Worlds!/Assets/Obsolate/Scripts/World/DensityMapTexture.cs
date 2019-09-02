using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityMapTexture : MonoBehaviour
{
	[Range(0, 16)] public int m_z = 0;
	public bool m_smooth = false;
	public bool m_Show_samples = false;

	Texture2D m_densityTexture;

	int m_densitySizex, m_densitySizey, m_densitySizez, m_lod;

	float[] m_densityMap;

	void Start()
	{
		m_densitySizex = transform.parent.GetComponent<WorldGenerator>().m_x_dim;
		m_densitySizey = transform.parent.GetComponent<WorldGenerator>().m_y_dim;
		m_densitySizez = transform.parent.GetComponent<WorldGenerator>().m_z_dim;
		m_lod = (int)Mathf.Pow(2, transform.parent.GetComponent<WorldGenerator>().m_LOD);
		m_densityMap = transform.parent.GetComponent<WorldGenerator>().m_densityMap;

		m_densityTexture = new Texture2D(m_densitySizex, m_densitySizey, TextureFormat.RGB24, false);
		m_densityTexture.wrapMode = TextureWrapMode.Clamp;
		m_densityTexture.filterMode = FilterMode.Point;
		GetComponent<MeshRenderer>().material.mainTexture = m_densityTexture;
	}
	
	void OnValidate()
	{
		for(int y = 0; y < m_densitySizey; y++)
		{
			for(int x = 0; x < m_densitySizex; x++)
			{
				if(m_smooth)
				{
					m_densityTexture.SetPixel(x, y, new Color(Mathf.Clamp01(m_densityMap[x + y * m_densitySizex +
															(int)Mathf.Clamp(m_z, 0f, (float)m_densitySizez - 1f) * m_densitySizex * m_densitySizey]),
															Mathf.Clamp01(-m_densityMap[x + y * m_densitySizex +
															(int)Mathf.Clamp(m_z, 0f, (float)m_densitySizez - 1f) * m_densitySizex * m_densitySizey]),
															1f));
				}
				else
				{
					m_densityTexture.SetPixel(x, y, new Color(Mathf.Ceil(Mathf.Clamp01(m_densityMap[x + y * m_densitySizex +
															(int)Mathf.Clamp(m_z, 0f, (float)m_densitySizez - 1f) * m_densitySizex * m_densitySizey])),
															Mathf.Ceil(Mathf.Clamp01(-m_densityMap[x + y * m_densitySizex +
															(int)Mathf.Clamp(m_z, 0f, (float)m_densitySizez - 1f) * m_densitySizex * m_densitySizey])),
															1f));
				}

				if(m_Show_samples && x % m_lod == 0 && y % m_lod == 0 && m_z % m_lod == 0)
				{
					m_densityTexture.SetPixel(x, y, m_densityTexture.GetPixel(x, y) * Color.yellow);
				}
				m_densityTexture.Apply();
			}
		}
	}
}
