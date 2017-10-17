using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scr_player : MonoBehaviour 
{
	// STATS -----------------------
	int hp = 100;
	public float normalSpeed = 13.1f;
	public float spdMult = 0;
	public float rotSpd = 70.0f;
	float jumpVelocity = 30;
	int dealingDmgAmt = 0;
	float groundLevel = 1.71f;
	public float visiMult = 1.0f;

	// CONTROLS --------------------
	public float horAxis = 0.0f;
	public float vertAxis = 0.0f;
	public float axisThresh = 0.02f; //TODO: once I'm sure about this value, I don't think I need this to be public, but cameracontroller sets its axisThresh to be this value in the start, so I'll need to update cameraController's code after I make this private
	int stillTime = 4;				//Amount of frames player is unresponsive after performing certain actions.

	// CAMERA -----------------------
	float camFollowAmt = 10f;
	public Vector3 plrMoveDirVect = Vector3.zero;

	// TRUTH VARIABLES ---------------------
	public bool amDying = false;
	public bool amGrounded = true;
	public bool amBlocking = false;
	public bool amRunning = false;
	public bool amStriking = false;
	[SerializeField] bool amRecoiling = false;
	[SerializeField] bool amThrowing = false;
	[SerializeField] bool amPreoccupied = false;

	// Object References ----------------------------
	static public scr_mgrMain g = null;
	Transform camTrans = null;
	Animator anim;

	//--ALARMS ------

	// ANIMATION COUNTERS --------------
	public int ac_normalStrike = 0;
	public int ac_block = 0;
	public int ac_throw = 0;
	public int ac_takingDmg = 0;

	// A N I M A T I O N   L E N G T H S  ---------------------
	//All animation lengths should add the amount specifed in the stillTime variable to their amount.

	static int animLength_normalStrike = 20;
	static int animLength_block = 34;
	static int animLength_recoil = 26;
	static int animLength_throw = 17;

	// DIAGNOSTIC ------------------------------------------
	//Transform moveSphere = null;
	//public Vector3 nuVect = Vector3.zero;
	//public float testFloat = 0.0f;
	//public float oldFloat = 0.0f;
//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 //----//----//----//----//----//----//  S T A R T  ---//----//----//----//----//----//----//----//----//
	void Start ()
	{
		g = GameObject.Find("GameManager").GetComponent<scr_mgrMain>();
		anim = this.GetComponent<Animator>();
		camTrans = GameObject.Find("Camera").transform;

		StartCoroutine("moveACs");
	}

//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 //----//----//----//----//----//----//----//  U P D A T E  -//----//----//----//----//----//----//----//	
	void Update()
	{
		//moveAlarms();

		// PAUSING/RESUMING ------------------------------------
		if ( Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Joystick1Button7) ) 
		{
			if ( !g.gamePaused ) //TODO: Might want class' own paused variable bc it might be inefficient to look into the main manager each update.
			{
				g.gamePaused = true;
				Time.timeScale = 0;
			}
			else
			{
				g.gamePaused = false;
				Time.timeScale = 1;
			}
		}


		if ( !g.gamePaused  )
		{
			/*
			//--------------------D E T E C T   D A M A G E ---------------------------//
			if ( distToOpponent < 6f && opponentScript.amAttacking == true && decTakingDmgAlarm == 0 )
			{
				decTakingDmgAlarm = frameCount_TakingDmg;
				hp -= opponentScript.dealingDmgAmt;
				Debug.Log("HP = " + hp);
			}
			*/
			moveAlarms ();

			if ( !amPreoccupied )
				getInput();
		}
	}
