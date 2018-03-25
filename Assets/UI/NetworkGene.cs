using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Visualisation of a NEAT gene
/// </summary>
[RequireComponent(typeof(RawImage))]
public class NetworkGene : MonoBehaviour
{
	private RawImage visual;

	[SerializeField]
	private Color inactiveColour = new Color(0.5f, 0.5f, 0.5f, 0.2f);
	[SerializeField]
	private Color activeColour = new Color(1, 1, 1, 0.5f);

	[SerializeField]
	private float width = 1.0f;


	/// <summary>
	/// The gene that this is a visualisation for
	/// </summary>
	private NeatGene netGene;

	private NetworkNode fromNode;
	private NetworkNode toNode;

	
	void Update ()
	{
		if (netGene == null || !netGene.isEnabled)
			return;


		// Draw connection between nodes
		Vector3 diff = toNode.transform.position - fromNode.transform.position;
		float angle = Vector3.Angle(Vector3.right, diff);
		if (diff.y < 0.0f)
			angle *= -1.0f;

		visual.rectTransform.SetPositionAndRotation(fromNode.transform.position + diff * 0.5f, Quaternion.AngleAxis(angle, Vector3.forward));
		visual.rectTransform.sizeDelta = new Vector2(diff.magnitude * 0.75f, width);

		// Change colour based on the current readings
		float value = fromNode.netNode.WorkingValue * netGene.weight;
		if (value < 0)
		{
			Color colour = inactiveColour * -value;
			colour.a = activeColour.a;
			visual.color = colour;
		}
		else
		{
			Color colour = activeColour * value;
			colour.a = activeColour.a;
			visual.color = colour;
		}
	}

	/// <summary>
	/// Update this visualisation
	/// </summary>
	/// <param name="from">Visual node connecting from</param>
	/// <param name="to">Visual node connecting to</param>
	/// <param name="gene">The gene this is a visualisation for</param>
	public void SetVisualisation(NetworkNode from, NetworkNode to, NeatGene gene)
	{
		visual = GetComponent<RawImage>();

		fromNode = from;
		toNode = to;
		netGene = gene;

		visual.enabled = gene.isEnabled;
	}
}
