using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour
{
    public CharacterController characterController { get; private set; }
	public bool IsPlayer { get { return GetComponent<PlayerInput>() != null; } }

	[Header("Objects")]
	[SerializeField]
    private ArrowProjectile defaultProjectile;

	[SerializeField]
	private ArrowShield defaultShield;

	public ArrowProjectile currentProjectile { get; private set; }
	public ArrowShield currentShield { get; private set; }

	[Header("Movement")]
	[SerializeField]
    private Vector3 gravityVector = Vector3.down;
    [SerializeField]
    private float dragFactor = 1.0f;
    [SerializeField]
    private float moveSpeed = 1.0f;
    [SerializeField]
    private float turnSpeed = 1.0f;

	[Header("Settings")]
	[SerializeField]
	public float shootDuration = 1.0f;
	[SerializeField]
	public float shieldStun = 0.5f;
	[SerializeField]
	public float shieldDuration = 3.0f;


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


	public enum Action
	{
		None,
		Shooting,
		Shielding
	}
	public Action currentAction { get; private set; }

	/// <summary>
	/// Initial time to prevent any actions after first spawn
	/// </summary>
	private float spawnTimer = 0;
	/// <summary>
	/// The time left until the character is no doing the action
	/// </summary>
	public float actionTimer { get; private set; }
	/// <summary>
	/// The time left until the arrow fires
	/// </summary>
	private float arrowTimer = 0;

	/// <summary>
	/// The time left until the sheild can be used again
	/// </summary>
	private float shieldTimer = 0;


	public float NormalizedShootTime { get { return currentAction == Action.Shooting ? actionTimer / shootDuration : 0.0f; } }
	public float NormalizedShieldStunTime { get { return currentAction == Action.Shielding ? actionTimer / shieldStun : 0.0f; } }
	public float NormalizedShieldReuseTime { get { return shieldTimer / shieldDuration; } }


	/// <summary>
	/// Time left before clearing the body
	/// </summary>
	private float deadClearTimer = 0;
	public bool isAlive { get; private set; }
	public bool IsDead { get { return !isAlive; } }

	public int killCount { get; private set; }
	public int blockCount { get; private set; }
	[System.NonSerialized]
	public int roundWinCount;

	/// <summary>
	/// Total score to display for this character
	/// </summary>
	public int TotalScore { get { return roundWinCount * 20 + killCount * 5 + blockCount; } }


	void Start ()
    {
        characterController = GetComponent<CharacterController>();
		isAlive = true;
		roundWinCount = 0;

		// Create shield
		if (currentShield == null)
		{
			currentShield = Instantiate(defaultShield);
			currentShield.owner = this;
		}
		Respawn();
	}

	void OnDestroy()
	{
		if (currentShield != null)
			Destroy(currentShield.gameObject);

		if (currentProjectile != null)
			Destroy(currentProjectile.gameObject);
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

		// Prevent movement while initially spawned in
		if (spawnTimer != 0.0f)
		{
			spawnTimer -= Time.deltaTime;
			if (spawnTimer < 0.0f)
				spawnTimer = 0.0f;
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
		if (actionTimer != 0)
		{
			if (currentAction == Action.Shooting)
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
			}

			// Wait for action to be finished
			actionTimer -= Time.deltaTime;
			if (actionTimer < 0.0f)
			{
				actionTimer = 0.0f;
				currentAction = Action.None;
			}
		}

		// Count down to re-use shield
		if (shieldTimer > 0.0f)
		{
			shieldTimer -= Time.deltaTime;
			if (shieldTimer < 0.0f)
				shieldTimer = 0.0f;
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
		if(currentAction == Action.None && isAlive && spawnTimer == 0.0f)
			deltaMovement += amount * moveSpeed;
    }

    /// <summary>
    /// Turn the character by some amount (+: right, -:left)
    /// </summary>
    /// <param name="amount">+: right, -:left</param>
    public void Turn(float amount)
    {
		if(isAlive && spawnTimer == 0.0f)
			deltaAngle += amount * turnSpeed;
    }

    /// <summary>
    /// Attempt to fire an arrow
    /// </summary>
    /// <returns>If arrow successfully fires</returns>
    public bool Fire()
    {
		if (currentAction != Action.None || IsDead || spawnTimer != 0.0f)
			return false;

		arrowTimer = shootDuration * 0.8f;
		actionTimer = shootDuration;
		currentAction = Action.Shooting;
		return true;
    }

	/// <summary>
	/// Attempt to deploy shield
	/// </summary>
	/// <returns>If shield successfully deploys</returns>
	public bool Block()
	{
		if (currentAction != Action.None || IsDead || shieldTimer > 0.0f || spawnTimer != 0.0f)
			return false;

		currentShield.Deploy();
		actionTimer = shieldStun;
		shieldTimer = shieldDuration;
		currentAction = Action.Shielding;
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
	/// Called when this character has successfully blocked a shot
	/// </summary>
	public void OnGoodBlock(ArrowProjectile arrow)
	{
		blockCount++;
	}


	/// <summary>
	/// Callback for when this character dies
	/// </summary>
	public void OnDead()
	{
		if (!isAlive)
			return;

		currentShield.gameObject.SetActive(false);
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
		currentShield.gameObject.SetActive(false);

		actionTimer = 0.0f;
		currentAction = Action.None;
		arrowTimer = 0.0f;
		shieldTimer = 0.0f;
		spawnTimer = 1.0f;
	}


	/// <summary>
	/// Set the colour for this character
	/// </summary>
	/// <param name="colour"></param>
	public void SetColour(Color colour)
	{
		// Colour model
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
			if (!renderer.gameObject.name.EndsWith("(Colourless)"))
				renderer.material.color = colour;


		// Create shield (If doesn't already exist)
		if (currentShield == null)
		{
			currentShield = Instantiate(defaultShield);
			currentShield.owner = this;
		}

		// Colour sheild
		if (!currentShield.name.EndsWith("(Colourless)"))
			currentShield.GetComponent<Renderer>().material.color = colour;
	}
}
