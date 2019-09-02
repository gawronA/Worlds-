using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SurfaceDrawTools;

public class PlanetGenerator : MonoBehaviour
{
	public VoxelMap voxelMap;
	public Vector3 position;
	public Quaternion rotation;
	public int radius;
	public float size;
	public int chunkGridResolution;
	public int chunksMultiplier;
	public int isoMultiplier;
	private int resolution;

	//public Isolevel isoMap;
	private VoxelMap voxelMapObject;
	public SurfaceMap surfaceMap;
	private void Awake()
	{
		resolution = chunkGridResolution * chunksMultiplier * isoMultiplier;
		Vector3Int center = new Vector3Int(	resolution / 2,
											resolution / 2,
											resolution / 2);

		surfaceMap = new SurfaceMap(resolution);
		SurfaceMap layer1 = new SurfaceMap(resolution);

		DrawSurface.Sphere(layer1, center, radius, 3, true);
		DrawSurface.NoiseSettings.lacunarity = 1;
		DrawSurface.NoiseSettings.persistance = 0;
		DrawSurface.NoiseSettings.octaves = 1;
		DrawSurface.NoiseSettings.frequency = 3;
		DrawSurface.NoiseSettings.gain = 5;
		DrawSurface.ApplyNoiseToSphere(layer1, center);
		SurfaceMap.MergeMaterialLayers(surfaceMap, layer1, 1);

		//SurfaceMap.MergeMaterialLayers(surfaceMap, layer1, 0);
		//DrawSurface.Cylinder(surfaceMap, new Vector3Int(7, 7, 7), new Vector3Int(13, 13, 10), 6, 2, 0);

		/*//DrawSurface.Line(layer1, new Vector3Int(1, 0, 0), new Vector3Int(1, 5, 0), 2, 1, false, 0);
		
		DrawSurface.NoiseSettings.lacunarity = 1;
		DrawSurface.NoiseSettings.persistance = 0;
		DrawSurface.NoiseSettings.octaves = 1;
		DrawSurface.NoiseSettings.frequency = 1;
		DrawSurface.NoiseSettings.gain = 20;
		DrawSurface.ApplyNoiseToSphere(layer1, center);
		SurfaceMap.MergeMaterialLayers(surfaceMap, layer1, 1);*/
		voxelMap.size = size;
		voxelMap.chunkResolution = chunksMultiplier;
		voxelMap.voxelResolution = chunkGridResolution;
		voxelMapObject = Instantiate<VoxelMap>(voxelMap, position, rotation);
		Refresh();

	}

	public void Refresh()
	{
		SetVoxels();
	}

