using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTextureCreator : MonoBehaviour
{
	[Range(2, 512)]
	public int resolution = 256;
	[Range(1, 512)]
	public int frequency;
	[Range(1, 3)]
	public int dimension = 3;
	[Range(1, 8)]
	public int octaves = 1;
	[Range(1f, 4f)]
	public float lacunarity = 2f;
	[Range(0f, 1f)]
	public float persistence = 0.5f;

	public NoiseMethodType type;
	private Texture2D texture;

	private void Awake()
	{
		texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
		texture.name = "NoiseTexture";
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Point;
		texture.anisoLevel = 9;
		GetComponent<MeshRenderer>().material.mainTexture = texture;
		FillTexture();
	}

	private void OnValidate()
	{
		FillTexture();
	}

	private void Update()
	{
		if(transform.hasChanged)
		{
			transform.hasChanged = false;
			FillTexture();
		}
	}

	public void FillTexture()
	{
		if(texture.width != resolution) texture.Resize(resolution, resolution);

		NoiseMethod method = Noise.noiseMethods[(int)type][dimension - 1];

		Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
		Vector3 point01 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
		Vector3 point10 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
		Vector3 point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));
		
		float stepSize = 1.0f / resolution;
		for(int y = 0; y < resolution; y++)
		{
			Vector3 point0 = Vector3.Lerp(point00, point10, (y + 0.5f) * stepSize);
			Vector3 point1 = Vector3.Lerp(point01, point11, (y + 0.5f) * stepSize);
			for(int x = 0; x < resolution; x++)
			{
				Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
				//texture.SetPixel(x, y, new Color(point.x, point.y, point.z));
				//texture.SetPixel(x, y, Color.white * method(point, frequency));
				texture.SetPixel(x, y, Color.white * Noise.Sum(method, point, frequency, octaves, lacunarity, persistence));
			}
		}
		texture.Apply();
	}
}
