using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


public class NeatSpecies
{
	/// <summary>
	/// The controller which spawned this species
	/// </summary>
	public readonly NeatController controller;

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
	public Color colour { get; private set; }

	/// <summary>
	/// The unique guid for this species
	/// </summary>
	public System.Guid guid { get; private set; }

	/// <summary>
	/// How many generations this specieis has lived for
	/// </summary>
	public int generationsLived { get; private set; }


	public NeatSpecies(NeatController controller, NeatNetwork representative)
	{
		this.controller = controller;
		this.representative = representative;
		colour = Color.HSVToRGB(Random.value, Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f));
		generationsLived = 0;
		guid = System.Guid.NewGuid();

		population = new List<NeatNetwork>();
		population.Add(representative);
		representative.assignedSpecies = this;
	}

	public NeatSpecies(NeatController controller, XmlElement content)
	{
		this.controller = controller;
		population = new List<NeatNetwork>();
		ReadXML(content);
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
			foreach (NeatNetwork other in controller.population)
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
		representative.assignedSpecies = null;


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

	/// <summary>
	/// Write data about this network to xml
	/// </summary>
	/// <param name="writer"></param>
	public void WriteXML(XmlWriter writer)
	{
		// Write general
		writer.WriteAttributeString("ColourR", "" + colour.r);
		writer.WriteAttributeString("ColourG", "" + colour.g);
		writer.WriteAttributeString("ColourB", "" + colour.b);
		writer.WriteAttributeString("Guid", "" + guid.ToString());
		writer.WriteAttributeString("generationsLived", "" + generationsLived);


		writer.WriteStartElement("Representative");
		representative.WriteXML(writer);
		writer.WriteEndElement();
	}

	/// <summary>
	/// Read in the xml for a specfic generation
	/// </summary>
	/// <param name="writer"></param>
	public void ReadXML(XmlElement entry)
	{
		colour = new Color(
			float.Parse(entry.GetAttribute("ColourR")),
			float.Parse(entry.GetAttribute("ColourG")),
			float.Parse(entry.GetAttribute("ColourB"))
		);
		guid = new System.Guid(entry.GetAttribute("Guid"));
		generationsLived = int.Parse(entry.GetAttribute("generationsLived"));
		population.Clear();


		foreach (XmlElement child in entry.ChildNodes)
		{
			if (child.Name == "Representative")
			{
				representative = new NeatNetwork(controller, child);
				break;
			}
		}
	}
}
