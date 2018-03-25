using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class NetworkPreview : MonoBehaviour
{
    [SerializeField]
    private RawImage display;

	private Texture2D displayTexture;
	


	public NeuralInput DebugTarget;
	

	/// <summary>
	/// The target which is currently being visualised
	/// </summary>
	private NeuralInput currentTarget;


	[SerializeField]
	private NetworkNode defaultNode;
	[SerializeField]
	private NetworkGene defaultGene;

	/// <summary>
	/// Area to place the output nodes
	/// </summary>
	[SerializeField]
	private RectTransform outputSection;

	/// <summary>
	/// Area to place the hidden nodes
	/// </summary>
	[SerializeField]
	private RectTransform hiddenSection;

	/// <summary>
	/// Area to place genes
	/// </summary>
	[SerializeField]
	private RectTransform geneSection;

	private Dictionary<int, NetworkNode> nodes;
	private Dictionary<int, NetworkGene> genes;


	void Start ()
    {
		// Create texture for visualisation
		displayTexture = new Texture2D(NeuralInput.ViewResolution, NeuralInput.ViewResolution);
		displayTexture.filterMode = FilterMode.Point;
		displayTexture.wrapMode = TextureWrapMode.Clamp;
		display.texture = displayTexture;

		nodes = new Dictionary<int, NetworkNode>();
		genes = new Dictionary<int, NetworkGene>();
	}
	
	void Update ()
	{
		if (currentTarget != null)
			RenderVision();

		// DEBUG
		if (DebugTarget != currentTarget)
			SetVisualisation(DebugTarget);
	}

	/// <summary>
	/// Update this visualisation
	/// </summary>
	/// <param name="node">The node for which this is a visualisation of</param>
	public void SetVisualisation(NeuralInput input)
	{
		currentTarget = input;
		bool initialised = nodes.Count != 0;

		// Spawn input nodes
		if (!initialised)
		{
			Vector2 step = display.rectTransform.sizeDelta * (4.0f/3.0f) / NeuralInput.ViewResolution;

			for (int i = 0; i < input.network.inputCount; ++i)
			{
				NetworkNode node = Instantiate(defaultNode, display.rectTransform);
				NeatNode logicNode = input.network.nodes[i];

				// Multiple inputs at the same pixel
				int n = i % (NeuralInput.ViewResolution * NeuralInput.ViewResolution);
				int x = n % NeuralInput.ViewResolution;
				int y = (NeuralInput.ViewResolution - n / NeuralInput.ViewResolution) - 1;


				// Put bias nodes under the display
				if (logicNode.type == NeatNode.NodeType.BiasInput)
				{
					RectTransform nodeRect = node.GetComponent<RectTransform>();
					Vector3 size = display.rectTransform.sizeDelta * (4.0f / 3.0f);
					nodeRect.position = display.rectTransform.position + new Vector3(size.x - step.x, -size.y - n * step.y) + new Vector3(nodeRect.sizeDelta.x * 0.5f, -nodeRect.sizeDelta.y * 0.5f);
				}
				// Move to be inline with the pixels
				else
				{
					RectTransform nodeRect = node.GetComponent<RectTransform>();
					nodeRect.position = display.rectTransform.position + new Vector3(x * step.x, -y * step.y) + new Vector3(nodeRect.sizeDelta.x * 0.5f, -nodeRect.sizeDelta.y * 0.5f);
				}

				// Update visual and cache
				node.SetVisualisation(logicNode);
				nodes[node.netNode.ID] = node;
			}
		}
		// Already spawned in, so just update them
		else
		{
			for (int i = 0; i < input.network.inputCount; ++i)
			{
				var logicNode = input.network.nodes[i];
				nodes[logicNode.ID].SetVisualisation(logicNode);
			}
		}


		// Spawn output nodes
		if (!initialised)
		{
			float step = outputSection.sizeDelta.y / input.network.outputCount;
			Vector3 anchor = outputSection.position + new Vector3(0, outputSection.sizeDelta.y * 0.5f);

			for (int i = 0; i < input.network.outputCount; ++i)
			{
				NetworkNode node = Instantiate(defaultNode, outputSection);
				NeatNode logicNode = input.network.nodes[input.network.inputCount + i];

				// Move to be inline with the pixels
				RectTransform nodeRect = node.GetComponent<RectTransform>();
				nodeRect.position = anchor + new Vector3(0, -i * step) + new Vector3(nodeRect.sizeDelta.x * 0.5f, -nodeRect.sizeDelta.y * 0.5f);

				// Update visual and cache
				node.SetVisualisation(logicNode);
				nodes[logicNode.ID] = node;
			}
		}
		// Already spawned in, so just update them
		else
		{
			for (int i = 0; i < input.network.outputCount; ++i)
			{
				var logicNode = input.network.nodes[input.network.inputCount + i];
				nodes[logicNode.ID].SetVisualisation(logicNode);
			}
		}

		// TODO - Spawn the rest of the nodes


		// Add genes
		foreach(NeatGene logicGene in input.network.genes)
		{
			if (!logicGene.isEnabled)
				continue;

			NetworkGene gene;
			if (genes.ContainsKey(logicGene.innovationId))
				gene = genes[logicGene.innovationId];
			else
			{
				gene = Instantiate(defaultGene, geneSection);
				genes[logicGene.innovationId] = gene;
			}

			gene.SetVisualisation(nodes[logicGene.fromNodeId], nodes[logicGene.toNodeId], logicGene);
		}
	}

	/// <summary>
	/// Updates the display to have the vision of the character
	/// </summary>
	void RenderVision()
	{
		/*
		 * Uncomment to view an individual channel
		for (int i = 0; i < currentTarget.networkInput.Length; ++i)
		{
			int x = i % NeuralInput.ViewResolution;
			int y = i / NeuralInput.ViewResolution;
			Color colour = Color.white * (currentTarget.networkInput[i] + 1.0f) * 0.5f;
			colour.a = 1.0f;
			displayTexture.SetPixel(x, y, colour);
		}
		displayTexture.Apply(false, false);
		return;
		*/

		// Draw NN view to texture   
		for (int x = 0; x < NeuralInput.ViewResolution; ++x)
			for (int y = 0; y < NeuralInput.ViewResolution; ++y)
			{
				if (currentTarget.display[x, y].containsArrow)
					displayTexture.SetPixel(x, y, Color.red);
				else if (currentTarget.display[x, y].containsCharacter)
					displayTexture.SetPixel(x, y, Color.black);
				else if (currentTarget.display[x, y].containsStage)
					displayTexture.SetPixel(x, y, Color.white);
				else
					displayTexture.SetPixel(x, y, new Color(0, 0, 0, 0.2f));
			}

		displayTexture.Apply(false, false);
	}
}
