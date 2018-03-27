using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NeatSpecies
{
	/// <summary>
	/// The network which represents this species
	/// </summary>
	public NeatNetwork representative { get; private set; }
	/// <summary>
	/// The population which makes up this species
	/// </summary>
	public readonly List<NeatNetwork> population;

	/// <summary>
	/// The colour to represent this species with
	/// </summary>
	public readonly Color colour;
	/// <summary>
	/// How many generations this specieis has lived for
	/// </summary>
	public int generationsLived { get; private set; }


	public NeatSpecies(NeatNetwork representative)
	{
		this.representative = representative;
		colour = Color.HSVToRGB(Random.value, Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f));
		generationsLived = 0;

		population = new List<NeatNetwork>();
		population.Add(representative);
		representative.assignedSpecies = this;
	}

	/// <summary>
	/// Attempt to add a network to this species
	/// </summary>
	/// <param name="other">The network to add</param>
	/// <returns>True if the network is added to this species, false if the network is not part of the same species</returns>
	public bool AttemptAdd(NeatNetwork other)
	{
		if (!IsCompatible(other))
			return false;

		population.Add(other);
		other.assignedSpecies = this;
		return true;
	}

	/// <summary>
	/// Is this network compatible with this species
	/// </summary>
	/// <returns>True if the network is part of this species</returns>
	public bool IsCompatible(NeatNetwork other)
	{
		return NeatNetwork.AreSameSpecies(representative, other);
	}

	/// <summary>
	/// Calculate the average adjusted fitness for this network
	/// </summary>
	/// <returns></returns>
	public float CalulateAverageAdjustedFitness()
	{
		float totalAdjustedFitness = 0.0f;

		foreach(NeatNetwork network in population)
		{
			// Count how many networks in this species align to this individual
			int matches = 0;
			foreach (NeatNetwork other in population)
				if (network == other || NeatNetwork.AreSameSpecies(network, other))
					matches++;

			totalAdjustedFitness += network.fitness / matches;
		}

		return totalAdjustedFitness / population.Count;
	}
	
	/// <summary>
	/// Create the next generation of networks from this species
	/// </summary>
	/// <param name="allocation">How many networks this species has been allocated</param>
	/// <param name="newPopulation">Where to store new networks</param>
	public void NextGeneration(int allocation, List<NeatNetwork> newPopulation)
	{
		// Sort population from highest fitness to lowest
		population.Sort();
		population.Reverse();
		 

		int breedRange = (int)(population.Count * representative.controller.breedConsideration); // Only consider the top x% for breeding
		int retainedCount = (int)(allocation * representative.controller.breedRetention); // Retain x% of top scorers into the next generation

		// Select a new represetative from current members
		representative = population[Random.Range(0, breedRange)];


		// Generate children
		for (int i = 0; i < allocation; ++i)
		{
			// Take the previous best scorers for the new generation
			if (i <= retainedCount && i < population.Count && population[i].fitness != 0.0f)
			{
				NeatNetwork clone = new NeatNetwork(population[i]);
				newPopulation.Add(clone);
			}
			// Breed an mutate a new network
			else
			{
				NeatNetwork A = population[Random.Range(0, breedRange)];
				NeatNetwork B = population[Random.Range(0, breedRange)];

				NeatNetwork child = NeatNetwork.Breed(A, B);
				child.CreateMutations();
				newPopulation.Add(child);
			}
		}
		

		// Update (Population will be reassigned if this species is going to survive)
		population.Clear();
		generationsLived++;
	}
}
