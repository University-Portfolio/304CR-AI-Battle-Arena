using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour
{
    public CharacterController characterController { get; private set; }

    [SerializeField]
    private ArrowProjectile defaultProjectile;

	public ArrowProjectile currentProjectile { get; private set; }

	[SerializeField]
    private Vector3 gravityVector = Vector3.down;
    [SerializeField]
    private float dragFactor = 1.0f;
    [SerializeField]
    private float moveSpeed = 1.0f;
    [SerializeField]
    private float turnSpeed = 1.0f;
	[SerializeField]
	public float shootDuration = 1.0f;


    /// <summary>
    /// The 2D movement velocity of this object
    /// </summary>
    public Vector2 velocity { get; private set; }
    /// <summary>
    /// The actual 3D velocity of this object
    /// </summary>
    public Vector3 trueVelocity { get; private set; }


    /// <summary>
    /// The direction that the character is currently facing
    /// </summary>
    public Vector2 direction { get { return new Vector2(Mathf.Sin(directionAngle), Mathf.Cos(directionAngle)); } }
    /// <summary>
    /// The angle that the direction is currently looking
    /// </summary>
    internal float directionAngle = 0.0f;

    /// <summary>
    /// The movement vector for this frame
    /// </summary>
    private float deltaMovement;
    /// <summary>
    /// The turning amount for this frame
    /// </summary>
    private float deltaAngle;


	/// <summary>
	/// The time left until the character is no longer shooting
	/// </summary>
	private float shootTimer = 0;
	/// <summary>
	/// The time left until the arrow fires
	/// </summary>
	private float arrowTimer = 0;
	public bool IsShooting { get { return shootTimer != 0; } }
	public float NormalizedShootTime { get { return shootTimer / shootDuration; } }


	/// <summary>
	/// Time left before clearing the body
	/// </summary>
	private float deadClearTimer = 0;
	public bool isAlive { get; private set; }
	public bool IsDead { get { return !isAlive; } }

	public int killCount { get; private set; }
	public int roundWinCount;


	void Start ()
    {
        characterController = GetComponent<CharacterController>();
		isAlive = true;
		roundWinCount = 0;
	}

    void Update()
    {
		if (IsDead)
		{
			// Make body disapear 
			if (deadClearTimer > 0.0f)
			{
				deadClearTimer -= Time.deltaTime;
				if (deadClearTimer <= 0.0f)
				{
					deadClearTimer = 0.0f;
					gameObject.SetActive(false);
				}
			}
			return;
		}

        // Turning
        {
            directionAngle = (directionAngle + Mathf.Clamp(deltaAngle, -turnSpeed, turnSpeed) * Time.deltaTime) % (Mathf.PI * 2.0f);
            transform.forward = new Vector3(direction.x, 0, direction.y);
            deltaAngle = 0;
        }

        // Movement
        {
            // Decay velocity
            velocity *= Mathf.Clamp01(1.0f - dragFactor * Time.deltaTime);

            // Add frame input
            velocity += direction * Mathf.Clamp(deltaMovement, -moveSpeed, moveSpeed) * Time.deltaTime;
            deltaMovement = 0.0f;


            // Convert to 3D velocity
            trueVelocity = new Vector3(velocity.x, trueVelocity.y, velocity.y);

            trueVelocity += gravityVector * Time.deltaTime;
            characterController.Move(trueVelocity * Time.deltaTime);

            trueVelocity = characterController.velocity; // Correct velocity, for any collisions
        }

		// Count down shoot timer
		if (shootTimer != 0)
		{
			// Fire arrow
			if (arrowTimer != 0)
			{
				arrowTimer -= Time.deltaTime;
				if (arrowTimer < 0)
				{
					// Remove current projectile
					if (currentProjectile != null)
						Destroy(currentProjectile.gameObject);

					currentProjectile = Instantiate(defaultProjectile.gameObject).GetComponent<ArrowProjectile>();
					currentProjectile.Fire(this);
					arrowTimer = 0;
				}
			}

			// Wait for shooting to be finished
			shootTimer -= Time.deltaTime;
			if (shootTimer < 0)
				shootTimer = 0;
		}

		// Kill this character
		if (transform.position.y < -30.0f)
			OnDead();
	}

    /// <summary>
    /// Move the character by some amount (+: forward, -:back)
    /// </summary>
    /// <param name="amount">+: forward, -:back</param>
    public void Move(float amount)
    {
		// Cannot move while shooting
		if(!IsShooting && isAlive)
			deltaMovement += amount * moveSpeed;
    }

    /// <summary>
    /// Turn the character by some amount (+: right, -:left)
    /// </summary>
    /// <param name="amount">+: right, -:left</param>
    public void Turn(float amount)
    {
		if(isAlive)
			deltaAngle += amount * turnSpeed;
    }

    /// <summary>
    /// Attempt to fire an arrow
    /// </summary>
    /// <returns>If arrow successfully fires</returns>
    public bool Fire()
    {
		if (IsShooting || IsDead)
			return false;

		arrowTimer = shootDuration * 0.8f;
		shootTimer = shootDuration;
        return true;
    }


	/// <summary>
	/// Called when this character has successfully shot another character
	/// </summary>
	/// <param name="target">The character that has been shot</param>
	public void OnGoodShot(Character target)
	{
		killCount++;
	}
	/// <summary>
	/// Called when this character has been shot by another character
	/// </summary>
	/// <param name="attacker">The character who shot this</param>
	public void OnBeenShot(Character attacker)
	{
		OnDead();
	}


	/// <summary>
	/// Callback for when this character dies
	/// </summary>
	public void OnDead()
	{
		if (!isAlive)
			return;

		characterController.enabled = false;
		isAlive = false;
		deadClearTimer = 3.0f;
	}
	/// <summary>
	/// Called when this character respawns
	/// </summary>
	public void Respawn()
	{
		gameObject.SetActive(true);
		isAlive = true;
		characterController.enabled = true;
		shootTimer = 0.0f;
		arrowTimer = 0.0f;
	}


	/// <summary>
	/// Set the colour for this character
	/// </summary>
	/// <param name="colour"></param>
	public void SetColour(Color colour)
	{
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
			if(!renderer.gameObject.name.EndsWith("(Colourless)"))
				renderer.material.color = colour;// .SetColor("_Albedo", colour);
	}
}
