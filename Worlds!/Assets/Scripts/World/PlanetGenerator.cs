using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
	public int m_radius;
	public int m_est_border;
	public float m_scale;
	public int m_xyzResolution = 16;
	public bool m_shaded = false;
    public Vector3 m_position;
    public float m_lod1;
    public float m_lod2;
    public float m_lod3;
    public Material m_planetMaterial;

	public GameObject m_planetPrefab;
	public GameObject m_planetChunkPrefab;

	public /*only for debug*/int m_length;
	int m_length2;
	int m_chunk_count;

    //some density-local-system crucial points
    Vector3 m_planet_center;

	[HideInInspector] public float[] m_densityMap;


    void Awake()
	{
		int length = 2 * m_est_border + 2 * m_radius;
		m_chunk_count = Mathf.CeilToInt((float)length / m_xyzResolution);
		m_length = m_chunk_count * m_xyzResolution;
		m_length2 = m_length * m_length;

        m_planet_center = Vector3.one * (m_xyzResolution * m_chunk_count) / 2;

        Vector3Int center = new Vector3Int(m_length / 2, m_length / 2, m_length / 2);
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

        m_planetMaterial.SetVector("_Offset", new Vector4(m_xyzResolution / 2, m_xyzResolution / 2, m_xyzResolution / 2, 0f));
        m_planetMaterial.SetFloat("_Scale", m_scale);
        InstantiatePlanet();
	}
	
	void Update ()
	{
		
	}

	void InstantiatePlanet()
	{
		GameObject planetObj = Instantiate(m_planetPrefab);
        planetObj.transform.position = m_position;
		planetObj.name = "Planet1";

		Planet planet = planetObj.GetComponent<Planet>();
		planet.Initalize(m_chunk_count, m_xyzResolution);

        Vector3 chunk_offset = Vector3.one * (m_xyzResolution * m_chunk_count / -2f + 0.5f * m_xyzResolution);

        for(int c_z = 0, id = 0; c_z < m_chunk_count; c_z++)
		{
			for(int c_y = 0; c_y < m_chunk_count; c_y++)
			{
				for(int c_x = 0; c_x < m_chunk_count; c_x++, id++)
				{
					int map_offset = c_x * m_xyzResolution + c_y * m_xyzResolution * m_length + c_z * m_xyzResolution * m_length2;

					GameObject chunkObj = Instantiate(m_planetChunkPrefab, planetObj.transform, false);
					chunkObj.name = planetObj.name + "_chunk" + id.ToString();
					chunkObj.transform.position = planetObj.transform.position + (chunk_offset + new Vector3(c_x * m_xyzResolution, c_y * m_xyzResolution, c_z * m_xyzResolution)) * m_scale;
					planet.AddChunk(chunkObj);

					PlanetChunk chunk = chunkObj.GetComponent<PlanetChunk>();
                    chunk.m_meshMaterial = m_planetMaterial;
                    chunk.m_lod1Distance = m_lod1;
                    chunk.m_lod2Distance = m_lod2;
                    chunk.m_lod3Distance = m_lod3;
                    chunk.Initalize(id, m_xyzResolution, m_scale, m_shaded, planetObj);

                    SetGeoCoords(chunk, c_x, c_y, c_z);

                    float[] densityMap = new float[m_xyzResolution * m_xyzResolution * m_xyzResolution];
					for(int z = 0; z < m_xyzResolution; z++)
					{
						for(int y = 0; y < m_xyzResolution; y++)
						{
							for(int x = 0; x < m_xyzResolution; x++)
							{
								densityMap[x + y * m_xyzResolution + z * m_xyzResolution * m_xyzResolution] =
									m_densityMap[map_offset + x + y * m_length + z * m_length2];
							}
						}
					}
					chunk.SetDensityMap(densityMap);
                }
			}
		}
		planet.AssignChunkNeighboursAndRefresh();
	}

    void SetGeoCoords(PlanetChunk chunk, int c_x, int c_y, int c_z)
    {
        chunk.m_geo[0].Point = new Vector3(c_x * m_xyzResolution, c_y * m_xyzResolution, c_z * m_xyzResolution) - m_planet_center;
        chunk.m_geo[1].Point = new Vector3((c_x + 1)*m_xyzResolution, c_y * m_xyzResolution, c_z * m_xyzResolution) - m_planet_center;
        chunk.m_geo[2].Point = new Vector3(c_x * m_xyzResolution, (c_y + 1) * m_xyzResolution, c_z * m_xyzResolution) - m_planet_center;
        chunk.m_geo[3].Point = new Vector3((c_x + 1) * m_xyzResolution, (c_y + 1) * m_xyzResolution, c_z * m_xyzResolution) - m_planet_center;
        chunk.m_geo[4].Point = new Vector3(c_x * m_xyzResolution, c_y * m_xyzResolution, (c_z + 1) * m_xyzResolution) - m_planet_center;
        chunk.m_geo[5].Point = new Vector3((c_x + 1) * m_xyzResolution, c_y * m_xyzResolution, (c_z + 1) * m_xyzResolution) - m_planet_center;
        chunk.m_geo[6].Point = new Vector3(c_x * m_xyzResolution, (c_y + 1) * m_xyzResolution, (c_z + 1) * m_xyzResolution) - m_planet_center;
        chunk.m_geo[7].Point = new Vector3((c_x + 1) * m_xyzResolution, (c_y + 1) * m_xyzResolution, (c_z + 1) * m_xyzResolution) - m_planet_center;
        /*chunk.m_geo[0].LambdaDeg = Vector3.SignedAngle(Vector3.back, new Vector3(c_x * m_xyzResolution, 0, c_z * m_xyzResolution) - m_lambda_center, Vector3.down);
        chunk.m_geo[0].PhiDeg = Vector3.SignedAngle(Vector3.back, new Vector3(0, c_y * m_xyzResolution, c_z * m_xyzResolution) - m_phi_center, Vector3.right);
        chunk.m_geo[0].Radius = Vector3.Distance(m_planet_center, new Vector3(c_x * m_xyzResolution, c_y * m_xyzResolution, c_z * m_xyzResolution));

        chunk.m_geo[1].LambdaDeg = Vector3.SignedAngle(Vector3.back, new Vector3((c_x + 1) * m_xyzResolution, 0, c_z * m_xyzResolution) - m_lambda_center, Vector3.down);
        chunk.m_geo[1].PhiDeg = Vector3.SignedAngle(Vector3.back, new Vector3(0, c_y * m_xyzResolution, c_z * m_xyzResolution) - m_phi_center, Vector3.right);
        chunk.m_geo[1].Radius = Vector3.Distance(m_planet_center, new Vector3((c_x + 1) * m_xyzResolution, c_y * m_xyzResolution, c_z * m_xyzResolution));
        chunk.m_geo[2].LambdaDeg = Vector3.SignedAngle(Vector3.back, new Vector3(c_x * m_xyzResolution, 0, c_z * m_xyzResolution) - m_lambda_center, Vector3.down);
        chunk.m_geo[2].PhiDeg = Vector3.SignedAngle(Vector3.back, new Vector3(0, (c_y + 1) * m_xyzResolution, c_z * m_xyzResolution) - m_phi_center, Vector3.right);
        chunk.m_geo[2].Radius = Vector3.Distance(m_planet_center, new Vector3(c_x * m_xyzResolution, (c_y + 1) * m_xyzResolution, c_z * m_xyzResolution));
        chunk.m_geo[3].LambdaDeg = Vector3.SignedAngle(Vector3.back, new Vector3((c_x + 1) * m_xyzResolution, 0, c_z * m_xyzResolution) - m_lambda_center, Vector3.down);
        chunk.m_geo[3].PhiDeg = Vector3.SignedAngle(Vector3.back, new Vector3(0, (c_y + 1) * m_xyzResolution, c_z * m_xyzResolution) - m_phi_center, Vector3.right);
        chunk.m_geo[3].Radius = Vector3.Distance(m_planet_center, new Vector3((c_x + 1) * m_xyzResolution, (c_y + 1) * m_xyzResolution, c_z * m_xyzResolution));
        chunk.m_geo[4].LambdaDeg = Vector3.SignedAngle(Vector3.back, new Vector3(c_x * m_xyzResolution, 0, (c_z + 1) * m_xyzResolution) - m_lambda_center, Vector3.down);
        chunk.m_geo[4].PhiDeg = Vector3.SignedAngle(Vector3.back, new Vector3(0, c_y * m_xyzResolution, (c_z + 1) * m_xyzResolution) - m_phi_center, Vector3.right);
        chunk.m_geo[4].Radius = Vector3.Distance(m_planet_center, new Vector3(c_x * m_xyzResolution, c_y * m_xyzResolution, (c_z + 1) * m_xyzResolution));
        chunk.m_geo[5].LambdaDeg = Vector3.SignedAngle(Vector3.back, new Vector3((c_x + 1) * m_xyzResolution, 0, (c_z + 1) * m_xyzResolution) - m_lambda_center, Vector3.down);
        chunk.m_geo[5].PhiDeg = Vector3.SignedAngle(Vector3.back, new Vector3(0, c_y * m_xyzResolution, (c_z + 1) * m_xyzResolution) - m_phi_center, Vector3.right);
        chunk.m_geo[5].Radius = Vector3.Distance(m_planet_center, new Vector3((c_x + 1) * m_xyzResolution, c_y * m_xyzResolution, (c_z + 1) * m_xyzResolution));
        chunk.m_geo[6].LambdaDeg = Vector3.SignedAngle(Vector3.back, new Vector3(c_x * m_xyzResolution, 0, (c_z + 1) * m_xyzResolution) - m_lambda_center, Vector3.down);
        chunk.m_geo[6].PhiDeg = Vector3.SignedAngle(Vector3.back, new Vector3(0, (c_y + 1) * m_xyzResolution, (c_z + 1) * m_xyzResolution) - m_phi_center, Vector3.right);
        chunk.m_geo[6].Radius = Vector3.Distance(m_planet_center, new Vector3(c_x * m_xyzResolution, (c_y + 1) * m_xyzResolution, (c_z + 1) * m_xyzResolution));
        chunk.m_geo[7].LambdaDeg = Vector3.SignedAngle(Vector3.back, new Vector3((c_x + 1) * m_xyzResolution, 0, (c_z + 1) * m_xyzResolution) - m_lambda_center, Vector3.down);
        chunk.m_geo[7].PhiDeg = Vector3.SignedAngle(Vector3.back, new Vector3(0, (c_y + 1) * m_xyzResolution, (c_z + 1) * m_xyzResolution) - m_phi_center, Vector3.right);
        chunk.m_geo[7].Radius = Vector3.Distance(m_planet_center, new Vector3((c_x + 1) * m_xyzResolution, (c_y + 1) * m_xyzResolution, (c_z + 1) * m_xyzResolution));*/
    }
}
