using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterObserver : MonoBehaviour
{
	[SerializeField]
	private float snappiness = 3.0f;

	[Header("Character UI")]
	[SerializeField]
	private PlayerPreview playerPreview;
	[SerializeField]
	private NetworkPreview networkPreview;

	private Character viewTarget;


	void Start()
	{
		networkPreview.gameObject.SetActive(false);
		playerPreview.gameObject.SetActive(false);
	}

	void Update()
	{
		if (viewTarget == null || viewTarget.IsDead)
			SelectNewTarget();
		else
		{
			Vector3 newPos = Vector3.Lerp(transform.position, viewTarget.transform.position, Mathf.Clamp01(snappiness * Time.deltaTime));
			newPos.y = transform.position.y;
			transform.position = newPos;
		}
	}

	/// <summary>
	/// Select the fitest network to view
	/// </summary>
	public void SelectNewTarget()
	{
		Character[] characters = GameMode.main.characters;
		viewTarget = null;

		if (GameMode.main.aliveCount == 0 || characters == null)
			return;

		for (int i = 0; i < characters.Length; ++i)
		{
			// Always select the player to view
			if (characters[i].IsPlayer && characters[i].isAlive)
			{
				viewTarget = characters[i];
				break;
			}

			// Select character with highest score
			if (characters[i].isAlive && (viewTarget == null || characters[i].TotalScore > viewTarget.TotalScore))
				viewTarget = characters[i];
		}


		// Pre-emptively disable eveything
		networkPreview.gameObject.SetActive(false);
		playerPreview.gameObject.SetActive(false);

		// Update UI
		if (viewTarget == null)
			return;
		PlayerInput playerInput = viewTarget.GetComponent<PlayerInput>();
		NeuralInputAgent neuralInput = viewTarget.GetComponent<NeuralInputAgent>();


		if (playerInput != null)
		{
			playerPreview.gameObject.SetActive(true);
			playerPreview.SetVisualisation(playerInput);
		}
		if (neuralInput != null)
		{
			networkPreview.gameObject.SetActive(true);
			networkPreview.SetVisualisation(neuralInput);
		}
		
	}
}
