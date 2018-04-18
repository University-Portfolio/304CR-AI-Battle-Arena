using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


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
	/// The total runtime for this simulation in seconds
	/// </summary>
	public int runTime;

	/// <summary>
	/// What generation are we currently on
	/// </summary>
	public int generationCounter { get; private set; }

	/// <summary>
	/// The name of this collection of networks
	/// </summary>
	public string collectionName { get; private set; }
	/// <summary>
	/// What folder to save/load data from
	/// </summary>
	public static string dataFolder = "Neat/Collections/";


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
	public float weightDiffCoefficient = 1.0f;

	/// <summary>
	/// The value threshold used to group nets under species (Greater difference score than this means not the same species)
	/// </summary>
	public float speciesDeltaThreshold = 2.5f;

	/// <summary>
	/// How much of (Top scores of) a species should be consider when breeding
	/// </summary>
	public float breedConsideration = 0.3f;
	/// <summary>
	/// How much of (Top scores of) a species should be brought into the next generation
	/// </summary>
	public float breedRetention = 0.1f;


	/// <summary>
	/// The currently active population
	/// </summary>
	public NeatNetwork[] population { get; private set; }
    /// <summary>
    /// The currently active species
    /// </summary>
    public List<NeatSpecies> activeSpecies { get; private set; }


	public NeatController(string collectionName)
	{
		this.collectionName = collectionName;
		activeSpecies = new List<NeatSpecies>();
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
	public NeatNetwork[] GenerateBasePopulation(int count, int inputCount, int outputCount, int initialMutations = 1, bool attemptLoad = true)
	{
		if (attemptLoad)
		{
			population = new NeatNetwork[count];

			if (LoadXML())
			{
				Debug.Log("Loaded NEAT collection '" + collectionName + "' from generation " + generationCounter);
				return BreedNextGeneration(count); // Can only load that generation as a starting point
			}
			else
				Debug.Log("Starting NEAT collection '" + collectionName + "' from scratch");
		}


		generationCounter = 1;
        population = new NeatNetwork[count];
		activeSpecies = new List<NeatSpecies>();


		for (int i = 0; i < count; ++i)
		{
			population[i] = new NeatNetwork(this, inputCount, outputCount);
			for(int m = 0; m< initialMutations; ++m)
				population[i].CreateMutations();
		}

		AssignPopulationToSpecies();
		Debug.Log("Generation " + generationCounter + " with " + activeSpecies.Count + " species");
		return population;
	}

	/// <summary>
	/// Create the next generation of networks
	/// </summary>
	/// <param name="newPopulationSize">How large the next population should be</param>
	/// <returns></returns>
	public NeatNetwork[] BreedNextGeneration(int newPopulationSize)
    {
		SaveXML();

		// Calulate the average adjusted fitness
		float totalFitness = 0.0f;
		Dictionary<NeatSpecies, float> fitness = new Dictionary<NeatSpecies, float>();

		foreach (NeatSpecies species in activeSpecies)
		{
			float adjustedFitness = species.CalulateAverageAdjustedFitness();
			totalFitness += adjustedFitness;
			fitness[species] = adjustedFitness;
		}


		// Determine how many networks each species has been allocated
		Dictionary<NeatSpecies, int> allocation = new Dictionary<NeatSpecies, int>();
		int totalNewCount = 0;

		foreach (NeatSpecies species in activeSpecies)
		{
			if (totalFitness <= 0.0f)
				allocation[species] = 0;
			else
			{
				int alloc = (int)((fitness[species] / totalFitness) * newPopulationSize);
				allocation[species] = alloc;
				totalNewCount += alloc;
			}
		}


		// Handle if new count is too high or low
		if (totalNewCount < newPopulationSize)
		{
			// Randomly allocate the remainder
			for (int i = 0; i < newPopulationSize - totalNewCount; ++i)
			{
				NeatSpecies species = activeSpecies[Random.Range(0, activeSpecies.Count)];
				allocation[species]++;
			}
		}
		else if (totalNewCount > newPopulationSize)
		{
			// Randomly deallocate the remainder
			for (int i = 0; i < totalNewCount - newPopulationSize; ++i)
			{
				NeatSpecies species = activeSpecies[Random.Range(0, activeSpecies.Count)];
				allocation[species]--;
			}
		}


		// Generate new popluation
		List<NeatNetwork> newPopulation = new List<NeatNetwork>();

		foreach (NeatSpecies species in activeSpecies)
			species.NextGeneration(allocation[species], newPopulation);

		population = newPopulation.ToArray();


		// Assign correct species to the new networks
		AssignPopulationToSpecies();


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
                NeatSpecies newSpecies = new NeatSpecies(this, network);
				activeSpecies.Add(newSpecies);
            }
		}


		// Remove species with no currently active members 
		for (int i = 0; i < activeSpecies.Count; ++i)
			if (activeSpecies[i].population.Count == 0)
			{
				Debug.Log("Species (" + activeSpecies[i].colour.ToString() + ") has died after " + activeSpecies[i].generationsLived + " generations");
				activeSpecies.RemoveAt(i);
				--i;
			}
	}

	/// <summary>
	/// Write the general settings for this network
	/// </summary>
	/// <returns></returns>
	public void WriteNeatSettings()
	{
		if (!System.IO.Directory.Exists(dataFolder + collectionName))
			System.IO.Directory.CreateDirectory(dataFolder + collectionName);

		string path = dataFolder + collectionName + ".neat.xml";

		using (XmlWriter writer = XmlWriter.Create(path))
		{
			writer.WriteStartDocument();
			writer.WriteStartElement("NeatControllers");

			writer.WriteElementString("structuralMutationChance", "" + structuralMutationChance);
			writer.WriteElementString("weightMutationChance", "" + weightMutationChance);
			writer.WriteElementString("geneStateFlipMutation", "" + geneStateFlipMutation);

			writer.WriteElementString("disjointCoefficient", "" + disjointCoefficient);
			writer.WriteElementString("excessCoefficient", "" + excessCoefficient);
			writer.WriteElementString("weightDiffCoefficient", "" + weightDiffCoefficient);

			writer.WriteElementString("speciesDeltaThreshold", "" + speciesDeltaThreshold);

			writer.WriteElementString("breedConsideration", "" + breedConsideration);
			writer.WriteElementString("breedRetention", "" + breedRetention);
			

			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		Debug.Log("Written to '" + path + "'");
	}


	/// <summary>
	/// Load the general settings for this collection
	/// </summary>
	/// <returns></returns>
	public bool LoadNeatSettings()
	{
		if (!System.IO.Directory.Exists(dataFolder + collectionName))
			return false;

		string path = dataFolder + collectionName + ".neat.xml";

		if (!System.IO.File.Exists(path))
			return false;


		XmlDocument document = new XmlDocument();
		document.Load(path);

		// Read document
		XmlElement root = document.DocumentElement;
		foreach (XmlElement child in root.ChildNodes)
		{
			if (child.Name == "structuralMutationChance")
				structuralMutationChance = float.Parse(child.InnerText);
			else if (child.Name == "weightMutationChance")
				weightMutationChance = float.Parse(child.InnerText);
			else if (child.Name == "geneStateFlipMutation")
				geneStateFlipMutation = bool.Parse(child.InnerText);

			else if (child.Name == "geneStateFlipMutation")
				geneStateFlipMutation = bool.Parse(child.InnerText);

			else if (child.Name == "disjointCoefficient")
				disjointCoefficient = float.Parse(child.InnerText);
			else if (child.Name == "excessCoefficient")
				excessCoefficient = float.Parse(child.InnerText);
			else if (child.Name == "weightDiffCoefficient")
				weightDiffCoefficient = float.Parse(child.InnerText);

			else if (child.Name == "speciesDeltaThreshold")
				speciesDeltaThreshold = float.Parse(child.InnerText);

			else if (child.Name == "breedConsideration")
				breedConsideration = float.Parse(child.InnerText);
			else if (child.Name == "breedRetention")
				breedRetention = float.Parse(child.InnerText);
		}

		Debug.Log("Read from '" + path + "'");
		return true;
	}

	/// <summary>
	/// Save the current working data in an xml
	/// </summary>
	public void SaveXML()
	{
		if (!System.IO.Directory.Exists(dataFolder + collectionName))
			System.IO.Directory.CreateDirectory(dataFolder + collectionName);

		string path = dataFolder + collectionName + "/gen_" + generationCounter + ".xml";

		using (XmlWriter writer = XmlWriter.Create(path))
		{
			writer.WriteStartDocument();
			writer.WriteStartElement("Generation");

			writer.WriteElementString("innovationCounter", "" + innovationCounter);
			writer.WriteElementString("generationCounter", "" + generationCounter);
			writer.WriteElementString("runTime", "" + runTime);

			// Write genes
			writer.WriteStartElement("innovations");
			foreach (var pair in innovationIds)
			{
				writer.WriteStartElement("gene");
				writer.WriteAttributeString("from", "" + pair.Key.x);
				writer.WriteAttributeString("to", "" + pair.Key.y);
				writer.WriteAttributeString("inno", "" + pair.Value);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();


			// Write species
			writer.WriteStartElement("ActiveSpecies");
			foreach (NeatSpecies species in activeSpecies)
			{
				writer.WriteStartElement("Species");
				species.WriteXML(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();


			// Write networks
			writer.WriteStartElement("Population");
			foreach (NeatNetwork network in population)
			{
				writer.WriteStartElement("Network");
				network.WriteXML(writer);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		Debug.Log("Written to '" + path + "'");
	}


	/// <summary>
	/// Read in the xml for a specfic generation
	/// </summary>
	/// <param name="generation">The desired generation to load (Will load highest found, if -1)</param>
	public bool LoadXML(int generation = -1)
	{
		if (!System.IO.Directory.Exists(dataFolder + collectionName))
			return false;

		// Attempt to find highest generation
		if (generation == -1)
		{
			generation = 0;
			while (true)
			{
				if (!System.IO.File.Exists(dataFolder + collectionName + "/gen_" + (generation + 1) + ".xml"))
					break;
				else
					generation++;
			}
		}

		string path = dataFolder + collectionName + "/gen_" + generation + ".xml";

		if (!System.IO.File.Exists(path))
			return false;


		XmlDocument document = new XmlDocument();
		document.Load(path);

		// Read document
		XmlElement root = document.DocumentElement;
		foreach (XmlElement child in root.ChildNodes)
		{
			if (child.Name == "innovationCounter")
				innovationCounter = System.Int32.Parse(child.InnerText);

			else if (child.Name == "generationCounter")
				generationCounter = System.Int32.Parse(child.InnerText);

			else if (child.Name == "runTime")
				runTime = System.Int32.Parse(child.InnerText);
			
			// Read genes
			else if (child.Name == "Generation")
			{
				innovationIds = new Dictionary<Vector2Int, int>();
				foreach (XmlElement gene in child.ChildNodes)
				{
					if (gene.Name != "gene")
						continue;

					int fromId = System.Int32.Parse(child.GetAttribute("from"));
					int toId = System.Int32.Parse(child.GetAttribute("to"));
					int inno = System.Int32.Parse(child.GetAttribute("inno"));
					innovationIds[new Vector2Int(fromId, toId)] = inno;
				}
			}


			// Read species (Need to read it before networks)
			else if (child.Name == "ActiveSpecies")
			{
				activeSpecies = new List<NeatSpecies>();
				foreach (XmlElement entry in child.ChildNodes)
				{
					if (entry.Name != "Species")
						continue;

					activeSpecies.Add(new NeatSpecies(this, entry));
				}
			}


			// Read networks
			else if (child.Name == "Population")
			{
				List<NeatNetwork> newPopulation = new List<NeatNetwork>();

				foreach (XmlElement entry in child.ChildNodes)
				{
					if (entry.Name != "Network")
						continue;

					NeatNetwork network = new NeatNetwork(this, entry);
					newPopulation.Add(network);
				}

				population = newPopulation.ToArray();
			}

		}

		Debug.Log("Read from '" + path + "'");
		return true;
	}
}
