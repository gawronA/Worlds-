using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayHelper
{
	public static void Copy(int[] origin, int[] dest)
	{
		for(int i = 0; i < origin.Length; i++) dest[i] = origin[i];
	}

	public static void Copy(float[] origin, float[] dest)
	{
		for(int i = 0; i < origin.Length; i++) dest[i] = origin[i];
	}
}
