using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceMap
{
	public int resolution { get; private set; }
	public int resolution2 { get; private set; }

	private byte[] map3D;
	public List<int> contour3D { get; private set; }
	public SurfaceMap(int _resolution)
	{
		Create3DMap(_resolution);
	}

	public void Create3DMap(int _resolution)
	{
		resolution = _resolution;
		resolution2 = resolution * resolution;
		map3D = new byte[resolution2 * resolution];
		contour3D = new List<int>();
	}

	public byte Read(int x, int y, int z)
	{
		return map3D[z * resolution2 + y * resolution + x];
	}

	private void Set(int x, int y, int z, byte value)
	{
		map3D[z * resolution2 + y * resolution + x] = value;
	}

	public bool IsEmpty(int x, int y, int z)
	{
		if(ReadMaterial(x, y, z) == 0) return true;
		else return false;
	}

	public bool IsContour(int x, int y, int z)
	{
		/*
		if((Read(x, y, z) & 0x80) != 0) return true;
		else return false;*/
		
		if(contour3D.Contains(z * resolution2 + y * resolution + x)) return true;
		else return false;
	}

	public void SetContour(int x, int y, int z)
	{
		//Set(x, y, z, (byte)(Read(x, y, z) | 0x80));
		contour3D.Add(z * resolution2 + y * resolution + x);
	}

	public void RemoveContour(int x, int y, int z)
	{
		//Set(x, y, z, (byte)(Read(x, y, z) & ~0x80));
		contour3D.Remove(z * resolution2 + y * resolution + x);
	}

	public int ReadBiome(int x, int y, int z)
	{
		return (Read(x, y, z) & 0x70) >> 4;
	}

	public void SetBiome(int x, int y, int z, int biome)
	{
		if(biome > 9) throw new Exception("InvalidBiome");
		byte value = Read(x, y, z);
		value &= 0x8F;
		value |= (byte)(biome << 4);
		Set(x, y, z, value);
	}

	public int ReadMaterial(int x, int y, int z)
	{
		return Read(x, y, z) & 0x0F;
	}

	public void SetMaterial(int x, int y, int z, int material)
	{
		if(material > 15) throw new Exception("InvalidMaterial");
		if(ReadBiome(x, y, z) == 0)
		{
			SetBiome(x, y, z, 1);
			Debug.LogWarning("Missing biome at (" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + "). Setting default (1).");
		}
		byte value = Read(x, y, z);
		value &= 0xF0;
		value |= (byte)material;
		Set(x, y, z, value);
	}

	/// <summary>
	/// <para>Merge layer top with bottom</para>
	/// <para>Mode: 0 - overlay, 1 - add, 2 - inverse</para>
	/// </summary>
	public static void MergeMaterialLayers(SurfaceMap bottom, SurfaceMap top, int mode)
	{
		if(mode < 0 || mode > 2) throw new Exception("InvalidMode");

		for(int i = 0; i < Mathf.Pow(bottom.resolution, 3); i++)
		{
			switch(mode)
			{
				case 0:
					bottom.map3D[i] = (byte)((bottom.map3D[i] & 0xF0) | (top.map3D[i] & 0x0F));
					break;

				case 1:
					if((bottom.map3D[i] & 0x0F) == 0) bottom.map3D[i] = (byte)((bottom.map3D[i] & 0xF0) | (top.map3D[i] & 0x0F));
					break;

				case 2:
					if((bottom.map3D[i] & 0x0F) != 0) bottom.map3D[i] = (byte)(bottom.map3D[i] & 0xF0);
					else bottom.map3D[i] = (byte)((bottom.map3D[i] & 0xF0) | (top.map3D[i] & 0x0F));
					break;
			}
		}

		bottom.contour3D.Clear();
		for(int i = 0; i < top.contour3D.Count; i++) bottom.contour3D.Add(top.contour3D[i]);
	}
	/*public void RecalculateContour()
	{
		int x_check, y_check, z_check;
		for(int z = 0; z < resolution; z++)
		{
			for(int y = 0; y < resolution; y++)
			{
				for(int x = 0; x < resolution; x++)
				{
					//krawędzie
					if(x == 0 || x == resolution - 1) SetContour(x, y, z);
					else
					{
						if(y == 0 || y == resolution - 1) SetContour(x, y, z);
						else
						{
							if(z == 0 || z == resolution - 1) SetContour(x, y, z);
							else
							{
								//przód/tył
								x_check = x;
								y_check = y;
								z_check = z + 1;

							}
						}
					}
				}
			}
		}
	}*/

	

}
