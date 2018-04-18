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
	}

	/// <summary>
	/// Read in the xml for a specfic generation
	/// </summary>
	/// <param name="generation">The desired generation to load (Will load highest found, if -1)</param>
	public bool LoadXML(int generation = -1)
	{
		if (!System.IO.Directory.Exists(dataFolder + collectionName))
			return false;

		// Attempt to find highest generation
		if (generation == -1)
		{
			generation = 0;
			while (true)
			{
				if (!System.IO.File.Exists(dataFolder + collectionName + "/gen_" + (generation + 1) + ".xml"))
					break;
				else
					generation++;
			}
		}

		string path = dataFolder + collectionName + "/gen_" + generation + ".xml";

		if (!System.IO.File.Exists(path))
			return false;


		XmlDocument document = new XmlDocument();
		document.Load(path);

		// Read document
		XmlElement root = document.DocumentElement;
		foreach (XmlElement child in root.ChildNodes)
		{
			if (child.Name == "innovationCounter")
				innovationCounter = System.Int32.Parse(child.InnerText);

			else if (child.Name == "generationCounter")
				generationCounter = System.Int32.Parse(child.InnerText);

			else if (child.Name == "runTime")
				runTime = System.Int32.Parse(child.InnerText);

			// Read genes
			else if (child.Name == "Generation")
			{
				innovationIds = new Dictionary<Vector2Int, int>();
				foreach (XmlElement gene in child.ChildNodes)
				{
					if (gene.Name != "gene")
						continue;

					int fromId = System.Int32.Parse(child.GetAttribute("from"));
					int toId = System.Int32.Parse(child.GetAttribute("to"));
					int inno = System.Int32.Parse(child.GetAttribute("inno"));
					innovationIds[new Vector2Int(fromId, toId)] = inno;
				}
			}


			// Read species (Need to read it before networks)
			else if (child.Name == "ActiveSpecies")
			{
				activeSpecies = new List<NeatSpecies>();
				foreach (XmlElement entry in child.ChildNodes)
				{
					if (entry.Name != "Species")
						continue;

					activeSpecies.Add(new NeatSpecies(this, entry));
				}
			}


			// Read networks
			else if (child.Name == "Population")
			{
				List<NeatNetwork> newPopulation = new List<NeatNetwork>();

				foreach (XmlElement entry in child.ChildNodes)
				{
					if (entry.Name != "Network")
						continue;

					NeatNetwork network = new NeatNetwork(this, entry);
					newPopulation.Add(network);
				}

				population = newPopulation.ToArray();
			}

		}

		Debug.Log("Read from '" + path + "'");
		return true;
	}


	/// <summary>
	/// Retreive names of any collections which exist on disc
	/// </summary>
	/// <returns></returns>
	public static string[] GetExistingCollections()
	{
		if (!System.IO.Directory.Exists(dataFolder))
			System.IO.Directory.CreateDirectory(dataFolder);

		string[] values = System.IO.Directory.GetDirectories(dataFolder);
		for (int i = 0; i < values.Length; ++i)
			values[i] = values[i].Substring(dataFolder.Length);

		return values;
	}
}
