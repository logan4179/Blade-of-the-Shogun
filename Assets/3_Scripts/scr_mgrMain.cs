using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_mgrMain : MonoBehaviour 
{
	// GAME STATE -----------------------------------
	public bool gamePaused = false;

	// OBJECTS/COMPONENTS ---------------------------
	public GameObject[] enemies;
	public GameObject[] surObjs;
	public Transform surPivot = null;
	public Transform[] surPts;
	public Transform primaryEnemyTrans = null;
	public GameObject[] pObs;
	public scr_pOb[] pScripts;

	public int numbAttacking = 0;
	public bool[] surPtIsOccupied;
	public float furthestEngagingDist = 100;
	public bool hasStarted = false;

	public int solidMask;

	void OnEnable ()
	{
		getEnemies(); //TODO: WHY NOT JUST TAKE AWAY THIS FUNCTION???

		pObs = GameObject.FindGameObjectsWithTag("pOb");

		pScripts = new scr_pOb[pObs.Length];
		for ( int i = 0; i < pObs.Length; i++ )
			pScripts[i] = pObs[i].GetComponent<scr_pOb>();

	}
	void Start () 
	{
		surPivot = GameObject.Find("surroundPivot").transform;

		surPts = new Transform[4];
		surPts[0] = GameObject.Find("surroundPt").transform;
		surPts[1] = GameObject.Find("surroundPt (1)").transform;
		surPts[2] = GameObject.Find("surroundPt (2)").transform;
		surPts[3] = GameObject.Find("surroundPt (3)").transform;

	}
	
	void Update () 
	{
		if ( numbAttacking > 1 )
			setFurthestEngagingDist();
		if ( !hasStarted )  //TODO: I'm temporarily using this hasstarted variable, I need to figure out the script esecution order stuff to get rid of it and call these methods without it
		{
			setPathDistances();
			hasStarted = true;
		}

	}
	//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 //----//----//----//----//----//----//  F U N C T I O N S  -//----//----//----//----//----//----//----//
	public void setEnemiesAndSurPts()
	{
		foreach (GameObject curEnemy in enemies )
		{
	 		curEnemy.GetComponent<scr_basicEnemyMONO>().amEngaging = false;
			curEnemy.GetComponent<scr_basicEnemyMONO>().surAnchor = null;
		}
	 	foreach ( Transform surPt in surPts )
	 		surPt.gameObject.GetComponent<scr_surPt>().isOccupied = false;

		for ( int i = 0; i < Mathf.Min(4, numbAttacking); i++ )
		{
			float curClosestDist = 100f;
			int curClosestIndex = 0;

			foreach( GameObject curEnemy in enemies )
			{  //Find the attacking enemy closest to the player that isn't yet engaging
				if ( curEnemy.GetComponent<scr_basicEnemyMONO>().enemyState == 2 && curEnemy.GetComponent<scr_basicEnemyMONO>().distToPlr < curClosestDist && curEnemy.GetComponent<scr_basicEnemyMONO>().amEngaging == false ) //TODO: Maybe it's better to use a for loop so that I can get away from using the System.Array.IndexOf method, but I don't know if this would be beneficial enough
				{
					curClosestDist = curEnemy.GetComponent<scr_basicEnemyMONO>().distToPlr;
					curClosestIndex = System.Array.IndexOf(enemies, curEnemy);
				}
			}

			if ( i == 0 )
			{
				primaryEnemyTrans = enemies[curClosestIndex].transform;
				surPivot.transform.rotation = Quaternion.LookRotation( enemies[curClosestIndex].transform.position - surPivot.transform.position, Vector3.up );
			}

			float closestSurPtDist = 100f;
			int surPtIndexNumb = 0;
			bool surPtFound = false;
			foreach ( Transform curSurPt in surPts )
			{  // Loop through all surroundPts
				if ( Vector3.Distance(enemies[curClosestIndex].transform.position , curSurPt.position) < closestSurPtDist && curSurPt.gameObject.GetComponent<scr_surPt>().isOccupied == false && curSurPt.gameObject.GetComponent<scr_surPt>().isColliding == false  )
				{  // Find the closest unoccupied surround point to the current enemy we're on
					closestSurPtDist = Vector3.Distance(enemies[curClosestIndex].transform.position ,curSurPt.position);
					surPtIndexNumb = System.Array.IndexOf( surPts, curSurPt );
					enemies[curClosestIndex].GetComponent<scr_basicEnemyMONO>().amEngaging = true;
					surPtFound = true;
				}
			}
			if ( surPtFound )
			{
				enemies[curClosestIndex].GetComponent<scr_basicEnemyMONO>().surAnchor = surPts[surPtIndexNumb];
				surPts[surPtIndexNumb].gameObject.GetComponent<scr_surPt>().isOccupied = true;
			}
		}
	}

	//--------------------------------------------------------------------------
	public void setPathDistances()
	{
		//GET INITIAL DISTANCES--------------
		solidMask = 1 << 8;
		foreach ( GameObject pObA in pObs )
		{
			foreach ( GameObject pObB in pObs )
			{
				if ( System.Array.IndexOf(pObs, pObA) != System.Array.IndexOf(pObs, pObB) && !Physics.Linecast(pObA.transform.position, pObB.transform.position, solidMask) )
					pObA.GetComponent<scr_pOb>().pDistances[System.Array.IndexOf(pObs, pObB)] = Vector3.Distance(pObA.transform.position, pObB.transform.position);
			}
		}

		//GET THE REST OF THE DISTANCES-------
		for ( int i = 0; i < pObs.Length; i++ )
		{// For as many times as there are pathing objects...
			foreach ( scr_pOb obA in pScripts )
			{
				int aIndex = System.Array.IndexOf (pScripts, obA );
				foreach ( scr_pOb obB in pScripts )
				{
					int bIndex = System.Array.IndexOf (pScripts, obB );
					if ( obA.pDistances[bIndex] > 0 )
					{
						for ( int dIndex = 0; dIndex < pObs.Length; dIndex++ )
						{
							float distance = obB.pDistances[dIndex];
							if ( distance > 0 && dIndex != aIndex && ( obA.pDistances[dIndex] == 0 || ((obA.pDistances[bIndex] + distance) < obA.pDistances[dIndex])) )
							{
								obA.pDistances[dIndex] = ( obA.pDistances[bIndex] + distance );
								// If "distance" is greater than 0 (IE: is set), and either the distance from obA to obB plus "distance" is less than the distance already set in obA's distance array entry corresponding to "distance",
								// or obA's distance value for distance hasn't been set...
								// ...set obA's distance entry corresponding to 'distance' to the distance from obA to obB plus 'distance'.
							}
						}
					}
				}
			}
		}
	}
	//--------------------------------------------------------------------------
	public void getEnemies()
	{
		enemies = GameObject.FindGameObjectsWithTag("basicEnemy"); //curious that you don't have to use the new keyword to create a new array here even though array sizes are supposed to be fixed. Maybe this literally returns a 'new' array
	}
	//--------------------------------------------------------------------------
	public void setFurthestEngagingDist()
	{
		float runningFurthestDist = 0;
		int furthestEngagingIndex = 0; 
		foreach (GameObject curEnemy in enemies )
		{
			if ( curEnemy.GetComponent<scr_basicEnemyMONO>().amEngaging && curEnemy.GetComponent<scr_basicEnemyMONO>().distToPlr > runningFurthestDist )
			{
				runningFurthestDist = curEnemy.GetComponent<scr_basicEnemyMONO>().distToPlr;
				furthestEngagingIndex = System.Array.IndexOf(enemies, curEnemy);
			}
		}
		furthestEngagingDist = enemies[furthestEngagingIndex].GetComponent<scr_basicEnemyMONO>().distToPlr;
	}
	//------------------------------------------------------------------------------------
}
