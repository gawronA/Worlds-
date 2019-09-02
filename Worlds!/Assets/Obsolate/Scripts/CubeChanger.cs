using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CubeChanger : MonoBehaviour
{
	public VoxelGrid voxelGridPrefab;
	public Text textObject;
	public float delay = 2.0f;
	public bool timer = false;
	public bool checkAll = false;
	public bool reset = false;

	private VoxelGrid voxelGridObject;
	private bool isTimer;
	private int[] wariantArray = {  /*0,												//wariant 0
									1, 2, 4, 8, 16, 32, 64, 128,						//wariant 1
									3, 17, 48, 34, 5, 80, 160, 10, 12, 68, 192, 136,	//wariant 2
									9, 20, 96, 130, 6, 65, 144, 40, 132, 72, 33, 18,	//wariant 3
									50, 35, 49, 19, 200, 140, 76, 196, 7, 81, 176, 42, 13, 84, 224, 138, 168, 14, 69, 208, 11, 21, 112, 162,	//wariant 4
									51, 204, 15, 240, 85, 170							//wariant 5
									50+4, 35+64, 49+8,19+128, 200+1, 140+16, 76+32,196+2, 7+128,81+8,176+4,42+64,13+32,84+2,224+1,138+16,168+1,14+16,69+32,208+2,11+64,21+128,112+8,162+4,	//wariant 6
									105, 150,    //wariant 7 
									43, 23, 113, 178, 142, 77, 212, 232	//wariant 8
									163, 27, 53, 114, 172, 78, 197, 216, 83, 177, 58, 39, 92, 228, 202, 141, 71, 209, 184, 46, 226, 139, 29, 116,	//wariant 9,15
									129, 36, 24, 66	//wariant 10
									131,67,25,145,52,56,98,38,37,133,82,88,161,164,26,74,28,44,70,100,193,194,152,137	//wariant 11
									134,73,148,104,41,22,97,146,
									*/165,90,195,60,153,102};
	private int i = 0;
	//private string wariantText = "Wariant: ";
	private void Start ()
	{
		voxelGridObject = Instantiate(voxelGridPrefab, transform);
		voxelGridObject.Initalize(2, 1.0f);
		StartCoroutine(work());
	}

	private void Update()
	{
		if(reset)
		{
			i = 0;
			reset = false;
		}
		if(!isTimer) StartCoroutine(work());
		if(Input.GetButtonDown("Jump"))
		{
			if(checkAll)
			{
				if(i < 255)
				{
					setVoxels(i);
					changeText("Wariant: " + Convert.ToString(i, 2) + "(" + i.ToString() + ")");
					i++;
				}
			}
			else
			{
				if(i < wariantArray.Length)
				{
					setVoxels(wariantArray[i]);
					changeText("Wariant: " + Convert.ToString(wariantArray[i], 2) + "(" + wariantArray[i].ToString() + ")");
					i++;
					if(!(i < wariantArray.Length)) i = 0;
				}
			}
		}
	}

	private void changeText(string text)
	{
		textObject.text = text;
	}

	private void setVoxels(int wariant)
	{
		voxelGridObject.SetVoxelRefresh(0, 0, 0, Convert.ToBoolean(wariant & 1));
		voxelGridObject.SetVoxelRefresh(1, 0, 0, Convert.ToBoolean(wariant & 2));
		voxelGridObject.SetVoxelRefresh(0, 1, 0, Convert.ToBoolean(wariant & 4));
		voxelGridObject.SetVoxelRefresh(1, 1, 0, Convert.ToBoolean(wariant & 8));
		voxelGridObject.SetVoxelRefresh(0, 0, 1, Convert.ToBoolean(wariant & 16));
		voxelGridObject.SetVoxelRefresh(1, 0, 1, Convert.ToBoolean(wariant & 32));
		voxelGridObject.SetVoxelRefresh(0, 1, 1, Convert.ToBoolean(wariant & 64));
		voxelGridObject.SetVoxelRefresh(1, 1, 1, Convert.ToBoolean(wariant & 128));
	}

	IEnumerator work()
	{
		while(timer)
		{
			isTimer = true;
			yield return StartCoroutine(delayWait());
		}
		isTimer = false;
	}
	IEnumerator delayWait()
	{
		yield return new WaitForSeconds(delay);
		if(checkAll)
		{
			if(i < 255)
			{
				setVoxels(i);
				changeText("Wariant: " + Convert.ToString(i, 2) + "(" + i.ToString() + ")");
				i++;
			}
		}
		else
		{
			if(i < wariantArray.Length)
			{
				setVoxels(wariantArray[i]);
				changeText("Wariant: " + Convert.ToString(wariantArray[i], 2) + "(" + wariantArray[i].ToString() + ")");
				i++;

			}
		}
	}
}
