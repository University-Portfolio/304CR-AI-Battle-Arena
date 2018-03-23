using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Information for a specific gene in a genome
/// </summary>
public class NeatGene : IEquatable<NeatGene>
{
    /// <summary>
    /// The unique ID for this gene
    /// </summary>
    public int innovationId;

    /// <summary>
    /// Is this gene currently enabled
    /// </summary>
    public bool isEnabled;

    /// <summary>
    /// The weight of this gene
    /// </summary>
    public float weight;

    /// <summary>
    /// ID of the node this gene comes from
    /// </summary>
    public int fromNodeId;

    /// <summary>
    /// Id of the node this gene goes to
    /// </summary>
    public int toNodeId;


    public NeatGene(int innovationId, int fromNodeId, int toNodeId, float startWeight, bool startEnabled = true)
    {
        this.innovationId = innovationId;
        this.fromNodeId = fromNodeId;
        this.toNodeId = toNodeId;
        this.weight = startWeight;
        this.isEnabled = startEnabled;
    }

    public bool Equals(NeatGene other)
    {
        return fromNodeId == other.fromNodeId && toNodeId == other.toNodeId;
    }
}
