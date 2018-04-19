using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Read in a decision tree structure from an xml file
/// </summary>
[RequireComponent(typeof(Character))]
public class TreeInputAgent : MonoBehaviour
{
	/// <summary>
	/// What folder to save/load data from
	/// </summary>
	public static string dataFolder = "AI/Decision Trees/";

	public Character character { get; private set; }
	public Color colour { get; private set; }


	/// <summary>
	/// How long until a new decision can be made
	/// </summary>
	private float decisionTimer;

	/// <summary>
	/// How often this agent will update in seconds
	/// </summary>
	[SerializeField]
	private float tickRate = 0.2f;
	
	/// <summary>
	/// The current tree being used
	/// </summary>
	public DecisionTree tree { get; private set; }


	/// <summary>
	/// Cached Profile vars
	/// </summary>
	private float moveSpeed;
	private float turnSpeed;
	private float attackAccuracy;
	private float attackDistance;

	private float nearCharacterRange;

	/// <summary>
	/// AI Frame vars
	/// </summary>
	private float distanceFromCentre;
	private float distanceFromEdge;
	private Character closestCharacter;
	private List<Character> nearbyCharacters = new List<Character>();
	private ArrowProjectile closestArrow;


	void Start()
	{
		character = GetComponent<Character>();
		character.SetColour(colour, false);

		// Offset agents
		decisionTimer = UnityEngine.Random.value * tickRate;
	}

	void Update()
	{
		if (character.IsDead)
			return;

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

		distanceFromCentre = Vector3.Distance(GameMode.main.stage.transform.position, character.transform.position);
		distanceFromEdge = Mathf.Max(0, GameMode.main.stage.currentSize - distanceFromCentre);

		tree.SetGlobalVar("CentreDist", distanceFromCentre);
		tree.SetGlobalVar("EdgeDist", distanceFromEdge);


		float closestCharDist = 99999.0f;
		closestCharacter = null;

		float closestArrowDist = 99999.0f;
		closestArrow = null;

		nearbyCharacters.Clear();

		foreach (Character other in GameMode.main.characters)
			if (other != character)
			{
				if (other.isAlive)
				{
					float distance = Vector3.Distance(other.transform.position, character.transform.position);

					// This is the closest char
					if (closestCharacter == null || distance < closestCharDist)
					{
						closestCharacter = other;
						closestCharDist = distance;
					}

					// Player is nearby
					if (distance < nearCharacterRange)
						nearbyCharacters.Add(other);
				}

				if (other.currentProjectile != null)
				{
					float distance = Vector3.Distance(other.currentProjectile.transform.position, character.transform.position);

					// This is the closest arrow
					if (closestArrow == null || distance < closestArrowDist)
					{
						closestArrow = other.currentProjectile;
						closestArrowDist = distance;
					}
				}
			}


		tree.SetGlobalVar("ClosestEnemy", closestCharDist);
		tree.SetGlobalVar("ClosestArrow", closestArrowDist);
		tree.SetGlobalVar("NearbyEnemies", nearbyCharacters.Count);
	}

	/// <summary>
	/// Move the character to face this location
	/// </summary>
	/// <param name="location">The location to look to</param>
	/// <param name="desiredConfidence">The desired condifdence to turn until</param>
	/// <returns>The confidence the agent has about it's current direction</returns>
	private float FaceLocation(Vector3 location, float desiredConfidence = 0.999f)
	{
		// Turn to put back to centre
		Vector3 toLocation = (location - character.transform.position).normalized;

		float forwardDot = Vector3.Dot(character.transform.forward, toLocation);
		float rightDot = Vector3.Dot(character.transform.right, toLocation);
		
		// Turn to centre
		if(forwardDot < desiredConfidence)
		{
			if (rightDot > 0)
				character.Turn(turnSpeed);
			else
				character.Turn(-turnSpeed);
		}

		return forwardDot;
	}

		
	void ActionSkirt()
	{
		// Prevent falling off edge
		float confidence = FaceLocation(GameMode.main.stage.transform.position);
		if (distanceFromCentre > 2)
		{
			if (confidence > 0.25f)
				character.Move(moveSpeed);
			else if (confidence < -0.25f)
				character.Move(-moveSpeed);
		}
	}

