using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayPanel : MonoBehaviour
{
	public static int MaxCharacterCount { get { return 32; } }

	[SerializeField]
	private IntSliderContainer totalCount;
	[SerializeField]
	private IntSliderContainer treeCount;
	[SerializeField]
	private IntSliderContainer neuralCount;

	[SerializeField]
	private Dropdown populationName;
	private string[] existingCollections;

	[SerializeField]
	private Toggle spawnPlayer;

	
	void Start ()
	{
		totalCount.SetRange(2, MaxCharacterCount);
		totalCount.Value = 32;
		
		treeCount.SetRange(0, MaxCharacterCount);
		treeCount.Value = treeCount.Max;

		neuralCount.SetRange(0, MaxCharacterCount);
		neuralCount.Value = 0;

		spawnPlayer.isOn = false;
	}

	void OnEnable()
	{
		existingCollections = NeatController.GetExistingCollections();

		// No collections on disc so cannot load
		if (existingCollections.Length == 0)
		{
			populationName.gameObject.SetActive(false);


			treeCount.SetRange(0, MaxCharacterCount);
			treeCount.Value = treeCount.Max;
			treeCount.Interactable = false;

			neuralCount.SetRange(0, MaxCharacterCount);
			neuralCount.Value = 0;
			neuralCount.Interactable = false;

			existingCollections = new string[] { "AiArena" };
		}
		else
		{
			populationName.gameObject.SetActive(true);
			treeCount.Interactable = true;
			neuralCount.Interactable = true;

			// Update displayed options
			populationName.ClearOptions();

			List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
			foreach (string collection in existingCollections)
				options.Add(new Dropdown.OptionData(collection));
			populationName.AddOptions(options);
		}

		populationName.value = 0;
	}

	public void OnTotalCountChange()
	{
		treeCount.SetRange(0, totalCount.Value);
		neuralCount.SetRange(0, totalCount.Value);

		treeCount.Value = totalCount.Value - neuralCount.Value;
	}

	public void OnTreeCountChange()
	{
		neuralCount.Value = totalCount.Value - treeCount.Value;
	}

	public void OnNeuralCountChange()
	{
		treeCount.Value = totalCount.Value - neuralCount.Value;
	}

	public void OnSpawnPlayerChange()
	{
		if (spawnPlayer.isOn)
		{
			totalCount.SetRange(2, MaxCharacterCount - 1);
			OnTotalCountChange();
		}
		else
		{
			totalCount.SetRange(2, MaxCharacterCount);
			OnTotalCountChange();
		}
	}

	public void OnStartPress()
	{
		GameMode.main.StartGame(spawnPlayer.isOn ? totalCount.Value + 1 : totalCount.Value, spawnPlayer.isOn, neuralCount.Value, existingCollections[populationName.value]);
	}
}
