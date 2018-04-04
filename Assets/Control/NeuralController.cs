using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls all the neural nets
/// </summary>
public class NeuralController : MonoBehaviour
{
	public static NeuralController main { get; private set; }

	/// <summary>
	/// The controller which generates and manages the NEAT networks
	/// </summary>
	public NeatController neatController { get; private set; }

	/// <summary>
	/// Weight for survival time on network fitness
	/// </summary>
	public float survialWeight = 1.0f;
	/// <summary>
	/// Weight for agent kills on network fitness
	/// </summary>
	public float killWeight = 15.0f;
	/// <summary>
	/// Weight for arrow blocks on network fitness
	/// </summary>
	public float blockWeight = 20.0f;
	/// <summary>
	/// Weight for agent winning a round on network fitness
	/// </summary>
	public float winnerWeight = 5.0f;

	/// <summary>
	/// How long this has been running (In seconds)
	/// </summary>
	public float runTime { get; private set; }


	void Start ()
	{
		Application.runInBackground = true;

		if (main == null)
			main = this;
		else
		{
			Debug.LogError("Multiple NeuralController have been created");
			return;
		}
		
		neatController = new NeatController("AiArena");
		neatController.breedRetention = 0.15f;
		neatController.breedConsideration = 0.5f;
		neatController.speciesDeltaThreshold = 2.5f;
		NeatNetwork[] population = neatController.GenerateBasePopulation(GameMode.Main.CharacterCount, NeuralInputAgent.InputCount, NeuralInputAgent.OutputCount, 1);

		// Use controller's runtime to start from
		runTime = neatController.runTime;

		GameMode.Main.ResetGame();
		CreateAgentsFromNetworks(population);
	}
	
	// Update is called once per frame
	void Update ()
	{
		runTime += Time.deltaTime;
		neatController.runTime = (int)runTime;

		// Start the next generation of nets
		if (GameMode.Main.IsGameFinished)
		{
			Debug.Log("Breeding next generation");
			NeatNetwork[] population = neatController.BreedNextGeneration();

			GameMode.Main.ResetGame();
			CreateAgentsFromNetworks(population);
		}
	}

	private void CreateAgentsFromNetworks(NeatNetwork[] population)
	{
		List<NeuralInputAgent> oldAgents = new List<NeuralInputAgent>();

		for (int i = 0; i < GameMode.Main.CharacterCount; ++i)
		{
			NeuralInputAgent agent = GameMode.Main.characters[i].GetComponent<NeuralInputAgent>();
			agent.AssignNetwork(population[i]);

			if (agent.network.age != 0)
			{
				oldAgents.Add(agent);
				agent.GetComponent<CharacterAccessories>().GrowHair(agent.network.age);
			}
		}

		// Sort surviving agents so highest fitness is firsts
		oldAgents.Sort((a, b) =>
			(a.network.previousFitness == b.network.previousFitness) ? 0 :
			(a.network.previousFitness > b.network.previousFitness) ? -1 : 1
		);

		// Give hats to top and bottom 3
		for (int i = 0; i < 3; ++i)
		{
			int c = i;
			int d = oldAgents.Count - 1 - i;

			// Give crown
			if (c < oldAgents.Count)
				oldAgents[c].GetComponent<CharacterAccessories>().GiveCrown(c);

			// Give dunce hat (Don't double up dunce and crowns)
			//if (d < oldAgents.Count && d > 2)
			//	oldAgents[c].GetComponent<CharacterAccessories>().GiveDunce(d);
		}
	}
}
