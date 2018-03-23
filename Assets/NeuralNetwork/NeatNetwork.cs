﻿using System.Collections;
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


    private List<NeatNode> nodes;
    private Dictionary<Vector2Int, NeatGene> genes;

    public readonly int inputCount;
    public readonly int outputCount;


    public NeatNetwork(NeatController controller, int inputCount, int outputCount)
    {
        this.controller = controller;

        this.inputCount = inputCount;
        this.outputCount = outputCount;

        InitializeNetwork();
    }

    /// <summary>
    /// Create the initial nodes and genes for the starting network
    /// </summary>
    private void InitializeNetwork()
    {
        nodes = new List<NeatNode>();
        genes = new Dictionary<Vector2Int, NeatGene>();


        // The first I nodes will be inputs and the following O nodes will be outputs
        for (int i = 0; i < inputCount; ++i)
            nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Input));

        for (int i = 0; i < outputCount; ++i)
            nodes.Add(new NeatNode(nodes.Count, NeatNode.NodeType.Output));


        // Create initial (minimal genes) connection inputs to outputs
        for (int i = 0; i < inputCount; ++i)
            for (int j = 0; j < outputCount; ++j)
            {
                genes[new Vector2Int(i, j)] = new NeatGene(controller.FetchInnovationId(i, j), i, j, Random.Range(-1.0f, 1.0f));
            }
    }

    /// <summary>
    /// Create a gene between 2 nodes
    /// </summary>
    /// <param name="inNodeId">The index of the node for the gene to leave from</param>
    /// <param name="outNode">The index of the node for the gene to enter</param>
    /// <returns>The newly created gene object</returns>
    private NeatGene CreateGene(int fromNodeId, int toNodeId)
    {
        NeatGene gene = new NeatGene(controller.FetchInnovationId(fromNodeId, toNodeId), fromNodeId, toNodeId, Random.Range(-1.0f, 1.0f));
        genes[new Vector2Int(fromNodeId, toNodeId)] = gene;

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
        for (int i = 0; i < inputCount; ++i)
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
}
