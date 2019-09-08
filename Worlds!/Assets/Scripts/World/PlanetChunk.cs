using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralTerrain.MarchingCubes;

public unsafe class PlanetChunk : MonoBehaviour
{
	Planet m_planet;
	public bool showNormals = false;
	public bool refresh = false;
	public bool lod1 = false;
	
	//chunk info
	int m_id;
	int m_res;
	int m_res2;
	float m_scale;

	//Density map
	public float[] m_densityMap { get; private set; }
	private float[] m_xChunkMap, m_yChunkMap, m_zChunkMap, m_xyChunkMap, m_yzChunkMap, m_xzChunkMap, m_xyzChunkMap;

	//Components
	MeshFilter m_meshFilter;
	MeshCollider m_meshCollider;

	//Mesh generation
	MarchingCubes m_mc;
	public ComputeShader m_MarchingCubesShader;
	public ComputeShader m_ClearVerticesShader;
	public ComputeShader m_CalculateNormalsShader;

	void Start ()
	{
		
	}
	
	void Update ()
	{
		if(refresh)
		{
			//refresh = false;
			Refresh();
		}
		if(lod1)
		{
			m_mc.Release();
			m_mc.Initalize(m_res, m_res, m_res, m_scale, 1);
			Refresh();
		}
	}

	private void OnValidate()
	{
		if(showNormals) DrawNormals();
	}

	private void OnDestroy()
	{
		m_mc.Release();
	}

	public void Initalize(int id, int res, float scale, bool sharpEdges)
	{
		m_id = id;
		m_res = res;
		m_res2 = m_res * m_res;
		m_scale = scale;

		m_meshFilter = GetComponent<MeshFilter>();
		m_meshCollider = GetComponent<MeshCollider>();

		m_mc = new MarchingCubes
		{
			m_marchingCubesShader = m_MarchingCubesShader,
			m_clearVerticesShader = m_ClearVerticesShader,
			m_calculateNormalsShader = m_CalculateNormalsShader,
			m_recalculateNormals = sharpEdges
		};
		m_mc.Initalize(m_res, m_res, m_res, m_scale, 0);
	}

	public void SetDensityMap(float[] map)
	{
		m_densityMap = map;
	}

	public void Refresh()
	{
		float[] borderMapx = new float[m_res2];
		float[] borderMapy = new float[m_res2];
		float[] borderMapz = new float[m_res2];
		float[] borderMapxy = new float[m_res];
		float[] borderMapyz = new float[m_res];
		float[] borderMapxz = new float[m_res];
		float	borderMapxyz = m_xyzChunkMap[0];
		for(int b = 0, i = 0; b < m_res; b++)
		{
			for(int a = 0; a < m_res; a++, i++)
			{
				borderMapx[i] = m_xChunkMap[/*1 + */a * m_res + b * m_res2];
				borderMapy[i] = m_yChunkMap[/*m_res + */a + b * m_res2];
				borderMapz[i] = m_zChunkMap[/*m_res2 + */a + b * m_res];
			}
			borderMapxy[b] = m_xyChunkMap[b * m_res2];
			borderMapyz[b] = m_yzChunkMap[b];
			borderMapxz[b] = m_xzChunkMap[b * m_res];
		}
		Mesh mesh = m_mc.ComputeMesh(m_densityMap, borderMapx, borderMapy, borderMapz, borderMapxy, borderMapyz, borderMapxz, borderMapxyz);
		mesh.name = name + "_" + m_id.ToString();

		m_meshFilter.mesh.Clear();
		m_meshFilter.mesh = mesh;

		m_meshCollider.sharedMesh = mesh;
	}

	public void AssignXChunkMap(float[] x_chunkMap)
	{
		m_xChunkMap = x_chunkMap;
	}
	
	public void AssignYChunkMap(float[] y_chunkMap)
	{
		m_yChunkMap = y_chunkMap;
	}

	public void AssignZChunkMap(float[] z_chunkMap)
	{
		m_zChunkMap = z_chunkMap;
	}

	public void AssignXYChunkMap(float[] xy_chunkMap)
	{
		m_xyChunkMap = xy_chunkMap;
	}

	public void AssignYZChunkMap(float[] yz_chunkMap)
	{
		m_yzChunkMap = yz_chunkMap;
	}

	public void AssignXZChunkMap(float[] xz_chunkMap)
	{
		m_xzChunkMap = xz_chunkMap;
	}

	public void AssignXYZChunkMap(float[] xyz_chunkMap)
	{
		m_xyzChunkMap = xyz_chunkMap;
	}

	void DrawNormals()
	{
		for(int i = 0; i < m_meshFilter.mesh.vertexCount; i++)
		{
			Debug.DrawLine(transform.TransformPoint(m_meshFilter.mesh.vertices[i]), transform.TransformPoint(m_meshFilter.mesh.vertices[i] + m_meshFilter.mesh.normals[i]), Color.green, 120f);
		}
	}
}
