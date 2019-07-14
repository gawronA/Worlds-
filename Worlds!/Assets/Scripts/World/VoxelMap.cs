
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMap : MonoBehaviour
{
	public float size;
	public int voxelResolution = 8;
	public int chunkResolution = 2;

	public VoxelGrid voxelGridPrefab;

	public List<VoxelGrid> chunks;
	private float chunkSize, voxelHalfSize, chunkHalfSize;

	private void Awake()
	{
		chunkSize = size * voxelResolution;
		chunkHalfSize = chunkSize * 0.5f;
		voxelHalfSize = size * 0.5f;
		//voxelSize = chunkSize / voxelResolution;

		chunks = new List<VoxelGrid>();
		for(int i = 0, z = 0; z < chunkResolution; z++)
		{
			for(int y = 0; y < chunkResolution; y++)
			{
				for(int x = 0; x < chunkResolution; x++)
				{
					CreateChunk(i++, x, y, z);
					
					//chunks[i - 1].RandomizeVoxelStates();
					//chunks[i - 1].Set();
				}
			}
		}
		SetNeighbors();
	}

	private void CreateChunk(int index, int x, int y, int z)
	{
		VoxelGrid chunk = Instantiate(voxelGridPrefab) as VoxelGrid;
		chunk.name = "VoxelGrid" + index.ToString();
		chunk.transform.parent = transform;
		chunk.transform.localPosition = new Vector3(x * chunkSize - chunkHalfSize * chunkResolution + voxelHalfSize,
													y * chunkSize - chunkHalfSize * chunkResolution + voxelHalfSize,
													z * chunkSize - chunkHalfSize * chunkResolution + voxelHalfSize);
		chunk.index = index;
		chunk.Initalize(voxelResolution, size);
		chunks.Add(chunk);
	}

	private void SetNeighbors()
	{
		int chunkResolution2 = chunkResolution * chunkResolution;
		int chunkResolution3 = chunkResolution2 * chunkResolution;
		for(int i = 0; i < chunkResolution3; i++)
		{
			chunks[i].neighbors = new VoxelGrid[27];
			for(int z = 0, index = 0; z < 3; z++)
			{
				for(int y = 0; y < 3; y++)
				{
					for(int x = 0; x < 3; x++, index++)
					{
						if(x * y * z == 1) continue;
						int id = -chunkResolution2 - chunkResolution - 1 + x + y * chunkResolution + z * chunkResolution2 + i;
						if(x < 1 && i % chunkResolution == 0) continue;
						if(y < 1 && i % chunkResolution2 < chunkResolution) continue;
						if(z < 1 && i % chunkResolution3 < chunkResolution2) continue;
						if(x > 1 && i % chunkResolution == chunkResolution - 1) continue;
						if(y > 1 && i % chunkResolution2 >= chunkResolution * (chunkResolution - 1)) continue;
						if(z > 1 && i % chunkResolution3 >= chunkResolution2 * (chunkResolution - 1)) continue;
						chunks[i].neighbors[index] = chunks[id];
					}
				}
			}
		}
	}

	public void Refresh()
	{
		for(int i = chunks.Count - 1; i >= 0; i--) chunks[i].Refresh();
		//for(int i = 0; i < chunkResolution; i++) chunks[i].Refresh();
	}
	/*private void EditVoxels(Vector3 point)
	{
		int voxelX = (int)((point.x + chunkHalfSize * chunkResolution) / size);
		int voxelY = (int)((point.y + chunkHalfSize * chunkResolution) / size);
		int voxelZ = (int)((point.z + chunkHalfSize * chunkResolution) / size);
		int chunkX = voxelX / voxelResolution;
		int chunkY = voxelY / voxelResolution;
		int chunkZ = voxelZ / voxelResolution;
		voxelX -= chunkX * voxelResolution;
		voxelY -= chunkY * voxelResolution;
		voxelZ -= chunkZ * voxelResolution;

		int chunkIndex = chunkZ * chunkResolution * chunkResolution + chunkY * chunkResolution + chunkX;
		chunks[chunkIndex].SetVoxelRefresh(voxelX, voxelY, voxelZ, true);

		if(chunks[chunkIndex].voxelWalls[chunks[chunkIndex].VoxelIndex(voxelX, voxelY, voxelZ)] != -1)
		{
			switch(chunks[chunkIndex].voxelWalls[chunks[chunkIndex].VoxelIndex(voxelX, voxelY, voxelZ)])
			{
				case -256:	//top
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					break;
				case -255:	//bottom
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					break;
				case -254:	//front
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					break;
				case -253:	//back
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					break;
				case -252:	//left
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();
					break;
				case -251:	//right
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();
					break;
				case -65024:    //top/front
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					if(chunks[chunkIndex].neighbors[7] != null) chunks[chunkIndex].neighbors[7].Refresh();
					break;
				case -64768:	//top/back
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					if(chunks[chunkIndex].neighbors[25] != null) chunks[chunkIndex].neighbors[25].Refresh();
					break;
				case -64512:	//top/left
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();
					if(chunks[chunkIndex].neighbors[15] != null) chunks[chunkIndex].neighbors[15].Refresh();
					break;
				case -64256:	//top/right
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();
					if(chunks[chunkIndex].neighbors[17] != null) chunks[chunkIndex].neighbors[17].Refresh();
					break;
				case -65023:	//bottom/front
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					if(chunks[chunkIndex].neighbors[1] != null) chunks[chunkIndex].neighbors[1].Refresh();
					break;
				case -64767:	//bottom/back
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					if(chunks[chunkIndex].neighbors[19] != null) chunks[chunkIndex].neighbors[19].Refresh();
					break;
				case -64511:	//bottom/left
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();
					if(chunks[chunkIndex].neighbors[9] != null) chunks[chunkIndex].neighbors[9].Refresh();
					break;
				case -64255:	//bottom/right
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();
					if(chunks[chunkIndex].neighbors[11] != null) chunks[chunkIndex].neighbors[11].Refresh();
					break;
				case -64510:    //front/left
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();
					if(chunks[chunkIndex].neighbors[3] != null) chunks[chunkIndex].neighbors[3].Refresh();
					break;
				case -64254:    //front/right
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();
					if(chunks[chunkIndex].neighbors[5] != null) chunks[chunkIndex].neighbors[5].Refresh();
					break;
				case -64509:    //back/left
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();
					if(chunks[chunkIndex].neighbors[21] != null) chunks[chunkIndex].neighbors[21].Refresh();
					break;
				case -64253:    //back/right
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();
					if(chunks[chunkIndex].neighbors[23] != null) chunks[chunkIndex].neighbors[23].Refresh();
					break;
				case -16514560: //top/front/left
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();

					if(chunks[chunkIndex].neighbors[3] != null) chunks[chunkIndex].neighbors[3].Refresh();
					if(chunks[chunkIndex].neighbors[6] != null) chunks[chunkIndex].neighbors[6].Refresh();
					if(chunks[chunkIndex].neighbors[7] != null) chunks[chunkIndex].neighbors[7].Refresh();
					if(chunks[chunkIndex].neighbors[15] != null) chunks[chunkIndex].neighbors[15].Refresh();
					break;
				case -16449024: //top/front/right
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();

					if(chunks[chunkIndex].neighbors[5] != null) chunks[chunkIndex].neighbors[5].Refresh();
					if(chunks[chunkIndex].neighbors[7] != null) chunks[chunkIndex].neighbors[7].Refresh();
					if(chunks[chunkIndex].neighbors[8] != null) chunks[chunkIndex].neighbors[8].Refresh();
					if(chunks[chunkIndex].neighbors[17] != null) chunks[chunkIndex].neighbors[17].Refresh();
					break;
				case -16514304: //top/back/left
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();

					if(chunks[chunkIndex].neighbors[15] != null) chunks[chunkIndex].neighbors[15].Refresh();
					if(chunks[chunkIndex].neighbors[21] != null) chunks[chunkIndex].neighbors[21].Refresh();
					if(chunks[chunkIndex].neighbors[24] != null) chunks[chunkIndex].neighbors[24].Refresh();
					if(chunks[chunkIndex].neighbors[25] != null) chunks[chunkIndex].neighbors[25].Refresh();
					break;
				case -16448768: //top/back/right
					if(chunks[chunkIndex].neighbors[16] != null) chunks[chunkIndex].neighbors[16].Refresh();
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();

					if(chunks[chunkIndex].neighbors[17] != null) chunks[chunkIndex].neighbors[6].Refresh();
					if(chunks[chunkIndex].neighbors[23] != null) chunks[chunkIndex].neighbors[6].Refresh();
					if(chunks[chunkIndex].neighbors[25] != null) chunks[chunkIndex].neighbors[6].Refresh();
					if(chunks[chunkIndex].neighbors[26] != null) chunks[chunkIndex].neighbors[26].Refresh();
					break;
				case -16514559: //bottom/front/left
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();

					if(chunks[chunkIndex].neighbors[0] != null) chunks[chunkIndex].neighbors[0].Refresh();
					if(chunks[chunkIndex].neighbors[1] != null) chunks[chunkIndex].neighbors[1].Refresh();
					if(chunks[chunkIndex].neighbors[3] != null) chunks[chunkIndex].neighbors[3].Refresh();
					if(chunks[chunkIndex].neighbors[9] != null) chunks[chunkIndex].neighbors[9].Refresh();
					break;
				case -16449023: //bottom/front/right
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					if(chunks[chunkIndex].neighbors[4] != null) chunks[chunkIndex].neighbors[4].Refresh();
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();

					if(chunks[chunkIndex].neighbors[1] != null) chunks[chunkIndex].neighbors[1].Refresh();
					if(chunks[chunkIndex].neighbors[2] != null) chunks[chunkIndex].neighbors[2].Refresh();
					if(chunks[chunkIndex].neighbors[5] != null) chunks[chunkIndex].neighbors[5].Refresh();
					if(chunks[chunkIndex].neighbors[11] != null) chunks[chunkIndex].neighbors[11].Refresh();
					break;
				case -16514303: //bottom/back/left
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					if(chunks[chunkIndex].neighbors[12] != null) chunks[chunkIndex].neighbors[12].Refresh();

					if(chunks[chunkIndex].neighbors[9] != null) chunks[chunkIndex].neighbors[9].Refresh();
					if(chunks[chunkIndex].neighbors[18] != null) chunks[chunkIndex].neighbors[18].Refresh();
					if(chunks[chunkIndex].neighbors[19] != null) chunks[chunkIndex].neighbors[19].Refresh();
					if(chunks[chunkIndex].neighbors[21] != null) chunks[chunkIndex].neighbors[21].Refresh();
					break;
				case -16448767: //bottom/back/right
					if(chunks[chunkIndex].neighbors[10] != null) chunks[chunkIndex].neighbors[10].Refresh();
					if(chunks[chunkIndex].neighbors[22] != null) chunks[chunkIndex].neighbors[22].Refresh();
					if(chunks[chunkIndex].neighbors[14] != null) chunks[chunkIndex].neighbors[14].Refresh();
					
					if(chunks[chunkIndex].neighbors[11] != null) chunks[chunkIndex].neighbors[11].Refresh();
					if(chunks[chunkIndex].neighbors[19] != null) chunks[chunkIndex].neighbors[19].Refresh();
					if(chunks[chunkIndex].neighbors[20] != null) chunks[chunkIndex].neighbors[20].Refresh();
					if(chunks[chunkIndex].neighbors[23] != null) chunks[chunkIndex].neighbors[23].Refresh();
					break;
			}
		}
		Debug.Log("Voxel:" + voxelX + ", " + voxelY + ", " + voxelZ + "Chunk: " + chunkX + ", " + chunkY + ", " + chunkZ);
	}*/
}
