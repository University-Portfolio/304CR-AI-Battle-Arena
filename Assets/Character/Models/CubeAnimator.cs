using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class CubeAnimator : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private Character character;

    private int tag_AbsSpeed;
    private int tag_Speed;
    private int tag_IsFiring;


    void Start ()
    {
        animator = GetComponent<Animator>();

        tag_AbsSpeed = Animator.StringToHash("AbsSpeed");
        tag_Speed = Animator.StringToHash("Speed");
        tag_IsFiring = Animator.StringToHash("IsFiring");
    }
	
	void Update ()
    {
        float dir = 1.0f;
        if (Vector2.Dot(character.velocity, character.direction) < 0.0f)
            dir = -1.0f;

        float speed = Mathf.Clamp01(character.velocity.magnitude);

        animator.SetFloat(tag_AbsSpeed, speed);
        animator.SetFloat(tag_Speed, speed * dir);

        animator.SetBool(tag_IsFiring, false);
    }
}
