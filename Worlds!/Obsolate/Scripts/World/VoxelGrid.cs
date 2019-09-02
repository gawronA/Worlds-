using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class VoxelGrid : MonoBehaviour
{
	public bool drawGizmos = false;
	private Text deltaTimeText;
	//initalization
	public GameObject voxelPrefab;
	public int resolution;
	public float size;
	[HideInInspector] public int index;

	//voxels
	private List<Voxel> voxels;
	public List<Material> voxelMaterials;
	private Mesh smoothMesh, crispMesh;
	private List<Vector3> smoothVertices, crispVertices;
	private List<int> smoothTriangles, crispTriangles;

	//components
	private MeshFilter meshFilter;
	private MeshCollider meshCollider;
	private BoxCollider boxCollider;
	

	//triangulating
	public int[] voxelWalls;
	public VoxelGrid[] neighbors = null;
	public Voxel dummyX1, dummyX2, dummyY1, dummyY2, dummyZ1, dummyZ2, dummyXZ1, dummyYZ1, dummyT1, dummyT2, dummyT3;
	private int resolution2;
	private int resolution3;
	private float sizeResolution;

	//caching
	private int[][] rowX;
	private int[][] rowZ;
	private int[][] mdrXY;
	private int[][] mdrZY;
	private int[] mdrY0;
	private int[] mdrY1;

	//other info
	private int[][] triangleOrder;

	public void Initalize(int _resolution, float _size)
	{
		//deltaTimeText = GameObject.Find("DeltaTimeText").GetComponent<Text>();
		resolution = _resolution;
		size = _size;

		resolution2 = resolution * resolution;
		resolution3 = resolution * resolution * resolution;
		sizeResolution = size * resolution;

		voxels = new List<Voxel>();
		voxelMaterials = new List<Material>();
		voxelWalls = new int[resolution3];
		neighbors = null;
		dummyX1 = new Voxel();
		dummyX2 = new Voxel();
		dummyY1 = new Voxel();
		dummyY2 = new Voxel();
		dummyZ1 = new Voxel();
		dummyZ2 = new Voxel();
		dummyXZ1 = new Voxel();
		dummyYZ1 = new Voxel();
		dummyT1 = new Voxel();
		dummyT2 = new Voxel();
		dummyT3 = new Voxel();

		//caching
		rowX = new int[resolution + 1][];
		rowZ = new int[resolution + 1][];
		for(int i = 0; i < resolution + 1; i++)
		{
			rowX[i] = new int[resolution];
			rowZ[i] = new int[resolution];
			for(int j = 0; j < resolution; j++) rowX[i][j] = rowZ[i][j] = -1;
		}

		mdrXY = new int[resolution][];
		mdrZY = new int[resolution][];
		for(int i = 0; i < resolution; i++)
		{
			mdrXY[i] = new int[resolution + 1];
			mdrZY[i] = new int[resolution + 1];
			for(int j = 0; j < resolution + 1; j++) mdrXY[i][j] = mdrZY[i][j] = -1;
		}

		mdrY0 = new int[resolution + 1];
		mdrY1 = new int[resolution + 1];
		for(int i = 0; i < resolution + 1; i++) mdrY0[i] = mdrY1[i] = -1;
	

		smoothMesh = new Mesh();
		smoothVertices = new List<Vector3>();
		smoothTriangles = new List<int>();
		crispMesh = new Mesh();
		crispVertices = new List<Vector3>();
		crispTriangles = new List<int>();
		
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		boxCollider = GetComponent<BoxCollider>();
		boxCollider.center = Vector3.one * resolution / 2 - Vector3.one * size / 2;
		boxCollider.size = Vector3.one * resolution - Vector3.one * 0.05f;
		for(int i = 0, z = 0; z < resolution; z++)
		{
			for(int y = 0; y < resolution; y++)
			{
				for(int x = 0; x < resolution; x++, i++)
				{
					CreateVoxel(i, x, y, z);
					int shift = 0;
					if(y == resolution - 1) { voxelWalls[i] |= 0 << shift; shift = shift + 8; }  //top
					if(y == 0) { voxelWalls[i] |= 1 << shift; shift = shift + 8; }               //bottom
					if(z == 0) { voxelWalls[i] |= 2 << shift; shift = shift + 8; }               //front
					if(z == resolution - 1) { voxelWalls[i] |= 3 << shift; shift = shift + 8; }  //back
					if(x == 0) { voxelWalls[i] |= 4 << shift; shift = shift + 8; }               //left
					if(x == resolution - 1) { voxelWalls[i] |= 5 << shift; shift = shift + 8; }  //right

					if(voxelWalls[i] == 516) { voxelWalls[i] |= 6 << shift; shift = shift + 8; }    //top/front/left
					if(voxelWalls[i] == 517) { voxelWalls[i] |= 7 << shift; shift = shift + 8; }    //top/front/right
					if(voxelWalls[i] == 772) { voxelWalls[i] |= 8 << shift; shift = shift + 8; }    //top/back/left
					if(voxelWalls[i] == 773) { voxelWalls[i] |= 9 << shift; shift = shift + 8; }    //top/back/right
					if(voxelWalls[i] == 66052) { voxelWalls[i] |= 10 << shift; shift = shift + 8; } //bottom/front/left
					if(voxelWalls[i] == 66053) { voxelWalls[i] |= 11 << shift; shift = shift + 8; } //bottom/front/right
					if(voxelWalls[i] == 66308) { voxelWalls[i] |= 12 << shift; shift = shift + 8; }	//bottom/back/left
					if(voxelWalls[i] == 66309) { voxelWalls[i] |= 13 << shift; shift = shift + 8; }	//bottom/back/right

					while(shift < 32)
					{
						voxelWalls[i] |= -1 << shift;
						shift = shift + 8;
					}
				}
			}
		}
		PrecalculateTriangleOrder();
		Refresh();
	}

	private void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			RaycastHit hitInfo;
			if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
			{
				if(hitInfo.collider.gameObject == gameObject)
				{
					EditVoxel(transform.InverseTransformPoint(hitInfo.point) + Vector3.one * size / 2);
					//deltaTimeText.text = "DeltaTime: " + Time.deltaTime.ToString();
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if(drawGizmos) DrawDebug();
	}
	private void CreateVoxel(int index, float x, float y, float z)
	{
		Voxel voxel = new Voxel(new Vector3(x, y, z), transform.parent.rotation, size);
		voxels.Add(voxel);

		/*GameObject o = Instantiate(voxelPrefab);
		o.name = "Voxel" + index.ToString();
		o.transform.parent = transform;
		o.transform.localPosition = voxel.position;
		o.transform.localScale = Vector3.one * 0.05f;
		voxelMaterials.Add(o.GetComponent<MeshRenderer>().material);*/
	}

	public void EditVoxel(Vector3 point)
	{
		int voxelX = (int)(point.x / size);
		int voxelY = (int)(point.y / size);
		int voxelZ = (int)(point.z / size);
		SetVoxelRefresh(voxelX, voxelY, voxelZ, voxels[VoxelIndex(voxelX, voxelY, voxelZ)].state ^ true);
		if(voxelWalls[VoxelIndex(voxelX, voxelY, voxelZ)] != -1)
		{
			switch(voxelWalls[VoxelIndex(voxelX, voxelY, voxelZ)])
			{
				case -256:  //top
					if(neighbors[16] != null) neighbors[16].Refresh();
					break;
				case -255:  //bottom
					if(neighbors[10] != null) neighbors[10].Refresh();
					break;
				case -254:  //front
					if(neighbors[4] != null) neighbors[4].Refresh();
					break;
				case -253:  //back
					if(neighbors[22] != null) neighbors[22].Refresh();
					break;
				case -252:  //left
					if(neighbors[12] != null) neighbors[12].Refresh();
					break;
				case -251:  //right
					if(neighbors[14] != null) neighbors[14].Refresh();
					break;
				case -65024:    //top/front
					if(neighbors[16] != null) neighbors[16].Refresh();
					if(neighbors[4] != null) neighbors[4].Refresh();
					if(neighbors[7] != null) neighbors[7].Refresh();
					break;
				case -64768:    //top/back
					if(neighbors[16] != null) neighbors[16].Refresh();
					if(neighbors[22] != null) neighbors[22].Refresh();
					if(neighbors[25] != null) neighbors[25].Refresh();
					break;
				case -64512:    //top/left
					if(neighbors[16] != null) neighbors[16].Refresh();
					if(neighbors[12] != null) neighbors[12].Refresh();
					if(neighbors[15] != null) neighbors[15].Refresh();
					break;
				case -64256:    //top/right
					if(neighbors[16] != null) neighbors[16].Refresh();
					if(neighbors[14] != null) neighbors[14].Refresh();
					if(neighbors[17] != null) neighbors[17].Refresh();
					break;
				case -65023:    //bottom/front
					if(neighbors[10] != null) neighbors[10].Refresh();
					if(neighbors[4] != null) neighbors[4].Refresh();
					if(neighbors[1] != null) neighbors[1].Refresh();
					break;
				case -64767:    //bottom/back
					if(neighbors[10] != null) neighbors[10].Refresh();
					if(neighbors[22] != null) neighbors[22].Refresh();
					if(neighbors[19] != null) neighbors[19].Refresh();
					break;
				case -64511:    //bottom/left
					if(neighbors[10] != null) neighbors[10].Refresh();
					if(neighbors[12] != null) neighbors[12].Refresh();
					if(neighbors[9] != null) neighbors[9].Refresh();
					break;
				case -64255:    //bottom/right
					if(neighbors[10] != null) neighbors[10].Refresh();
					if(neighbors[14] != null) neighbors[14].Refresh();
					if(neighbors[11] != null) neighbors[11].Refresh();
					break;
				case -64510:    //front/left
					if(neighbors[4] != null) neighbors[4].Refresh();
					if(neighbors[12] != null) neighbors[12].Refresh();
					if(neighbors[3] != null) neighbors[3].Refresh();
					break;
				case -64254:    //front/right
					if(neighbors[4] != null) neighbors[4].Refresh();
					if(neighbors[14] != null) neighbors[14].Refresh();
					if(neighbors[5] != null) neighbors[5].Refresh();
					break;
				case -64509:    //back/left
					if(neighbors[22] != null) neighbors[22].Refresh();
					if(neighbors[12] != null) neighbors[12].Refresh();
					if(neighbors[21] != null) neighbors[21].Refresh();
					break;
				case -64253:    //back/right
					if(neighbors[22] != null) neighbors[22].Refresh();
					if(neighbors[14] != null) neighbors[14].Refresh();
					if(neighbors[23] != null) neighbors[23].Refresh();
					break;
				case -16514560: //top/front/left
					if(neighbors[16] != null) neighbors[16].Refresh();
					if(neighbors[4] != null) neighbors[4].Refresh();
					if(neighbors[12] != null) neighbors[12].Refresh();

					if(neighbors[3] != null) neighbors[3].Refresh();
					if(neighbors[6] != null) neighbors[6].Refresh();
					if(neighbors[7] != null) neighbors[7].Refresh();
					if(neighbors[15] != null) neighbors[15].Refresh();
					break;
				case -16449024: //top/front/right
					if(neighbors[16] != null) neighbors[16].Refresh();
					if(neighbors[4] != null) neighbors[4].Refresh();
					if(neighbors[14] != null) neighbors[14].Refresh();

					if(neighbors[5] != null) neighbors[5].Refresh();
					if(neighbors[7] != null) neighbors[7].Refresh();
					if(neighbors[8] != null) neighbors[8].Refresh();
					if(neighbors[17] != null) neighbors[17].Refresh();
					break;
				case -16514304: //top/back/left
					if(neighbors[16] != null) neighbors[16].Refresh();
					if(neighbors[22] != null) neighbors[22].Refresh();
					if(neighbors[12] != null) neighbors[12].Refresh();

					if(neighbors[15] != null) neighbors[15].Refresh();
					if(neighbors[21] != null) neighbors[21].Refresh();
					if(neighbors[24] != null) neighbors[24].Refresh();
					if(neighbors[25] != null) neighbors[25].Refresh();
					break;
				case -16448768: //top/back/right
					if(neighbors[16] != null) neighbors[16].Refresh();
					if(neighbors[22] != null) neighbors[22].Refresh();
					if(neighbors[14] != null) neighbors[14].Refresh();

					if(neighbors[17] != null) neighbors[17].Refresh();
					if(neighbors[23] != null) neighbors[23].Refresh();
					if(neighbors[25] != null) neighbors[25].Refresh();
					if(neighbors[26] != null) neighbors[26].Refresh();
					break;
				case -16514559: //bottom/front/left
					if(neighbors[10] != null) neighbors[10].Refresh();
					if(neighbors[4] != null) neighbors[4].Refresh();
					if(neighbors[12] != null) neighbors[12].Refresh();

					if(neighbors[0] != null) neighbors[0].Refresh();
					if(neighbors[1] != null) neighbors[1].Refresh();
					if(neighbors[3] != null) neighbors[3].Refresh();
					if(neighbors[9] != null) neighbors[9].Refresh();
					break;
				case -16449023: //bottom/front/right
					if(neighbors[10] != null) neighbors[10].Refresh();
					if(neighbors[4] != null) neighbors[4].Refresh();
					if(neighbors[14] != null) neighbors[14].Refresh();

					if(neighbors[1] != null) neighbors[1].Refresh();
					if(neighbors[2] != null) neighbors[2].Refresh();
					if(neighbors[5] != null) neighbors[5].Refresh();
					if(neighbors[11] != null) neighbors[11].Refresh();
					break;
				case -16514303: //bottom/back/left
					if(neighbors[10] != null) neighbors[10].Refresh();
					if(neighbors[22] != null) neighbors[22].Refresh();
					if(neighbors[12] != null) neighbors[12].Refresh();

					if(neighbors[9] != null) neighbors[9].Refresh();
					if(neighbors[18] != null) neighbors[18].Refresh();
					if(neighbors[19] != null) neighbors[19].Refresh();
					if(neighbors[21] != null) neighbors[21].Refresh();
					break;
				case -16448767: //bottom/back/right
					if(neighbors[10] != null) neighbors[10].Refresh();
					if(neighbors[22] != null) neighbors[22].Refresh();
					if(neighbors[14] != null) neighbors[14].Refresh();

					if(neighbors[11] != null) neighbors[11].Refresh();
					if(neighbors[19] != null) neighbors[19].Refresh();
					if(neighbors[20] != null) neighbors[20].Refresh();
					if(neighbors[23] != null) neighbors[23].Refresh();
					break;
			}
		}
	}

	public void SetVoxelRefresh(int x, int y, int z, bool state)
	{
		voxels[z * resolution2 + y * resolution + x].state = state;
		Refresh();
	}

	public void SetVoxel(int x, int y, int z, bool state)
	{
		voxels[z * resolution2 + y * resolution + x].state = state;
	}

	public void SetVoxelCrossings(int x, int y, int z, float interpX, float interpY, float interpZ)
	{
		voxels[z * resolution2 + y * resolution + x].SetCrossings(interpX, interpY, interpZ);
	}

	public int VoxelIndex(int x, int y, int z)
	{
		return z * resolution2 + y * resolution + x;
	}

	private void SetVoxelColors()
	{
		for(int i = 0; i < voxelMaterials.Count; i++)
		{
			voxelMaterials[i].color = voxels[i].state ? Color.white : Color.grey;
		}
	}

	public void Refresh()
	{
		SetVoxelColors();
		Triangulate();
		
		meshFilter.mesh = crispMesh;
		meshFilter.mesh.RecalculateNormals();
		meshCollider.sharedMesh = smoothMesh;
	}

	private void Triangulate()
	{
		smoothMesh.Clear();
		smoothVertices.Clear();
		smoothTriangles.Clear();
		crispMesh.Clear();
		crispVertices.Clear();
		crispTriangles.Clear();

		TriangulateCellRows();

		smoothMesh.vertices = smoothVertices.ToArray();
		smoothMesh.triangles = smoothTriangles.ToArray();
		crispMesh.vertices = crispVertices.ToArray();
		crispMesh.triangles = crispTriangles.ToArray();
	}

	private void TriangulateCellRows()
	{
		int cells = resolution - 1;
		CacheFirstZ();              //punkt 0 Z
		int i, z, y, x;
		for(i = 0, z = 0; z < cells; z++, i = i + resolution)
		{
			CacheSwapXZ();			//punkt 1 Z
			CacheFirstY(z);			//punkt 0 Y
			for(y = 0; y < cells; y++, i++)
			{
				CacheSwapY();       //punkt 1Y
				CacheFirstX(z, y);
				for(x = 0; x < cells; x++, i++)
				{
					CacheZRow(x, y + 1, voxels[i + resolution2 + resolution], voxels[(i + 1) + resolution2 + resolution]);
					CacheMdrY1(x + 1, voxels[i + 1 + resolution], voxels[i + 1 + resolution2 + resolution]);
					CacheMdrZY(x + 1, y, voxels[i + 1 + resolution2], voxels[i + 1 + resolution2 + resolution]);
					TriangulateCell(i,
									voxels[i],
									voxels[i + 1],
									voxels[i + resolution],
									voxels[i + resolution + 1],
									voxels[i + (resolution2)],
									voxels[i + (resolution2) + 1],
									voxels[i + (resolution2) + resolution],
									voxels[i + (resolution2) + resolution + 1]);
				}
				if(neighbors != null && neighbors[14] != null) CacheTriangulateGapCell(z, y, x); //fill gap X cell
			}
			if(neighbors != null && neighbors[16] != null) CacheTriangulateGapRow(z, y);
		}
		if(neighbors != null && neighbors[22] != null) CacheTriangulateGapWall(z);
	}

	private void TriangulateCell(int i, Voxel a, Voxel b, Voxel c, Voxel d, Voxel e, Voxel f, Voxel g, Voxel h)
	{
		int i_div_res = (i % resolution2) / resolution;
		int i_mod_res = i % resolution;
		int[] indicesTable = new int[] {    rowX[i_div_res][i_mod_res], mdrY0[i_mod_res], rowZ[i_div_res][i_mod_res], mdrY0[i_mod_res + 1],
											mdrXY[i_div_res][i_mod_res], mdrZY[i_div_res][i_mod_res], mdrZY[i_div_res][i_mod_res + 1], mdrXY[i_div_res][i_mod_res + 1],
											rowX[i_div_res + 1][i_mod_res], mdrY1[i_mod_res], rowZ[i_div_res + 1][i_mod_res], mdrY1[i_mod_res + 1]
																																										};

		Vector3[] verticesTable = new Vector3[] {   a.xEdgePosition, a.zEdgePosition, e.xEdgePosition, b.zEdgePosition,
													a.yEdgePosition, e.yEdgePosition, f.yEdgePosition, b.yEdgePosition,
													c.xEdgePosition, c.zEdgePosition, g.xEdgePosition, d.zEdgePosition};

		int cellType = 0;
		if(a.state) cellType |= 1;
		if(b.state) cellType |= 2;
		if(c.state) cellType |= 4;
		if(d.state) cellType |= 8;
		if(e.state) cellType |= 16;
		if(f.state) cellType |= 32;
		if(g.state) cellType |= 64;
		if(h.state) cellType |= 128;
		for(int j = 0; triangleOrder[cellType] != null && j < triangleOrder[cellType].Length; j++)
		{
			//smoothVertices.Add(verticesTable[triangleOrder[cellType][j]]);
			smoothTriangles.Add(indicesTable[triangleOrder[cellType][j]]);
			crispTriangles.Add(crispVertices.Count);
			crispVertices.Add(verticesTable[triangleOrder[cellType][j]]);
		}
	}
	
	private void CacheTriangulateGapCell(int z, int y, int x)
	{
		dummyX1.BecomeXDummyOf(neighbors[14].voxels[z * resolution2 + y * resolution], sizeResolution);
		dummyX2.BecomeXDummyOf(neighbors[14].voxels[(z + 1) * resolution2 + y * resolution], sizeResolution);
		dummyY1.BecomeXDummyOf(neighbors[14].voxels[z * resolution2 + (y + 1) * resolution], sizeResolution);
		dummyY2.BecomeXDummyOf(neighbors[14].voxels[(z + 1) * resolution2 + (y + 1) * resolution], sizeResolution);

		CacheZRow(x, y + 1, voxels[(z + 1) * resolution2 + (y + 1) * resolution + x], dummyY2);
		CacheMdrY1(x + 1, dummyY1, dummyY2);
		CacheMdrZY(x + 1, y, dummyX2, dummyY2);


		TriangulateCell(z * resolution2 + y * resolution + x,
						voxels[z * resolution2 + y * resolution + x],
						dummyX1,
						voxels[z * resolution2 + (y + 1) * resolution + x],
						dummyY1,
						voxels[(z + 1) * resolution2 + y * resolution + x],
						dummyX2,
						voxels[(z + 1) * resolution2 + (y + 1) * resolution + x],
						dummyY2);
	}
	
	private void CacheTriangulateGapRow(int z, int y)
	{
		//CacheFirstX;
		CacheSwapY();
		dummyY1.BecomeYDummyOf(neighbors[16].voxels[z * resolution2], sizeResolution);
		dummyZ1.BecomeYDummyOf(neighbors[16].voxels[(z + 1) * resolution2], sizeResolution);
		CacheMdrZY(0, y, voxels[(z + 1) * resolution2 + y * resolution], dummyZ1);
		CacheMdrY1(0, dummyY1, dummyZ1);
		

		int x;
		for(x = 0; x < resolution - 1; x++)
		{
			dummyZ1.BecomeYDummyOf(neighbors[16].voxels[(z + 1) * resolution2 + x], sizeResolution);
			dummyZ2.BecomeYDummyOf(neighbors[16].voxels[(z + 1) * resolution2 + x + 1], sizeResolution);
			dummyY1.BecomeYDummyOf(neighbors[16].voxels[z * resolution2 + x], sizeResolution);
			dummyY2.BecomeYDummyOf(neighbors[16].voxels[z * resolution2 + x + 1], sizeResolution);

			CacheZRow(x, y + 1, dummyZ1, dummyZ2);
			CacheMdrY1(x + 1, dummyY2, dummyZ2);
			CacheMdrZY(x + 1, y, voxels[(z + 1) * resolution2 + y * resolution + x + 1], dummyZ2);

			TriangulateCell(z * resolution2 + y * resolution + x,
							voxels[z * resolution2 + y * resolution + x],
							voxels[z * resolution2 + y * resolution + x + 1],
							dummyY1,
							dummyY2,
							voxels[(z + 1) * resolution2 + y * resolution + x],
							voxels[(z + 1) * resolution2 + y * resolution + x + 1],
							dummyZ1,
							dummyZ2);
		}

		if(neighbors != null && neighbors[14] && neighbors[17] != null)
		{
			dummyX1.BecomeXDummyOf(neighbors[14].voxels[z * resolution2 + y * resolution], sizeResolution);
			dummyX2.BecomeXDummyOf(neighbors[14].voxels[(z + 1) * resolution2 + y * resolution], sizeResolution);
			dummyZ1.BecomeYDummyOf(neighbors[16].voxels[(z + 1) * resolution2 + x], sizeResolution);
			dummyZ2.BecomeXYDummyOf(neighbors[17].voxels[(z + 1) * resolution2], sizeResolution);
			dummyY1.BecomeYDummyOf(neighbors[16].voxels[z * resolution2 + x], sizeResolution);
			dummyY2.BecomeXYDummyOf(neighbors[17].voxels[z * resolution2], sizeResolution);

			CacheZRow(x, y + 1, dummyZ1, dummyZ2);
			CacheMdrY1(x + 1, dummyY2, dummyZ2);
			CacheMdrZY(x + 1, y, dummyX2, dummyZ2);

			TriangulateCell(z * resolution2 + y * resolution + x,
							voxels[z * resolution2 + y * resolution + x],
							dummyX1,
							dummyY1,
							dummyY2,
							voxels[(z + 1) * resolution2 + y * resolution + x],
							dummyX2,
							dummyZ1,
							dummyZ2);
		}
	}
	
	private void CacheTriangulateGapWall(int z)
	{
		
		CacheSwapXZ();
		int x, y;
		//CacheFirstY;
		for(x = 0; x < resolution - 1; x++) 
		{
			dummyZ1.BecomeZDummyOf(neighbors[22].voxels[x], sizeResolution);
			dummyZ2.BecomeZDummyOf(neighbors[22].voxels[x + 1], sizeResolution);
			CacheZRow(x, 0, dummyZ1, dummyZ2);
			CacheMdrY1(x, voxels[z * resolution2 + x], dummyZ1);
		}
		CacheMdrY1(x, voxels[z * resolution2 + x], dummyZ2);
		if(neighbors != null && neighbors[14] != null && neighbors[23] != null)
		{
			dummyX1.BecomeXDummyOf(neighbors[14].voxels[z * resolution2], sizeResolution);
			dummyZ1.BecomeZDummyOf(neighbors[22].voxels[x], sizeResolution);
			dummyZ2.BecomeXZDummyOf(neighbors[23].voxels[0], sizeResolution);
			CacheZRow(x, 0, dummyZ1, dummyZ2);
			CacheMdrY1(x + 1, dummyX1, dummyZ2);
		}
		//

		for(y = 0; y < resolution - 1; y++)
		{
			
			CacheSwapY();
			//CacheFirstX
			dummyZ1.BecomeZDummyOf(neighbors[22].voxels[y * resolution], sizeResolution);
			dummyY1.BecomeZDummyOf(neighbors[22].voxels[(y + 1) * resolution], sizeResolution);
			CacheMdrZY(0, y, dummyZ1, dummyY1);
			CacheMdrY1(0, voxels[z * resolution2 + (y + 1) * resolution], dummyY1);
			//

			for(x = 0; x < resolution - 1; x++)
			{
				dummyY1.BecomeZDummyOf(neighbors[22].voxels[(y + 1) * resolution + x], sizeResolution);
				dummyY2.BecomeZDummyOf(neighbors[22].voxels[(y + 1) * resolution + x + 1], sizeResolution);
				dummyX1.BecomeZDummyOf(neighbors[22].voxels[y * resolution + x], sizeResolution);
				dummyX2.BecomeZDummyOf(neighbors[22].voxels[y * resolution + x + 1], sizeResolution);
				CacheZRow(x, y + 1, dummyY1, dummyY2);
				CacheMdrY1(x + 1, voxels[z * resolution2 + (y + 1) * resolution + x + 1], dummyY2);
				CacheMdrZY(x + 1, y, dummyX2, dummyY2);
				TriangulateCell(z * resolution2 + y * resolution + x,
								voxels[z * resolution2 + y * resolution + x],
								voxels[z * resolution2 + y * resolution + x + 1],
								voxels[z * resolution2 + (y + 1) * resolution + x],
								voxels[z * resolution2 + (y + 1) * resolution + x + 1],
								dummyX1,
								dummyX2,
								dummyY1,
								dummyY2);
			}
			if(neighbors != null && neighbors[14] != null && neighbors[23] != null)
			{
				dummyY1.BecomeZDummyOf(neighbors[22].voxels[(y + 1) * resolution + x], sizeResolution);
				dummyY2.BecomeXZDummyOf(neighbors[23].voxels[(y + 1) * resolution], sizeResolution);
				dummyZ1.BecomeZDummyOf(neighbors[22].voxels[y * resolution + x], sizeResolution);
				dummyZ2.BecomeXZDummyOf(neighbors[23].voxels[y * resolution], sizeResolution);
				dummyX1.BecomeXDummyOf(neighbors[14].voxels[z * resolution2 + y * resolution], sizeResolution);
				dummyX2.BecomeXDummyOf(neighbors[14].voxels[z * resolution2 + (y + 1) * resolution], sizeResolution);

				CacheZRow(x, y + 1, dummyY1, dummyY2);
				CacheMdrY1(x + 1, dummyX2, dummyY2);
				CacheMdrZY(x + 1, y, dummyZ2, dummyY2);

				TriangulateCell(z * resolution2 + y * resolution + x,
								voxels[z * resolution2 + y * resolution + x],
								dummyX1,
								voxels[z * resolution2 + (y + 1) * resolution + x],
								dummyX2,
								dummyZ1,
								dummyZ2,
								dummyY1,
								dummyY2);
			}
		}
		if(neighbors != null && neighbors[16] != null && neighbors[25] != null)
		{
			//CacheFirstX;
			CacheSwapY();
			dummyY1.BecomeYDummyOf(neighbors[16].voxels[z * resolution2], sizeResolution);
			dummyZ1.BecomeZDummyOf(neighbors[22].voxels[y * resolution], sizeResolution);
			dummyZ2.BecomeYZDummyOf(neighbors[25].voxels[0], sizeResolution);
			CacheMdrZY(0, y, dummyZ1, dummyZ2);
			CacheMdrY1(0, dummyY1, dummyZ2);

			for(x = 0; x < resolution - 1; x++)
			{
				dummyY1.BecomeYZDummyOf(neighbors[25].voxels[x], sizeResolution);
				dummyY2.BecomeYZDummyOf(neighbors[25].voxels[x + 1], sizeResolution);
				dummyZ1.BecomeZDummyOf(neighbors[22].voxels[y * resolution + x], sizeResolution);
				dummyZ2.BecomeZDummyOf(neighbors[22].voxels[y * resolution + x + 1], sizeResolution);
				dummyX1.BecomeYDummyOf(neighbors[16].voxels[z * resolution2 + x], sizeResolution);
				dummyX2.BecomeYDummyOf(neighbors[16].voxels[z * resolution2 + x + 1], sizeResolution);

				CacheZRow(x, y + 1, dummyY1, dummyY2);
				CacheMdrY1(x + 1, dummyX2, dummyY2);
				CacheMdrZY(x + 1, y, dummyZ2, dummyY2);

				TriangulateCell(z * resolution2 + y * resolution + x,
								voxels[z * resolution2 + y * resolution + x],
								voxels[z * resolution2 + y * resolution + x + 1],
								dummyX1,
								dummyX2,
								dummyZ1,
								dummyZ2,
								dummyY1,
								dummyY2);
			}
			if(neighbors != null && neighbors[14] && neighbors[17] != null && neighbors[23] != null && neighbors[26] != null)
			{
				dummyY1.BecomeYZDummyOf(neighbors[25].voxels[x], sizeResolution);
				dummyY2.BecomeXYZDummyOf(neighbors[26].voxels[0], sizeResolution);
				dummyZ1.BecomeZDummyOf(neighbors[22].voxels[y * resolution + x], sizeResolution);
				dummyZ2.BecomeXZDummyOf(neighbors[23].voxels[y * resolution], sizeResolution);
				dummyX1.BecomeYDummyOf(neighbors[16].voxels[z * resolution2 + x], sizeResolution);
				dummyX2.BecomeXYDummyOf(neighbors[17].voxels[z * resolution2], sizeResolution);
				dummyT1.BecomeXDummyOf(neighbors[14].voxels[z * resolution2 + y * resolution], sizeResolution);

				CacheZRow(x, y + 1, dummyY1, dummyY2);
				CacheMdrY1(x + 1, dummyX2, dummyY2);
				CacheMdrZY(x + 1, y, dummyZ2, dummyY2);

				TriangulateCell(z * resolution2 + y * resolution + x,
								voxels[z * resolution2 + y * resolution + x],
								dummyT1,
								dummyX1,
								dummyX2,
								dummyZ1,
								dummyZ2,
								dummyY1,
								dummyY2);
			}
		}
	}
	
	private void CacheFirstZ()
	{
		int i, x, y;
		for(y = 0, i = 0; y < resolution - 1; y++)
		{
			for(x = 0; x < resolution - 1; x++, i++)
			{
				CacheZRow(x, y, voxels[i], voxels[i + 1]);
				CacheMdrZY(x, y, voxels[i], voxels[i + resolution]);
			}
			CacheMdrZY(x, y, voxels[i], voxels[i + resolution]);
			if(neighbors != null && neighbors[14] != null)
			{
				dummyX1.BecomeXDummyOf(neighbors[14].voxels[y * resolution], sizeResolution);
				dummyY1.BecomeXDummyOf(neighbors[14].voxels[(y + 1) * resolution], sizeResolution);
				CacheZRow(x, y, voxels[i], dummyX1);
				CacheMdrZY(x + 1, y, dummyX1, dummyY1);
			}
			i++;
		}
		for(x = 0; x < resolution - 1; x++, i++)
		{
			CacheZRow(x, y, voxels[i], voxels[i + 1]);
			if(neighbors != null && neighbors[16] != null)
			{
				dummyY1.BecomeYDummyOf(neighbors[16].voxels[x], sizeResolution);
				CacheMdrZY(x, y, voxels[i], dummyY1);
			}
		}
		if(neighbors != null && neighbors[14] != null)
		{
			dummyX1.BecomeXDummyOf(neighbors[14].voxels[y * resolution], sizeResolution);
			CacheZRow(x, y, voxels[i], dummyX1);
		}
		if(neighbors != null && neighbors[16] != null)
		{
			dummyY1.BecomeYDummyOf(neighbors[16].voxels[x], sizeResolution);
			CacheMdrZY(x, y, voxels[i], dummyY1);
		}
		if((neighbors != null && neighbors[14] && neighbors[17] != null))
		{
			dummyX1.BecomeXDummyOf(neighbors[14].voxels[y * resolution], sizeResolution);
			dummyY1.BecomeXYDummyOf(neighbors[17].voxels[0], sizeResolution);
			CacheMdrZY(x + 1, y, dummyX1, dummyY1);
		}
		if(neighbors != null && neighbors[16] != null)
		{
			for(x = 0; x < resolution - 1; x++)
			{
				dummyX1.BecomeYDummyOf(neighbors[16].voxels[x], sizeResolution);
				dummyX2.BecomeYDummyOf(neighbors[16].voxels[x + 1], sizeResolution);
				CacheZRow(x, y + 1, dummyX1, dummyX2);
			}
			if(neighbors[17] != null)
			{
				dummyX1.BecomeXYDummyOf(neighbors[17].voxels[0], sizeResolution);
				CacheZRow(x, y + 1, dummyX2, dummyX1);
			}
		}
	}

	private void CacheFirstY(int z)
	{
		int i, x;
		for(x = 0, i = z * resolution2; x < resolution - 1; x++, i++)
		{
			CacheZRow(x, 0, voxels[i + resolution2], voxels[(i + 1) + resolution2]);
			CacheMdrY1(x, voxels[i], voxels[i + resolution2]);
		}
		CacheMdrY1(x, voxels[i], voxels[i + resolution2]);

		if(neighbors != null && neighbors[14] != null)
		{
			dummyX1.BecomeXDummyOf(neighbors[14].voxels[z * resolution2], sizeResolution);
			dummyZ1.BecomeXDummyOf(neighbors[14].voxels[(z + 1) * resolution2], sizeResolution);
			CacheZRow(x, 0, voxels[i + resolution2], dummyZ1);
			CacheMdrY1(x + 1, dummyX1, dummyZ1);
		}
	}

	private void CacheFirstX(int z, int y)
	{
		CacheMdrZY(0, y, voxels[(z + 1) * resolution2 + y * resolution], voxels[(z + 1) * resolution2 + (y + 1) * resolution]);
		CacheMdrY1(0, voxels[z * resolution2 + (y + 1) * resolution], voxels[(z + 1) * resolution2 + (y + 1) * resolution]);
	}

	private void CacheZRow(int x, int y, Voxel zMin, Voxel zMax)
	{
		if(zMin.state != zMax.state)
		{
			rowZ[y][x] = smoothVertices.Count;
			smoothVertices.Add(zMin.xEdgePosition);
		}
		else rowZ[y][x] = -1;
	}

	private void CacheMdrZY(int x, int y, Voxel zMin, Voxel yMin)
	{
		if(zMin.state != yMin.state)
		{
			mdrZY[y][x] = smoothVertices.Count;
			smoothVertices.Add(zMin.yEdgePosition);
		}
		else mdrZY[y][x] = -1;
	}

	private void CacheMdrY1(int x, Voxel xMin, Voxel zMin)
	{
		if(xMin.state != zMin.state)
		{
			mdrY1[x] = smoothVertices.Count;
			smoothVertices.Add(xMin.zEdgePosition);
		}
		else mdrY1[x] = -1;
	}

	private void CacheSwapXZ()
	{
		int[][] swapRow = rowX;
		int[][] swapMdr = mdrXY;
		rowX = rowZ;
		rowZ = swapRow;
		mdrXY = mdrZY;
		mdrZY = swapMdr;

	}

	private void CacheSwapY()
	{
		int[] swapMdr = mdrY0;
		mdrY0 = mdrY1;
		mdrY1 = swapMdr;
	}

	public void RandomizeVoxelStates()
	{
		System.Random rnd = new System.Random();
		int rand;
		foreach(Voxel voxel in voxels)
		{
			rand = rnd.Next(0, 2);
			voxel.state = Convert.ToBoolean(rand);
			Debug.Log(rand);
		}
		Refresh();
	}

	public void Set()
	{
		foreach(Voxel voxel in voxels) voxel.state = true;
		Refresh();
	}

	public void DrawDebug()
	{
		foreach(Voxel voxel in voxels)
		{
			if(voxel.state) Gizmos.color = Color.white;
			else Gizmos.color = Color.gray;
			Gizmos.DrawSphere(transform.TransformPoint(voxel.position), 0.05f);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.TransformPoint(voxel.position), transform.TransformPoint(voxel.xEdgePosition));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.TransformPoint(voxel.position), transform.TransformPoint(voxel.yEdgePosition));
			Gizmos.color = Color.green;
			Gizmos.DrawLine(transform.TransformPoint(voxel.position), transform.TransformPoint(voxel.zEdgePosition));
		}
	}

	private void PrecalculateTriangleOrder()
	{
		triangleOrder = new int[256][];

		//wariant 0
		triangleOrder[0] = new int[] { };
		//dop
		triangleOrder[255 - 0] = new int[] { };

		//wariant 1
		triangleOrder[1] = new int[] { 0, 4, 1 };
		triangleOrder[2] = new int[] { 3, 7, 0 };
		triangleOrder[4] = new int[] { 9, 4, 8 };
		triangleOrder[8] = new int[] { 8, 7, 11 };
		triangleOrder[16] = new int[] { 1, 5, 2 };
		triangleOrder[32] = new int[] { 2, 6, 3 };
		triangleOrder[64] = new int[] { 10, 5, 9 };
		triangleOrder[128] = new int[] { 11, 6, 10 };
		//dop
		triangleOrder[255 - 1] = new int[triangleOrder[1].Length];
		triangleOrder[255 - 2] = new int[triangleOrder[1].Length];
		triangleOrder[255 - 4] = new int[triangleOrder[1].Length];
		triangleOrder[255 - 8] = new int[triangleOrder[1].Length];
		triangleOrder[255 - 16] = new int[triangleOrder[1].Length];
		triangleOrder[255 - 32] = new int[triangleOrder[1].Length];
		triangleOrder[255 - 64] = new int[triangleOrder[1].Length];
		triangleOrder[255 - 128] = new int[triangleOrder[1].Length];
		for(int i = 0; i < triangleOrder[1].Length; i++)
		{
			triangleOrder[255 - 1][i] = triangleOrder[1][triangleOrder[1].Length - 1 - i];
			triangleOrder[255 - 2][i] = triangleOrder[2][triangleOrder[1].Length - 1 - i];
			triangleOrder[255 - 4][i] = triangleOrder[4][triangleOrder[1].Length - 1 - i];
			triangleOrder[255 - 8][i] = triangleOrder[8][triangleOrder[1].Length - 1 - i];
			triangleOrder[255 - 16][i] = triangleOrder[16][triangleOrder[1].Length - 1 - i];
			triangleOrder[255 - 32][i] = triangleOrder[32][triangleOrder[1].Length - 1 - i];
			triangleOrder[255 - 64][i] = triangleOrder[64][triangleOrder[1].Length - 1 - i];
			triangleOrder[255 - 128][i] = triangleOrder[128][triangleOrder[1].Length - 1 - i];
		}

		//wariant 2
		triangleOrder[3] = new int[] { 1, 7, 4, 1, 3, 7 };
		triangleOrder[17] = new int[] { 2, 4, 5, 2, 0, 4 };
		triangleOrder[48] = new int[] { 3, 5, 6, 3, 1, 5 };
		triangleOrder[34] = new int[] { 0, 6, 7, 0, 2, 6 };
		triangleOrder[5] = new int[] { 1, 8, 9, 1, 0, 8 };
		triangleOrder[80] = new int[] { 2, 9, 10, 2, 1, 9 };
		triangleOrder[160] = new int[] { 3, 10, 11, 3, 2, 10 };
		triangleOrder[10] = new int[] { 0, 11, 8, 0, 3, 11 };
		triangleOrder[12] = new int[] { 4, 11, 9, 4, 7, 11 };
		triangleOrder[68] = new int[] { 5, 8, 10, 5, 4, 8 };
		triangleOrder[192] = new int[] { 6, 9, 11, 6, 5, 9 };
		triangleOrder[136] = new int[] { 7, 10, 8, 7, 6, 10 };
		//dop
		triangleOrder[255 - 3] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 17] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 48] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 34] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 5] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 80] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 160] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 10] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 12] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 68] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 192] = new int[triangleOrder[3].Length];
		triangleOrder[255 - 136] = new int[triangleOrder[3].Length];
		for(int i = 0; i < triangleOrder[3].Length; i++)
		{
			triangleOrder[255 - 3][i] = triangleOrder[3][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 17][i] = triangleOrder[17][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 48][i] = triangleOrder[48][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 34][i] = triangleOrder[34][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 5][i] = triangleOrder[5][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 80][i] = triangleOrder[80][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 160][i] = triangleOrder[160][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 10][i] = triangleOrder[10][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 12][i] = triangleOrder[12][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 68][i] = triangleOrder[68][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 192][i] = triangleOrder[192][triangleOrder[3].Length - 1 - i];
			triangleOrder[255 - 136][i] = triangleOrder[136][triangleOrder[3].Length - 1 - i];
		}

		//wariant 3
		triangleOrder[9] = new int[] { 0, 4, 1, 8, 7, 11 };
		triangleOrder[20] = new int[] { 1, 5, 2, 9, 4, 8 };
		triangleOrder[96] = new int[] { 2, 6, 3, 10, 5, 9 };
		triangleOrder[130] = new int[] { 3, 7, 0, 11, 6, 10 };
		triangleOrder[6] = new int[] { 3, 7, 0, 9, 4, 8 };
		triangleOrder[65] = new int[] { 0, 4, 1, 10, 5, 9 };
		triangleOrder[144] = new int[] { 1, 5, 2, 11, 6, 10 };
		triangleOrder[40] = new int[] { 2, 6, 3, 8, 7, 11 };
		triangleOrder[132] = new int[] { 9, 4, 8, 11, 6, 10 };
		triangleOrder[72] = new int[] { 8, 7, 11, 10, 5, 9 };
		triangleOrder[33] = new int[] { 0, 4, 1, 2, 6, 3 };
		triangleOrder[18] = new int[] { 3, 7, 0, 1, 5, 2 };
		//dop ambiguous NIBY OK NIBY NIE OK!
		triangleOrder[246] = new int[] { 1, 7, 0, 1, 11, 7, 1, 8, 11, 1, 4, 8 };
		triangleOrder[235] = new int[] { 2, 4, 1, 2, 8, 4, 2, 9, 8, 2, 5, 9 };
		triangleOrder[159] = new int[] { 3, 5, 2, 3, 9, 5, 3, 10, 9, 3, 6, 10 };
		triangleOrder[125] = new int[] { 0, 6, 3, 0, 10, 6, 0, 11, 10, 0, 7, 11 };
		triangleOrder[249] = new int[] { 3, 0, 4, 3, 4, 9, 3, 9, 8, 3, 8, 7 };
		triangleOrder[190] = new int[] { 0, 1, 5, 0, 5, 10, 0, 10, 9, 0, 9, 4 };
		triangleOrder[111] = new int[] { 1, 2, 6, 1, 6, 11, 1, 11, 10, 1, 10, 5 };
		triangleOrder[215] = new int[] { 2, 3, 7, 2, 7, 8, 2, 8, 11, 2, 11, 6 };
		triangleOrder[123] = new int[] { 4, 9, 10, 4, 10, 6, 4, 6, 11, 4, 11, 8 };
		triangleOrder[183] = new int[] { 7, 8, 9, 7, 9, 5, 7, 5, 10, 7, 10, 11 };
		triangleOrder[222] = new int[] { 4, 0, 3, 4, 3, 6, 4, 6, 2, 4, 2, 1 };
		triangleOrder[237] = new int[] { 7, 3, 2, 7, 2, 5, 7, 5, 1, 7, 1, 0 };
		/*triangleOrder[255 - 9] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 20] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 96] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 130] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 6] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 65] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 144] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 40] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 132] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 72] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 33] = new int[triangleOrder[9].Length];
		triangleOrder[255 - 18] = new int[triangleOrder[9].Length];
		for(int i = 0; i < triangleOrder[9].Length; i++)
		{
			triangleOrder[255 - 9][i] = triangleOrder[9][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 20][i] = triangleOrder[20][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 96][i] = triangleOrder[96][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 130][i] = triangleOrder[130][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 6][i] = triangleOrder[6][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 65][i] = triangleOrder[65][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 144][i] = triangleOrder[144][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 40][i] = triangleOrder[40][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 132][i] = triangleOrder[132][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 72][i] = triangleOrder[72][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 33][i] = triangleOrder[33][triangleOrder[9].Length - 1 - i];
			triangleOrder[255 - 18][i] = triangleOrder[18][triangleOrder[9].Length - 1 - i];
		}*/

		//wariant 4
		triangleOrder[50] = new int[] { 7, 5, 6, 7, 1, 5, 7, 0, 1 };
		triangleOrder[35] = new int[] { 4, 6, 7, 4, 2, 6, 4, 1, 2 };
		triangleOrder[49] = new int[] { 6, 4, 5, 6, 0, 4, 6, 3, 0 };
		triangleOrder[19] = new int[] { 5, 7, 4, 5, 2, 3, 5, 3, 7 };
		triangleOrder[200] = new int[] { 5, 7, 6, 5, 9, 8, 5, 8, 7 };
		triangleOrder[140] = new int[] { 6, 4, 7, 6, 10, 9, 6, 9, 4 };
		triangleOrder[76] = new int[] { 7, 5, 4, 7, 11, 10, 7, 10, 5 };
		triangleOrder[196] = new int[] { 4, 6, 5, 4, 8, 11, 4, 11, 6 };
		triangleOrder[7] = new int[] { 3, 9, 1, 3, 7, 8, 3, 8, 9 };
		triangleOrder[81] = new int[] { 0, 10, 2, 0, 4, 9, 0, 9, 10 };
		triangleOrder[176] = new int[] { 1, 11, 3, 1, 5, 10, 1, 10, 11 };
		triangleOrder[42] = new int[] { 2, 8, 0, 2, 6, 11, 2, 11, 8 };
		triangleOrder[13] = new int[] { 1, 11, 9, 1, 0, 7, 1, 7, 11 };
		triangleOrder[84] = new int[] { 2, 8, 10, 2, 1, 4, 2, 4, 8 };
		triangleOrder[224] = new int[] { 3, 9, 11, 3, 2, 5, 3, 5, 9 };
		triangleOrder[138] = new int[] { 0, 10, 8, 0, 3, 6, 0, 6, 10 };
		triangleOrder[168] = new int[] { 8, 2, 10, 8, 7, 3, 8, 3, 2 };
		triangleOrder[14] = new int[] { 9, 3, 11, 9, 4, 0, 9, 0, 3 };
		triangleOrder[69] = new int[] { 10, 0, 8, 10, 5, 1, 10, 1, 0 };
		triangleOrder[208] = new int[] { 11, 1, 9, 11, 6, 2, 11, 2, 1 };
		triangleOrder[11] = new int[] { 11, 1, 3, 11, 8, 4, 11, 4, 1 };
		triangleOrder[21] = new int[] { 8, 2, 0, 8, 9, 5, 8, 5, 2 };
		triangleOrder[112] = new int[] { 9, 3, 1, 9, 10, 6, 9, 6, 3 };
		triangleOrder[162] = new int[] { 10, 0, 2, 10, 11, 7, 10, 7, 0 };
		//dop
		triangleOrder[255 - 50] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 35] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 49] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 19] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 200] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 140] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 76] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 196] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 7] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 81] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 176] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 42] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 13] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 84] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 224] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 138] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 168] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 14] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 69] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 208] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 11] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 21] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 112] = new int[triangleOrder[50].Length];
		triangleOrder[255 - 162] = new int[triangleOrder[50].Length];
		for(int i = 0; i < triangleOrder[50].Length; i++)
		{
			triangleOrder[255 - 50][i] = triangleOrder[50][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 35][i] = triangleOrder[35][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 49][i] = triangleOrder[49][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 19][i] = triangleOrder[19][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 200][i] = triangleOrder[200][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 140][i] = triangleOrder[140][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 76][i] = triangleOrder[76][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 196][i] = triangleOrder[196][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 7][i] = triangleOrder[7][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 81][i] = triangleOrder[81][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 176][i] = triangleOrder[176][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 42][i] = triangleOrder[42][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 13][i] = triangleOrder[13][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 84][i] = triangleOrder[84][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 224][i] = triangleOrder[224][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 138][i] = triangleOrder[138][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 168][i] = triangleOrder[168][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 14][i] = triangleOrder[14][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 69][i] = triangleOrder[69][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 208][i] = triangleOrder[208][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 11][i] = triangleOrder[11][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 21][i] = triangleOrder[21][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 112][i] = triangleOrder[112][triangleOrder[50].Length - 1 - i];
			triangleOrder[255 - 162][i] = triangleOrder[162][triangleOrder[50].Length - 1 - i];
		}

		//wariant 5
		triangleOrder[51] = new int[] { 4, 5, 6, 4, 6, 7 };
		triangleOrder[204] = new int[] { 7, 6, 4, 6, 5, 4 };
		triangleOrder[15] = new int[] { 3, 11, 1, 11, 9, 1 };
		triangleOrder[240] = new int[] { 1, 9, 11, 1, 11, 3 };
		triangleOrder[85] = new int[] { 8, 10, 2, 8, 2, 0 };
		triangleOrder[170] = new int[] { 0, 2, 8, 2, 10, 8 };

		//wariant 6
		triangleOrder[50 + 4] = new int[] { 7, 5, 6, 7, 1, 5, 7, 0, 1, 9, 4, 8 };
		triangleOrder[35 + 64] = new int[] { 4, 6, 7, 4, 2, 6, 4, 1, 2, 10, 5, 9 };
		triangleOrder[49+8] = new int[] { 6, 4, 5, 6, 0, 4, 6, 3, 0, 8, 7, 11 };
		triangleOrder[19+128] = new int[] { 5, 7, 4, 5, 2, 3, 5, 3, 7, 11, 6, 10 };
		triangleOrder[200+1] = new int[] { 5, 7, 6, 5, 9, 8, 5, 8, 7, 0, 4, 1 };
		triangleOrder[140+16] = new int[] { 6, 4, 7, 6, 10, 9, 6, 9, 4, 1, 5, 2 };
		triangleOrder[76+32] = new int[] { 7, 5, 4, 7, 11, 10, 7, 10, 5, 2, 6, 3 };
		triangleOrder[196+2] = new int[] { 4, 6, 5, 4, 8, 11, 4, 11, 6, 3, 7, 0 };
		triangleOrder[7+128] = new int[] { 3, 9, 1, 3, 7, 8, 3, 8, 9, 11, 6, 10 };
		triangleOrder[81+8] = new int[] { 0, 10, 2, 0, 4, 9, 0, 9, 10, 8, 7, 11 };
		triangleOrder[176+4] = new int[] { 1, 11, 3, 1, 5, 10, 1, 10, 11, 9, 4, 8 };
		triangleOrder[42+64] = new int[] { 2, 8, 0, 2, 6, 11, 2, 11, 8, 10, 5, 9 };
		triangleOrder[13+32] = new int[] { 1, 11, 9, 1, 0, 7, 1, 7, 11, 2, 6, 3 };
		triangleOrder[84+2] = new int[] { 2, 8, 10, 2, 1, 4, 2, 4, 8, 3, 7, 0 };
		triangleOrder[224+1] = new int[] { 3, 9, 11, 3, 2, 5, 3, 5, 9, 0, 4, 1 };
		triangleOrder[138+16] = new int[] { 0, 10, 8, 0, 3, 6, 0, 6, 10, 1, 5, 2 };
		triangleOrder[168+1] = new int[] { 8, 2, 10, 8, 7, 3, 8, 3, 2, 0, 4, 1 };
		triangleOrder[14+16] = new int[] { 9, 3, 11, 9, 4, 0, 9, 0, 3, 1, 5, 2 };
		triangleOrder[69+32] = new int[] { 10, 0, 8, 10, 5, 1, 10, 1, 0, 2, 6, 3 };
		triangleOrder[208+2] = new int[] { 11, 1, 9, 11, 6, 2, 11, 2, 1, 3, 7, 0 };
		triangleOrder[11+64] = new int[] { 11, 1, 3, 11, 8, 4, 11, 4, 1, 10, 5, 9 };
		triangleOrder[21+128] = new int[] { 8, 2, 0, 8, 9, 5, 8, 5, 2, 11, 6, 10 };
		triangleOrder[112+8] = new int[] { 9, 3, 1, 9, 10, 6, 9, 6, 3, 8, 7, 11 };
		triangleOrder[162+4] = new int[] { 10, 0, 2, 10, 11, 7, 10, 7, 0, 9, 4, 8 };

		//wariant 7
		triangleOrder[105] = new int[] { 0, 4, 1, 8, 7, 11, 2, 6, 3, 10, 5, 9 };
		triangleOrder[150] = new int[] { 3, 7, 0, 9, 4, 8, 1, 5, 2, 11, 6, 10 };

		//wariant 8
		triangleOrder[43] = new int[] { 8, 6, 11, 8, 2, 6, 2, 8, 4, 2, 4, 1 };
		triangleOrder[23] = new int[] { 9, 7, 8, 9, 3, 7, 3, 9, 5, 3, 5, 2 };
		triangleOrder[113] = new int[] { 10, 4, 9, 10, 0, 4, 0, 10, 6, 0, 6, 3 };
		triangleOrder[178] = new int[] { 11, 5, 10, 11, 1, 5, 1, 11, 7, 1, 7, 0 };
		triangleOrder[142] = new int[] { 3, 4, 0, 3, 9, 4, 9, 3, 6, 9, 6, 10 };
		triangleOrder[77] = new int[] { 0, 5, 1, 0, 10, 5, 10, 0, 7, 10, 7, 11 };
		triangleOrder[212] = new int[] { 1, 6, 2, 1, 11, 6, 11, 1, 4, 11, 4, 8 };
		triangleOrder[232] = new int[] { 2, 7, 3, 2, 8, 7, 8, 2, 5, 8, 5, 9 };

		//wariant 9, 15
		triangleOrder[163] = new int[] { 7, 10, 11, 7, 1, 10, 7, 4, 1, 1, 2, 10 };
		triangleOrder[27] = new int[] { 4, 11, 8, 4, 2, 11, 4, 5, 2, 2, 3, 11 };
		triangleOrder[53] = new int[] { 5, 8, 9, 5, 3, 8, 5, 6, 3, 3, 0, 8 };
		triangleOrder[114] = new int[] { 6, 9, 10, 6, 0, 9, 6, 7, 0, 0, 1, 9 };
		triangleOrder[172] = new int[] { 7, 3, 2, 7, 2, 9, 7, 9, 4, 9, 2, 10 };
		triangleOrder[78] = new int[] { 4, 0, 3, 4, 3, 10, 4, 10, 5, 10, 3, 11 };
		triangleOrder[197] = new int[] { 5, 1, 0, 5, 0, 11, 5, 11, 6, 11, 0, 8 };
		triangleOrder[216] = new int[] { 6, 2, 1, 6, 1, 8, 6, 8, 7, 8, 1, 9 };
		triangleOrder[83] = new int[] { 4, 9, 10, 4, 10, 3, 4, 3, 7, 3, 10, 2 };
		triangleOrder[177] = new int[] { 5, 10, 11, 5, 11, 0, 5, 0, 4, 0, 11, 3 };
		triangleOrder[58] = new int[] { 6, 11, 8, 6, 8, 1, 6, 1, 5, 1, 8, 0 };
		triangleOrder[39] = new int[] { 7, 8, 9, 7, 9, 2, 7, 2, 6, 2, 9, 1 };
		triangleOrder[92] = new int[] { 4, 7, 11, 4, 11, 2, 4, 2, 1, 10, 2, 11 };
		triangleOrder[228] = new int[] { 5, 4, 8, 5, 8, 3, 5, 3, 2, 11, 3, 8 };
		triangleOrder[202] = new int[] { 6, 5, 9, 6, 9, 0, 6, 0, 3, 8, 0, 9 };
		triangleOrder[141] = new int[] { 7, 6, 10, 7, 10, 1, 7, 1, 0, 9, 1, 10 };
		triangleOrder[71] = new int[] { 1, 3, 7, 1, 7, 10, 1, 10, 5, 8, 10, 7 };
		triangleOrder[209] = new int[] { 2, 0, 4, 2, 4, 11, 2, 11, 6, 9, 11, 4 };
		triangleOrder[184] = new int[] { 3, 1, 5, 3, 5, 8, 3, 8, 7, 10, 8, 5 };
		triangleOrder[46] = new int[] { 0, 2, 6, 0, 6, 9, 0, 9, 4, 11, 9, 6 };
		triangleOrder[226] = new int[] { 2, 5, 9, 2, 9, 7, 2, 7, 0, 7, 9, 11 };
		triangleOrder[139] = new int[] { 3, 6, 10, 3, 10, 4, 3, 4, 1, 4, 10, 8 };
		triangleOrder[29] = new int[] { 0, 7, 11, 0, 11, 5, 0, 5, 2, 5, 11, 9 };
		triangleOrder[116] = new int[] { 1, 4, 8, 1, 8, 6, 1, 6, 3, 6, 8, 10 };

		//wariant 10
		triangleOrder[129] = new int[] { 0, 4, 1, 11, 6, 10 };
		triangleOrder[36] = new int[] { 9, 4, 8, 2, 6, 3 };
		triangleOrder[24] = new int[] { 8, 7, 11, 1, 5, 2 };
		triangleOrder[66] = new int[] { 3, 7, 0, 10, 5, 9 };
		//dop
		triangleOrder[255 - 129] = new int[triangleOrder[129].Length];
		triangleOrder[255 - 36] = new int[triangleOrder[129].Length];
		triangleOrder[255 - 24] = new int[triangleOrder[129].Length];
		triangleOrder[255 - 66] = new int[triangleOrder[129].Length];
		for(int i = 0; i < triangleOrder[129].Length; i++)
		{
			triangleOrder[255 - 129][i] = triangleOrder[129][triangleOrder[129].Length - 1 - i];
			triangleOrder[255 - 36][i] = triangleOrder[36][triangleOrder[129].Length - 1 - i];
			triangleOrder[255 - 24][i] = triangleOrder[24][triangleOrder[129].Length - 1 - i];
			triangleOrder[255 - 66][i] = triangleOrder[66][triangleOrder[129].Length - 1 - i];
		}

		//wariant 11
		triangleOrder[131] = new int[] { 1, 7, 4, 1, 3, 7, 11, 6, 10 };
		triangleOrder[67] = new int[] { 1, 7, 4, 1, 3, 7, 10, 5, 9 };
		triangleOrder[25] = new int[] { 2, 4, 5, 2, 0, 4, 8, 7, 11 };
		triangleOrder[145] = new int[] { 2, 4, 5, 2, 0, 4, 11, 6, 10 };
		triangleOrder[52] = new int[] { 3, 5, 6, 3, 1, 5, 9, 4, 8 };
		triangleOrder[56] = new int[] { 3, 5, 6, 3, 1, 5, 8, 7, 11 };
		triangleOrder[98] = new int[] { 0, 6, 7, 0, 2, 6, 10, 5, 9 };
		triangleOrder[38] = new int[] { 0, 6, 7, 0, 2, 6, 9, 4, 8 };
		triangleOrder[37] = new int[] { 1, 8, 9, 1, 0, 8, 2, 6, 3 };
		triangleOrder[133] = new int[] { 1, 8, 9, 1, 0, 8, 11, 6, 10 };
		triangleOrder[82] = new int[] { 2, 9, 10, 2, 1, 9, 3, 7, 0 };
		triangleOrder[88] = new int[] { 2, 9, 10, 2, 1, 9, 8, 7, 11 };
		triangleOrder[161] = new int[] { 3, 10, 11, 3, 2, 10, 0, 4, 1 };
		triangleOrder[164] = new int[] { 3, 10, 11, 3, 2, 10, 9, 4, 8 };
		triangleOrder[26] = new int[] { 0, 11, 8, 0, 3, 11, 1, 5, 2 };
		triangleOrder[74] = new int[] { 0, 11, 8, 0, 3, 11, 10, 5, 9 };
		triangleOrder[28] = new int[] { 4, 11, 9, 4, 7, 11, 1, 5, 2 };
		triangleOrder[44] = new int[] { 4, 11, 9, 4, 7, 11, 2, 6, 3 };
		triangleOrder[70] = new int[] { 5, 8, 10, 5, 4, 8, 3, 7, 0 };
		triangleOrder[100] = new int[] { 5, 8, 10, 5, 4, 8, 2, 6, 3 };
		triangleOrder[193] = new int[] { 6, 9, 11, 6, 5, 9, 0, 4, 1 };
		triangleOrder[194] = new int[] { 6, 9, 11, 6, 5, 9, 3, 7, 0 };
		triangleOrder[152] = new int[] { 7, 10, 8, 7, 6, 10, 1, 5, 2 };
		triangleOrder[137] = new int[] { 7, 10, 8, 7, 6, 10, 0, 4, 1 };
		//dop
		triangleOrder[255 - 131] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 67] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 25] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 145] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 52] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 56] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 98] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 38] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 37] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 133] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 82] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 88] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 161] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 164] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 26] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 74] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 28] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 44] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 70] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 100] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 193] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 194] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 152] = new int[triangleOrder[131].Length];
		triangleOrder[255 - 137] = new int[triangleOrder[131].Length];
		for(int i = 0; i < triangleOrder[131].Length; i++)
		{
			triangleOrder[255 - 131][i] = triangleOrder[131][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 67][i] = triangleOrder[67][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 25][i] = triangleOrder[25][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 145][i] = triangleOrder[145][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 52][i] = triangleOrder[52][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 56][i] = triangleOrder[56][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 98][i] = triangleOrder[98][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 38][i] = triangleOrder[38][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 37][i] = triangleOrder[37][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 133][i] = triangleOrder[133][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 82][i] = triangleOrder[82][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 88][i] = triangleOrder[88][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 161][i] = triangleOrder[161][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 164][i] = triangleOrder[164][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 26][i] = triangleOrder[26][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 74][i] = triangleOrder[74][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 28][i] = triangleOrder[28][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 44][i] = triangleOrder[44][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 70][i] = triangleOrder[70][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 100][i] = triangleOrder[100][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 193][i] = triangleOrder[193][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 194][i] = triangleOrder[194][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 152][i] = triangleOrder[152][triangleOrder[131].Length - 1 - i];
			triangleOrder[255 - 137][i] = triangleOrder[137][triangleOrder[131].Length - 1 - i];
		}

		//wariant 12
		triangleOrder[134] = new int[] { 3, 7, 0, 9, 4, 8, 11, 6, 10 };
		triangleOrder[73] = new int[] { 0, 4, 1, 10, 5, 9, 8, 7, 11 };
		triangleOrder[148] = new int[] { 1, 5, 2, 9, 4, 8, 11, 6, 10 };
		triangleOrder[104] = new int[] { 2, 6, 3, 10, 5, 9, 8, 7, 11 };
		triangleOrder[41] = new int[] { 0, 4, 1, 8, 7, 11, 2, 6, 3 };
		triangleOrder[22] = new int[] { 1, 5, 2, 9, 4, 8, 3, 7, 0 };
		triangleOrder[97] = new int[] { 0, 4, 1, 10, 5, 9, 2, 6, 3 };
		triangleOrder[146] = new int[] { 1, 5, 2, 11, 6, 10, 3, 7, 0 };
		//dop
		triangleOrder[255 - 134] = new int[triangleOrder[134].Length];
		triangleOrder[255 - 73] = new int[triangleOrder[134].Length];
		triangleOrder[255 - 148] = new int[triangleOrder[134].Length];
		triangleOrder[255 - 104] = new int[triangleOrder[134].Length];
		triangleOrder[255 - 41] = new int[triangleOrder[134].Length];
		triangleOrder[255 - 22] = new int[triangleOrder[134].Length];
		triangleOrder[255 - 97] = new int[triangleOrder[134].Length];
		triangleOrder[255 - 146] = new int[triangleOrder[134].Length];
		for(int i = 0; i < triangleOrder[134].Length; i++)
		{
			triangleOrder[255 - 134][i] = triangleOrder[134][triangleOrder[134].Length - 1 - i];
			triangleOrder[255 - 73][i] = triangleOrder[73][triangleOrder[134].Length - 1 - i];
			triangleOrder[255 - 148][i] = triangleOrder[148][triangleOrder[134].Length - 1 - i];
			triangleOrder[255 - 104][i] = triangleOrder[104][triangleOrder[134].Length - 1 - i];
			triangleOrder[255 - 41][i] = triangleOrder[41][triangleOrder[134].Length - 1 - i];
			triangleOrder[255 - 22][i] = triangleOrder[22][triangleOrder[134].Length - 1 - i];
			triangleOrder[255 - 97][i] = triangleOrder[97][triangleOrder[134].Length - 1 - i];
			triangleOrder[255 - 146][i] = triangleOrder[146][triangleOrder[134].Length - 1 - i];
		}

		//wariant 13
		triangleOrder[165] = new int[] { 8, 9, 1, 8, 1, 0, 10, 11, 3, 10, 3, 2 };
		triangleOrder[90] = new int[] { 11, 8, 0, 11, 0, 3, 9, 10, 2, 9, 2, 1 };
		triangleOrder[195] = new int[] { 4, 1, 3, 4, 3, 7, 11, 6, 5, 11, 5, 9 };
		triangleOrder[60] = new int[] { 9, 4, 7, 9, 7, 11, 6, 3, 1, 6, 1, 5 };
		triangleOrder[153] = new int[] { 8, 7, 6, 8, 6, 10, 0, 4, 5, 0, 5, 2 };
		triangleOrder[102] = new int[] { 7, 0, 2, 7, 2, 6, 10, 5, 4, 10, 4, 8 };
	}
}

