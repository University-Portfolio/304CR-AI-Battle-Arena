using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NeuralObserver : MonoBehaviour
{
	[SerializeField]
	private NetworkPreview previewWindow;

	[SerializeField, Range(0.0f,1.0f)]
	private float snappiness = 0.05f;

	private NeuralInputAgent viewTarget;

	
	void Update ()
	{
		if (viewTarget == null || viewTarget.character.IsDead)
			SelectNewTarget();
		else
		{
			Vector3 newPos = transform.position * (1.0f - snappiness) + viewTarget.transform.position * snappiness;
			newPos.y = transform.position.y;
			transform.position = newPos;
		}
	}

	/// <summary>
	/// Select the fitest network to view
	/// </summary>
	void SelectNewTarget()
	{
		NeuralInputAgent[] agents = FindObjectsOfType<NeuralInputAgent>();

		viewTarget = null;

		for (int i = 0; i < agents.Length; ++i)
			if (agents[i].character.isAlive && (viewTarget == null || agents[i].network.fitness > viewTarget.network.fitness)) 
				viewTarget = agents[i];

		previewWindow.SetVisualisation(viewTarget);
	}
}
