using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shield which can be spawned in to protect a player
/// </summary>
public class ArrowShield : MonoBehaviour
{
	[System.NonSerialized]
	public Character owner;

	[SerializeField]
	private float deployDuration = 1.5f;

	/// <summary>
	/// Is this shield currently active
	/// </summary>
	public bool IsActive { get; private set; }

	/// <summary>
	/// How long until this shield is destroyed
	/// </summary>
	private float lifeTimer;


	void Start()
	{
		gameObject.SetActive(false);
		IsActive = false;
	}

	void Update()
	{
		if (lifeTimer > 0.0f)
		{
			lifeTimer -= Time.deltaTime;

			if (lifeTimer < 0.0f)
			{
				lifeTimer = 0.0f;
				gameObject.SetActive(false);
				IsActive = false;
			}
		}
	}

	public void Deploy()
	{
		transform.parent = null;
		transform.position = owner.transform.position;
		lifeTimer = deployDuration;
		gameObject.SetActive(true);
		IsActive = true;
	}

	void OnTriggerEnter(Collider collider)
	{
		ArrowProjectile arrow = collider.gameObject.GetComponent<ArrowProjectile>();
		if (arrow != null)
		{
			owner.OnGoodBlock(arrow);
			//gameObject.SetActive(false);
			//IsActive = false;
			Destroy(collider.gameObject);
		}
	}
}
