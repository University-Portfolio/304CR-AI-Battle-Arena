using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyValueText : MonoBehaviour
{
	[SerializeField]
	private Text keyText;
	[SerializeField]
	private Text valueText;


	public string Key
	{
		get { return keyText.text; }
		set { keyText.text = value; }
	}
	public string Value
	{
		get { return valueText.text; }
		set { valueText.text = value; }
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
