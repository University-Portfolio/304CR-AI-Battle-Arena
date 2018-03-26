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
	private int characterCount = 30;
	public int CharacterCount { get { return characterCount; } }

	public StageController stage { get; private set; }
	public Character[] characters { get; private set; }


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
		
	}

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

	public void ResetGame(bool spawnPlayer = false)
	{
		stage.ResetStage();
		SpawnCharacters(spawnPlayer);
	}
}
