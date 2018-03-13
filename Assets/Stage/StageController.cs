using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
	[SerializeField]
	private float startSize = 10.0f;
	[SerializeField]
	private float decayRate = 1.0f;

	public float currentSize { get; private set; }


	void Start ()
	{
		currentSize = startSize;
	}
	
	void Update ()
	{
		currentSize -= decayRate * Time.deltaTime;
		if (currentSize < 0)
			currentSize = 0;

		transform.localScale = new Vector3(currentSize, transform.localScale.y, currentSize);
	}
}
