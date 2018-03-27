using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Main controller for a particular NEAT neural net
/// Holds key values for the growth of this NN
/// </summary>
public class NeatController
{
    /// <summary>
    /// Used to assign unique ids to genes
    /// </summary>
    private int innovationCounter = 0;
    private Dictionary<Vector2Int, int> innovationIds;

    /// <summary>
    /// The chance of structural mutations for a genome
    /// </summary>
    public float structuralMutationChance = 0.25f;

    /// <summary>
    /// The chance of any change in weight occuring for a genome
    /// </summary>
    public float weightMutationChance = 0.80f;
	/// <summary>
	/// Can the genes flip state as a possible mutation
	/// </summary>
	public bool geneStateFlipMutation = false;


	/// <summary>
	/// The species coefficient to use to for disjoin genes when determining species
	/// </summary>
	public float disjointCoefficient = 1.0f;
	/// <summary>
	/// The species coefficient to use to for excess genes when determining species
	/// </summary>
	public float excessCoefficient = 1.0f;
	/// <summary>
	/// The species coefficient to use to for average weight difference across a net when determining species
	/// </summary>
	public float weightDiffCoefficient = 0.4f;

	/// <summary>
	/// The value threshold used to group nets under species (Greater difference score than this means not the same species)
	/// </summary>
	public float speciesDeltaThreshold = 3.0f;


    /// <summary>
    /// What generation are we currently on
    /// </summary>
    public int generationCounter { get; private set; }

	/// <summary>
	/// The currently active population
	/// </summary>
	public NeatNetwork[] population { get; private set; }
    /// <summary>
    /// The currently active species
    /// </summary>
    public List<NeatSpecies> activeSpecies { get; private set; }


	public NeatController()
	{
		innovationIds = new Dictionary<Vector2Int, int>();
	}

	/// <summary>
	/// Safely retreive (or create) a innovation number for a gene
	/// </summary>
	/// <param name="fromNodeId">The id of the node the gene starts at</param>
	/// <param name="toNodeId">The id of the node the gene ends at</param>
	/// <returns>The unique innovation id for this gene</returns>
	public int FetchInnovationId(int fromNodeId, int toNodeId)
    {
        Vector2Int key = new Vector2Int(fromNodeId, toNodeId);
        if (innovationIds.ContainsKey(key))
            return innovationIds[key];
        else
        {
            innovationIds[key] = innovationCounter++;
            return innovationIds[key];
        }
    }

	/// <summary>
	/// Does a gene exist which connects these 2 nodes
	/// </summary>
	/// <param name="fromNodeId">The id of the node the gene starts at</param>
	/// <param name="toNodeId">The id of the node the gene ends at</param>
	/// <returns>True if the gene exists</returns>
	public bool DoesGeneExist(int fromNodeId, int toNodeId)
	{
		return innovationIds.ContainsKey(new Vector2Int(fromNodeId, toNodeId));
	}

	/// <summary>
	/// Generate a starting population
	/// </summary>
	public NeatNetwork[] GenerateBasePopulation(int count, int inputCount, int outputCount)
	{
        generationCounter = 1;
        population = new NeatNetwork[count];
		activeSpecies = new List<NeatSpecies>();


		for (int i = 0; i < count; ++i)
		{
			population[i] = new NeatNetwork(this, inputCount, outputCount);
			population[i].CreateMutations(); // Start with initial mutation to avoid boring stuff
		}

		AssignPopulationToSpecies();
		Debug.Log("Generation " + generationCounter + " with " + activeSpecies.Count + " species");
		return population;
	}

    /// <summary>
    /// Create the next generation of networks
    /// </summary>
    public NeatNetwork[] BreedNextGeneration()
    {
		// TODO - ACTUAL BREEDING
		foreach (NeatNetwork network in population)
			network.CreateMutations();

		
		// Clear species
		foreach(NeatSpecies species in activeSpecies)
			species.NextGeneration();

		AssignPopulationToSpecies();

		// Remove species with no currently active members 
		for (int i = 0; i < activeSpecies.Count; ++i)
			if (activeSpecies[i].population.Count == 0)
			{
				activeSpecies.RemoveAt(i);
				--i;
			}


		generationCounter++;
		Debug.Log("Generation " + generationCounter + " with " + activeSpecies.Count + " species");
		return population;
    }

    /// <summary>
    /// Assign a species to each network in the population
    /// </summary>
    private void AssignPopulationToSpecies()
    {
        foreach (NeatNetwork network in population)
        {
            // Attempt to add an existing species
            bool added = false;
            foreach (NeatSpecies species in activeSpecies)
            {
                added = species.AttemptAdd(network);
                if (added)
                    break;
            }

            // Create a new species with this as a rep
            if(!added)
            {
                NeatSpecies newSpecies = new NeatSpecies(network);
				activeSpecies.Add(newSpecies);
            }
        }
    }
}
