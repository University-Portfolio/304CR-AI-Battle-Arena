using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreePreview : MonoBehaviour
{
	/// <summary>
	/// The target which is currently being visualised
	/// </summary>
	private TreeInputAgent currentTarget;


	[Header("Text Fields")]
	[SerializeField]
	private Text roundText;
	[SerializeField]
	private Text actionText;
	[SerializeField]
	private KeyValueText leftColumnDefault;
	[SerializeField]
	private KeyValueText rightColumnDefault;

	private List<KeyValueText> leftColumn = new List<KeyValueText>();
	private List<KeyValueText> rightColumn = new List<KeyValueText>();


	void Start()
	{
		leftColumnDefault.gameObject.SetActive(false);
		rightColumnDefault.gameObject.SetActive(false);
	}

	void Update()
	{
		if (currentTarget != null)
		{
			roundText.text = GameMode.main.currentRound + "/" + GameMode.main.TotalRounds;
			actionText.text = currentTarget.tree.currentActionName;

			// Left column is agent vars
			foreach (var kv in leftColumn)
				kv.Value = "" + currentTarget.tree.agentProfile.variables[kv.Key];

			// Right column is global vars
			foreach (var kv in rightColumn)
				kv.Value = "" + currentTarget.tree.globalVars.variables[kv.Key];
		}
		else
		{
			roundText.text = GameMode.main.currentRound + "/" + GameMode.main.TotalRounds;
			actionText.text = "?";
		}
	}

	/// <summary>
	/// Update this visualisation
	/// </summary>
	/// <param name="input">The player to view</param>
	public void SetVisualisation(TreeInputAgent input)
	{
		currentTarget = input;

		// Clear columns
		foreach (KeyValueText kv in leftColumn)
			Destroy(kv.gameObject);
		foreach (KeyValueText kv in rightColumn)
			Destroy(kv.gameObject);

		leftColumn.Clear();
		rightColumn.Clear();

		// Create new columns
		if (currentTarget != null)
		{
			// Left column is agent vars
			int i = 0;
			foreach (var pair in currentTarget.tree.agentProfile.variables)
			{
				KeyValueText kv = Instantiate(leftColumnDefault, leftColumnDefault.transform.parent);
				RectTransform trans = kv.GetComponent<RectTransform>();

				trans.position = trans.position + new Vector3(0, -trans.rect.height * i * 1.25f, 0);

				kv.Key = pair.Key;
				kv.Value = "" + pair.Value;
				kv.gameObject.SetActive(true);
				leftColumn.Add(kv);
				++i;
			}

			// Right column is global vars
			i = 0;
			foreach (var pair in currentTarget.tree.globalVars.variables)
			{
				KeyValueText kv = Instantiate(rightColumnDefault, rightColumnDefault.transform.parent);
				RectTransform trans = kv.GetComponent<RectTransform>();

				trans.position = trans.position + new Vector3(0, -trans.rect.height * i * 1.25f, 0);

				kv.Key = pair.Key;
				kv.Value = "" + pair.Value;
				kv.gameObject.SetActive(true);
				rightColumn.Add(kv);
				++i;
			}
		}
	}
}
