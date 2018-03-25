using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Collection of normalized pixels which a network can see
/// </summary>
public struct NeuralPixel
{
    public bool containsStage;
    public bool containsCharacter;
    public bool containsArrow;

	///
	/// This data should be passed into the network as 2 floats
	/// stage and character will share a float [-1: stage 1: character]
	/// Arrow has a float all to it's self
	///
}


/// <summary>
/// Character input profile which uses a NEAT net to controller it
/// </summary>
[RequireComponent(typeof(Character))]
public class NeuralInput : MonoBehaviour
{
    public static int ViewResolution { get { return 16; } }

    public Character character { get; private set; }
	private Transform cachedTrans;
    
    public NeuralPixel[,] display { get; private set; }
	public float displayScale = 1.5f;

	/// <summary>
	/// The network that this profile is currently using
	/// </summary>
	public NeatNetwork network;
	private float[] networkInput;
	private float[] networkOutput;


	void Start ()
    {
        character = GetComponent<Character>();
		cachedTrans = transform;

		display = new NeuralPixel[ViewResolution, ViewResolution];
		networkInput = new float[ViewResolution * ViewResolution * 2];

		// DEBUG: TODO REMOVE
		NeatController controller = new NeatController();
		network = new NeatNetwork(controller, networkInput.Length, 3);
		
		//network.CreateMutations();
	}
	
    void Update ()
    {
		// No point updating if dead
		if (character.IsDead)
			return;


		RenderVision();
		networkOutput = network.GenerateOutput(networkInput);


		// Output 0: Move
		character.Move(networkOutput[0]);

		// Output 1: Turn
		character.Turn(networkOutput[1]);

		// Output 2: Shoot 
		if(networkOutput[2] >= 0.5f)
			character.Fire();

		// DEBUG
		if (Input.GetKeyDown(KeyCode.N))
		{
			network.AddMutatedNode();
			DEBUG_REBUILD = true;
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			network.AddMutatedConnection();
			DEBUG_REBUILD = true;
		}
		if (Input.GetKeyDown(KeyCode.W))
		{
			network.MutateWeights();
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			network.CreateMutations();
			DEBUG_REBUILD = true;
		}
	}
	public bool DEBUG_REBUILD = false;


#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		// DEBUG Draw view area
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

	/// <summary>
	/// Update the input for what this net can currently see
	/// </summary>
	void RenderVision()
	{
		StageController stage = GameMode.Main.stage;
		Vector3 stageCentre = stage.transform.position;
		float stageSize = stage.currentSize + 0.25f;

		Vector3 forward = cachedTrans.forward;
		Vector3 right = cachedTrans.right;


		// Arena view
		for (int x = 0; x < ViewResolution; ++x)
			for (int y = 0; y < ViewResolution; ++y)
			{
				display[x, y] = new NeuralPixel(); // Reset pixel

				Vector2 dir = new Vector2(x - ViewResolution / 2, y - ViewResolution / 2) * displayScale;
				Vector3 check = cachedTrans.position + forward * dir.y + right * dir.x;

				// Calculate if inside of circle
				float a = check.x - stageCentre.x;
				float b = check.z - stageCentre.z;
				if (a * a + b * b <= stageSize * stageSize)
					display[x, y].containsStage = true;
			}

		// Display characters
		foreach (Character other in GameMode.Main.characters)
		{
			if (other.IsDead)
				continue;

			// Draw character
			Vector2Int pos = WorldToRender(other.transform.position);

			if (pos.x >= 0 && pos.x < ViewResolution && pos.y >= 0 && pos.y < ViewResolution)
				display[pos.x, pos.y].containsCharacter = true;

			// Draw arrow
			if (other.currentProjectile != null)
			{
				pos = WorldToRender(other.currentProjectile.transform.position);

				if (pos.x >= 0 && pos.x < ViewResolution && pos.y >= 0 && pos.y < ViewResolution)
					display[pos.x, pos.y].containsArrow = true;
			}
		}


		// Update inputs
		for (int x = 0; x < ViewResolution; ++x)
			for (int y = 0; y < ViewResolution; ++y)
			{
				int stageCharIndex = x + y * ViewResolution;
				int arrowIndex = ViewResolution * ViewResolution + x + y * ViewResolution;
				NeuralPixel pixel = display[x, y];

				networkInput[stageCharIndex] = pixel.containsCharacter ? 1.0f : pixel.containsStage ? -1.0f : 0.0f;
				networkInput[arrowIndex] = pixel.containsArrow ? 1.0f : -1.0f;
			}
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

}
