using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour
{
    public CharacterController characterController { get; private set; }

    [SerializeField]
    private ArrowProjectile defaultProjectile;

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
    private float directionAngle = 0.0f;

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
	

	public bool isAlive { get; private set; }
	public bool IsDead { get { return !isAlive; } }



    void Start ()
    {
        characterController = GetComponent<CharacterController>();
		isAlive = true;

	}

    void Update()
    {
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
					ArrowProjectile projectile = Instantiate(defaultProjectile.gameObject).GetComponent<ArrowProjectile>();
					projectile.Fire(this);
					arrowTimer = 0;
				}
			}

			// Wait for shooting to be finished
			shootTimer -= Time.deltaTime;
			if (shootTimer < 0)
				shootTimer = 0;
		}

		// Kill this character
		if (transform.position.y < -1.0f)
			isAlive = false;
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
	}
	
	/// <summary>
	/// Called when this character has been shot by another character
	/// </summary>
	/// <param name="attacker">The character who shot this</param>
	public void OnBeenShot(Character attacker)
	{
		characterController.enabled = false;
		isAlive = false;
	}
}
