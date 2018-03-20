using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class NetworkPreview : MonoBehaviour
{
    [SerializeField]
    private RawImage display;

	private Texture2D m_texture;

    [SerializeField]
    private NeuralInput currentInput;

    void Start ()
    {
		m_texture = new Texture2D(NeuralInput.ViewResolution, NeuralInput.ViewResolution);
		m_texture.filterMode = FilterMode.Point;
		m_texture.wrapMode = TextureWrapMode.Clamp;
		display.texture = m_texture;
	}
	
	void Update ()
    {
        if (currentInput == null)
            return;
        
        for (int x = 0; x < NeuralInput.ViewResolution; ++x)
            for (int y = 0; y < NeuralInput.ViewResolution; ++y)
            {
				if (currentInput.display[x, y].arrow != 0.0f)
					m_texture.SetPixel(x, y, Color.red);
                else if (currentInput.display[x, y].character != 0.0f)
					m_texture.SetPixel(x, y, Color.black);
                else if (currentInput.display[x, y].stage != 0.0f)
					m_texture.SetPixel(x, y, Color.white);
				else
					m_texture.SetPixel(x, y, new Color(0, 0, 0, 0.2f));
            }

		m_texture.Apply(false, false);
	}
}
