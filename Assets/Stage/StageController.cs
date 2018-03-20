using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
	[SerializeField]
	private float startSize = 10.0f;
	[SerializeField]
	private float decayRate = 1.0f;

	/// <summary>
	/// The small amount of delay before the ring starts to shrink
	/// </summary>
	private float delayTimer = 3.0f;

	public float currentSize { get; private set; }
	public float DefaultSize { get { return startSize; } }


	void Start ()
	{
		currentSize = startSize;
		transform.localScale = new Vector3(currentSize, transform.localScale.y, currentSize);
    }
	
	void Update ()
	{
		// Wait a bit before shrinking
		if (delayTimer > 0.0f)
		{
			delayTimer -= Time.deltaTime;
			return;
		}

		currentSize -= decayRate * Time.deltaTime;
		if (currentSize < 0)
			currentSize = 0;

		transform.localScale = new Vector3(currentSize, transform.localScale.y, currentSize);
	}

	public void ResetStage()
	{
		delayTimer = 3.0f;
		currentSize = startSize;
		transform.localScale = new Vector3(currentSize, transform.localScale.y, currentSize);
	}
}