//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	//----//----//----//----//----//----//  L A T E   U P D A T E  -//----//----//----//----//----//----//----//	
	void LateUpdate()
	{
		if( !g.gamePaused )
		{
			//----------------------S E T   A N I M A T I O N S  --------------------------//
			if( amDying )
			{
				
			}
			else if( amRecoiling )
			{
				if( ac_takingDmg == 0 )
					anim.SetTrigger ( "animTakingDmg" );
			}
			else if( amBlocking )
			{
				if( ac_block == 0 )
					anim.SetTrigger( "animBlocking" );
			}
			else if( amThrowing )
			{
				if( ac_throw == 0 )
					anim.SetTrigger( "animThrowing" );
			}
			else if( amStriking )
			{
				if( ac_normalStrike == 0 )
					anim.SetTrigger( "animPunching" );
			}
			else if( amRunning )
			{
				anim.SetBool( "animRunning", true );
			}
			else
			{
				setTruth( false, false, false, false, false );

				anim.SetBool( "animRunning", false );
			}
			//---------------------- S E T   P R E O C C U P I E D  --------------------------//
			if( amStriking || amBlocking || amRecoiling || amThrowing )
			{
				amPreoccupied = true;

				amRunning = false;
				anim.SetBool( "animRunning", false );
			}
			else
				amPreoccupied = false;
		}
	}
//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 //----//----//----//----//----//----//  M E T H O D S  -//----//----//----//----//----//----//----//
	public void getInput()
	{
		horAxis = Input.GetAxis ("Horizontal");
		vertAxis = Input.GetAxis ("Vertical");

		// M O V E M E N T --------------------------------------------------------------------------
		if( Mathf.Abs(horAxis) > axisThresh || Mathf.Abs(vertAxis) > axisThresh )
		{
			// POSITIONAL MOVEMENT -------------------------------------------------------------
			plrMoveDirVect = (camTrans.forward * vertAxis) + (camTrans.right * horAxis);
			plrMoveDirVect.y = 0;

			spdMult = Mathf.Min( 1.0f, new Vector3(horAxis, 0, vertAxis).magnitude );

			transform.Translate( plrMoveDirVect.normalized * normalSpeed * spdMult * Time.deltaTime, Space.World );

			// ROTATIONAL MOVEMENT -------------------------------------------------------------
			transform.LookAt( transform.position + plrMoveDirVect ); //TODO: See if lerping to faceVect might be better

			amRunning = true;

		}
		else
			amRunning = false;

		if( Input.GetKeyDown(KeyCode.JoystickButton2) )
		{
			amStriking = true;
			ac_normalStrike = 0;
		}

		if( Input.GetKeyDown(KeyCode.G) )
			moveHealth( -10 );

	}
	//---------------------------------------------------------------------------------------------------
	void setTruth( bool blocking, bool running, bool attacking, bool recoilling, bool throwing )
	{
		amBlocking = blocking;
		amRunning = running;
		amStriking = attacking;
		amRecoiling = recoilling;
		amThrowing = throwing;

	}

	//---------------------------------------------------------------------------------------------------

	void moveAlarms()
	{


	}
	//---------------------------------------------------------------------------------------------------

	public void moveHealth ( int amt )
	{
		hp += amt;
		GameObject.Find("hpSlider").GetComponent<Slider>().value += amt;
		GameObject.Find("hpText").GetComponent<Text>().text = ( hp + "/" + 100 );
	}
	//---------------------------------------------------------------------------------------------------

	IEnumerator moveACs()
	{
		for ( ; ; )
		{
			if ( ac_takingDmg < animLength_recoil )
			{
				ac_takingDmg++;
			}
			else
				ac_takingDmg = animLength_recoil;

			if ( ac_normalStrike < animLength_normalStrike )
			{
				ac_normalStrike++;
			}
			else
			{
				ac_normalStrike = animLength_normalStrike;
				amStriking = false;
			}

			if ( ac_block < animLength_block )
			{
				ac_block++;
			}
			else
				ac_block = animLength_block;

			if ( ac_throw < animLength_throw )
			{
				ac_throw++;
			}
			else
				ac_throw = animLength_throw;

			yield return new WaitForSeconds(0.0417f); //makes it run 24 frames per second, like the animations are made in Maya
		}

	}
}

