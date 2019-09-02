//using System;
//using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlanetBaseGenerator : MonoBehaviour
{
	public int m_MaterialsCount;

	public int m_PlanetToTilesSubdivides;
	public int m_TileToPolygonsSubdivides;
	public float m_Scale;

	//public string prefabFolder;
	//public string meshFolder;
	public string planetName;
	//[HideInInspector] public int currentMesh = 0;

	//public Transform m_TilePrefab;
	//public Sphere planet;

	void Start ()
	{
		CreateGameObject();
		/*
		planet = new Sphere(m_MaterialsCount);
		Debug.Log("Creating planet: " + m_PlanetToTilesSubdivides.ToString() + " Tiles subdivides, " + m_TileToPolygonsSubdivides.ToString() + "Polygon subdivides");
		planet.CreateIcosphere(m_PlanetToTilesSubdivides, m_TileToPolygonsSubdivides);
		//planet.RandomizeMaterials();
		GenerateTiles();*/
	}

	/*public void GenerateTiles()
	{
		Transform tileObject;
		int tileObjectIndex = 0;

		for(int i = 0; i < planet.m_Tiles.Count; i++)
		{
			Mesh terrainMesh = new Mesh();
			int verticesCount = planet.m_Tiles[i].m_Polygons.Count * 3;
			Vector3[] vertices = new Vector3[verticesCount];
			int[][] indices = new int[m_MaterialsCount][];
			for(int j = 0; j < m_MaterialsCount; j++) indices[j] = new int[verticesCount];      //zoptymalizować!

			for(int j = 0; j < planet.m_Tiles[i].m_Polygons.Count; j++)
			{
				indices[planet.m_Tiles[i].m_Polygons[j].m_MaterialIndex][j * 3 + 0] = j * 3 + 0;
				indices[planet.m_Tiles[i].m_Polygons[j].m_MaterialIndex][j * 3 + 1] = j * 3 + 1;
				indices[planet.m_Tiles[i].m_Polygons[j].m_MaterialIndex][j * 3 + 2] = j * 3 + 2;
				vertices[j * 3 + 0] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[0]];
				vertices[j * 3 + 1] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[1]];
				vertices[j * 3 + 2] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[2]];
			}

			terrainMesh.vertices = vertices;
			terrainMesh.subMeshCount = m_MaterialsCount;
			for(int j = 0; j < m_MaterialsCount; j++)
			{
				terrainMesh.SetTriangles(indices[j], j);
			}
			//terrainMesh.SetTriangles(indices, 0);
			terrainMesh.RecalculateNormals();

			tileObject = Instantiate(m_TilePrefab, transform, false);
			tileObject.name = "Tile" + (tileObjectIndex++).ToString();
			tileObject.GetComponent<MeshFilter>().mesh = terrainMesh;
			tileObject.GetComponent<MeshCollider>().sharedMesh = terrainMesh;
		}
	}*/

	public void CreateGameObject()
	{
		Sphere planet = new Sphere(0);
		planet.CreateIcosphere(m_PlanetToTilesSubdivides, m_TileToPolygonsSubdivides);

		GameObject planetObject = new GameObject();
		planetObject.name = planetName;

		var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
		for(int i = 0; i < planet.m_Tiles.Count; i++)
		{
			Mesh terrainMesh = new Mesh();
			int verticesCount = planet.m_Tiles[i].m_Polygons.Count * 3;
			Vector3[] vertices = new Vector3[verticesCount];
			int[] indices = new int[verticesCount];

			for(int j = 0; j < planet.m_Tiles[i].m_Polygons.Count; j++)
			{
				indices[j * 3 + 0] = j * 3 + 0;
				indices[j * 3 + 1] = j * 3 + 1;
				indices[j * 3 + 2] = j * 3 + 2;
				vertices[j * 3 + 0] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[0]];
				vertices[j * 3 + 1] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[1]];
				vertices[j * 3 + 2] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[2]];
			}

			terrainMesh.vertices = vertices;
			terrainMesh.SetTriangles(indices, 0);
			terrainMesh.RecalculateNormals();

			GameObject tileObject = new GameObject();
			tileObject.transform.SetParent(planetObject.transform);
			tileObject.name = "Tile" + i.ToString();
			tileObject.AddComponent<MeshFilter>();
			tileObject.AddComponent<MeshRenderer>();
			tileObject.AddComponent<MeshCollider>();
			tileObject.GetComponent<MeshFilter>().mesh = terrainMesh;
			tileObject.GetComponent<MeshRenderer>().material = mat;
			tileObject.GetComponent<MeshCollider>().sharedMesh = terrainMesh;
		}
		planetObject.transform.localScale = new Vector3(m_Scale, m_Scale, m_Scale);
		Debug.Log("Done");
		Debug.Log("Distance: " + Vector3.Distance(planet.m_Tiles[0].m_Vertices[0], planet.m_Tiles[0].m_Vertices[1]).ToString());
	}
	/*public void CreatePrefab()
	{
		Sphere planet = new Sphere(0);
		planet.CreateIcosphere(m_PlanetToTilesSubdivides, m_TileToPolygonsSubdivides);

		GameObject planetObject = new GameObject();
		planetObject.name = planetName;
		for(int i = 0; i < planet.m_Tiles.Count; i++)
		{
			Mesh terrainMesh = new Mesh();
			int verticesCount = planet.m_Tiles[i].m_Polygons.Count * 3;
			Vector3[] vertices = new Vector3[verticesCount];
			int[] indices = new int[verticesCount];

			for(int j = 0; j < planet.m_Tiles[i].m_Polygons.Count; j++)
			{
				indices[j * 3 + 0] = j * 3 + 0;
				indices[j * 3 + 1] = j * 3 + 1;
				indices[j * 3 + 2] = j * 3 + 2;
				vertices[j * 3 + 0] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[0]];
				vertices[j * 3 + 1] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[1]];
				vertices[j * 3 + 2] = planet.m_Tiles[i].m_Vertices[planet.m_Tiles[i].m_Polygons[j].m_Vertices[2]];
			}

			terrainMesh.vertices = vertices;
			terrainMesh.SetTriangles(indices, 0);
			terrainMesh.RecalculateNormals();

			AssetDatabase.CreateAsset(terrainMesh, meshFolder + "tile" + i.ToString() + ".mesh");
			

			GameObject tileObject = new GameObject();
			tileObject.transform.SetParent(planetObject.transform);
			tileObject.name = "Tile" + i.ToString();
			tileObject.AddComponent<MeshFilter>();
			tileObject.AddComponent<MeshRenderer>();
			tileObject.AddComponent<MeshCollider>();
			tileObject.GetComponent<MeshFilter>().mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshFolder + "tile" + i.ToString() + ".mesh", typeof(Mesh));
			tileObject.GetComponent<MeshCollider>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshFolder + "tile" + i.ToString() + ".mesh", typeof(Mesh));

			var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
			tileObject.GetComponent<MeshRenderer>().material = mat;

			//Debug.Log(i.ToString() + "/" + planet.m_Tiles.Count + " " + ((float)(i / planet.m_Tiles.Count) * 100.0f).ToString());
		}
		AssetDatabase.SaveAssets();
		PrefabUtility.CreatePrefab(prefabFolder + planetName + ".prefab", planetObject);
		DestroyImmediate(planetObject);
		Debug.Log("Done");
	}*/

	public class Polygon
	{
		public List<int> m_Vertices;
		public int m_MaterialIndex;
		
		public Polygon(int a, int b, int c)
		{
			m_Vertices = new List<int>() { a, b, c };
			m_MaterialIndex = 0;
		}
	}

	public class Tile
	{
		public List<Vector3> m_Vertices;
		public List<Polygon> m_Polygons;
		public List<int> m_Boundaries; //<direction 0-prawo-góra, 1-lewo-góra, 3-lewo-dół, 4-prawo-dół; lista indeksów wierzchołków>

		public List<int> m_neighborsIndex;
		public Dictionary<int, List<Dictionary<int, int>>> m_connections;// <index wierzchołka, lista połączeń tile-vertex>
		//public Dictionary<Dictionary<int, int>, int> m_connections; //<this vertex, neighbor vertex>, neighbor index>

		public Tile()
		{
			m_Vertices = new List<Vector3>();
			m_Polygons = new List<Polygon>();
			m_neighborsIndex = new List<int>();
			m_Boundaries = new List<int>();
			m_connections = new Dictionary<int, List<Dictionary<int, int>>>();
			//m_connections = new Dictionary<Dictionary<int, int>, int>();
		}
		public void AddPoly(Polygon poly)
		{
			m_Polygons.Add(poly);
		}
	}

	public class Edge
	{
		public int v1;
		public int v2;
		public int m_polyIndex;
		public Edge(int _v1, int _v2, int _m_polyIndex)
		{
			v1 = _v1;
			v2 = _v2;
			m_polyIndex = _m_polyIndex;
		}

		public static List<Edge> GetEdges(Tile tile)
		{
			List <Edge> returnEdge = new List<Edge>();
			for(int i = 0; i < tile.m_Polygons.Count; i++)
			{
				returnEdge.Add(new Edge(tile.m_Polygons[i].m_Vertices[0], tile.m_Polygons[i].m_Vertices[1], i));
				returnEdge.Add(new Edge(tile.m_Polygons[i].m_Vertices[1], tile.m_Polygons[i].m_Vertices[2], i));
				returnEdge.Add(new Edge(tile.m_Polygons[i].m_Vertices[2], tile.m_Polygons[i].m_Vertices[0], i));
			}
			return returnEdge;
		}

		public static List<Edge> FindBoundary(List<Edge> edges)
		{
			List<Edge> returnEdge = new List<Edge>(edges);
			for(int i = returnEdge.Count - 1; i > 0; i--)
			{
				for(int j = i - 1; j >= 0; j--)
				{
					if(returnEdge[i].v1 == returnEdge[j].v2 && returnEdge[i].v2 == returnEdge[j].v1)
					{
						returnEdge.RemoveAt(i);
						returnEdge.RemoveAt(j);
						i--;
						break;
					}
				}
			}
			return returnEdge;
		}

		public static List<Edge> SortEdges(List<Edge> aEdges)
		{
			List<Edge> result = new List<Edge>(aEdges);
			for(int i = 0; i < result.Count - 2; i++)
			{
				Edge E = result[i];
				for(int n = i + 1; n < result.Count; n++)
				{
					Edge a = result[n];
					if(E.v2 == a.v1)
					{
						// in this case they are already in order so just continoue with the next one
						if(n == i + 1)
							break;
						// if we found a match, swap them with the next one after "i"
						result[n] = result[i + 1];
						result[i + 1] = a;
						break;
					}
				}
			}
			return result;
		}
	}

	public class Sphere
	{
		public List<Tile> m_Tiles;

		private int m_MaterialsCount;

		public Sphere(int _m_MaterialsCount)
		{
			m_MaterialsCount = _m_MaterialsCount;
		}

		public void CreateIcosphere(int TilesRecursions, int PolygonRecursions)
		{
			m_Tiles = new List<Tile>();

			List<Vector3> vertices = new List<Vector3>();
			Tile tile;

			//Creating world vertices
			float fi = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;    //golden ratio

			vertices.Add(new Vector3(-1, 0, fi).normalized);      //A	0
			vertices.Add(new Vector3(1, 0, fi).normalized);       //B	1
			vertices.Add(new Vector3(1, 0, -fi).normalized);      //C	2
			vertices.Add(new Vector3(-1, 0, -fi).normalized);		//D	3
			vertices.Add(new Vector3(0, fi, -1).normalized);      //E	4
			vertices.Add(new Vector3(0, fi, 1).normalized);       //F	5
			vertices.Add(new Vector3(0, -fi, 1).normalized);      //G	6
			vertices.Add(new Vector3(0, -fi, -1).normalized);		//H	7
			vertices.Add(new Vector3(fi, -1, 0).normalized);      //I	8
			vertices.Add(new Vector3(fi, 1, 0).normalized);       //J	9
			vertices.Add(new Vector3(-fi, 1, 0).normalized);      //K	10
			vertices.Add(new Vector3(-fi, -1, 0).normalized);     //L	11

			//Creating tile vertices and polygons
			tile = new Tile();	//1
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[9]);
			tile.m_Vertices.Add(vertices[5]);
			tile.m_Vertices.Add(vertices[1]);
			m_Tiles.Add(tile);

			tile = new Tile();	//2
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[0]);
			tile.m_Vertices.Add(vertices[1]);
			tile.m_Vertices.Add(vertices[5]);
			m_Tiles.Add(tile);

			tile = new Tile();	//3
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[5]);
			tile.m_Vertices.Add(vertices[10]);
			tile.m_Vertices.Add(vertices[0]);
			m_Tiles.Add(tile);

			tile = new Tile();	//4
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[11]);
			tile.m_Vertices.Add(vertices[0]);
			tile.m_Vertices.Add(vertices[10]);
			m_Tiles.Add(tile);

			tile = new Tile();	//5
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[10]);
			tile.m_Vertices.Add(vertices[5]);
			tile.m_Vertices.Add(vertices[4]);
			m_Tiles.Add(tile);

			tile = new Tile();	//6
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[9]);
			tile.m_Vertices.Add(vertices[4]);
			tile.m_Vertices.Add(vertices[5]);
			m_Tiles.Add(tile);

			tile = new Tile();	//7
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[4]);
			tile.m_Vertices.Add(vertices[3]);
			tile.m_Vertices.Add(vertices[10]);
			m_Tiles.Add(tile);

			tile = new Tile();	//8
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[11]);
			tile.m_Vertices.Add(vertices[10]);
			tile.m_Vertices.Add(vertices[3]);
			m_Tiles.Add(tile);

			tile = new Tile();	//9
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[3]);
			tile.m_Vertices.Add(vertices[4]);
			tile.m_Vertices.Add(vertices[2]);
			m_Tiles.Add(tile);

			tile = new Tile();	//10
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[9]);
			tile.m_Vertices.Add(vertices[2]);
			tile.m_Vertices.Add(vertices[4]);
			m_Tiles.Add(tile);

			tile = new Tile();	//11
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[2]);
			tile.m_Vertices.Add(vertices[7]);
			tile.m_Vertices.Add(vertices[3]);
			m_Tiles.Add(tile);

			tile = new Tile();	//12
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[11]);
			tile.m_Vertices.Add(vertices[3]);
			tile.m_Vertices.Add(vertices[7]);
			m_Tiles.Add(tile);

			tile = new Tile();	//13
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[7]);
			tile.m_Vertices.Add(vertices[2]);
			tile.m_Vertices.Add(vertices[8]);
			m_Tiles.Add(tile);

			tile = new Tile();	//14
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[9]);
			tile.m_Vertices.Add(vertices[8]);
			tile.m_Vertices.Add(vertices[2]);
			m_Tiles.Add(tile);

			tile = new Tile();	//15
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[8]);
			tile.m_Vertices.Add(vertices[6]);
			tile.m_Vertices.Add(vertices[7]);
			m_Tiles.Add(tile);

			tile = new Tile();	//16
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[11]);
			tile.m_Vertices.Add(vertices[7]);
			tile.m_Vertices.Add(vertices[6]);
			m_Tiles.Add(tile);

			tile = new Tile();	//17
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[6]);
			tile.m_Vertices.Add(vertices[8]);
			tile.m_Vertices.Add(vertices[1]);
			m_Tiles.Add(tile);

			tile = new Tile();	//18
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[9]);
			tile.m_Vertices.Add(vertices[1]);
			tile.m_Vertices.Add(vertices[8]);
			m_Tiles.Add(tile);

			tile = new Tile();	//19
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[1]);
			tile.m_Vertices.Add(vertices[0]);
			tile.m_Vertices.Add(vertices[6]);
			m_Tiles.Add(tile);

			tile = new Tile();	//20
			tile.AddPoly(new Polygon(0, 1, 2));
			tile.m_Vertices.Add(vertices[11]);
			tile.m_Vertices.Add(vertices[6]);
			tile.m_Vertices.Add(vertices[0]);
			m_Tiles.Add(tile);
			
			//Merging one vertex
			for(int i = 0; i < m_Tiles.Count; i = i + 2)
			{
				m_Tiles[i].m_Vertices.Add(m_Tiles[i + 1].m_Vertices[0]);
				m_Tiles[i].m_Polygons.Add(new Polygon(1, 3, 2));
			}
			for(int i = m_Tiles.Count - 1; i > 0; i = i - 2) m_Tiles.RemoveAt(i);

			PlanetToTilesSubdivide(TilesRecursions);
			TileToPolygonsSubdivide(PolygonRecursions);

			//Dictionary<List<Edge>, int> edges = new Dictionary<List<Edge>, int>();
			/*
			//Creating boundaries
			List<Edge> edges = Edge.FindBoundary(Edge.GetEdges(m_Tiles[0]));
			List<Edge> sortedEdges = Edge.SortEdges(edges);

			List<int> verticesIndex = new List<int>();
			for(int i = 0; i < sortedEdges.Count; i++)
			{
				verticesIndex.Add(sortedEdges[i].v1);
			}

			for(int i = 0; i < m_Tiles.Count; i++)
			{
				m_Tiles[i].m_Boundaries = verticesIndex;
			}
			Debug.Log("Created boundaries");*/
			/*
			for(int i = 0; i < m_Tiles.Count; i++)
			{
				for(int j = 0; j < m_Tiles[i].m_Boundaries.Count; j++)
				{
					var lista = new List<Dictionary<int, int>>();
					for(int k = 0; k < m_Tiles.Count; k++)
					{
						if(i == k) continue;
						for(int l = 0; l < m_Tiles[k].m_Boundaries.Count; l++)
						{
							if(m_Tiles[i].m_Vertices[m_Tiles[i].m_Boundaries[j]] == m_Tiles[k].m_Vertices[m_Tiles[k].m_Boundaries[l]])
							{
								var tmp = new Dictionary<int, int>();
								tmp.Add(k, m_Tiles[k].m_Boundaries[l]);
								lista.Add(tmp);
								//m_Tiles[i].m_neighborsIndex.Add(j);
								//var tmp = new Dictionary<int, int>();
								//tmp.Add(j, m_Tiles[j].m_Boundaries[l]);
								//m_Tiles[i].m_connections.Add(m_Tiles[i].m_Boundaries[k], tmp);
							}
						}
					}
					m_Tiles[i].m_connections.Add(j, lista);
				}
			}*/
			Debug.Log("Created connections");
			/*foreach(List<Edge> primkey in edges.Keys)
			{
				foreach(List<Edge> seckey in edges.Keys)
				{
					if(primkey == seckey) continue;
					foreach(Edge primedge in primkey)
					{
						foreach(Edge secedge in seckey)
						{
							if(m_Tiles[edges[primkey]].m_Vertices[primedge.v1] == m_Tiles[edges[seckey]].m_Vertices[secedge.v1])
							{
								
								var tmp = new Dictionary<int, int>();
								int index = edges[seckey];
								tmp.Add(primedge.v1, secedge.v1);
								if(!m_Tiles[edges[primkey]].m_connections.ContainsKey(tmp))
								{
									m_Tiles[edges[primkey]].m_connections.Add(tmp, edges[seckey]);
									m_Tiles[edges[primkey]].m_neighborsIndex.Add(edges[seckey]);
								}
							}
							else if(m_Tiles[edges[primkey]].m_Vertices[primedge.v1] == m_Tiles[edges[seckey]].m_Vertices[secedge.v2])
							{
								var tmp = new Dictionary<int, int>();
								int index = edges[seckey];
								tmp.Add(primedge.v1, secedge.v2);
								if(!m_Tiles[edges[primkey]].m_connections.ContainsKey(tmp))
								{
									m_Tiles[edges[primkey]].m_connections.Add(tmp, edges[seckey]);
									m_Tiles[edges[primkey]].m_neighborsIndex.Add(edges[seckey]);
								}
							}
							else if(m_Tiles[edges[primkey]].m_Vertices[primedge.v2] == m_Tiles[edges[seckey]].m_Vertices[secedge.v1])
							{
								var tmp = new Dictionary<int, int>();
								int index = edges[seckey];
								tmp.Add(primedge.v2, secedge.v1);
								if(!m_Tiles[edges[primkey]].m_connections.ContainsKey(tmp))
								{
									m_Tiles[edges[primkey]].m_connections.Add(tmp, edges[seckey]);
									m_Tiles[edges[primkey]].m_neighborsIndex.Add(edges[seckey]);
								}
							}
							else if(m_Tiles[edges[primkey]].m_Vertices[primedge.v2] == m_Tiles[edges[seckey]].m_Vertices[secedge.v2])
							{
								var tmp = new Dictionary<int, int>();
								int index = edges[seckey];
								tmp.Add(primedge.v2, secedge.v2);
								if(!m_Tiles[edges[primkey]].m_connections.ContainsKey(tmp))
								{
									m_Tiles[edges[primkey]].m_connections.Add(tmp, edges[seckey]);
									m_Tiles[edges[primkey]].m_neighborsIndex.Add(edges[seckey]);
								}
							}
						}
					}
				}
			}*/
			/*for(int i = 0; i < m_Tiles.Count; i++)
			{
				for(int j = 0; j < m_Tiles.Count; j++)
				{
					if(i == j) continue;
					for(int k = 0; k < m_Tiles[i].m_Vertices.Count; k++)
					{
						for(int l = 0; l < m_Tiles[j].m_Vertices.Count; l++)
						{
							if(m_Tiles[i].m_Vertices[k] == m_Tiles[j].m_Vertices[l])
							{
								m_Tiles[i].m_neighborsIndex.Add(j);
								var tmp = new Dictionary<int, int>();
								tmp.Add(k, l);
								m_Tiles[i].m_connections.Add(tmp, j);
							}
						}
					}
				}
			}*/



			/*
			Dictionary<Dictionary<int, int>, int>.KeyCollection neighborKeys;
			neighborKeys = m_Tiles[0].m_connections.Keys;
			Debug.Log("Tile0 neighbors: " + m_Tiles[0].m_neighborsIndex[0].ToString() + m_Tiles[0].m_neighborsIndex[1].ToString() + m_Tiles[0].m_neighborsIndex[2].ToString());
			foreach(Dictionary<int, int> neighborkey in neighborKeys)
			{
				Dictionary<int, int>.KeyCollection vertexKeys;
				vertexKeys = neighborkey.Keys;
				foreach(int vertexkey in vertexKeys)
				{
					Debug.Log("Tile0 neighbor " + m_Tiles[0].m_connections[neighborkey].ToString() + " commonVertices: " + vertexkey + " " + neighborkey[vertexkey]);
				}
				
			}*/
		}

		public void PlanetToTilesSubdivide(int recursions)
		{
			//Dictionary<int, int> midPointCache = new Dictionary<int, int>();
			for(int i=0;i<recursions;i++)
			{
				List<Tile> newTiles = new List<Tile>();
				foreach(Tile tile in m_Tiles)
				{
					Vector3 a = tile.m_Vertices[0];
					Vector3 b = tile.m_Vertices[1];
					Vector3 c = tile.m_Vertices[2];
					Vector3 d = tile.m_Vertices[3];

					Vector3 ab = Vector3.Lerp(a, b, 0.5f).normalized;
					Vector3 bc = Vector3.Lerp(b, c, 0.5f).normalized;
					Vector3 ca = Vector3.Lerp(c, a, 0.5f).normalized;
					Vector3 bd = Vector3.Lerp(b, d, 0.5f).normalized;
					Vector3 dc = Vector3.Lerp(d, c, 0.5f).normalized;

					Tile tmp;

					tmp = new Tile(); //1
					tmp.m_Polygons.Add(new Polygon(0, 1, 2));
					tmp.m_Polygons.Add(new Polygon(1, 3, 2));
					tmp.m_Vertices.Add(a);
					tmp.m_Vertices.Add(ab);
					tmp.m_Vertices.Add(ca);
					tmp.m_Vertices.Add(bc);
					newTiles.Add(tmp);

					tmp = new Tile(); //2
					tmp.m_Polygons.Add(new Polygon(0, 1, 2));
					tmp.m_Polygons.Add(new Polygon(1, 3, 2));
					tmp.m_Vertices.Add(ca);
					tmp.m_Vertices.Add(bc);
					tmp.m_Vertices.Add(c);
					tmp.m_Vertices.Add(dc);
					newTiles.Add(tmp);

					tmp = new Tile(); //3
					tmp.m_Polygons.Add(new Polygon(0, 1, 2));
					tmp.m_Polygons.Add(new Polygon(1, 3, 2));
					tmp.m_Vertices.Add(ab);
					tmp.m_Vertices.Add(b);
					tmp.m_Vertices.Add(bc);
					tmp.m_Vertices.Add(bd);
					newTiles.Add(tmp);

					tmp = new Tile(); //4
					tmp.m_Polygons.Add(new Polygon(0, 1, 2));
					tmp.m_Polygons.Add(new Polygon(1, 3, 2));
					tmp.m_Vertices.Add(bc);
					tmp.m_Vertices.Add(bd);
					tmp.m_Vertices.Add(dc);
					tmp.m_Vertices.Add(d);
					newTiles.Add(tmp);
					/*
					List<Polygon> newPolys = new List<Polygon>();
					foreach(Polygon poly in tile.m_Polygons)
					{
						int a = poly.m_Vertices[0];
						int b = poly.m_Vertices[1];
						int c = poly.m_Vertices[2];

						int ab = GetMidPointIndex(midPointCache, a, b);
						int bc = GetMidPointIndex(midPointCache, b, c);
						int ca = GetMidPointIndex(midPointCache, c, a);

						newPolys.Add(new Polygon(a, ab, ca));
						newPolys.Add(new Polygon(b, bc, ab));
						newPolys.Add(new Polygon(c, ca, bc));
						newPolys.Add(new Polygon(ab, bc, ca));
					}*/

				}
				m_Tiles = newTiles;
				//m_Polygons = newPolys;
			}
		}

		public void TileToPolygonsSubdivide(int recursions)
		{
			foreach(Tile tile in m_Tiles)
			{
				Dictionary<int, int> midPointCache = new Dictionary<int, int>();
				for(int i = 0; i < recursions; i++)
				{
					List<Polygon> newPolys = new List<Polygon>();
					foreach(Polygon poly in tile.m_Polygons)
					{
						int a = poly.m_Vertices[0];
						int b = poly.m_Vertices[1];
						int c = poly.m_Vertices[2];

						int ab = GetMidPointIndex(tile, midPointCache, a, b);
						int bc = GetMidPointIndex(tile, midPointCache, b, c);
						int ca = GetMidPointIndex(tile, midPointCache, c, a);

						newPolys.Add(new Polygon(a, ab, ca));
						newPolys.Add(new Polygon(b, bc, ab));
						newPolys.Add(new Polygon(c, ca, bc));
						newPolys.Add(new Polygon(ab, bc, ca));
					}
					tile.m_Polygons = newPolys;
				}
			}
		}

		public int GetMidPointIndex(Tile tile, Dictionary<int, int> cache, int indexA, int indexB)
		{
			int smallerIndex = Mathf.Min(indexA, indexB);
			int greaterIndex = Mathf.Max(indexA, indexB);
			int key = (smallerIndex << 16) + greaterIndex;

			int ret;
			if(cache.TryGetValue(key, out ret)) return ret;

			Vector3 p1 = tile.m_Vertices[indexA];
			Vector3 p2 = tile.m_Vertices[indexB];
			Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

			ret = tile.m_Vertices.Count;
			tile.m_Vertices.Add(middle);

			cache.Add(key, ret);
			return ret;
		}

		public void RandomizeMaterials()
		{
			foreach(Tile tile in m_Tiles)
			{
				foreach(Polygon poly in tile.m_Polygons)
				{
					poly.m_MaterialIndex = Random.Range((int)0, m_MaterialsCount);
				}
			}
		}
	}

	/*public class CommandPrompt
	{
		System.Diagnostics.Process cmd;
		System.Threading.Thread th;
		public void init()
		{
			//System.Diagnostics.Process.Start("cmd.exe");
			//System.Console.WriteLine("echo aa");

			th = new System.Threading.Thread(new System.Threading.ThreadStart(start));
			th.Start();
			th.
		}

		public void start()
		{
			cmd = new System.Diagnostics.Process();
			cmd.StartInfo.FileName = "cmd.exe";
			cmd.StartInfo.CreateNoWindow = false;
			cmd.StartInfo.RedirectStandardInput = true;
			cmd.StartInfo.RedirectStandardOutput = true;
			cmd.StartInfo.UseShellExecute = false;
			cmd.Start();
		}

		public void write(string text)
		{
			//cmd.StandardInput.Flush();
			cmd.StandardInput.WriteLine("echo aa");
			cmd.StandardInput.Flush();
			//cmd.StandardInput.Close();
			//cmd.WaitForExit();
			System.Console.WriteLine(cmd.StandardOutput.ReadToEnd());
		}

		public void kill()
		{
			cmd.Kill();
		}
	}*/
}
