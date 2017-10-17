using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_basicEnemyMEMBERS : MonoBehaviour 
{
	//TODO: change appropriate integers and alarms to to sbyte, which runs from -128 to 127, 
	//and see if cpu profiler logs a difference

	//OBJECT/REFERENCE  -------------------
	static public Transform plrTrans = null;
	static public scr_player plrScript = null;
	protected Animator anim = null;   //TODO: Should this be a static animator?  I should investigate.
	static public scr_mgrMain g = null;

	//  S T A T S   --------------------
	protected int hp = 100; //TODO:
	static public float swaySpeed = 1f; //speed enemies sway around player back and forth when engaging
	static public float patrolSpd = 1.8f; 
	static public float runSpd = 3.2f;
	static public float rotSpd = 90.0f;
	static public float rotFastSpd = 180.0f;
	static public int dealingDmgAmt = 0; //TODO:
	static public float groundLevel = 1.71f;
	static public float visibilityRadius = 60;
	static public float maxVisibleDistance = 40;
	public float curStrikeDist = 5.0f; //TODO: take this away when not diagnosing
	// SPECIAL --------------------------
	static protected float calcAIDistance = 150.0f; //Distance entity has to be within to need its AI to run.
	public int enemyState = 0; //TODO:

	// T R U T H   V A R I A B L E S -----
	public bool plrVisible = false;
	protected bool amGrounded = true;
	[SerializeField] protected bool amBlocking = false;
	[SerializeField] protected bool amRecoilling = false;
	[SerializeField] protected bool amRunning = false;
	[SerializeField] protected bool amWalking = false;
	protected bool amRotating = false;
	public bool amEngaging = false;
	[SerializeField]protected bool amStriking = false;
	[SerializeField] protected bool amDying = false;

	//  P O S I T I O N S  ----------------
	[SerializeField] protected Vector3 startPos = Vector3.zero;
	public Vector3 nextPos = Vector3.zero;
	[SerializeField] protected Vector3 plrLastSeenPos = Vector3.zero;
	protected float dirToPlrLastSeenPos = 0;
	public Transform surAnchor = null;

	public float distToPlr;
	[SerializeField] protected float dirToPlr;
	protected float dirPlrToMe;
	[SerializeField] protected float dirToNextPatrol = 0;
	[SerializeField] protected float distToNextPatrol = 0;
	[SerializeField] protected float distToSurAnchor = 0;
	[SerializeField] protected float decPlrVisibilityBuild = 0; //A value that builds as long as the enemy can see the player until it reaches 100 and the enemy gets into the attack state.

	protected float visRot = 0;
	protected float visDist = 0;
	protected float visAmt = 0;

	static protected int solidMask = 1 << 8;
	// A L A R M S  ----------------------
	static protected int decEvery2Alarm = 0;		 //Constantly counts from 0 to 3 
	[SerializeField] protected int decPatrolWaitAlarm = 50;  //Random amount to wait in patrol state after reaching patrol point
	[SerializeField] protected int decSpottedPauseAlarm = 0; //500 frames the enemy will pause for when he first catches a glimpse of the player before starting to move toward that location to investigate.
	[SerializeField] protected int decStayInAttackStateAlarm = 0; //800 updates the enemy will remain in the attacking state after losing sight of the player, while the enemy looks around.
	[SerializeField] protected int decSuspPauseAlarm = 0;	 //random amt the enemy waits at the spot he saw the player after he moves there to investigate.
	[SerializeField] protected int decEngageSwayAlarm = 0;	 //How long to wait in the same spot after swaying to a new spot while engaging the player
	[SerializeField] protected int decAttackWaitAlarm = 0;	 //How long(random) to wait 'til striking the player
	[SerializeField] protected int decBlockWaitAlarm = 0; //Time to wait(random) when player strikes before blocking.

	// A N I M A T I O N    C O U N T E R S ----------------
	[SerializeField] protected int ac_normalAttack = 0;		 //Counts through attack animation
	[SerializeField] protected int ac_Block = 0; 	//Counts through block animation
	[SerializeField] protected int ac_TakeDmg = 0;	//Counts through recoil animation
	[SerializeField] protected int ac_Dying = 0;		//Counts through death animation

	// A N I M A T I O N   L E N G T H S  ---------------------
	static protected int animLength_block = 15;
	static protected int animLength_takeDmg = 22;
	static protected int animLength_throw = 13;
	static protected int animLength_death = 20;
	static protected int animLength_normalStrike = 20;

	// DIAGNOSTIC -------------------

	//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 //----//----//----//----//----//----//  M E T H O D S  -//----//----//----//----//----//----//----//
	public Vector3 getNextPosition( Vector3 patrolAround, float patDist )
	{
		Vector3 newPos = Vector3.up;
		RaycastHit hit;

		newPos = new Vector3( Random.Range(patrolAround.x + patDist, patrolAround.x - patDist), transform.position.y, Random.Range(patrolAround.z + patDist,  patrolAround.z - patDist) ); 

		Physics.Linecast( this.transform.position, newPos, out hit, solidMask );

		/*DIAGNOSTIC*/ //diagMethod( newPos, hit, 1 ); //TODO: Delete this after satisfied
		while ( hit.transform != null || Vector3.Distance(this.transform.position, newPos) < (patDist / 3) )
		{
			if ( hit.transform != null )
			{
				if (Vector3.Distance(this.transform.position, hit.point) < (patDist / 3))
				{
					newPos = new Vector3( Random.Range(patrolAround.x + patDist, patrolAround.x - patDist), transform.position.y, Random.Range(patrolAround.z + patDist,  patrolAround.z - patDist) ); 
					Physics.Linecast( this.transform.position, newPos, out hit, solidMask );
				}
				else
				{
					newPos = hit.point;
					break;
				}

			}
			else   //Vector3.Distance(this.transform.position, newPos) < (patDist / 3)
			{
				newPos = new Vector3( Random.Range(patrolAround.x + patDist, patrolAround.x - patDist), transform.position.y, Random.Range(patrolAround.z + patDist,  patrolAround.z - patDist) ); 
				Physics.Linecast( this.transform.position, newPos, out hit, solidMask );
			}
		}

		return newPos;
	}
	//------------------------------------------------------------------------------------
	public void diagMethod( Vector3 nuPos, RaycastHit nuHit, int thisLvl )
	{
		if ( nuPos.x == 0 && nuPos.z == 0 )
		{
			print ( "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
			print(name + "'s newPos is: " + nuPos + " Level: " + thisLvl );
			print ( "Initial Collider hit: " + nuHit.transform );

			Physics.Linecast( this.transform.position, nuPos, out nuHit, solidMask );

			print ( "Initial Collider hit: " + nuHit.transform );
		}
		else
			print ( name + " Level 1: " + nuPos );
	}
	//------------------------------------------------------------------------------------
	public void setTruth( bool striking, bool blocking, bool recoil, bool dying, bool running, bool walking, bool rotating )
	{ //TODO: get rid of amt integer in the parameters if not using this to set animations
		amStriking = striking;
		amBlocking = blocking;
		amRecoilling = recoil;
		amDying = dying;
		amRunning = running;
		amWalking = walking;
		amRotating = rotating;
	}
	//------------------------------------------------------------------------------------
	//public void moveAlarms()
	//{
	IEnumerator moveAlarms ()
	{
		for (; ;) 
		{
			if (decEvery2Alarm > 0)
				decEvery2Alarm--;
			else
				decEvery2Alarm = 2;

			if (decPatrolWaitAlarm > 0 && distToNextPatrol > 2)
				decPatrolWaitAlarm--;
			else
				decPatrolWaitAlarm = 0;

			if (decSpottedPauseAlarm > 0)
				decSpottedPauseAlarm--;
			else
				decSpottedPauseAlarm = 0;

			if (decStayInAttackStateAlarm > 0)
				decStayInAttackStateAlarm--;
			else
				decStayInAttackStateAlarm = 0;

			if (decPlrVisibilityBuild > 0.9f && plrVisible == false)
				decPlrVisibilityBuild -= 0.9f;
			else if (plrVisible == false)
				decPlrVisibilityBuild = 0;

			if (decSuspPauseAlarm > 0 && distToNextPatrol < 5)
				decSuspPauseAlarm--;
			else if (decSuspPauseAlarm < 1)
				decSuspPauseAlarm = 0;

			if (decEngageSwayAlarm > 0)
				decEngageSwayAlarm--;
			else
				decEngageSwayAlarm = 0;
			
			if ( amEngaging && decAttackWaitAlarm > 0 && distToPlr < 20) //TODO: it may be better to make this different than 20
				decAttackWaitAlarm--;
			else if ( amEngaging && decAttackWaitAlarm < 1 )
				decAttackWaitAlarm = Random.Range (50, 500);
			else
				decAttackWaitAlarm = 0;

			if ( decBlockWaitAlarm > 0 )
				decBlockWaitAlarm--;
			else
				decBlockWaitAlarm = 0;
			
			yield return new WaitForSeconds(0.04f); //Makes this method fire at 25 frames per second
		}
	}
	//------------------------------------------------------------------------------------
	IEnumerator moveACs ()
	{
		for (; ;) 
		{
			if( ac_normalAttack < animLength_normalStrike )
				ac_normalAttack++;
			else
			{
				ac_normalAttack = animLength_normalStrike;
				amStriking = false;
			}
			if( ac_Block < animLength_block )
				ac_Block++;
			else
			{
				ac_Block = animLength_block;
				amBlocking = false;
			}
			if( ac_TakeDmg < animLength_takeDmg )
				ac_TakeDmg++;
			else
			{
				ac_TakeDmg = animLength_takeDmg;
				amRecoilling = false;
			}
			if( ac_Dying < animLength_death )
				ac_Dying++;
			else
			{
				ac_Dying = animLength_death;
				amDying = false;
			}
			yield return new WaitForSeconds(0.04f); //TODO: need to make this fire at the speed of my animations
		}
	}
}
