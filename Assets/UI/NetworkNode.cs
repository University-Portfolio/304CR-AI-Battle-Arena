using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Visualisation of a NEAT node 
/// </summary>
public class NetworkNode : MonoBehaviour
{
	[SerializeField]
	private RawImage visual;

	[SerializeField]
	private Color inactiveColour = new Color(0.5f, 0.5f, 0.5f, 0.2f);
	[SerializeField]
	private Color activeColour = new Color(1, 1, 1, 0.5f);

	/// <summary>
	/// The node that this is a visualisation for
	/// </summary>
	public NeatNode netNode { get; private set; }


	/// <summary>
	/// Update this visualisation
	/// </summary>
	/// <param name="node">The node for which this is a visualisation of</param>
	public void SetVisualisation(NeatNode node)
	{
		netNode = node;

		// Disable visual for input nodes
		gameObject.SetActive(netNode.type != NeatNode.NodeType.Input);
	}

	
	void Update ()
	{
		if (netNode == null)
			return;

		// Change colour based on the current readings
		float value = netNode.WorkingValue;
		if (value < 0)
			visual.color = inactiveColour;
		else
		{
			Color colour = activeColour * value;
			colour.a = activeColour.a;
			visual.color = colour;
		}
	}
}
