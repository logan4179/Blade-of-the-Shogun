using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_basicEnemyMONO : scr_basicEnemyMEMBERS 
{
	//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 	 //----//----//----//----//----//----//  S T A R T  ---//----//----//----//----//----//----//----//----//
	void Start () 
	{
		plrTrans = GameObject.Find("PLAYER").transform;
		plrScript = plrTrans.gameObject.GetComponent<scr_player>();
		anim = this.GetComponent<Animator>();
		g = GameObject.Find("GameManager").GetComponent<scr_mgrMain>();

		enemyState = 0;
		startPos = transform.position;
		nextPos = getNextPosition( startPos, 20 );

		StartCoroutine("moveAlarms"); //This seems to work here instead of in the update. I've since removed the old movealarms function
		StartCoroutine("moveACs"); // Makes the animation counters count through their animation lengths
	}
	
	//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//----//
	 	 //----//----//----//----//----//----//----//  U P D A T E  -//----//----//----//----//----//----//----//		
	void Update () 
	{
		if( !g.gamePaused ) //TODO: Might be more efficient for each class to have its own paused variable, could make it static for more efficiency as well
		{
			distToPlr = Vector3.Distance(transform.position , plrTrans.position);
			dirToPlr = Quaternion.Angle( transform.rotation, Quaternion.LookRotation(plrTrans.position - transform.position, Vector3.up) );

			//================[[ D E T E C T   D A M A G E ]]====================//
			/*
			if ( damage conditions are true )
			{
				hp -= plrScript.dealingDmgAmount;

				if ( hp <= 0 )
					setTruth( false, false, false, true, false, false, false, deathAnimLength );
				else
					setTruth( false, false, true, false, false, false, false, recoilAnimLength );

			}
			*/
			if( !amDying && !amStriking && !amRecoilling && !amBlocking && distToPlr < calcAIDistance )
			{
				//TODO: I need to take the animation stuff out of the state section of the script below

				//===========[[ D E T E C T   V I S I B I L I T Y ]]===============//
				if ( distToPlr < (maxVisibleDistance * plrScript.visiMult) && dirToPlr <= visibilityRadius && !Physics.Linecast(this.transform.position, plrTrans.position, solidMask) )
				{
					plrVisible = true;
					plrLastSeenPos = plrTrans.position;
					dirToPlrLastSeenPos = dirToPlr;
					dirToNextPatrol = dirToPlr;
					distToNextPatrol = Vector3.Distance(transform.position , plrLastSeenPos);

					if ( decPlrVisibilityBuild < 100 )
						decPlrVisibilityBuild += ( visibilityRadius/Mathf.Max(dirToPlr, 1) * (1 - (distToPlr/maxVisibleDistance)) );

					if ( enemyState == 0 )
					{
						decSpottedPauseAlarm = 500;
						decSuspPauseAlarm = 1200;
					}
				}
				else
					plrVisible = false;

				//==================[[ C H O O S E   S T A T E ]]=======================//
				if ( decPlrVisibilityBuild == 0 && decSuspPauseAlarm == 0 && decStayInAttackStateAlarm == 0 )
					enemyState = 0;
				else if ( (util.isBetween(decPlrVisibilityBuild, 0, 100) && enemyState == 0) || (enemyState == 2 && distToPlr > maxVisibleDistance) )
				{
					if ( enemyState == 2 && g.numbAttacking > 0 )
						g.numbAttacking --;
					enemyState = 1;
				}
				else if ( decPlrVisibilityBuild > 100 && enemyState != 2 )
				{
					enemyState = 2;
					g.numbAttacking++;
					g.setEnemiesAndSurPts();
				}

				// ================[[ S T A T E   L O G I C ]]========================//
				if ( enemyState == 0 )
				{//-----  P A T R O L L I N G  ------
					dirToNextPatrol = Quaternion.Angle( transform.rotation, Quaternion.LookRotation(nextPos - transform.position, Vector3.up) );
					distToNextPatrol = Vector3.Distance(transform.position, nextPos);
					nextPos.y = transform.position.y;

					if ( decPatrolWaitAlarm == 0 )
					{
						if ( dirToNextPatrol > 10 )
						{
							transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation ( nextPos - transform.position, Vector3.up), rotSpd * Time.deltaTime );
							setTruth( false, false, false, false, false, false, true ); //amRotating
						}
						else
						{
							if ( dirToNextPatrol > 1 )
							{
								transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation ( nextPos - transform.position, Vector3.up), rotSpd * Time.deltaTime );
								//setTruth( false, false, false, false, false, false, true, 0 ); //don't need to rotate here, just walk which is triggered on next if statement
							}
							if ( distToNextPatrol > 4 )
							{
								transform.Translate(0, 0, patrolSpd * Time.deltaTime);
								setTruth( false, false, false, false, false, true, false ); //walk animation
							}
							else
							{	nextPos = getNextPosition( startPos, 20 );
								decPatrolWaitAlarm = Random.Range( 120, 1080 );
								setTruth( false, false, false, false, false, false, false ); //idle animation
							}
						}
					}
				}
				else if ( enemyState == 1 )
				{//-----  S U S P I C I O U S   ------------
					// print("suspicious triggered for " + name + "decPlrVisibilityBuild == " + decPlrVisibilityBuild ); /*DIAGNOSTIC*/-------------------------

					distToNextPatrol = Vector3.Distance(transform.position, plrLastSeenPos);

					if ( dirToPlrLastSeenPos > 10.0f )
					{
						transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation ( plrLastSeenPos - transform.position, Vector3.up), rotSpd * Time.deltaTime );
						setTruth( false, false, false, false, false, false, true ); //amRotating
					}
					else if ( decSpottedPauseAlarm == 0 )
					{
						if ( dirToPlrLastSeenPos > 2.0f )
						{
							transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation ( plrLastSeenPos - transform.position, Vector3.up), rotSpd * Time.deltaTime );
							//setTruth( false, false, false, false, false, false, true, 0 ); //don't actually need to trigger rotate animation because not rotating much, just let the next if statement trigger the walking animation
						}
						if ( distToNextPatrol > 2.0f )
						{
							transform.Translate(0, 0, patrolSpd * Time.deltaTime);
							setTruth( false, false, false, false, false, true, false ); // amWalking
						}
						else
							setTruth( false, false, false, false, false, false, false ); // idle

					}
				}
				else if ( enemyState == 2 )
				{//-----  A T T A C K I N G  ------------
					//print("attack triggered for " + name + " decPlrVisibilityBuild == " + decPlrVisibilityBuild ); //DIAGNOSTIC------------------------
					dirToPlrLastSeenPos = Quaternion.Angle( transform.rotation, Quaternion.LookRotation(plrLastSeenPos - transform.position, Vector3.up) );

					if( plrVisible )
					{
						decStayInAttackStateAlarm = 800; //This keeps the enemy in the attack state after losing sight of the player for 800 updates.

						if ( g.numbAttacking > 1 && ((!amEngaging && distToPlr < g.furthestEngagingDist) || (amEngaging && Vector3.Distance(transform.position, surAnchor.position) > 7.0f) || ( !amEngaging && g.numbAttacking < 4)) )
							g.setEnemiesAndSurPts();

						if ( !amRecoilling && !amStriking && !amBlocking )
						{
							if ( decBlockWaitAlarm == 1 )
							{
								setTruth( false, true, false, false, false, false, false ); // amBlocking

							}
							else
							{ //Not blocking stuff
								if ( plrScript.amStriking && dirPlrToMe < 40 && decBlockWaitAlarm == 0 )
									decBlockWaitAlarm = Random.Range( 3, 13 );

								//----------------------------------------------------------------------------------------------------------------------------------------------------------
								if ( dirToPlr > 10 )
								{
									transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation ( plrTrans.position - transform.position, Vector3.up), rotFastSpd * Time.deltaTime );
									setTruth( false, false, false, false, false, false, true ); // amRotating
								}
								else
								{
									if ( dirToPlr > 2.0f )
									{
										transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation ( plrTrans.position - transform.position, Vector3.up), rotFastSpd * Time.deltaTime );
										//setTruth( false, false, false, false, false, false, true, 0 ); //you don't actually want this, because you want the running to be triggered, which will happen in the next if statement
									}
									if ( distToPlr > 10.0f )
									{
										transform.position = Vector3.MoveTowards( transform.position, plrTrans.position, runSpd * Time.deltaTime );
										setTruth( false, false, false, false, true, false, false ); // amRunning
									}
								}

								if ( amEngaging )
								{
									distToSurAnchor = Vector3.Distance(transform.position, surAnchor.position);

									if ( util.isBetween(decAttackWaitAlarm, 1, 20) && distToPlr > 2.0f )
									{
										transform.position = Vector3.MoveTowards( transform.position, plrTrans.position, patrolSpd * Time.deltaTime );
										setTruth( false, false, false, false, false, true, false ); //amWalking
									}
									else if ( decAttackWaitAlarm == 1 && distToPlr < (curStrikeDist + 0.8f) )
									{  //ATTACK! ---------
										setTruth( true, false, false, false, false, false, false ); //amStriking
										// chooseAttack(); //TODO: make a method that decides next attack
										ac_normalAttack = 0;
									}
									else if ( distToSurAnchor >= 1.6f && distToPlr > 1.6f )
									{ //TODO: 4/18/17 - If you test it out, there is definitely some funkiness here where the enemy will stick close to the player on occasion when distToPlr < 1.6f (maybe to check if plrScript.isMoving != true)
										transform.position = Vector3.MoveTowards( transform.position, surAnchor.position, runSpd * Time.deltaTime );
										amRunning = true;
										nextPos = transform.position;
										setTruth( false, false, false, false, true, false, false ); // amRunning
									}
									else if ( distToSurAnchor < 1.6f )
									{
										if ( Vector3.Distance(transform.position, nextPos) < 0.05f && decEngageSwayAlarm == 0 )
										{
											nextPos = getNextPosition( surAnchor.position, 1f );
											decEngageSwayAlarm = Random.Range( 150, 270 );
										}
										else if( decEngageSwayAlarm == 0 )
										{
											transform.position = Vector3.MoveTowards( transform.position, nextPos, swaySpeed * Time.deltaTime );
											setTruth( false, false, false, false, false, true, false ); // amWalking
										}
										else
											setTruth( false, false, false, false, false, false, false ); // Idle
									}
									else if ( distToPlr > maxVisibleDistance )
									{
										setTruth( false, false, false, false, false, false, false ); // Idle
									}
								}
							}
						}
					}
					else // !plrVisible
					{
						distToNextPatrol = Vector3.Distance(transform.position, plrLastSeenPos);

						if ( dirToPlrLastSeenPos > 1 )
							transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation ( plrLastSeenPos - transform.position, Vector3.up), rotSpd * Time.deltaTime );
						else if ( distToNextPatrol > 4 )
						{
							transform.Translate(0, 0, runSpd * Time.deltaTime);
							setTruth( false, false, false, false, true, false, false ); // amRunning
						}
						else
						{	nextPos = getNextPosition( plrLastSeenPos, 8 );
							decPatrolWaitAlarm = Random.Range( 120, 1080 );
							setTruth( false, false, false, false, false, false, false ); // idle
						}
					}
				}
			} // End of if( hp > 0 && dirToPlr < calcAIDistance )
		} // End of if( !g.gamePaused ) 
	} // End of Update()
	void LateUpdate ()
	{
		if( !g.gamePaused )
		{
			//=============[[ S E T   A N I M A T I O N ]]====================//
			if( amDying )
			{
				//if( ac_Dying == 0 )
					//anim.SetBool( "animDying", true );
			}
			else if( amRecoilling )
			{
				if( ac_TakeDmg == 0 )
					anim.SetTrigger( "animTakingDmg" );
			}
			else if( amBlocking )
			{
				if( ac_Block == 0 )
					anim.SetTrigger( "animBlocking" );
			}
			else if( amStriking )
			{
				if( ac_normalAttack == 0 )
					anim.SetTrigger( "animPunching" );
			}
			else if( amWalking ) //TODO: try to rearrange this stuff with this at the beginning to see if cpu profiler logs a difference
			{
				anim.SetBool( "animWalking", true );
			}
			else if( amRotating )
			{
				//anim.SetBool( "animRotating", true );
			}
			else if( amRunning )
			{
				anim.SetBool( "animRunning", true );
			}
			else
			{
				setTruth( false, false, false, false, false, false, false ); //Idle

				anim.SetBool( "animRunning", false );
				anim.SetBool( "animWalking", false );
			}
		}
	}
}