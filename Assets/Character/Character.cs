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



    void Start ()
    {
        characterController = GetComponent<CharacterController>();
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
    }

    /// <summary>
    /// Move the character by some amount (+: forward, -:back)
    /// </summary>
    /// <param name="amount">+: forward, -:back</param>
    public void Move(float amount)
    {
        deltaMovement += amount * moveSpeed;
    }

    /// <summary>
    /// Turn the character by some amount (+: right, -:left)
    /// </summary>
    /// <param name="amount">+: right, -:left</param>
    public void Turn(float amount)
    {
        deltaAngle += amount * turnSpeed;
    }

    /// <summary>
    /// Attempt to fire an arrow
    /// </summary>
    /// <returns>If arrow successfully fires</returns>
    public bool Fire()
    {
        ArrowProjectile projectile = Instantiate(defaultProjectile.gameObject).GetComponent<ArrowProjectile>();
        projectile.Fire(this);
        return true;
    }
}
