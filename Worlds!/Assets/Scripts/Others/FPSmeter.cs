using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSmeter : MonoBehaviour
{
	Text fps;
	// Use this for initialization
	void Start ()
	{
		fps = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		float capturedTime = Time.deltaTime;
		fps.text = (1 / capturedTime).ToString() + "FPS (" + capturedTime.ToString() + ")";
	}
}
