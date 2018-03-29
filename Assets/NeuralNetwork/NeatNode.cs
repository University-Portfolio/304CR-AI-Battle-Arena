using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Representation for a node inside of a NEAT neural network
/// </summary>
public class NeatNode
{
    public enum NodeType
    {
        Input,
        Output,
        Hidden
    }

    /// <summary>
    /// The particular type of node this is
    /// </summary>
    public readonly NodeType type;

    /// <summary>
    /// The ID for this currrent node
    /// </summary>
    public readonly int ID;

	/// <summary>
	/// The network this node is a part of
	/// </summary>
	public readonly NeatNetwork network;


    /// <summary>
    /// Genes entering this node
    /// </summary>
    public readonly List<NeatGene> inputGenes;

    /// <summary>
    /// Genes leaving this node
    /// </summary>
    public readonly List<NeatGene> outputGenes;


    /// <summary>
    /// The current working weight for this node
    /// </summary>
    public float workingValue;

    /// <summary>
    /// Is the current working value the final value
    /// </summary>
    public bool workingValueFinal = false;
	private bool workingOutValue = false;


    public NeatNode(int ID, NodeType type, NeatNetwork network)
    {
        this.ID = ID;
        this.type = type;
		this.network = network;


		this.inputGenes = new List<NeatGene>();
        this.outputGenes = new List<NeatGene>();
        workingValue = 0.0f;
    }

    /// <summary>
    /// Calculate the value for this node be reading incoming connections 
    /// </summary>
    /// <param name="network">List of all the nodes in the network</param>
    /// <returns>The activated value for this node</returns>
    public float CalculateValue()
    {
        if (type == NodeType.Input || workingValueFinal)
            return workingValue; // Value is set or cached for us

		else if(workingOutValue) // Can only really return approximation (Occurs in event of loop)
			return (float)Math.Tanh(workingValue);


        // Haven't worked out the value yet, so need to work it out now
        workingValue = 0.0f;
		workingOutValue = true;

		foreach (NeatGene input in inputGenes)
        {
            // Only considered enabled genes
            if (!input.isEnabled)
                continue;

            NeatNode inNode = network.nodes[input.fromNodeId];
            workingValue += inNode.CalculateValue() * input.weight;
        }


        // Pass summation through activation function and cache until net inputs
        workingValue = (float)Math.Tanh(workingValue);
		workingValueFinal = true;
		workingOutValue = false;
		
		return workingValue;
    }

	/// <summary>
	/// Furthest path to output
	/// </summary>
	/// <param name="searchedNodes">Used to stop stack overflow</param>
	/// <returns></returns>
	public int FurthestDistanceFromOutput(HashSet<int> searchedNodes = null)
	{
		if (type == NodeType.Output)
			return 0;
		else
		{
			int furthest = 0;

			if (searchedNodes == null)
				searchedNodes = new HashSet<int>();


			for (int i = 0; i < outputGenes.Count; ++i)
			{
				int toId = outputGenes[i].toNodeId;

				// Prevent infinite recursion in loops
				if (searchedNodes.Contains(toId))
					continue; 

				searchedNodes.Add(toId);
				int distance = network.nodes[toId].FurthestDistanceFromOutput(searchedNodes);

				if (distance > furthest)
					furthest = distance;
			}

			return furthest + 1;
		}
	}

    /// <summary>
    /// Furthest path from input
    /// </summary>
    /// <param name="searchedNodes">Used to stop stack overflow</param>
    /// <returns></returns>
    public int FurthestDistanceFromInput(HashSet<int> searchedNodes = null)
    {
        if (type == NodeType.Input)
            return 0;
        else
        {
            int furthest = 0;

            if (searchedNodes == null)
                searchedNodes = new HashSet<int>();


            for (int i = 0; i < inputGenes.Count; ++i)
            {
                int fromId = inputGenes[i].fromNodeId;

                // Prevent infinite recursion in loops
                if (searchedNodes.Contains(fromId))
                    continue;

                searchedNodes.Add(fromId);
                int distance = network.nodes[fromId].FurthestDistanceFromInput(searchedNodes);

                if (distance > furthest)
                    furthest = distance;
            }

            return furthest + 1;
        }
    }
}