using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_surroundPivot : MonoBehaviour {

	public scr_mgrMain gm = null; //TODO: stop making this public when ready to export game

	void Start () 
	{
		gm = GameObject.Find("GameManager").GetComponent<scr_mgrMain>();
	}

	// Update is called once per frame
	void Update () 
	{
		if ( gm.primaryEnemyTrans != null && Quaternion.Angle( transform.rotation, Quaternion.LookRotation(gm.primaryEnemyTrans.position - transform.position, Vector3.up)) > 10.0f )
			transform.rotation = Quaternion.LookRotation ( gm.primaryEnemyTrans.position - transform.position, Vector3.up);

	}
}
