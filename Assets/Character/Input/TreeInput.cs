using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Character))]
public class TreeInput : MonoBehaviour
{
	public Character character { get; private set; }
	

	void Start()
	{
		character = GetComponent<Character>();
		character.SetColour(new Color(0.4f, 0.4f, 0.4f));
	}

	void Update()
	{
		character.Move(1.0f);
		character.Turn(0.0f);

		if (true)
			character.Fire();
		if (true)
			character.Block();
	}
}
