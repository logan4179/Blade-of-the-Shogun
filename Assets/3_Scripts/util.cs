using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class util
{

	//------------------------------------------------------------------------------------

	//TODO: It may be less processor-intesive to just include this function as part of the calling class definition.  I should use the cpu profiler or whatever to figure this out when the time comes.
	//TODO: I might not even need the int version of this.  I think ints get converted to floats as needed.
	static public bool isBetween ( int myValue, int lowVal, int highVal )
	{
		if ( myValue > lowVal && myValue < highVal )
			return true;
		else 
			return false;
	}
	static public bool isBetween ( float myValue, float lowVal, float highVal )
	{
		if ( myValue > lowVal && myValue < highVal )
			return true;
		else 
			return false;
	}
}