	private void SetVoxels()
	{
		/*int isoMapResolution = isoMap.resolution;
		for(int z = isoMapResolution - 1; z >= 0; z--)
		{
			for(int y = isoMapResolution - 1; y >= 0; y--)
			{
				for(int x = isoMapResolution - 1; x >= 0; x--)
				{

					if(x % isoMultiplier == 0 && y % isoMultiplier == 0 && z % isoMultiplier == 0)
					{
						if(!surfaceMap.IsEmpty(x, y, z))
						{
							voxelMapObject.chunks[	(z / (chunkGridResolution * isoMultiplier)) * chunksMultiplier * chunksMultiplier +
													(y / (chunkGridResolution * isoMultiplier)) * chunksMultiplier +
													(x / (chunkGridResolution * isoMultiplier))]
												.SetVoxel((x / isoMultiplier) % chunkGridResolution, (y / isoMultiplier) % chunkGridResolution, (z / isoMultiplier) % chunkGridResolution, true);
						}
						else
						{
							voxelMapObject.chunks[	(z / (chunkGridResolution * isoMultiplier)) * chunksMultiplier * chunksMultiplier +
													(y / (chunkGridResolution * isoMultiplier)) * chunksMultiplier +
													(x / (chunkGridResolution * isoMultiplier))]
												.SetVoxel((x / isoMultiplier) % chunkGridResolution, (y / isoMultiplier) % chunkGridResolution, (z / isoMultiplier) % chunkGridResolution, false);
						}
					}
				}
			}
		}
		voxelMapObject.Refresh();*/

		int isoX = 0, isoY = 0, isoZ = 0;
		float interpX = 0.0f, interpY = 0.0f, interpZ = 0.0f;
		for(int z = 0; z < chunkGridResolution * chunksMultiplier; z++)
		{
			isoY = 0;
			for(int y = 0; y < chunkGridResolution * chunksMultiplier; y++)
			{
				isoX = 0;
				for(int x = 0; x < chunkGridResolution * chunksMultiplier; x++)
				{
					/*for(int i = 0; i < isoMultiplier; i++)
					{
						if(!surfaceMap.IsEmpty(isoX + i, isoY, isoZ)) interpX = i / isoMultiplier;
						if(!surfaceMap.IsEmpty(isoX, isoY + i, isoZ)) interpY = i / isoMultiplier;
						if(!surfaceMap.IsEmpty(isoX, isoY, isoZ + i)) interpZ = i / isoMultiplier;
					}
					voxelMapObject.chunks[	(z / chunkGridResolution) * chunksMultiplier * chunksMultiplier +
											(y / chunkGridResolution) * chunksMultiplier +
											 x / chunkGridResolution]
										.SetVoxelCrossings(x % chunkGridResolution, y % chunkGridResolution, z % chunkGridResolution, interpX, interpY, interpZ);*/
					
					if(!surfaceMap.IsEmpty(isoX, isoY, isoZ))
					{
						interpX = interpY = interpZ = 0.0f;
						for(int i = 0; i < isoMultiplier; i++)
						{
							if(!surfaceMap.IsEmpty(isoX + i, isoY, isoZ)) interpX = (float)i / (float)isoMultiplier;
							if(!surfaceMap.IsEmpty(isoX, isoY + i, isoZ)) interpY = (float)i / (float)isoMultiplier;
							if(!surfaceMap.IsEmpty(isoX, isoY, isoZ + i)) interpZ = (float)i / (float)isoMultiplier;
						}
						voxelMapObject.chunks[(z / chunkGridResolution) * chunksMultiplier * chunksMultiplier +
											(y / chunkGridResolution) * chunksMultiplier +
											 x / chunkGridResolution]
										.SetVoxelCrossings(x % chunkGridResolution, y % chunkGridResolution, z % chunkGridResolution, interpX, interpY, interpZ);

						voxelMapObject.chunks[(z / chunkGridResolution) * chunksMultiplier * chunksMultiplier +
												(y / chunkGridResolution) * chunksMultiplier +
												 x / chunkGridResolution]
							.SetVoxel(x % chunkGridResolution, y % chunkGridResolution, z % chunkGridResolution, true);
					}
					else
					{
						interpX = interpY = interpZ = 1.0f;
						for(int i = isoMultiplier - 1; i >= 0; i--) 
						{
							if(!surfaceMap.IsEmpty(isoX + i, isoY, isoZ)) interpX = (float)i / (float)isoMultiplier;
							if(!surfaceMap.IsEmpty(isoX, isoY + i, isoZ)) interpY = (float)i / (float)isoMultiplier;
							if(!surfaceMap.IsEmpty(isoX, isoY, isoZ + i)) interpZ = (float)i / (float)isoMultiplier;
						}
						voxelMapObject.chunks[(z / chunkGridResolution) * chunksMultiplier * chunksMultiplier +
											(y / chunkGridResolution) * chunksMultiplier +
											 x / chunkGridResolution]
										.SetVoxelCrossings(x % chunkGridResolution, y % chunkGridResolution, z % chunkGridResolution, interpX, interpY, interpZ);

						voxelMapObject.chunks[(z / chunkGridResolution) * chunksMultiplier * chunksMultiplier +
												(y / chunkGridResolution) * chunksMultiplier +
												 x / chunkGridResolution]
							.SetVoxel(x % chunkGridResolution, y % chunkGridResolution, z % chunkGridResolution, false);
					}

					isoX = isoX + isoMultiplier;
				}
				isoY = isoY + isoMultiplier;
			}
			isoZ = isoZ + isoMultiplier;
		}
		voxelMapObject.Refresh();
	}

	public void InstantiateCubes()
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.transform.parent = transform;

		for(int z = 0; z < resolution; z++)
		{
			for(int y = 0; y < resolution; y++)
			{
				for(int x = 0; x < resolution; x++)
				{
					int material = surfaceMap.ReadMaterial(x, y, z);
					switch(material)
					{
						case 1:
							obj.GetComponent<MeshRenderer>().material.color = Color.green * 0.65f;
							Instantiate(obj, new Vector3(x, y, z), new Quaternion(), transform);
							break;
						case 2:
							obj.GetComponent<MeshRenderer>().material.color = new Color(98.0f / 256.0f, 46.0f / 256.0f, 3.0f / 256.0f);
							Instantiate(obj, new Vector3(x, y, z), new Quaternion(), transform);
							break;
						case 3:
							obj.GetComponent<MeshRenderer>().material.color = Color.grey * 0.80f;
							Instantiate(obj, new Vector3(x, y, z), new Quaternion(), transform);
							break;
					}
				}
			}
		}
	}

}
