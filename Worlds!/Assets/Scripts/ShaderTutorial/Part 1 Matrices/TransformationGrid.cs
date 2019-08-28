using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformationGrid : MonoBehaviour
{
	public Transform prefab;
	public int gridResolution = 10;

	Transform[] grid;

	List<Transformation> transformations;
	Matrix4x4 transformation;
	void Awake ()
	{
		grid = new Transform[gridResolution * gridResolution * gridResolution];
		transformations = new List<Transformation>();

		for(int z = 0, i = 0; z < gridResolution; z++)
		{
			for(int y = 0; y < gridResolution; y++)
			{
				for(int x = 0; x < gridResolution; x++, i++)
				{
					grid[i] = CreateGridPoint(x, y, z);
				}
			}
		}
	}

	private void Update()
	{
		UpdateTransformation();
		for(int z = 0, i = 0; z < gridResolution; z++)
		{
			for(int y = 0; y < gridResolution; y++)
			{
				for(int x = 0; x < gridResolution; x++, i++)
				{
					grid[i].localPosition = TransformPoint(x, y, z);
				}
			}
		}
	}

	private void UpdateTransformation()
	{
		GetComponents<Transformation>(transformations);
		if(transformations.Count > 1)
		{
			transformation = transformations[0].Matrix;
			for(int i = 1; i < transformations.Count; i++)
			{
				transformation = transformations[i].Matrix * transformation;
			}
		}
	}

	Transform CreateGridPoint(int x, int y, int z)
	{
		Transform point = Instantiate<Transform>(prefab);
		point.name = (x + y * gridResolution + z * gridResolution * gridResolution).ToString();
		point.localPosition = GetCoordinates(x, y, z);
		point.GetComponent<MeshRenderer>().material.color = new Color((float)x / gridResolution, (float)y / gridResolution, (float)z / gridResolution);
		return point;
	}

	Vector3 GetCoordinates(int x, int y, int z)
	{
		return new Vector3(x - (gridResolution - 1) / 2, y - (gridResolution - 1) / 2, z - (gridResolution - 1) / 2);
	}

	Vector3 TransformPoint(int x, int y, int z)
	{
		Vector3 coordinates = GetCoordinates(x, y, z);
		return transformation.MultiplyPoint(coordinates);
	}
}
