using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using ProceduralTerrain.MarchingCubes;


public unsafe class PlanetChunk : MonoBehaviour
{
    public bool drawChunkBorders = false;
    public bool drawBoundingBoxe = false;
    public bool drawCenter = false;
	
	//chunk info
	int m_id;
	int m_res;
	int m_res2;
	float m_scale;

    //Environment
    private Planet m_planet;
    private PlanetChunk[] m_neighbourChunks;
    private Transform m_player;

    //Density map
    public float[] m_densityMap { get; private set; }
    

	//Components
	MeshFilter m_meshFilter;
	MeshCollider m_meshCollider;

    //Mesh and collider generation
    public float m_lod1Distance = 100f, m_lod2Distance = 500f, m_lod3Distance = 1000f;
    private MarchingCubes m_mcRender, m_mcCollider;
    private BorderDensities m_borderMaps;
	public ComputeShader m_MCRenderShader;
    public ComputeShader m_MCColliderShader;
	public ComputeShader m_ClearVerticesShader;
	public ComputeShader m_CalculateNormalsShader;
    public Material m_meshMaterial;
    private int m_lod = 4;
    
    private void Start()
	{
        m_player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
	{

        float playerDistance = Vector3.Distance(m_player.position, transform.position);
        if(playerDistance > m_lod3Distance && m_lod != 3) RefreshMesh(3);
        else if(playerDistance <= m_lod3Distance && playerDistance > m_lod2Distance && m_lod != 2) RefreshMesh(2);
        else if(playerDistance <= m_lod2Distance && playerDistance > m_lod1Distance && m_lod != 1) RefreshMesh(1);
        else if(playerDistance <= m_lod1Distance && m_lod != 0) RefreshMesh(0);

        m_mcRender.DrawMesh();
	}

    private void OnDrawGizmos()
    {
        if(drawChunkBorders) { Gizmos.color = Color.green; Gizmos.DrawWireCube(transform.position, Vector3.one * m_res * m_scale); }
        if(drawBoundingBoxe) { Gizmos.color = Color.white; Gizmos.DrawWireCube(m_mcRender.m_bounds.center, m_mcRender.m_bounds.size); };
        if(drawCenter) { Gizmos.color = Color.red; Gizmos.DrawSphere(transform.position, 2f * m_scale); }
    }

    private void OnDestroy()
	{
		m_mcRender.Release();
        m_mcCollider.Release();
	}

	public void Initalize(int id, int res, float scale, bool shaded)
	{
		m_id = id;
		m_res = res;
		m_res2 = m_res * m_res;
		m_scale = scale;

        m_neighbourChunks = new PlanetChunk[27];

		m_meshFilter = GetComponent<MeshFilter>();
		m_meshCollider = GetComponent<MeshCollider>();

        m_mcRender = new MarchingCubes
        {
            m_MCRenderShader = m_MCRenderShader,
            m_clearVerticesShader = m_ClearVerticesShader,
            m_calculateNormalsShader = m_CalculateNormalsShader,
            m_meshMaterial = m_meshMaterial,
            m_recalculateNormals = !shaded,
            m_chunkTransform = transform
		};
        m_mcRender.InitalizeRenderMesh(m_res, m_res, m_res, m_scale);

        m_mcCollider = new MarchingCubes
        {
            m_MCColliderShader = m_MCColliderShader,
            m_clearVerticesShader = m_ClearVerticesShader,

        };
        m_mcCollider.InitalizeColliderCompute(m_res, m_res, m_res, scale);

        m_borderMaps.borderMapx = new float[2 * m_res2];
        m_borderMaps.borderMapy = new float[2 * m_res2];
        m_borderMaps.borderMapz = new float[2 * m_res2];
        m_borderMaps.borderMapxy = new float[3 * m_res];
        m_borderMaps.borderMapyz = new float[3 * m_res];
        m_borderMaps.borderMapxz = new float[3 * m_res];
        m_borderMaps.borderMapxyz = new float[4];
    }

	public void SetDensityMap(float[] map)
	{
		m_densityMap = map;
	}

    public void RefreshMesh(int lod)
    {
        if(m_lod != lod)
        {
            m_lod = lod;
            m_mcRender.SetLOD(m_lod);
        }
        m_mcRender.ComputeRenderMesh(m_densityMap, m_borderMaps);
    }

    public void RefreshCollider()
    {
        Mesh mesh;
        CopyBorderMaps();

        mesh = m_mcCollider.ComputeColliderMesh(m_densityMap, m_borderMaps);
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
                    m_borderMaps.borderMapx[i] = m_neighbourChunks[14].m_densityMap[a * m_res + b * m_res2];
                    m_borderMaps.borderMapx[i + m_res2] = m_neighbourChunks[14].m_densityMap[1 + a * m_res + b * m_res2];
                }
                if(m_neighbourChunks[22] != null)
                {
                    m_borderMaps.borderMapy[i] = m_neighbourChunks[22].m_densityMap[a + b * m_res2];
                    m_borderMaps.borderMapy[i + m_res2] = m_neighbourChunks[22].m_densityMap[m_res + a + b * m_res2];
                }
                if(m_neighbourChunks[16] != null)
                {
                    m_borderMaps.borderMapz[i] = m_neighbourChunks[16].m_densityMap[a + b * m_res];
                    m_borderMaps.borderMapz[i + m_res2] = m_neighbourChunks[16].m_densityMap[m_res2 + a + b * m_res];
                }
            }
            if(m_neighbourChunks[23] != null)
            {
                m_borderMaps.borderMapxy[b] = m_neighbourChunks[23].m_densityMap[b * m_res2];
                m_borderMaps.borderMapxy[b + m_res] = m_neighbourChunks[23].m_densityMap[1 + b * m_res2]; //kopia w xsie
                m_borderMaps.borderMapxy[b + 2 * m_res] = m_neighbourChunks[23].m_densityMap[m_res + b * m_res2]; //kopia w y
            }
            if(m_neighbourChunks[25] != null)
            {
                m_borderMaps.borderMapyz[b] = m_neighbourChunks[25].m_densityMap[b];
                m_borderMaps.borderMapyz[b + m_res] = m_neighbourChunks[25].m_densityMap[m_res + b]; //kopia w y
                m_borderMaps.borderMapyz[b + 2 * m_res] = m_neighbourChunks[25].m_densityMap[m_res2 + b]; //kopia w z
            }
            if(m_neighbourChunks[17] != null)
            {
                m_borderMaps.borderMapxz[b] = m_neighbourChunks[17].m_densityMap[b * m_res];
                m_borderMaps.borderMapxz[b + m_res] = m_neighbourChunks[17].m_densityMap[1 + b * m_res]; //kopia w x
                m_borderMaps.borderMapxz[b + 2 * m_res] = m_neighbourChunks[17].m_densityMap[m_res2 + b * m_res]; //kopia w z
            }
        }
        if(m_neighbourChunks[26] != null)
        {
            m_borderMaps.borderMapxyz[0] = m_neighbourChunks[26].m_densityMap[0];
            m_borderMaps.borderMapxyz[1] = m_neighbourChunks[26].m_densityMap[1];
            m_borderMaps.borderMapxyz[2] = m_neighbourChunks[26].m_densityMap[m_res];
            m_borderMaps.borderMapxyz[3] = m_neighbourChunks[26].m_densityMap[m_res2];
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