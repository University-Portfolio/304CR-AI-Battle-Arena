using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class SpamInput : MonoBehaviour
{
	public Character character { get; private set; }

	public float moveAmount = 0.0f;
	public float turnAmount = 0.0f;
	public bool shoot = false;
	public bool block = false;

	void Start()
	{
		character = GetComponent<Character>();
	}

	void Update()
	{
		character.Move(moveAmount);
		character.Turn(turnAmount);
		
		if (shoot)
			character.Fire();
		if (block)
			character.Block();
	}
}
