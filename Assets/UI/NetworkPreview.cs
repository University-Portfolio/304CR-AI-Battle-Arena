using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class NetworkPreview : MonoBehaviour
{
    [Header("Visual Fields")]
    [SerializeField]
    private RawImage display;
    [SerializeField]
    private RawImage populationDisplay;

    private Texture2D displayTexture;
    private Texture2D populationTexture;


    /// <summary>
    /// The target which is currently being visualised
    /// </summary>
    private NeuralInputAgent currentTarget;


    [Header("Text Fields")]
    [SerializeField]
	private Text roundText;
	[SerializeField]
	private Text fitnessText;
	[SerializeField]
	private Text timeText;
	[SerializeField]
    private Text generationText;
	[SerializeField]
	private Text killsText;
	[SerializeField]
	private Text blocksText;
	[SerializeField]
    private Text ageText;
    [SerializeField]
    private Text speciesText;


    [Header("Preview Fields")]
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

    [SerializeField]
    private RectTransform hiddenMin;
    [SerializeField]
    private RectTransform hiddenMax;


    /// <summary>
    /// Area to place genes
    /// </summary>
    [SerializeField]
	private RectTransform geneSection;

	private Dictionary<int, NetworkNode> nodes = new Dictionary<int, NetworkNode>();
	private Dictionary<int, NetworkGene> genes = new Dictionary<int, NetworkGene>();



	void Start ()
    {
        // Create texture for visualisation
        displayTexture = new Texture2D(NeuralInputAgent.ViewResolution, NeuralInputAgent.ViewResolution);
		displayTexture.filterMode = FilterMode.Point;
		displayTexture.wrapMode = TextureWrapMode.Clamp;
		display.texture = displayTexture;

        // Create texture for populatio visual 
        populationTexture = new Texture2D(2, 2);
        populationTexture.filterMode = FilterMode.Point;
        populationTexture.wrapMode = TextureWrapMode.Clamp;
        populationDisplay.texture = populationTexture;
	}
	
	void Update ()
	{
		if (currentTarget != null)
		{
			RenderVision();
            RenderPopulation();


            roundText.text = GameMode.main.currentRound + "/" + GameMode.main.TotalRounds;
			fitnessText.text = "" + currentTarget.network.fitness;
			generationText.text = "" + currentTarget.network.controller.generationCounter;
            killsText.text = "" + currentTarget.killCount;
			blocksText.text = "" + currentTarget.blockCount;
			ageText.text = "" + currentTarget.network.age;

            speciesText.text = currentTarget.network.assignedSpecies.guid.ToString();
            speciesText.color = currentTarget.network.assignedSpecies.colour;

        }
		else
		{
			roundText.text = "?";
			fitnessText.text = "-";
			generationText.text = "?";
            killsText.text = "-";
			blocksText.text = "-";
			ageText.text = "-";
            speciesText.text = "-";
            speciesText.color = Color.white;
        }

		NeuralController controller = FindObjectOfType<NeuralController>();
		if (controller != null)
		{
			// Format time
			int seconds = (int)controller.runTime;

			int minutes = seconds / 60;
			seconds %= 60;

			int hours = minutes / 60;
			minutes %= 60;

			timeText.text = (hours < 10 ? "0" + hours : "" + hours) + ":" + (minutes < 10 ? "0" + minutes : "" + minutes) + ":" + (seconds < 10 ? "0" + seconds : "" + seconds);

		}
		else
			timeText.text = "..";
	}

	/// <summary>
	/// Layers used for drawing hidden nodes
	/// </summary>
	class NodeLayer
	{
		public Vector2Int id;
		public List<NeatNode> nodes;

		public NodeLayer(Vector2Int id)
		{
			this.id = id;
			nodes = new List<NeatNode>();
		}
	}

	/// <summary>
	/// Update this visualisation
	/// </summary>
	/// <param name="node">The node for which this is a visualisation of</param>
	public void SetVisualisation(NeuralInputAgent input)
	{
		// Cleanup old visual stuff
		if (currentTarget != null)
		{
			// Destroy genes
			foreach (var pair in genes)
				Destroy(pair.Value.gameObject);
			genes.Clear();

			// Destroy hidden nodes
			List<KeyValuePair<int, NetworkNode>> delete = new List<KeyValuePair<int, NetworkNode>>();
			foreach (var pair in nodes)
			{
				if (pair.Key >= currentTarget.network.inputCount + currentTarget.network.outputCount)
				{
					Destroy(pair.Value.gameObject);
					delete.Add(pair);
				}
			}
			foreach (var pair in delete)
				nodes.Remove(pair.Key);
		}


		// Update new visuals
		currentTarget = input;
		bool initialised = nodes.Count != 0;

		if (input == null)
			return;


		// Spawn input nodes
		if (!initialised)
		{
			Vector2 step = display.rectTransform.sizeDelta * (4.0f/3.0f) / NeuralInputAgent.ViewResolution;

			for (int i = 0; i < input.network.inputCount; ++i)
			{
				NetworkNode node = Instantiate(defaultNode, display.rectTransform);
				NeatNode logicNode = input.network.nodes[i];

				// Multiple inputs at the same pixel
				int n = i % (NeuralInputAgent.ViewResolution * NeuralInputAgent.ViewResolution);
				int x = n % NeuralInputAgent.ViewResolution;
				int y = (NeuralInputAgent.ViewResolution - n / NeuralInputAgent.ViewResolution) - 1;


				// Put extra nodes next to the display
                if(i >= NeuralInputAgent.ResolutionInputCount)
				{
					n = i - NeuralInputAgent.ResolutionInputCount;
					RectTransform nodeRect = node.GetComponent<RectTransform>();
					Vector3 size = display.rectTransform.sizeDelta * (4.0f / 3.0f);
					nodeRect.position = display.rectTransform.position + new Vector3(size.x + step.x, (-n - 0.25f) * step.y) + new Vector3(nodeRect.sizeDelta.x * 0.5f, -nodeRect.sizeDelta.y * 0.5f);
				}
				// Move to be inline with the pixels
				else
				{
					RectTransform nodeRect = node.GetComponent<RectTransform>();
					nodeRect.position = display.rectTransform.position + new Vector3(x * step.x, -y * step.y) + new Vector3(nodeRect.sizeDelta.x * 0.5f, -nodeRect.sizeDelta.y * 0.5f);
				}

				// Update visual and cache
				node.SetVisualisation(logicNode, i < NeuralInputAgent.ResolutionInputCount);
				nodes[node.netNode.ID] = node;
			}
		}
		// Already spawned in, so just update them
		else
		{
			for (int i = 0; i < input.network.inputCount; ++i)
			{
				var logicNode = input.network.nodes[i];
				nodes[logicNode.ID].SetVisualisation(logicNode, i < NeuralInputAgent.ResolutionInputCount);
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



		// Spawn hidden nodes
		// Place in layers
		Dictionary<Vector2Int, NodeLayer> nodeDistances = new Dictionary<Vector2Int, NodeLayer>();
		
		for (int i = input.network.inputCount + input.network.outputCount; i < input.network.nodes.Count; ++i)
		{
			NeatNode logicNode = input.network.nodes[i];
			Vector2Int id = new Vector2Int(logicNode.FurthestDistanceFromInput(), logicNode.FurthestDistanceFromOutput());

			// Don't draw dettached nodes in visuals
			if (!logicNode.HasActiveInputs && !logicNode.HasActiveOutputs)
				continue;

			if (!nodeDistances.ContainsKey(id))
				nodeDistances.Add(id, new NodeLayer(id));

			nodeDistances[id].nodes.Add(logicNode);
		}

		List<NodeLayer> layers = new List<NodeLayer>(nodeDistances.Values);
		layers.Sort((a, b) =>
			a.id.x < b.id.x ? - 1 :
			a.id.x == b.id.x ? (a.id.x < b.id.x ? -1 : 1) : 0
		);

		// Place nodes as layers
		for (int i = 0; i < layers.Count; ++i)
		{
			float dx = layers.Count <= 1 ? 0.5f : i / (float)(layers.Count - 1);

			// Sort nodes
			layers[i].nodes.Sort((a, b) => a.ID.CompareTo(a.ID));

			int nodeCount = layers[i].nodes.Count;
			//float yoffset = (1 / (float)nodeCount) * 0.5f;

			// Place nodes centred 
			for (int n = 0; n < nodeCount; ++n)
			{
				float dy = nodeCount <= 1 ? 0.5f : n / (float)(nodeCount - 1);
				
				NeatNode logicNode = layers[i].nodes[n];
				NetworkNode node = Instantiate(defaultNode, hiddenSection);
				
				node.transform.position = new Vector3(
					Mathf.Lerp(hiddenMin.position.x, hiddenMax.position.x, dx),
					Mathf.Lerp(hiddenMin.position.y, hiddenMax.position.y, dy),
					0.0f
				);

				// Update visual and cache
				node.SetVisualisation(logicNode);
				nodes[node.netNode.ID] = node;
			}
		}
		

		// Add genes
		foreach (NeatGene logicGene in input.network.genes)
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
    /// Renders the visualisation for the current population
    /// </summary>
    void RenderPopulation()
    {
        List<NeatNetwork> population = new List<NeatNetwork>(NeuralController.main.neatController.population);
        population.Sort((x, y) => x.CompareTo(y));
        populationTexture.Resize(population.Count, 2);

        for (int i = 0; i < population.Count; ++i)
        {
            NeatNetwork current = population[i];

            populationTexture.SetPixel(i, 0, current.assignedSpecies.colour);

            if (current == currentTarget.network)
                populationTexture.SetPixel(i, 1, Color.white);
            else
                populationTexture.SetPixel(i, 1, current.assignedSpecies.colour);
        }
        
        populationTexture.Apply(false, false);
    }

	/// <summary>
	/// Updates the display to have the vision of the character
	/// </summary>
	void RenderVision()
	{
		if (currentTarget.character.IsDead)
		{
			// Draw blank view 
			for (int x = 0; x < NeuralInputAgent.ViewResolution; ++x)
				for (int y = 0; y < NeuralInputAgent.ViewResolution; ++y)
					displayTexture.SetPixel(x, y, new Color(0, 0, 0, 0.2f));
		}
		else
		{
			// Draw NN view to texture   
			for (int x = 0; x < NeuralInputAgent.ViewResolution; ++x)
				for (int y = 0; y < NeuralInputAgent.ViewResolution; ++y)
				{
					if (currentTarget.display[x, y].containsShield)
						displayTexture.SetPixel(x, y, Color.yellow);
					else if (currentTarget.display[x, y].containsArrow)
						displayTexture.SetPixel(x, y, Color.red);
					else if (currentTarget.display[x, y].containsCharacter)
						displayTexture.SetPixel(x, y, Color.black);
					else if (currentTarget.display[x, y].containsStage)
						displayTexture.SetPixel(x, y, Color.white);
					else
						displayTexture.SetPixel(x, y, new Color(0, 0, 0, 0.2f));
				}
		}

		displayTexture.Apply(false, false);
	}


}
