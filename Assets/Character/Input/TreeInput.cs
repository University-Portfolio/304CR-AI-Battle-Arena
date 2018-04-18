using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Read in a decision tree structure from an xml file
/// </summary>
[RequireComponent(typeof(Character))]
public class TreeInput : MonoBehaviour
{
	public Character character { get; private set; }



	/// <summary>
	/// How long until a new decision can be made
	/// </summary>
	private float decisionTimer;

	/// <summary>
	/// How many times to update this tree per second
	/// </summary>
	[SerializeField]
	private float tickRate = 0.2f;

	private DecisionTree tree;


	void Start()
	{
		character = GetComponent<Character>();
		character.SetColour(new Color(0.4f, 0.4f, 0.4f));

		// Offset agents
		decisionTimer = UnityEngine.Random.value * tickRate;


		tree = new DecisionTree();

		// Set vars initialy
		UpdateDecisionVars();

		// Create states
		tree.RegisterActionState("Flee", ActionFlee);
		tree.RegisterActionState("Block", ActionBlock);
		tree.RegisterActionState("Shoot", ActionShoot);
		tree.RegisterActionState("Skirt", ActionSkirt);

		tree.DebugMake();
	}

	void Update()
	{
		if (decisionTimer > 0.0f)
		{
			decisionTimer -= Time.deltaTime;
		}
		else
		{
			// Only update tree every once in a while
			UpdateDecisionVars();
			tree.Recalculate();
			decisionTimer = tickRate;
		}

		tree.Run();
	}

	void UpdateDecisionVars()
	{
		tree.SetGlobalVar("StageSize", GameMode.main.stage.currentSize);
		tree.SetGlobalVar("AliveCount", GameMode.main.aliveCount);
	}

	void ActionSkirt()
	{
		character.Move(0.1f);
		//character.Turn(0.0f);

		//if (true)
		//	character.Fire();
		//if (true)
		//	character.Block();
	}

	void ActionFlee()
	{
		character.Move(-1.0f);
	}

	void ActionBlock()
	{
	}

	void ActionShoot()
	{
	}
}
