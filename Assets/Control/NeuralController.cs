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
	/// The current population size that will be used
	/// </summary>
	public int populationSize { get; private set; }

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
	}
	
	void Update ()
	{
		// Increment training time
		if (GameMode.main.isTrainingGame)
		{
			runTime += Time.deltaTime;
			neatController.runTime = (int)runTime;
		}
	}

	/// <summary>
	/// Called when a training session successfully completes
	/// </summary>
	public void OnTrainingSessionEnd()
	{
		Debug.Log("Breeding next generation");
		NeatNetwork[] population = neatController.BreedNextGeneration(populationSize);
	}

	/// <summary>
	/// Create networks
	/// </summary>
	/// <param name="collectionName">The name of the collection to load/create</param>
	/// <param name="count">Number of networks to create</param>
	public void InitialiseController(string collectionName, int count)
	{
		populationSize = count;

		// Load NEAT controller
		neatController = new NeatController(collectionName);
		if (!neatController.LoadNeatSettings())
		{
			Debug.Log("Using default NEAT settings");
			neatController.WriteNeatSettings();
		}
		

		NeatNetwork[] population = neatController.GenerateBasePopulation(count, NeuralInputAgent.InputCount, NeuralInputAgent.OutputCount, 1);

		// Use controller's runtime to start from
		runTime = neatController.runTime;
	}

	/// <summary>
	/// Attach the networks to these characters
	/// </summary>
	/// <param name="targets">All the agents to apply networks to</param>
	public void AttachNetworks(NeuralInputAgent[] targets)
	{
		// Store networks older than 1, to give cosmetics
		List<NeuralInputAgent> oldAgents = new List<NeuralInputAgent>();

		for (int i = 0; i < populationSize; ++i)
		{
			NeuralInputAgent agent = targets[i];
			agent.AssignNetwork(neatController.population[i]);

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
