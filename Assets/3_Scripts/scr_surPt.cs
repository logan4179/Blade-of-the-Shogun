using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_surPt : MonoBehaviour {

	public static scr_mgrMain gm = null; //TODO: stop making this public when ready to export game
	public bool isOccupied = false;
	public bool isColliding = false;

	void Start () 
	{
		gm = GameObject.Find("GameManager").GetComponent<scr_mgrMain>();
	}
	
	void Update () 
	{
		
	}

	void OnTriggerEnter( Collider thisTrigger )
	{
		if ( thisTrigger.tag == "solid" && gm.numbAttacking > 0)
		{
			//print ("setEnemiesAndSurPts called by " + name + ".OnTriggerEnter");

			isColliding = true;
			gm.setEnemiesAndSurPts();
		}
	}

	void OnTriggerExit ( Collider thisTrigger )
	{
		if ( thisTrigger.tag == "solid" && gm.numbAttacking > 0 )
		{
			//print ("setEnemiesAndSurPts called by " + name + ".OnTriggerExit");

			isColliding = false;
			gm.setEnemiesAndSurPts();
		}
	}
}
