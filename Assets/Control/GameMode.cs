using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controlls the orginisation of the current game
/// </summary>
public class GameMode : MonoBehaviour
{
	public static GameMode main { get; private set; }

	[SerializeField]
	private Character defaultCharacter;
	
	
	public StageController stage { get; private set; }
	public Character[] characters { get; private set; }

	public bool playerExists { get; private set; }
	public int characterCount { get; private set; }
	public int neuralAgentCount { get; private set; }

	[SerializeField]
	private int rounds = 5;
	public int currentRound { get; private set; }
	public int aliveCount { get; private set; }
	private float nextRoundCooldown;

	public bool isTrainingGame { get; private set; }
	public bool isGameActive { get; private set; }

	public bool IsGameFinished { get { return currentRound > rounds; } }
	public int TotalRounds { get { return rounds; } }


	void Start ()
	{
		Debug.Log("GameMode created");
		if (main == null)
			main = this;
		else
		{
			Debug.LogError("Multiple GameModes have been created");
			return;
		}


		// Fetch required objects
		stage = FindObjectOfType<StageController>();
		if (stage == null)
			Debug.LogError("GameMode requires for a StageController to exist");


		characters = null;
		isGameActive = false;
		isTrainingGame = false;

		StartGame(32, true, 5);
		//StartTraining(32);
	}
	
	void Update ()
	{
		if (!isGameActive)
			return;

		// Game Over
		if (IsGameFinished)
		{
			if (isTrainingGame)
				NeuralController.main.OnTrainingSessionEnd();

			ResetGame();
			//isGameActive = false;
			return;
		}

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
		aliveCount = remaining.Count;


		// Reset when only 1 character exists
		if (aliveCount <= 1)
		{
			// This is the winner
			if (aliveCount > 0)
				remaining[0].roundWinCount++;

			nextRoundCooldown = 1.0f;
		}
	}

	/// <summary>
	/// Create objects for all the characters
	/// </summary>
	/// <param name="spawnPlayer">Should a player be spawned</param>
	private void SpawnCharacters()
	{
		// Cleanup old characters
		if(characters != null)
			foreach (Character character in characters)
				if (character != null)
					Destroy(character.gameObject);


		// Spawn new collections
		Debug.Log("Spawning " + characterCount + " characters");
		characters = new Character[characterCount];

		for (int i = 0; i < characterCount; ++i)
			characters[i] = Instantiate(defaultCharacter);


		// Add player
		if (playerExists)
			characters[0].gameObject.AddComponent<PlayerInput>();


		// Add agents
		int totalAgents = playerExists ? characterCount - 1 : characterCount;
		NeuralInputAgent[] neuralAgents = new NeuralInputAgent[neuralAgentCount];

		for (int i = 0; i < totalAgents; ++i)
		{
			int index = playerExists ? i + 1 : i;
			if (i < neuralAgentCount)
			{
				NeuralInputAgent agent = characters[index].gameObject.AddComponent<NeuralInputAgent>();
				neuralAgents[i] = agent;
			}
			else
				characters[index].gameObject.AddComponent<TreeInput>();
		}

		PlaceCharactersInRing(); // Place characters
		NeuralController.main.AttachNetworks(neuralAgents); // Attach networks to nn agents
	}

	/// <summary>
	/// Moves all characters into a ring
	/// </summary>
	private void PlaceCharactersInRing()
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
		}
	}

	/// <summary>
	/// Respawn the current character (Don't recreate the objects)
	/// </summary>
	private void RespawnCharacters()
	{
		// Force everything to respawn
		foreach (Character character in characters)
			character.Respawn();

		PlaceCharactersInRing();

		// Reset observer
		CharacterObserver observer = FindObjectOfType<CharacterObserver>();
		observer.SelectNewTarget();
	}


	/// <summary>
	/// Start a new game
	/// </summary>
	/// <param name="totalCharacters">How many characters are going to be spawned</param>
	/// <param name="spawnPlayer">Should the player be spawned in too?</param>
	/// <param name="neuralCount">How many neural network agents to spawn</param>
	public void StartGame(int totalCharacters, bool spawnPlayer, int neuralCount)
	{
		Debug.Log("Starting game");
		currentRound = 1;
		isGameActive = true;
		isTrainingGame = false;

		// Set agent values correctly to avoid overflows
		playerExists = spawnPlayer;
		characterCount = System.Math.Max(2, totalCharacters);
		int totalAgents = spawnPlayer ? characterCount - 1 : characterCount;
		neuralAgentCount = System.Math.Min(totalAgents, neuralCount);

		NeuralController.main.InitialiseController(neuralAgentCount);
		stage.ResetStage();
		SpawnCharacters();
	}

	/// <summary>
	/// Starts the game in training mode
	/// </summary>
	/// <param name="totalCharacters">How many characters are going to be spawned</param>
	public void StartTraining(int totalCharacters)
	{
		Debug.Log("Starting Training");
		currentRound = 1;
		isGameActive = true;
		isTrainingGame = true;

		playerExists = false;
		characterCount = totalCharacters;
		neuralAgentCount = totalCharacters;

		NeuralController.main.InitialiseController(neuralAgentCount);
		stage.ResetStage();
		SpawnCharacters();
	}

	/// <summary>
	/// Restarts the gamemode with the current settings
	/// </summary>
	public void ResetGame()
	{
		Debug.Log("Resetting game");
		currentRound = 1;
		isGameActive = true;

		stage.ResetStage();
		SpawnCharacters();
	}

	/// <summary>
	/// Close the game, no matter what state it's in
	/// </summary>
	public void CancelGame()
	{
		Debug.Log("Cancelling game");

		isGameActive = false;
		isTrainingGame = false;
		stage.ResetStage();

		// Cleanup old characters
		foreach (Character character in characters)
			if (character != null)
				Destroy(character.gameObject);
	}
}
