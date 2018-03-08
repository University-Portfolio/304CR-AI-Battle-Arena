using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class ArrowProjectile : MonoBehaviour
{
    private Rigidbody _body;
    public Rigidbody body
    {
        get
        {
            if (_body == null)
                _body = GetComponent<Rigidbody>();
            return _body;
        }
    }

    [SerializeField]
    private float speed = 10.0f;

    private Character owner;
    private bool inFlight = false;

	
	void Update ()
    {
        if(inFlight)
            transform.forward = body.velocity;
    }

    public void Fire(Character archer)
    {
        owner = archer;
        Vector2 aim = archer.direction;

        body.velocity = new Vector3(aim.x * speed, 5.0f, aim.y * speed);
        transform.position = archer.transform.position + new Vector3(aim.x, 0, aim.y) * 1.0f;
        inFlight = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Stop arrow
        if (collision.gameObject.CompareTag("Stage"))
        {
            inFlight = false;
            GetComponent<Collider>().enabled = false;
            body.isKinematic = true;
        }
    }
}
