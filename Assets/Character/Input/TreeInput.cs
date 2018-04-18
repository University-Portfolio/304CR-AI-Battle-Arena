using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Read in a decision tree structure from an xml file
/// </summary>
[RequireComponent(typeof(Character))]
public class TreeInput : MonoBehaviour
{
	/// <summary>
	/// What folder to save/load data from
	/// </summary>
	public static string dataFolder = "AI/Decision Trees/";

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
	
	/// <summary>
	/// The current tree being used
	/// </summary>
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
		tree.RegisterActionState("Defend", ActionDefend);
		tree.RegisterActionState("Attack", ActionAttack);
		tree.RegisterActionState("Skirt", ActionSkirt);

		LoadTree("helloworld.ai.xml");
		//tree.DebugMake();
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
		tree.SetGlobalVar("ActionEnum", (float)character.currentAction);
		tree.SetGlobalVar("ActionCooldown", character.actionTimer);
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

	void ActionDefend()
	{
	}

	void ActionAttack()
	{
		character.Fire();
	}

	/// <summary>
	/// Read the XML file for a specific tree
	/// </summary>
	/// <param name="name">The name of the tree to load</param>
	/// <returns></returns>
	public bool LoadTree(string name)
	{
		if (!tree.LoadXML(dataFolder + name))
		{
			Debug.LogWarning("Cannot load tree '" + dataFolder + name + "'");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Retreive names of any trees which exist on disc
	/// </summary>
	/// <returns></returns>
	public static string[] GetExistingTrees()
	{
		if (!System.IO.Directory.Exists(dataFolder))
			System.IO.Directory.CreateDirectory(dataFolder);

		string[] values = System.IO.Directory.GetDirectories(dataFolder);
		for (int i = 0; i < values.Length; ++i)
			values[i] = values[i].Substring(dataFolder.Length);

		return values;
	}
}
