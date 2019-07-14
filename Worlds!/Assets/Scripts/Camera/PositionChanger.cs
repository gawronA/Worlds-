using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionChanger : MonoBehaviour {

	public bool changePosition = false;
	public int position = 0;
	Quaternion rotation;
	void Update ()
	{
		if(changePosition)
		{
			switch(position)
			{
				case 0:
					rotation = Quaternion.Euler(new Vector3(0, 0, 0));
					transform.position = new Vector3(0, 0, -20);
					transform.rotation = rotation;
					changePosition = false;
					position++;
					break;

				case 1:
					rotation = Quaternion.Euler(new Vector3(0, 90, 0));
					transform.position = new Vector3(-20, 0, 0);
					transform.rotation = rotation;
					changePosition = false;
					position++;
					break;

				case 2:
					rotation = Quaternion.Euler(new Vector3(-90, 0, 180));
					transform.position = new Vector3(0, -20, 0);
					transform.rotation = rotation;
					changePosition = false;
					position++;
					break;

				case 3:
					rotation = Quaternion.Euler(new Vector3(0, 180, 0));
					transform.position = new Vector3(0, 0, 20);
					transform.rotation = rotation;
					changePosition = false;
					position++;
					break;

				case 4:
					rotation = Quaternion.Euler(new Vector3(0, -90, 0));
					transform.position = new Vector3(20, 0, 0);
					transform.rotation = rotation;
					changePosition = false;
					position++;
					break;

				case 5:
					rotation = Quaternion.Euler(new Vector3(90, 0, -180));
					transform.position = new Vector3(0, 20, 0);
					transform.rotation = rotation;
					changePosition = false;
					position = 0;
					break;
			}
		}
	}
}
