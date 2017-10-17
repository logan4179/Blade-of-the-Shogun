using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_cameraController : MonoBehaviour 
{
	// POSITIONAL ----------------------------
	Vector3 targetPosition = Vector3.zero;
	Vector3 vPlrToCam = Vector3.zero;

	// OBJECTS/COMPONENTS ---------------------
	scr_mgrMain g = null;
	public Transform plrTrans;

	// CAM STATS ------------------------------
	public float camSpeed = 40.0f;		// Speed camera can move (lerp) with the player
	public float rotSpd = 70.0f;		// Speed camera can rotate
	public float followDist = 6f;		// Distance camera should follow behind the player
	public float aboveDist = 10.0f;		// Distance camera should hover above the player
	public float lookAboveAmt = 2.0f;	// Default amount camera looks higher than the player
	public float lookAboveOffset = 0.0f;// Amount camera looks higher or lower than the lookAboveAmt when the right stick is tilted
	public float camTiltSpd = 0.09f;	// Speed camera can tilt vertically
	public float camMaxTilt = 2.0f;		// Maximum amount cam can tilt upwards or downward with the right stick.

	// CONTROLS -------------------------------
	float axisThresh = 0; //gets set in start to be same as player axisThresh
	public float horAxis = 0.0f;
	public float vertAxis = 0.0f;

	// TODO: DIAGNOSTIC, DELETE WHEN DONE ------------------------
	//public Transform truePosSphere = null;
	//public Transform lerpGoalSphere = null;
	//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 	 //----//----//----//----//----//----//  S T A R T  ---//----//----//----//----//----//----//----//----//
	void Start ()
	{
		g = GameObject.Find("GameManager").GetComponent<scr_mgrMain>();
		plrTrans = GameObject.FindGameObjectWithTag("Player").transform;

		transform.position = plrTrans.position - plrTrans.forward * followDist;
		targetPosition = transform.position;

		axisThresh = plrTrans.gameObject.GetComponent<scr_player>().axisThresh;

		vPlrToCam = ( plrTrans.position - transform.position );
		vPlrToCam.y = plrTrans.position.y;
		vPlrToCam = Vector3.Normalize(vPlrToCam);

		//DIAGNOSTIC -------
		//truePosSphere = GameObject.Find("truePosSphere").transform;
		//lerpGoalSphere = GameObject.Find("lerpGoalSphere").transform;
	}
//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 //----//----//----//----//----//----//----//  U P D A T E  -//----//----//----//----//----//----//----//	
	void Update () 
	{
		horAxis = Input.GetAxis("RHorizontal");
		vertAxis = Input.GetAxis("RVertical");

	}
	//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
		 //----//----//----//----//----//----//  L A T E   U P D A T E  -//----//----//----//----//----//----//----//
	void LateUpdate ()
	{
		if( !g.gamePaused )
		{
			targetPosition = plrTrans.position - ( vPlrToCam * followDist ) + ( plrTrans.up * aboveDist );

			transform.position = Vector3.Lerp( transform.position, targetPosition, camSpeed * Time.deltaTime ); //this caused a rotation slowdown as the player moved in a certain direction with respect to the camera. It seems to get better when I set the camSpeed to a very high amount.  I think I'll use that instead of the next line.
			//transform.position = targetPosition;
	
			//truePosSphere.position = targetPosition; //TODO: DIAG
			//lerpGoalSphere.position = Vector3.Lerp( transform.position, targetPosition, camSpeed * Time.deltaTime ); //TODO: DIAG

			// ROTATING THE CAMERA --------------------------
			if( Mathf.Abs(horAxis) > 0 )
			{
				transform.RotateAround( plrTrans.position, plrTrans.up, rotSpd * horAxis * Time.deltaTime );

				vPlrToCam = ( plrTrans.position - transform.position );
				vPlrToCam.y = 0;
				vPlrToCam = Vector3.Normalize(vPlrToCam);
			}

			// LOOKING UPWARDS/DOWNWARDS ---------------------
			if( Mathf.Abs(vertAxis) > 0 )
				lookAboveOffset = Mathf.Lerp( lookAboveOffset, (camMaxTilt * Mathf.Abs(vertAxis) * Mathf.Sign(vertAxis)), camTiltSpd );
			//else
				//lookAboveOffset = Mathf.Lerp(lookAboveOffset, 0, 0.5f);

			transform.LookAt( plrTrans.position + (plrTrans.up * (lookAboveAmt + lookAboveOffset)) );
			//lerpGoalSphere.transform.position = plrTrans.position + (plrTrans.up * (lookAboveAmt + lookAboveOffset));
		}
	}

}
