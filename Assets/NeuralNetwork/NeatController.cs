using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Main controller for a particular NEAT neural net
/// </summary>
public class NeatController
{
    /// <summary>
    /// Used to assign unique ids to genes
    /// </summary>
    private int innovationCounter = 0;
    private Dictionary<Vector2Int, int> innovationIds;


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
}
