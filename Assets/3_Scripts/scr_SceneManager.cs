using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_SceneManager : MonoBehaviour 
{
	//I think maybe these should all be static, like maybe I don't even need an object instance of this and all the methods should be static.
	public void LoadScene( string levelName )
	{
		Debug.Log("Level load requested for: " );

		//Application.LoadLevel( levelName );
			
	}

	public void QuitRequest()
	{
		Debug.Log("I want to quit!");
		Application.Quit();
	}

	public void Player1Chosen( string charName)
	{
		//Application.LoadLevel( charName );

	} 
}
