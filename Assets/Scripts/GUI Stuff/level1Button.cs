﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level1Button : MonoBehaviour 
{

	// Use this for initialization.
	void Start () 
	{
		Button button1 = GetComponent<Button> ();

		button1.onClick.AddListener(clickEventListener);

		mGlobalData = GameObject.Find ("globalData").GetComponent<GlobalData> ();
	}
	
	// Update is called once per frame.
	void Update () 
	{
		
	}

	private void clickEventListener()
	{
		//Debug.Log("Clicked!");

		string sceneName = "level_garage";

		Destroy(GameObject.Find("MainMenuCamera"));
		mGlobalData.ChangeMap (sceneName);

	}

	private GlobalData mGlobalData;
}