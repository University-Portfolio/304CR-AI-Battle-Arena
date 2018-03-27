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


	public NeatSpecies(NeatNetwork representative)
	{
		this.representative = representative;
		colour = Color.HSVToRGB(Random.value, Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f));

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
	/// Prepares this species for next generation
	/// </summary>
	public void NextGeneration()
	{
		// Select a new represetative from current members
		representative = population[Random.Range(0, population.Count)];
		population.Clear();
	}
}
