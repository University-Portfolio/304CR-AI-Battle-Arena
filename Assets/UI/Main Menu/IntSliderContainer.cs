using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntSliderContainer : MonoBehaviour
{
	[SerializeField]
	private Text valueText;
	[SerializeField]
	private Slider slider;

	public int Value {
		get { return Mathf.RoundToInt(slider.value); }
		set { slider.value = Mathf.Clamp(value, slider.minValue, slider.maxValue); }
	}

	public int Max { get { return Mathf.RoundToInt(slider.maxValue); } }
	public int Min { get { return Mathf.RoundToInt(slider.minValue); } }


	void Awake()
	{
		OnValueChanged(slider.value);
	}

	public void SetRange(int min, int max)
	{
		slider.minValue = min;
		slider.maxValue = max;
		Value = Value;
		valueText.text = "" + Value;
	}

	public void OnValueChanged(float value)
	{
		valueText.text = "" + Value;
	}
}
