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


	void Start ()
	{
		if (main == null)
			main = this;
		else
		{
			Debug.LogError("Multiple NeuralController have been created");
			return;
		}
		
		neatController = new NeatController();
		NeatNetwork[] population = neatController.GenerateBasePopulation(GameMode.Main.CharacterCount, NeuralInputAgent.InputCount, NeuralInputAgent.OutputCount);


		GameMode.Main.ResetGame();
		for (int i = 0; i < GameMode.Main.CharacterCount; ++i)
		{
			NeuralInputAgent agent = GameMode.Main.characters[i].GetComponent<NeuralInputAgent>();
			population[i].CreateMutations();
			agent.AssignNetwork(population[i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
