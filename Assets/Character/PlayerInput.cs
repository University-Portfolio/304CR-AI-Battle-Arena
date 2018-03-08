using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Character))]
public class PlayerInput : MonoBehaviour
{
    public Character character { get; private set; }


	void Start ()
    {
        character = GetComponent<Character>();
	}
	
	void Update ()
    {
        if (Input.GetKey(KeyCode.W))
            character.Move(1);
        if (Input.GetKey(KeyCode.S))
            character.Move(-1);

        if (Input.GetKey(KeyCode.A))
            character.Turn(-1);
        if (Input.GetKey(KeyCode.D))
            character.Turn(1);

        if (Input.GetKeyDown(KeyCode.Space))
            character.Fire();
    }
}
