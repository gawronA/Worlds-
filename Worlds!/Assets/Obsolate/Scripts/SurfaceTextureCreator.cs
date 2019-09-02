using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceTextureCreator : MonoBehaviour {


	private PlanetGenerator planetGenerator;
	private Texture2D texture2D;

	public bool drawVoxels = false;
	public bool voxelsOverlay = false;
	public bool drawContour = false;
	public int resolution;
	public int resolution2;
	public int z;
	private void Start()
	{
		planetGenerator = transform.parent.gameObject.GetComponent<PlanetGenerator>();
		resolution = planetGenerator.chunkGridResolution * planetGenerator.chunksMultiplier * planetGenerator.isoMultiplier;
		resolution2 = resolution * resolution;

		texture2D = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
		texture2D.name = "Isolevel texture";
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.filterMode = FilterMode.Point;
		GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture2D;
		FillTexture(z);
	}

	public void FillTexture(int z)
	{
		for(int y = 0, i=0 ; y < resolution; y++)
		{
			for(int x = 0; x < resolution; x++, i++)
			{
				Color pixelColor = new Color();
				switch(planetGenerator.surfaceMap.ReadMaterial(x, y, z))
				{
					case 0:
						pixelColor = Color.black;
						break;

					case 1:
						pixelColor = Color.green * 0.65f;
						break;

					case 2:
						pixelColor.r = 98.0f / 256.0f;
						pixelColor.g = 46.0f / 256.0f;
						pixelColor.b = 3.0f / 256.0f;
							break;
					case 3:
						pixelColor = Color.grey * 0.80f;
						break;
				}
				if(drawContour && planetGenerator.surfaceMap.IsContour(x, y, z)) pixelColor = Color.cyan;
				if(drawVoxels && voxelsOverlay && (x % planetGenerator.isoMultiplier == 0 && y % planetGenerator.isoMultiplier == 0))
					pixelColor = Color.Lerp(pixelColor, Color.blue, 0.25f);
				else if(drawVoxels && (x % planetGenerator.isoMultiplier == 0 && y % planetGenerator.isoMultiplier == 0 && z % planetGenerator.isoMultiplier == 0))
					pixelColor = Color.Lerp(pixelColor, Color.blue, 0.25f);

				texture2D.SetPixel(x, y, pixelColor);
			}
		}
		texture2D.Apply();
	}
}
