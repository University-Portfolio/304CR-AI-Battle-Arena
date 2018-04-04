using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAccessories : MonoBehaviour
{
	[Header("Socket")]
	public Transform hatSocket;
	public Transform maskSocket;

	[Header("Assets")]
	public GameObject hatCrownFirst;
	public GameObject hatCrownSecond;
	public GameObject hatCrownThird;

	public GameObject hatDunceFirst;
	public GameObject hatDunceSecond;
	public GameObject hatDunceThird;
	
	public GameObject[] hairAges;


	/// <summary>
	/// Gives a crown to this player
	/// </summary>
	/// <param name="place">The current placement of this character</param>
	public void GiveCrown(int place)
	{
		if (place == 0)
			Instantiate(hatCrownFirst, hatSocket);
		else if (place == 1)
			Instantiate(hatCrownSecond, hatSocket);
		else if (place == 2)
			Instantiate(hatCrownThird, hatSocket);
	}

	/// <summary>
	/// Gives a dunce hat to this player
	/// </summary>
	/// <param name="place">The current placement of this character</param>
	public void GiveDunce(int place)
	{
		if (place == 0)
			Instantiate(hatDunceFirst, hatSocket);
		else if (place == 1)
			Instantiate(hatDunceSecond, hatSocket);
		else if (place == 2)
			Instantiate(hatDunceThird, hatSocket);
	}

	/// <summary>
	/// Grow appropriate hair based on an age
	/// </summary>
	/// <param name="age">The current age of this agent</param>
	public void GrowHair(int age)
	{
		if (age == 0)
			return;

		int index = System.Math.Min(age / 4, hairAges.Length - 1);
		Instantiate(hairAges[index], maskSocket);
	}
}
