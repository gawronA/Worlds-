using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureHelper : MonoBehaviour
{
	public ComputeShader slicer;
	public int voxelSize;
	RenderTexture Copy3DSliceToRenderTexture(RenderTexture source, int layer)
	{
		RenderTexture render = new RenderTexture(voxelSize, voxelSize, 0, RenderTextureFormat.ARGB32);
		render.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
		render.enableRandomWrite = true;
		render.wrapMode = TextureWrapMode.Clamp;
		render.Create();

		int kernelIndex = slicer.FindKernel("CSMain");
		slicer.SetTexture(kernelIndex, "voxels", source);
		slicer.SetInt("layer", layer);
		slicer.SetTexture(kernelIndex, "Result", render);
		slicer.Dispatch(kernelIndex, voxelSize, voxelSize, 1);

		return render;
	}

	Texture2D ConvertFromRenderTexture(RenderTexture rt)
	{
		Texture2D output = new Texture2D(voxelSize, voxelSize);
		RenderTexture.active = rt;
		output.ReadPixels(new Rect(0, 0, voxelSize, voxelSize), 0, 0);
		output.Apply();
		return output;
	}

	Texture3D ConvertToTexture3D(RenderTexture rt)
	{
		//Texture3D export = new Texture3D(voxelSize, voxelSize, voxelSize, TextureFormat.ARGB32, false);

		RenderTexture[] layers = new RenderTexture[voxelSize];
		for(int i = 0; i < voxelSize; i++)
			layers[i] = Copy3DSliceToRenderTexture(rt, i);

		Texture2D[] finalSlices = new Texture2D[voxelSize];
		for(int i = 0; i < voxelSize; i++)
			finalSlices[i] = ConvertFromRenderTexture(layers[i]);

		Texture3D output = new Texture3D(voxelSize, voxelSize, voxelSize, TextureFormat.ARGB32, true);
		output.filterMode = FilterMode.Trilinear;
		Color[] outputPixels = output.GetPixels();

		for(int k = 0; k < voxelSize; k++)
		{
			Color[] layerPixels = finalSlices[k].GetPixels();
			for(int i = 0; i < voxelSize; i++)
				for(int j = 0; j < voxelSize; j++)
				{
					outputPixels[i + j * voxelSize + k * voxelSize * voxelSize] = layerPixels[i + j * voxelSize];
				}
		}

		output.SetPixels(outputPixels);
		output.Apply();

		return output;
	}
}
