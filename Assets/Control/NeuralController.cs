using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls all the neural nets
/// </summary>
public class NeuralController : MonoBehaviour
{
	public static NeuralController main { get; private set; }

	
	void Start ()
	{
		if (main == null)
			main = this;
		else
		{
			Debug.LogError("Multiple NeuralController have been created");
			return;
		}


	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
