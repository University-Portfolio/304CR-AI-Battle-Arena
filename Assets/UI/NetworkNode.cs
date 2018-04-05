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
	private Color positiveColour = new Color(1, 1, 1, 1);
	[SerializeField]
	private Color negativeColour = new Color(0, 0, 0, 1);

	/// <summary>
	/// The node that this is a visualisation for
	/// </summary>
	public NeatNode netNode { get; private set; }


	/// <summary>
	/// Update this visualisation
	/// </summary>
	/// <param name="node">The node for which this is a visualisation of</param>
	public void SetVisualisation(NeatNode node, bool isHidden = false)
	{
		netNode = node;

		// Disable visual for input nodes
		gameObject.SetActive(!isHidden);
	}

	
	void Update ()
	{
		if (netNode == null)
			return;

		// Change colour based on the current readings
		float value = netNode.workingValue;
		if (value < 0)
			visual.color = Color.Lerp(inactiveColour, negativeColour, -value);
		else
			visual.color = Color.Lerp(inactiveColour, positiveColour, value);
	}
}