	void ActionFlee()
	{
		// Attempt to move away from the masses
		Vector3 centre = new Vector3();
		int count = 0;

		foreach (Character other in nearbyCharacters)
			if (other.isAlive)
			{
				centre += other.transform.position;
				count++;
			}

		if (count == 0)
			return;
		centre /= count;


		// Move away from group
		float confidence = FaceLocation(centre);
		if (confidence > 0.25f)
			character.Move(-moveSpeed);
		else if (confidence < -0.25f)
			character.Move(moveSpeed);
	}

	void ActionDefend()
	{
		if (closestArrow == null)
			return;

		// Attempt to run away from nearest arrow
		Vector3 toArrow = (character.transform.position - closestArrow.transform.position).normalized;
		float arrowDot = Vector3.Dot(closestArrow.Direction, toArrow);
		
		// Is probably going to hit
		if (arrowDot >= 0.8)
			character.Block();

		// Try to run away from arrow
		Vector3 away = Vector3.Cross(closestArrow.Direction, toArrow);
		FaceLocation(character.transform.position + away);
		character.Move(arrowDot > 0 ? -moveSpeed : moveSpeed);
	}	

	void ActionAttack()
	{
		if (closestCharacter != null && closestCharacter.isAlive)
		{
			// Calculate the expected location
			Vector3 expected = closestCharacter.transform.position + closestCharacter.trueVelocity * Time.deltaTime;
			float distance = Vector3.Distance(character.transform.position, expected);

			float confidence = FaceLocation(expected);

			if (distance <= attackDistance)
			{
				// Shoot target
				if (confidence > 0.85f * attackAccuracy)
					character.Fire();
			}
			else
			{
				// Move closer to target
				if (confidence > 0.25f)
					character.Move(moveSpeed);
			}
		}
	}

	/// <summary>
	/// Read the XML file for a specific tree
	/// </summary>
	/// <param name="name">The name of the tree to load</param>
	/// <returns></returns>
	public bool LoadTree(string name)
	{
		tree = new DecisionTree();
		
		// Create states
		tree.RegisterActionState("Flee", ActionFlee);
		tree.RegisterActionState("Defend", ActionDefend);
		tree.RegisterActionState("Attack", ActionAttack);
		tree.RegisterActionState("Skirt", ActionSkirt);


		if (!tree.LoadXML(dataFolder + name))
		{
			Debug.LogWarning("Cannot load tree '" + dataFolder + name + "'");
			return false;
		}

		// Add default vars
		float r, g, b;

		if (!tree.agentProfile.HasVar("Colour.R"))
			tree.agentProfile.SetVar("Colour.R", 0.4f);
		r = tree.agentProfile.GetVar("Colour.R");

		if (!tree.agentProfile.HasVar("Colour.G"))
			tree.agentProfile.SetVar("Colour.G", 0.4f);
		g = tree.agentProfile.GetVar("Colour.G");

		if (!tree.agentProfile.HasVar("Colour.B"))
			tree.agentProfile.SetVar("Colour.B", 0.4f);
		b = tree.agentProfile.GetVar("Colour.B");
		colour = new Color(r, g, b);


		if (!tree.agentProfile.HasVar("MoveSpeed"))
			tree.agentProfile.SetVar("MoveSpeed", 0.25f);
		moveSpeed = tree.agentProfile.GetVar("MoveSpeed");

		if (!tree.agentProfile.HasVar("TurnSpeed"))
			tree.agentProfile.SetVar("TurnSpeed", 0.1f);
		turnSpeed = tree.agentProfile.GetVar("TurnSpeed");

		if (!tree.agentProfile.HasVar("AttackAccuracy"))
			tree.agentProfile.SetVar("AttackAccuracy", 0.1f);
		attackAccuracy = tree.agentProfile.GetVar("AttackAccuracy");

		if (!tree.agentProfile.HasVar("NearRange"))
			tree.agentProfile.SetVar("NearRange", 7.0f);
		nearCharacterRange = tree.agentProfile.GetVar("NearRange");

		if (!tree.agentProfile.HasVar("ShootDist"))
			tree.agentProfile.SetVar("ShootDist", 10.0f);
		attackDistance = tree.agentProfile.GetVar("ShootDist");

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
		
		string[] values = System.IO.Directory.GetFiles(dataFolder);
		List<string> actualValues = new List<string>();

		for (int i = 0; i < values.Length; ++i)
			if (values[i].EndsWith(".ai.xml", System.StringComparison.CurrentCultureIgnoreCase))
				actualValues.Add(values[i].Substring(dataFolder.Length));

		return actualValues.ToArray();
	}
}
