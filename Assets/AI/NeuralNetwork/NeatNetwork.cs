using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


/// <summary>
/// Represents a single specific NEAT nerual network/genome
/// </summary>
public class NeatNetwork : System.IComparable<NeatNetwork>
{
    /// <summary>
    /// The controller which spawned this network
    /// </summary>
    public readonly NeatController controller;

    /// <summary>
    /// The species this network is a member of
    /// </summary>
    public NeatSpecies assignedSpecies;


    public List<NeatNode> nodes { get; private set; }
	public List<NeatGene> genes { get; private set; }
	private Dictionary<Vector2Int, NeatGene> geneTable;

    public int inputCount { get; private set; }
	public int outputCount { get; private set; }


	/// <summary>
	/// The fitness of this network
	/// </summary>
	public float fitness;
	/// <summary>
	/// The fitness of this network in a past generation
	/// </summary>
	public float previousFitness { get; private set; }
    /// <summary>
    /// How old this network
    /// </summary>
    public int age { get; private set; }


	public NeatNetwork(NeatController controller, int inputCount, int outputCount)
    {
        this.controller = controller;
        age = 0;

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
	/// Create a copy of an existing network structure (Only structual values are copies)
	/// </summary>
	/// <param name="controller"></param>
	/// <param name="inputCount"></param>
	/// <param name="outputCount"></param>
	public NeatNetwork(NeatNetwork other)
	{
		this.controller = other.controller;
		previousFitness = other.fitness;
        this.age = other.age + 1;

        this.inputCount = other.inputCount;
		this.outputCount = other.outputCount;

		// Initialise network
		nodes = new List<NeatNode>(other.nodes);
		genes = new List<NeatGene>(other.genes);
		geneTable = new Dictionary<Vector2Int, NeatGene>(other.geneTable);
	}

	public NeatNetwork(NeatController controller, XmlElement content)
	{
		this.controller = controller;
		ReadXML(content);
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
				else if(controller.geneStateFlipMutation)
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
			controller.excessCoefficient * (geneCount == 0 ? 0 : excessCount / geneCount) +
			controller.disjointCoefficient * (geneCount == 0 ? 0 : disjointCount / geneCount) +
			controller.weightDiffCoefficient * (matchCount == 0 ? 0 : (matchTotalWeightDiff / (float)matchCount));

		// Each network can be considered under the same species, if their difference is in acceptable range
		return networkDelta <= controller.speciesDeltaThreshold;
	}

    /// <summary>
    /// Perform matching on 2 networks to breed them
    /// </summary>
    /// <returns>The new child network</returns>
    public static NeatNetwork Breed(NeatNetwork networkA, NeatNetwork networkB)
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

        // Add new genes 
        List<NeatGene> childGenes = new List<NeatGene>();

        foreach (var pair in geneTable)
        {
            NeatGene[] genes = pair.Value;

            // Inherit equally
            if (networkA.fitness == networkB.fitness)
            {
                // Only A posseses the gene
                if (genes[0] != null && genes[1] == null)
                    childGenes.Add(genes[0]);
                // Only B posseses the gene
                else if (genes[0] == null && genes[1] != null)
                    childGenes.Add(genes[1]);

                // Both posses the gene, so take random
                else
                    childGenes.Add(genes[Random.Range(0, 2)]);
            }
            // One nework is better than the other
            else
            {
                // Both posses the gene, so take random
                if (genes[0] != null && genes[1] != null)
                    childGenes.Add(genes[Random.Range(0, 2)]);

                // Take gene from A
                else if (networkA.fitness > networkB.fitness && genes[0] != null)
                    childGenes.Add(genes[0]);

                // Take gene from B
                else if (networkB.fitness > networkA.fitness && genes[1] != null)
                    childGenes.Add(genes[1]);
            }
        }


        // Construct child network
        NeatNetwork childNetwork = new NeatNetwork(networkA.controller, networkA.inputCount, networkB.outputCount);

        foreach (NeatGene gene in childGenes)
        {
            int maxId = System.Math.Max(gene.fromNodeId, gene.toNodeId);

            // Ensure required genes exist (Must initilise in index order)
            while (childNetwork.nodes.Count <= maxId)
                childNetwork.nodes.Add(new NeatNode(childNetwork.nodes.Count, NeatNode.NodeType.Hidden, childNetwork));

            // Add gene
            NeatGene newGene = childNetwork.CreateGene(gene.fromNodeId, gene.toNodeId);
            newGene.isEnabled = gene.isEnabled;
            newGene.weight = gene.weight;
        }

        return childNetwork;
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

	/// <summary>
	/// Write data about this network to xml
	/// </summary>
	/// <param name="writer"></param>
	public void WriteXML(XmlWriter writer)
	{
		// Write general
		writer.WriteAttributeString("fitness", "" + fitness);
        writer.WriteAttributeString("previousFitness", "" + previousFitness);
        writer.WriteAttributeString("age", "" + age);

        if (assignedSpecies != null)
			writer.WriteAttributeString("speciesGuid", "" + assignedSpecies.guid.ToString());


		// Write nodes
		writer.WriteStartElement("Nodes");
		writer.WriteAttributeString("inputCount", "" + inputCount);
		writer.WriteAttributeString("outputCount", "" + outputCount);
        writer.WriteAttributeString("totalCount", "" + nodes.Count);
        writer.WriteEndElement();


		// Write genes
		writer.WriteStartElement("Genes");
		foreach (NeatGene gene in genes)
		{
			writer.WriteStartElement("Gene");
			writer.WriteAttributeString("to", "" + gene.toNodeId);
			writer.WriteAttributeString("from", "" + gene.fromNodeId);
			writer.WriteAttributeString("isEnabled", "" + gene.isEnabled);
			writer.WriteAttributeString("weight", "" + gene.weight);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}
	
	/// <summary>
	/// Read in the xml for a specfic generation
	/// </summary>
	/// <param name="writer"></param>
	public void ReadXML(XmlElement entry)
	{
		fitness = float.Parse(entry.GetAttribute("fitness"));
        previousFitness = float.Parse(entry.GetAttribute("previousFitness"));
        age = int.Parse(entry.GetAttribute("age"));


        foreach (XmlElement child in entry.ChildNodes)
		{
			// Parse nodes
			if (child.Name == "Nodes")
			{
				nodes = new List<NeatNode>();

				inputCount = int.Parse(child.GetAttribute("inputCount"));
				outputCount = int.Parse(child.GetAttribute("outputCount"));
				int totalCount = int.Parse(child.GetAttribute("totalCount"));
				
				// Create nodes
				for (int i = 0; i < totalCount; ++i)
				{
					if (i < inputCount)
						nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Input, this));
					else if (i < inputCount + outputCount)
						nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Output, this));
					else
						nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Hidden, this));
				}
			}

			// Parse genes
			else if (child.Name == "Genes")
			{
				genes = new List<NeatGene>();
				geneTable = new Dictionary<Vector2Int, NeatGene>();

				foreach (XmlElement gene in child.ChildNodes)
				{
					if (gene.Name != "Gene")
						continue;

					int toId = int.Parse(gene.GetAttribute("to"));
					int fromId = int.Parse(gene.GetAttribute("from"));

					NeatGene newGene = CreateGene(fromId, toId);
					newGene.isEnabled = bool.Parse(gene.GetAttribute("isEnabled"));
					newGene.weight = float.Parse(gene.GetAttribute("weight"));
				}
			}
		}


		// Fetch assigned species from this
		if (entry.HasAttribute("speciesGuid"))
		{
			System.Guid guid = new System.Guid(entry.GetAttribute("speciesGuid"));
			
			// Fetch species
			foreach (var species in controller.activeSpecies)
				if (species.guid == guid)
				{
					if (!species.AttemptAdd(this))
						Debug.LogWarning("Couldn't assign network desired species");
					break;
				}
		}

	}
}
