using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TrainPanel : MonoBehaviour
{
	public static int MaxCharacterCount { get { return 32; } }

	[SerializeField]
	private IntSliderContainer totalCount;

	[SerializeField]
	private InputField populationNameNew;
	[SerializeField]
	private Dropdown populationNameExisting;
	private string[] existingCollections;

	[SerializeField]
	private Toggle newPopulationToggle;
	

	void Start ()
	{
		totalCount.SetRange(2, MaxCharacterCount);
		totalCount.Value = MaxCharacterCount;

		populationNameNew.gameObject.SetActive(true);
		populationNameExisting.gameObject.SetActive(false);
		newPopulationToggle.isOn = true;
	}

	void OnEnable()
	{
		existingCollections = NeatController.GetExistingCollections();

		// No collections on disc so cannot load
		if (existingCollections.Length == 0)
			newPopulationToggle.interactable = false;
		else
			newPopulationToggle.interactable = true;
	}

	public void OnStartPress()
	{
		if (newPopulationToggle.isOn)
		{
			if (populationNameNew.text.Length != 0 && populationNameNew.text.IndexOfAny(System.IO.Path.GetInvalidPathChars()) < 0)
				GameMode.main.StartTraining(totalCount.Value, populationNameNew.text);
		}
		else
			GameMode.main.StartTraining(totalCount.Value, existingCollections[populationNameExisting.value]);
	}

	public void OnToggleNewPopulation()
	{
		if (newPopulationToggle.isOn)
		{
			populationNameNew.gameObject.SetActive(true);
			populationNameExisting.gameObject.SetActive(false);
		}
		else
		{
			// Update displayed options
			populationNameExisting.ClearOptions();

			List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
			foreach (string collection in existingCollections)
				options.Add(new Dropdown.OptionData(collection));
			populationNameExisting.AddOptions(options);

			populationNameNew.gameObject.SetActive(false);
			populationNameExisting.gameObject.SetActive(true);
		}
	}
}
