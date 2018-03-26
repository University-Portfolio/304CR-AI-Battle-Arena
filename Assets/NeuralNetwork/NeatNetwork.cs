using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents a single specific NEAT nerual network/genome
/// </summary>
public class NeatNetwork : System.IComparable<NeatNetwork>
{
    /// <summary>
    /// The controller which spawned this network
    /// </summary>
    public readonly NeatController controller;


    public List<NeatNode> nodes { get; private set; }
	public List<NeatGene> genes { get; private set; }
	private Dictionary<Vector2Int, NeatGene> geneTable;

    public readonly int inputCount;
    public readonly int outputCount;


    /// <summary>
    /// The fitness of this network
    /// </summary>
    public float fitness;


    public NeatNetwork(NeatController controller, int inputCount, int outputCount)
    {
        this.controller = controller;

        this.inputCount = inputCount;
        this.outputCount = outputCount;

		// Initialise network
		nodes = new List<NeatNode>();
		genes = new List<NeatGene>();
		geneTable = new Dictionary<Vector2Int, NeatGene>();


		// The first I nodes will be inputs and the following O nodes will be outputs
		for (int i = 0; i < inputCount; ++i)
			nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Input, this));

		for (int i = 0; i < outputCount; ++i)
			nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Output, this));


		//
		// Avoid creation of non-essential genes (Requires testing)
		//
		// Create initial (minimal genes) connection inputs to outputs
		//for (int i = 0; i < inputCount; ++i)
		//    for (int j = 0; j < outputCount; ++j)
		//		CreateGene(i, inputCount + j);
	}

    /// <summary>
    /// Create a gene between 2 nodes
    /// </summary>
    /// <param name="inNodeId">The index of the node for the gene to leave from</param>
    /// <param name="outNode">The index of the node for the gene to enter</param>
    /// <returns>The newly created gene object</returns>
    private NeatGene CreateGene(int fromNodeId, int toNodeId)
    {
        Vector2Int key = new Vector2Int(fromNodeId, toNodeId);

        // Check gene doesn't alreay exist (DEBUG)
        if (geneTable.ContainsKey(key))
            Debug.LogError("Gene from (" + fromNodeId + "->" + toNodeId + ") already exists in this genome");

        NeatGene gene = new NeatGene(controller.FetchInnovationId(fromNodeId, toNodeId), fromNodeId, toNodeId, Random.Range(-1.0f, 1.0f));
		genes.Add(gene);
		geneTable[key] = gene;

        nodes[fromNodeId].outputGenes.Add(gene);
        nodes[toNodeId].inputGenes.Add(gene);
        return gene;
    }

    /// <summary>
    /// Pass some input through the network
    /// </summary>
    /// <param name="input">Inputs to feed the network (Expected to be size of inputCount)</param>
    /// <returns>Output values from the network (Will be size of outputCount)</returns>
    public float[] GenerateOutput(float[] input)
    {
        // Set the input node values
        for (int i = 0; i < input.Length; ++i)
        {
            nodes[i].workingValue = input[i];
            nodes[i].workingValueFinal = false;
        }


        // Clear all working values
        for (int i = inputCount; i < nodes.Count; ++i)
        {
            nodes[i].workingValue = 0.0f;
            nodes[i].workingValueFinal = false;
        }
        

        // Convert output nodes into array of values
        float[] output = new float[outputCount];
        for (int i = 0; i < outputCount; ++i)
            output[i] = nodes[inputCount + i].CalculateValue();

        return output;
    }


    /// <summary>
    /// Create random mutations in the network
    /// </summary>
    public void CreateMutations()
    {
		// Change weights
		MutateWeights();
		float chance;

		// Add connection
		chance = Random.value;
		if (chance <= controller.structuralMutationChance)
			AddMutatedConnection();

		// Add node
		chance = Random.value;
		if (chance <= controller.structuralMutationChance)
			AddMutatedNode();
	}

	/// <summary>
	/// Randomly selects genes and alter their weights
	/// </summary>
	private void MutateWeights()
	{
		foreach (NeatGene gene in genes)
		{
			float chance = Random.value;

			if (chance <= controller.weightMutationChance)
			{
				// Change type of weight mutation
				float subChance = Random.value;

				// Flip sign of weight
				if (subChance <= 0.2f)
					gene.weight *= -1.0f;

				// Pick a random weight [-1,1]
				else if (subChance <= 0.4f)
					gene.weight = Random.Range(-1.0f, 1.0f);

				// Increase weight in range of [0%,100%]
				else if (subChance <= 0.6f)
					gene.weight *= 1.0f + Random.value;

				// Decrease weight in range of [0%,100%]
				else if (subChance <= 0.8f)
					gene.weight *= Random.value;

				// Flip enabled state for gene
				else
					gene.isEnabled = !gene.isEnabled;
			}
		}
	}

	/// <summary>
	/// Attempt to randomly connect a pair of unconnected nodes
	/// </summary>
	private void AddMutatedConnection()
	{
		// 30 attempts to create a gene
		for (int i = 0; i<30; ++i)
		{
			int fromId = Random.Range(0, nodes.Count - outputCount);
			int toId = Random.Range(inputCount, nodes.Count);

			// Prevent from node being output node
			if (fromId > inputCount)
				fromId += outputCount;
			

			if (!controller.DoesGeneExist(fromId, toId))
			{
				CreateGene(fromId, toId);
				return;
			}
		}
	}

	/// <summary>
	/// Randomly place a node in the middle of an existing gene
	/// </summary>
	private void AddMutatedNode()
	{
		if (genes.Count == 0)
			return;

		// 30 attempts to create a node
		for (int i = 0; i < 30; ++i)
		{
			int geneId = Random.Range(0, genes.Count);
			NeatGene gene = genes[geneId];

			// Only add node in enabled genes
			if (!gene.isEnabled)
				continue;

			// Add the node
			NeatNode newNode = new NeatNode(nodes.Count, NeatNode.NodeType.Hidden, this);
			nodes.Add(newNode);

			// Disable old gene and add connected genes to new node
			gene.isEnabled = false;
			CreateGene(gene.fromNodeId, newNode.ID).weight = 1.0f; // Use 1.0 to avoid initial impact of adding the node
			CreateGene(newNode.ID, gene.toNodeId).weight = gene.weight;
			return;
		}
	}

	/// <summary>
	/// Can these 2 networks be consider the same species
	/// </summary>
	/// <returns></returns>
	public static bool AreSameSpecies(NeatNetwork networkA, NeatNetwork networkB)
	{
		// Construct gene table (A genes in index 0 B genes in index 1)
		Dictionary<int, NeatGene[]> geneTable = new Dictionary<int, NeatGene[]>();

		foreach (NeatGene gene in networkA.genes)
			geneTable.Add(gene.innovationId, new NeatGene[] { gene, null });

		foreach (NeatGene gene in networkB.genes)
		{
			if (geneTable.ContainsKey(gene.innovationId))
				geneTable[gene.innovationId][1] = gene;
			else
				geneTable.Add(gene.innovationId, new NeatGene[] { null, gene });
		}


		// Consider genes in order
		List<int> geneIds = new List<int>(geneTable.Keys);
		geneIds.Sort((x, y) => -x.CompareTo(y)); // Sort from last to first


		int excessCount = 0;
		int disjointCount = 0;
		int matchCount = 0;
		float matchTotalWeightDiff = 0.0f;

		bool checkingForExcess = true;
		bool checkingExcessFromA = false; // Otherwise read from B

		// Calculate counts by considering each gene
		for (int i = 0; i < geneIds.Count; ++i)
		{
			NeatGene[] genes = geneTable[geneIds[i]];

			// Gene is excess if exists at hanging at end of gene list
			if (checkingForExcess)
			{
				// Check which side we're to read excess from
				if (i == 0)
					checkingExcessFromA = (genes[0] != null);

				if (checkingExcessFromA && genes[1] == null)
					++excessCount;
				else if (!checkingExcessFromA && genes[0] == null)
					++excessCount;

				// Finished reading the excess (either the network we're reading doesn't have this gene or the other network also has this gene)
				else
					checkingForExcess = false;
			}

			// (Can be changed above, so still check for disjoint of matching)
			if (!checkingForExcess)
			{
				// Matching gene
				if (genes[0] != null && genes[1] != null)
				{
					++matchCount;
					matchTotalWeightDiff += Mathf.Abs(genes[0].weight - genes[1].weight);
				}
				// One net doesn't have this gene, so disjoint
				else
					++disjointCount;
			}
		}

		NeatController controller = networkA.controller; // Should be the same for B
		float geneCount = Mathf.Max(networkA.genes.Count, networkB.genes.Count);
		
		float networkDelta =
			controller.excessCoefficient * excessCount / geneCount +
			controller.disjointCoefficient * disjointCount / geneCount +
			controller.weightDiffCoefficient * (matchTotalWeightDiff / (float)matchCount);

		// Each network can be considered under the same species, if their difference is in acceptable range
		return networkDelta <= controller.speciesDeltaThreshold;
	}

	public int CompareTo(NeatNetwork other)
	{
		if (fitness == other.fitness)
			return 0;
		else if (fitness < other.fitness)
			return -1;
		else
			return 1;
	}
}
