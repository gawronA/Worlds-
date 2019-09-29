using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    //debug
    public bool showPlayerPos = false;
    public bool showClippingPlane = false;
    public bool ShowPX = false;
    public bool showPlaneXYZ = false;

    GeographicCoordinate PX;
    public GeographicCoordinate P1, P2, P3, P4;
    Vector3 x_dir, y_dir, z_dir;
    //info

    //environment
        //player
    public struct Player
    {
        public GameObject obj;
        public Camera camera;
        public GeographicCoordinate GeoPosition;
    }
    public Player m_player;
    //GameObject m_player;
    //GeographicCoordinate m_playerGeoPosition;

	//chunks
	int m_chunkLength;
	int m_chunkRes;
	List<GameObject> m_chunkObjs;
	List<PlanetChunk> m_chunks;


    private void Start()
    {
        m_player = new Player();
        m_player.obj = GameObject.FindGameObjectWithTag("Player");
        m_player.camera = Camera.main;
        m_player.GeoPosition = new GeographicCoordinate();
        //m_playerGeoPosition = new GeographicCoordinate();
        //m_player = GameObject.FindGameObjectWithTag("Player");

        PX = new GeographicCoordinate();

        P1 = new GeographicCoordinate();
        P2 = new GeographicCoordinate();
        P3 = new GeographicCoordinate();
        P4 = new GeographicCoordinate();
        x_dir = new Vector3();
        y_dir = new Vector3();
        z_dir = new Vector3();
    }

    private void LateUpdate()
    {
        m_player.GeoPosition.Point = transform.InverseTransformPoint(m_player.obj.transform.position);
        PX.Theta = m_player.GeoPosition.Theta;
        PX.PhiDeg = m_player.GeoPosition.PhiDeg - 10f;
        PX.Radius = m_player.GeoPosition.Radius - 2f;
        Culling();
    }

    private void OnDrawGizmos()
    {
        if(showPlayerPos) Gizmos.DrawWireSphere(m_player.GeoPosition.Point, 2f);
        if(showClippingPlane)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(P1.Point, 1f);
            Gizmos.DrawSphere(P2.Point, 1f);
            Gizmos.DrawSphere(P3.Point, 1f);
            Gizmos.DrawSphere(P4.Point, 1f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(P1.Point, P2.Point);
            Gizmos.DrawLine(P2.Point, P3.Point);
            Gizmos.DrawLine(P3.Point, P4.Point);
            Gizmos.DrawLine(P4.Point, P1.Point);
        }
        if(ShowPX) { Gizmos.color = Color.red; Gizmos.DrawSphere(PX.Point, 1f); }
        if(showPlaneXYZ) 
        { 
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + x_dir);
            Gizmos.DrawLine(transform.position, transform.position + y_dir);
            Gizmos.DrawLine(transform.position, transform.position + z_dir);
        }
    }

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
    
    void Culling()
    {
        /*x_dir = (m_player.camera.transform.rotation * Vector3.right).normalized;
        y_dir = (m_player.camera.transform.rotation * Vector3.up).normalized;
        z_dir = (m_player.camera.transform.rotation * Vector3.forward).normalized;*/

        P1.Point = m_player.camera.transform.rotation * new Vector3(-1, -1, 0).normalized;
        P2.Point = m_player.camera.transform.rotation * new Vector3(1, -1, 0).normalized;
        P3.Point = m_player.camera.transform.rotation * new Vector3(-1, 1, 0).normalized;
        P4.Point = m_player.camera.transform.rotation * new Vector3(1, 1, 0).normalized;

        /*for(int i = 0; i < m_chunks.Count; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                if(m_chunks[i].m_geo[j].Theta >= P1.Theta) { m_chunks[i].DrawMesh(); break; }
                //else if(m_chunks[i].m_geo[j].Theta <= P2.Theta) m_chunks[i].DrawMesh();
            }
        }*/
        /*P1.Theta = m_player.GeoPosition.Theta - Mathf.PI / 2f;
        P1.Phi = m_player.GeoPosition.Phi - Mathf.PI / 2f;
        P1.Radius = 15f;

        P2.Theta = m_player.GeoPosition.Theta + Mathf.PI / 2f;
        P2.Phi = m_player.GeoPosition.Phi - Mathf.PI / 2f;
        P2.Radius = 15f;

        P3.Theta = m_player.GeoPosition.Theta - Mathf.PI / 2f;
        P3.Phi = m_player.GeoPosition.Phi + Mathf.PI / 2f;
        P3.Radius = 15f;

        P4.Theta = m_player.GeoPosition.Theta + Mathf.PI / 2f;
        P4.Phi = m_player.GeoPosition.Phi + Mathf.PI / 2f;
        P4.Radius = 15f;*/

        /*float theta_min = GeographicCoordinate.Wrap2PI(m_playerGeoPosition.Theta - Mathf.PI / 2f);
        float theta_max = GeographicCoordinate.Wrap2PI(m_playerGeoPosition.Theta + Mathf.PI / 2f);
        float phi_min = GeographicCoordinate.Wrap2PI(m_playerGeoPosition.Phi)*/


    }
}
