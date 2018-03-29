﻿using System.Collections;
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
	/// Weight for agent winning a round on network fitness
	/// </summary>
	public float winnerWeight = 5.0f;


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
		neatController.breedRetention = 0.1f;
		neatController.breedConsideration = 0.4f;
		neatController.speciesDeltaThreshold = 2.0f;
		NeatNetwork[] population = neatController.GenerateBasePopulation(GameMode.Main.CharacterCount, NeuralInputAgent.InputCount, NeuralInputAgent.OutputCount, 1);


		GameMode.Main.ResetGame();
		for (int i = 0; i < GameMode.Main.CharacterCount; ++i)
		{
			NeuralInputAgent agent = GameMode.Main.characters[i].GetComponent<NeuralInputAgent>();
			agent.AssignNetwork(population[i]);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Start the next generation of nets
		if (GameMode.Main.IsGameFinished)
		{
			Debug.Log("Breeding next generation");
			NeatNetwork[] population = neatController.BreedNextGeneration();

			GameMode.Main.ResetGame();
			for (int i = 0; i < GameMode.Main.CharacterCount; ++i)
			{
				NeuralInputAgent agent = GameMode.Main.characters[i].GetComponent<NeuralInputAgent>();
				agent.AssignNetwork(population[i]);
			}
		}
	}
}
