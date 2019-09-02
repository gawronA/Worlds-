using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Isolevel {

	public int resolution;
	private int resolution2;
	private int bytes;

	private byte[] isolevelTable;

	public Isolevel(int _resolution)
	{
		Initalize(_resolution);
	}

	public void Initalize(int _resolution)
	{
		resolution = _resolution;
		resolution2 = resolution * resolution;
		bytes = (int)Mathf.Ceil((resolution2 * resolution) / 8f);
		isolevelTable = new byte[bytes];
	}

	public bool ReadIsolevelTable(int x, int y, int z)
	{
		int i = z * resolution2 + y * resolution + x;
		int index = i / 8;
		byte mask = (byte)(1 << (i % 8));
		return (isolevelTable[index] & mask) == mask ? true : false;
	}

	public void SetIsolevelTable(int x, int y, int z, bool state)
	{
		int i = z * resolution2 + y * resolution + x;
		int index = i / 8;
		if(state) isolevelTable[index] |= (byte)(1 << i % 8);
		else isolevelTable[index] &= (byte)(~(1 << i % 8));
	}
	/*
	public void CreateSphere(float radius)
	{
		if(radius > 2 * resolution) Debug.Log("Za duża kula");
		else
		{
			for(int z = 0; z < resolution; z++)
			{
				for(int y = 0; y < resolution; y++)
				{
					for(int x = 0; x < resolution; x++)
					{
						if(Mathf.Pow(x - (resolution / 2), 2) + Mathf.Pow(y - (resolution / 2), 2) + Mathf.Pow(z - (resolution / 2), 2) < Mathf.Pow(radius, 2))
						{
							isolevelTable[z][y][x] = true;
						}
						else isolevelTable[z][y][x] = false;
					}
				}
			}
		}
	}*/
}
