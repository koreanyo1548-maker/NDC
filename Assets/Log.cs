using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Log : MonoBehaviour {

	public List<GameObject> list = new List<GameObject> ();
	public int order = 0;

	public void Start(){
		for (int i = 0; i < list.Count; i++) {
			list [i].SetActive (false);
		}
		list [order].SetActive (true);
	}

	public void TurnRight(){
		list [order].SetActive (false);
		order += 1;
		if (order >= list.Count) {
			order = 0;
		}
		list [order].SetActive (true);
	}

	public void TurnLeft(){
		list [order].SetActive (false);
		order -= 1;
		if (order < 0) {
			order = list.Count - 1;
		}
		list [order].SetActive (true);
	}

	public void Test(){
		Debug.Log ("button");
	}
}
