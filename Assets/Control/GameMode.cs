using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controlls the orginisation of the current game
/// </summary>
public class GameMode : MonoBehaviour
{
	public static GameMode Main { get; private set; }

	[SerializeField]
	private Character defaultCharacter;
	[SerializeField]
	private int characterCount = 32;
	public int CharacterCount { get { return characterCount; } }
	
	public StageController stage { get; private set; }
	public Character[] characters { get; private set; }

	[SerializeField]
	private int rounds = 5;
	public int currentRound { get; private set; }
	private float nextRoundCooldown;

	public bool IsGameFinished { get { return currentRound > rounds; } }
	public int TotalRounds { get { return rounds; } }


	void Start ()
	{
		Debug.Log("GameMode created");
		if (Main == null)
			Main = this;
		else
		{
			Debug.LogError("Multiple GameModes have been created");
			return;
		}


		// Fetch required objects
		stage = FindObjectOfType<StageController>();
		if (stage == null)
			Debug.LogError("GameMode requires for a StageController to exist");


		characters = new Character[characterCount];
	}
	
	void Update ()
	{
		// Game over
		if (IsGameFinished)
			return;

		// Count down to next round
		if (nextRoundCooldown != 0.0f)
		{
			nextRoundCooldown -= Time.deltaTime;
			if (nextRoundCooldown < 0.0f)
			{
				nextRoundCooldown = 0.0f;

				// Advance the round
				currentRound++;
				if (currentRound <= rounds)
				{
					Debug.Log("Starting round " + currentRound);
					stage.ResetStage();
					RespawnCharacters();
				}
			}
			return;
		}

		// Count alive characters
		List<Character> remaining = new List<Character>();
		foreach (Character character in characters)
			if (character.isAlive)
				remaining.Add(character);
		

		// Reset when only 1 character exists
		if (remaining.Count <= 1)
		{
			// This is the winner
			if (remaining.Count > 0)
				remaining[0].roundWinCount++;

			nextRoundCooldown = 1.0f;
		}
	}

	/// <summary>
	/// Create objects for all the characters
	/// </summary>
	/// <param name="spawnPlayer">Should a player be spawned</param>
	private void SpawnCharacters(bool spawnPlayer)
	{
		// Cleanup old characters
		foreach (Character character in characters)
			if (character != null)
				Destroy(character.gameObject);


		// Spawn new collections
		Debug.Log("Spawning " + characterCount + " characters");
		for (int i = 0; i < characterCount; ++i)
		{
			float angle = (i / (float)characterCount) * Mathf.PI * 2.0f;

			characters[i] = Instantiate(defaultCharacter.gameObject,
				stage.transform.position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * (stage.DefaultSize - 3) + new Vector3(0, 1, 0),
				Quaternion.identity,
				null
			).GetComponent<Character>();

			characters[i].directionAngle = angle + Mathf.PI;

			if (spawnPlayer && i == 0)
				characters[i].gameObject.AddComponent<PlayerInput>();
			else
			{
				characters[i].gameObject.AddComponent<NeuralInputAgent>();
			}
		}
	}

	/// <summary>
	/// Respawn the current character (Don't recreate the objects)
	/// </summary>
	private void RespawnCharacters()
	{
		// Shuffle order
		List<Character> oldOrder = new List<Character>(characters);
		for (int i = 0; i < characterCount; ++i)
		{
			int index = Random.Range(0, oldOrder.Count);
			characters[i] = oldOrder[index];
			oldOrder.RemoveAt(index);
		}

		// Place them in ring
		for (int i = 0; i < characterCount; ++i)
		{
			float angle = (i / (float)characterCount) * Mathf.PI * 2.0f;

			characters[i].transform.position = stage.transform.position + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * (stage.DefaultSize - 3) + new Vector3(0, 1, 0);
			characters[i].directionAngle = angle + Mathf.PI;
			characters[i].Respawn();
		}
	}

	/// <summary>
	/// Restarts the entire game
	/// </summary>
	/// <param name="spawnPlayer">Should we spawn a player</param>
	public void ResetGame(bool spawnPlayer = false)
	{
		Debug.Log("Reseting game");
		currentRound = 1;
		stage.ResetStage();
		SpawnCharacters(spawnPlayer);
	}
}
