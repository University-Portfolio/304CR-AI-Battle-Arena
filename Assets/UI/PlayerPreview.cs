using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPreview : MonoBehaviour
{
	/// <summary>
	/// The target which is currently being visualised
	/// </summary>
	private PlayerInput currentTarget;


	[Header("Text Fields")]
	[SerializeField]
	private Text roundText;
	[SerializeField]
	private Text killsText;
	[SerializeField]
	private Text blocksText;
	[SerializeField]
	private Text roundWinsText;
	[SerializeField]
	private Text totalScoreText;


	void Update()
	{
		if (currentTarget != null)
		{
			roundText.text = GameMode.main.currentRound + "/" + GameMode.main.TotalRounds;

			killsText.text = "" + currentTarget.character.killCount;
			blocksText.text = "" + currentTarget.character.blockCount;
			roundWinsText.text = "" + currentTarget.character.roundWinCount;
			totalScoreText.text = "" + currentTarget.character.TotalScore;

		}
		else
		{
			roundText.text = GameMode.main.currentRound + "/" + GameMode.main.TotalRounds;

			killsText.text = "-";
			blocksText.text = "-";
			roundWinsText.text = "-";
			totalScoreText.text = "-";
		}
	}

	/// <summary>
	/// Update this visualisation
	/// </summary>
	/// <param name="input">The player to view</param>
	public void SetVisualisation(PlayerInput input)
	{
		currentTarget = input;
	}
}
