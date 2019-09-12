using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using ProceduralTerrain.MarchingCubes;


public unsafe class PlanetChunk : MonoBehaviour
{
	
	public bool showNormals = false;
	public bool refresh = true;
    public bool continousRefresh = false;
	
	//chunk info
	int m_id;
	int m_res;
	int m_res2;
	float m_scale;
    Vector3 m_center;

    //Environment
    private Planet m_planet;
    private PlanetChunk[] m_neighbourChunks;
    private Transform m_player;

    //Density map
    public float[] m_densityMap { get; private set; }
    

	//Components
	MeshFilter m_meshFilter;
	MeshCollider m_meshCollider;

    //Mesh generation
    public float m_lod1Distance = 100f, m_lod2Distance = 500f, m_lod3Distance = 1000f;
    private MarchingCubes m_mc;
    private float[] m_borderMapx, m_borderMapy, m_borderMapz, m_borderMapxy, m_borderMapyz, m_borderMapxz, m_borderMapxyz;
	public ComputeShader m_MarchingCubesShader;
	public ComputeShader m_ClearVerticesShader;
	public ComputeShader m_CalculateNormalsShader;
    public Material m_DrawMeshShader;
    private int m_lod;
    //private bool m_needRefresh;

    //Jobs
    //NativeArray<Vector3> n_meshVertices;
    //NativeArray<Vector3> n_mesh
    private void Start()
	{
        m_player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
	{
		/*if(refresh || continousRefresh)
		{
			refresh = false;
			RefreshMeshAndCollider(lod);
        }*/

        float playerDistance = Vector3.Distance(m_player.position, m_center);
        if(playerDistance > m_lod3Distance && m_lod != 3) { RefreshMesh(3); Debug.Log("LOD" + m_lod.ToString()); }
        else if(playerDistance <= m_lod3Distance && playerDistance > m_lod2Distance && m_lod != 2) { RefreshMesh(2); Debug.Log("LOD" + m_lod.ToString()); }
        else if(playerDistance <= m_lod2Distance && playerDistance > m_lod1Distance && m_lod != 1) { RefreshMesh(1); Debug.Log("LOD" + m_lod.ToString()); }
        else if(playerDistance <= m_lod1Distance && m_lod != 0) { RefreshMesh(0); Debug.Log("LOD" + m_lod.ToString()); }
        
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
        m_center = transform.TransformPoint(new Vector3((float)m_res / 2, (float)m_res / 2, (float)m_res / 2));

        m_neighbourChunks = new PlanetChunk[27];

		m_meshFilter = GetComponent<MeshFilter>();
		m_meshCollider = GetComponent<MeshCollider>();

        m_mc = new MarchingCubes
        {
            m_marchingCubesShader = m_MarchingCubesShader,
            m_clearVerticesShader = m_ClearVerticesShader,
            m_calculateNormalsShader = m_CalculateNormalsShader,
            m_drawMesh = m_DrawMeshShader,
			m_recalculateNormals = sharpEdges
		};
		//m_mc.Initalize(m_res, m_res, m_res, m_scale, m_lod);

        m_borderMapx = new float[2 * m_res2];
        m_borderMapy = new float[2 * m_res2];
        m_borderMapz = new float[2 * m_res2];
        m_borderMapxy = new float[3 * m_res];
        m_borderMapyz = new float[3 * m_res];
        m_borderMapxz = new float[3 * m_res];
        m_borderMapxyz = new float[4];

        //RefreshCollider();
    }

	public void SetDensityMap(float[] map)
	{
		m_densityMap = map;
	}

	public void RefreshMeshAndCollider(int lod)
	{
        Mesh mesh;
        CopyBorderMaps();

        if(m_lod != lod)
        {
            m_lod = lod;
            m_mc.Release();
            m_mc.Initalize(m_res, m_res, m_res, m_scale, m_lod);
        }

        mesh = m_mc.ComputeMesh(m_densityMap, m_borderMapx, m_borderMapy, m_borderMapz, m_borderMapxy, m_borderMapyz, m_borderMapxz, m_borderMapxyz);
        mesh.name = name + "_" + m_id.ToString() + "_" + m_lod.ToString();
        m_meshFilter.mesh.Clear();
        m_meshFilter.mesh = mesh;

        if(lod != 0)
        {
            m_lod = 0;
            m_mc.Release();
            m_mc.Initalize(m_res, m_res, m_res, m_scale, 0);
            mesh = m_mc.ComputeMesh(m_densityMap, m_borderMapx, m_borderMapy, m_borderMapz, m_borderMapxy, m_borderMapyz, m_borderMapxz, m_borderMapxyz);
            mesh.name = name + "_" + m_id.ToString() + "_" + m_lod.ToString();
        }
        m_meshCollider.sharedMesh = mesh;
	}

    public void RefreshMesh(int lod)
    {
        Mesh mesh;
        CopyBorderMaps();

        if(m_lod != lod)
        {
            m_lod = lod;
            m_mc.Release();
            m_mc.Initalize(m_res, m_res, m_res, m_scale, m_lod);
        }

        mesh = m_mc.ComputeMesh(m_densityMap, m_borderMapx, m_borderMapy, m_borderMapz, m_borderMapxy, m_borderMapyz, m_borderMapxz, m_borderMapxyz);
        mesh.name = name + "_" + m_id.ToString() + "_" + m_lod.ToString();
        m_meshFilter.mesh.Clear();
        m_meshFilter.mesh = mesh;
    }

    public void RefreshCollider()
    {
        Mesh mesh;
        CopyBorderMaps();

        m_lod = 0;
        //m_mc.Release();
        m_mc.Initalize(m_res, m_res, m_res, m_scale, 0);
        mesh = m_mc.ComputeMesh(m_densityMap, m_borderMapx, m_borderMapy, m_borderMapz, m_borderMapxy, m_borderMapyz, m_borderMapxz, m_borderMapxyz);
        mesh.name = name + "_" + m_id.ToString() + "_" + m_lod.ToString();

        m_meshCollider.sharedMesh = mesh;
    }
    private void CopyBorderMaps()
    {
        for(int b = 0, i = 0; b < m_res; b++)
        {
            for(int a = 0; a < m_res; a++, i++)
            {
                if(m_neighbourChunks[14] != null)
                {
                    m_borderMapx[i] = m_neighbourChunks[14].m_densityMap[a * m_res + b * m_res2];
                    m_borderMapx[i + m_res2] = m_neighbourChunks[14].m_densityMap[1 + a * m_res + b * m_res2];
                }
                if(m_neighbourChunks[22] != null)
                {
                    m_borderMapy[i] = m_neighbourChunks[22].m_densityMap[a + b * m_res2];
                    m_borderMapy[i + m_res2] = m_neighbourChunks[22].m_densityMap[m_res + a + b * m_res2];
                }
                if(m_neighbourChunks[16] != null)
                {
                    m_borderMapz[i] = m_neighbourChunks[16].m_densityMap[a + b * m_res];
                    m_borderMapz[i + m_res2] = m_neighbourChunks[16].m_densityMap[m_res2 + a + b * m_res];
                }
            }
            if(m_neighbourChunks[23] != null)
            {
                m_borderMapxy[b] = m_neighbourChunks[23].m_densityMap[b * m_res2];
                m_borderMapxy[b + m_res] = m_neighbourChunks[23].m_densityMap[1 + b * m_res2]; //kopia w xsie
                m_borderMapxy[b + 2 * m_res] = m_neighbourChunks[23].m_densityMap[m_res + b * m_res2]; //kopia w y
            }
            if(m_neighbourChunks[25] != null)
            {
                m_borderMapyz[b] = m_neighbourChunks[25].m_densityMap[b];
                m_borderMapyz[b + m_res] = m_neighbourChunks[25].m_densityMap[m_res + b]; //kopia w y
                m_borderMapyz[b + 2 * m_res] = m_neighbourChunks[25].m_densityMap[m_res2 + b]; //kopia w z
            }
            if(m_neighbourChunks[17] != null)
            {
                m_borderMapxz[b] = m_neighbourChunks[17].m_densityMap[b * m_res];
                m_borderMapxz[b + m_res] = m_neighbourChunks[17].m_densityMap[1 + b * m_res]; //kopia w x
                m_borderMapxz[b + 2 * m_res] = m_neighbourChunks[17].m_densityMap[m_res2 + b * m_res]; //kopia w z
            }
        }
        if(m_neighbourChunks[26] != null)
        {
            m_borderMapxyz[0] = m_neighbourChunks[26].m_densityMap[0];
            m_borderMapxyz[1] = m_neighbourChunks[26].m_densityMap[1];
            m_borderMapxyz[2] = m_neighbourChunks[26].m_densityMap[m_res];
            m_borderMapxyz[3] = m_neighbourChunks[26].m_densityMap[m_res2];
        }
    }

    public void AssignNeighbour(PlanetChunk chunk, int side)
    {
        m_neighbourChunks[side] = chunk;
    }

	void DrawNormals()
	{
		for(int i = 0; i < m_meshFilter.mesh.vertexCount; i++)
		{
			Debug.DrawLine(transform.TransformPoint(m_meshFilter.mesh.vertices[i]), transform.TransformPoint(m_meshFilter.mesh.vertices[i] + m_meshFilter.mesh.normals[i]), Color.green, 120f);
		}
	}
}