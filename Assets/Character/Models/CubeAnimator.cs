using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class CubeAnimator : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private Character character;
    

	void Start ()
    {
        animator = GetComponent<Animator>();
    }
	
	void Update ()
    {
        float dir = 1.0f;
        if (Vector2.Dot(character.velocity, character.direction) < 0.0f)
            dir = -1.0f;

        float speed = Mathf.Clamp01(character.velocity.magnitude);

        animator.SetFloat("AbsSpeed", speed);
        animator.SetFloat("Speed", speed * dir);
    }
}
