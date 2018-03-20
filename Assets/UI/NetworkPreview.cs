using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class NetworkPreview : MonoBehaviour
{
    [SerializeField]
    private RectTransform defaultDisplay;
    private RawImage[,] display;

    [SerializeField]
    private NeuralInput currentInput;

    void Start ()
    {
        // Duplicate the display object to fill with pixels
        Rect pixelRect = defaultDisplay.rect;
        display = new RawImage[NeuralInput.ViewResolution, NeuralInput.ViewResolution];

        pixelRect.width /= NeuralInput.ViewResolution;
        pixelRect.height /= NeuralInput.ViewResolution;

        for (int x = 0; x < NeuralInput.ViewResolution; ++x)
            for (int y = 0; y < NeuralInput.ViewResolution; ++y)
            {
                RectTransform pixel = Instantiate(defaultDisplay.gameObject, defaultDisplay.transform.parent).GetComponent<RectTransform>();
                pixel.position = new Vector2(defaultDisplay.position.x + pixelRect.width * x, defaultDisplay.position.y - pixelRect.height * y);
                pixel.sizeDelta = new Vector2(pixelRect.width, pixelRect.height);

                pixel.gameObject.name = "Pixel (" + x + ", " + y + ")";
                display[x, y] = pixel.GetComponent<RawImage>();
            }


        Destroy(defaultDisplay.gameObject);
    }
	
	void Update ()
    {
        if (currentInput == null)
            return;
        
        for (int x = 0; x < NeuralInput.ViewResolution; ++x)
            for (int y = 0; y < NeuralInput.ViewResolution; ++y)
            {
                if (currentInput.display[x, y].arrow != 0.0f)
                    display[x, y].color = Color.red;
                else if (currentInput.display[x, y].character != 0.0f)
                    display[x, y].color = Color.black;
                else if (currentInput.display[x, y].stage != 0.0f)
                    display[x, y].color = Color.white;
                else
                    display[x, y].color = new Color(0, 0, 0, 0.2f);
            }
    }
}