/*
	private void TriangulateCell(int xx1, int xx2, int xy1, int xy2, int zz1, int zz2, int zy1, int zy2)
	{
		int[] indicesTable = new int[] {	rowX[][xx1], mdrY0[xx1], rowZ[zz1], mdrY0[xx2],
											mdrXY[xx1], mdrZY[zz1], mdrZY[zz2], mdrXY[xx2],
											rowX[xy1], mdrY1[xy1], rowZ[zy1], mdrY1[xy2]	};
	}
	/*private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d, Voxel e, Voxel f, Voxel g, Voxel h)
	{
		Vector3[] verticesTable = new Vector3[] {   a.xEdgePosition, a.zEdgePosition, e.xEdgePosition, b.zEdgePosition,
													a.yEdgePosition, e.yEdgePosition, f.yEdgePosition, b.yEdgePosition,
													c.xEdgePosition, c.zEdgePosition, g.xEdgePosition, d.zEdgePosition};

		int cellType = 0;
		if(a.state) cellType |= 1;
		if(b.state) cellType |= 2;
		if(c.state) cellType |= 4;
		if(d.state) cellType |= 8;
		if(e.state) cellType |= 16;
		if(f.state) cellType |= 32;
		if(g.state) cellType |= 64;
		if(h.state) cellType |= 128;

		int vertexIndex = smoothVertices.Count;
		for(int i = 0; triangleOrder[cellType] != null && i < triangleOrder[cellType].Length; i++)
		{
			smoothVertices.Add(verticesTable[triangleOrder[cellType][i]]);
			smoothTriangles.Add(vertexIndex + i);
		}
	}*/
