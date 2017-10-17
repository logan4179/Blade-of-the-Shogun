using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_pOb : MonoBehaviour {

	static public scr_mgrMain gm = null;
	public float[] pDistances;
	bool hasStarted = false;

	//public GameObject[] pObs;  TESTING
	//public float[] distances;	 TESTING


	// Use this for initialization
	void Start () 
	{
		gm = GameObject.Find("GameManager").GetComponent<scr_mgrMain>();
		pDistances = new float[GameObject.FindGameObjectsWithTag("pOb").Length];

	}
	
	// Update is called once per frame
	void Update () 
	{

		
	}
}
