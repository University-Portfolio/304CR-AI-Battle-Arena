using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Collection of normalized pixels which a network can see
/// </summary>
public struct NeuralPixel
{
    public float stage;
    public float character;
    public float arrow;
}


[RequireComponent(typeof(Character))]
public class NeuralInput : MonoBehaviour
{
    public static int ViewResolution { get { return 20; } }

    public Character character { get; private set; }
	private Transform cachedTrans;
    
    public NeuralPixel[,] display { get; private set; }
	public float displayScale = 1.0f;


    void Start ()
    {
        character = GetComponent<Character>();
        display = new NeuralPixel[ViewResolution, ViewResolution];
		cachedTrans = transform;
	}

    /// <summary>
    /// Convert a world position into a render pixel position
    /// </summary>
    public Vector2Int WorldToRender(Vector3 position)
    {
        Vector3 pos = position - cachedTrans.position;

        int x = Mathf.RoundToInt(Vector3.Dot(pos, cachedTrans.right) / displayScale) + ViewResolution / 2;
        int y = Mathf.RoundToInt(Vector3.Dot(pos, cachedTrans.forward) / displayScale) + ViewResolution / 2;
        return new Vector2Int(x, y);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw debug view
        Vector3 a = transform.position + (transform.forward * (ViewResolution / 2) + transform.right * (ViewResolution / 2)) * displayScale;
        Vector3 b = transform.position + (transform.forward * (ViewResolution / 2) + transform.right * (-ViewResolution / 2)) * displayScale;
        Vector3 c = transform.position + (transform.forward * (-ViewResolution / 2) + transform.right * (ViewResolution / 2)) * displayScale;
        Vector3 d = transform.position + (transform.forward * (-ViewResolution / 2) + transform.right * (-ViewResolution / 2)) * displayScale;

        Debug.DrawLine(a, b, Color.red, 0.0f);
        Debug.DrawLine(c, d, Color.red, 0.0f);

        Debug.DrawLine(a, c, Color.red, 0.0f);
        Debug.DrawLine(b, d, Color.red, 0.0f);
    }
#endif

    void FixedUpdate ()
    {
        StageController stage = GameMode.Main.stage;
        Vector3 stageCentre = stage.transform.position;
        float stageSize = stage.currentSize + 0.25f;

		Vector3 forward = cachedTrans.forward;
		Vector3 right = cachedTrans.right;

        // Clear view
        for (int x = 0; x < ViewResolution; ++x)
            for (int y = 0; y < ViewResolution; ++y)
            {
                display[x, y] = new NeuralPixel();
                
                Vector2 dir = new Vector2(x - ViewResolution / 2, y - ViewResolution / 2) * displayScale;
                Vector3 check = cachedTrans.position + forward * dir.y + right * dir.x;

                // Calculate if inside of circle
                float a = check.x - stageCentre.x;
                float b = check.z - stageCentre.z;
                if (a * a + b * b <= stageSize * stageSize)
                    display[x, y].stage = 1.0f;
            }

        // Display characters
        foreach (Character other in GameMode.Main.characters)
        {
            if (other.IsDead)
                continue;

			// Draw character
            Vector2Int pos = WorldToRender(other.transform.position);
            
            if (pos.x >= 0 && pos.x < ViewResolution && pos.y >= 0 && pos.y < ViewResolution)
                display[pos.x, pos.y].character = 1.0f;

			// Draw arrow
			if (other.currentProjectile != null)
			{
				pos = WorldToRender(other.currentProjectile.transform.position);

				if (pos.x >= 0 && pos.x < ViewResolution && pos.y >= 0 && pos.y < ViewResolution)
					display[pos.x, pos.y].arrow = 1.0f;
			}
        }
        
    }
}
