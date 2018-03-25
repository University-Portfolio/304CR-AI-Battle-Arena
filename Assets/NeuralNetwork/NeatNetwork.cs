using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents a single specific NEAT nerual network/genome
/// </summary>
public class NeatNetwork
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


    public NeatNetwork(NeatController controller, int inputCount, int outputCount, int biasInputCount = 1)
    {
        this.controller = controller;

        this.inputCount = inputCount + biasInputCount;
        this.outputCount = outputCount;

		// Initialise network
		nodes = new List<NeatNode>();
		genes = new List<NeatGene>();
		geneTable = new Dictionary<Vector2Int, NeatGene>();


		// The first I nodes will be inputs and the following O nodes will be outputs
		for (int i = 0; i < inputCount; ++i)
			nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Input));

		for (int i = 0; i < biasInputCount; ++i)
			nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.BiasInput));

		for (int i = 0; i < outputCount; ++i)
			nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Output));


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
            nodes[i].WorkingValue = input[i];
            nodes[i].workingValueFinal = false;
        }


        // Clear all working values
        for (int i = inputCount; i < nodes.Count; ++i)
        {
            nodes[i].WorkingValue = 0.0f;
            nodes[i].workingValueFinal = false;
        }
        

        // Convert output nodes into array of values
        float[] output = new float[outputCount];
        for (int i = 0; i < outputCount; ++i)
            output[i] = nodes[inputCount + i].CalculateValue(nodes);

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
	public void MutateWeights()
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
	public void AddMutatedConnection()
	{
		// 30 attempts to create a gene
		for (int i = 0; i<30; ++i)
		{
			int fromId = Random.Range(0, nodes.Count - outputCount);
			int toId = Random.Range(inputCount, nodes.Count);

			// Prevent from node being output node
			if (fromId >= inputCount)
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
	public void AddMutatedNode()
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
			NeatNode newNode = new NeatNode(nodes.Count, NeatNode.NodeType.Hidden);
			nodes.Add(newNode);

			// Disable old gene and add connected genes to new node
			gene.isEnabled = false;
			CreateGene(gene.fromNodeId, newNode.ID).weight = 1.0f; // Use 1.0 to avoid initial impact of adding the node
			CreateGene(newNode.ID, gene.toNodeId).weight = gene.weight;
			return;
		}
	}
}
