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
        BiasInput,
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
    public float WorkingValue
    {
        get
        {
            if (type == NodeType.BiasInput)
                return 1.0f;
            else
                return _workingValue;
        }
        set { _workingValue = value; }
    }
    private float _workingValue;

    /// <summary>
    /// Is the current working value the final value
    /// </summary>
    public bool workingValueFinal = false;


    public NeatNode(int ID, NodeType type)
    {
        this.ID = ID;
        this.type = type;

        this.inputGenes = new List<NeatGene>();
        this.outputGenes = new List<NeatGene>();

        if (type == NodeType.BiasInput)
            _workingValue = 1.0f;
        else
            _workingValue = 0.0f;
    }

    /// <summary>
    /// Calculate the value for this node be reading incoming connections 
    /// </summary>
    /// <param name="network">List of all the nodes in the network</param>
    /// <returns>The activated value for this node</returns>
    public float CalculateValue(List<NeatNode> network)
    {
        if (type == NodeType.BiasInput)
            return 1.0f;
        else if (type == NodeType.Input || workingValueFinal)
            return _workingValue; // Value is set or cached for us


        // Haven't worked out the value yet, so need to work it out now
        _workingValue = 0.0f;

        foreach (NeatGene input in inputGenes)
        {
            // Only considered enabled genes
            if (!input.isEnabled)
                continue;

            NeatNode inNode = network[input.fromNodeId];
            _workingValue += inNode.CalculateValue(network) * input.weight;
        }


        // Pass summation through activation function and cache until net inputs
        _workingValue = (float)Math.Tanh(_workingValue);
        workingValueFinal = true;

        return _workingValue;
    }
}
