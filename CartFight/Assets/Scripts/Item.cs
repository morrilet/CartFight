﻿using UnityEngine;
using System.Collections;

public class Item : PausableObject
{
	public enum ItemType
	{
		Meat, Veggie, Cheese
	};
	public ItemType itemType;

	public Sprite[] itemImages;

	//Perhaps make there
	[HideInInspector]
	public float followDistance;
	[HideInInspector]
	public float speed;

	private bool pickedUp = false;
	private Player player = null; //The player that's carrying us.

	public bool isPickedUp() { return pickedUp; }

	public int pointValue; //Points to add to the player at an interval.
	public float pointTime; //How long between adding points.
	private float pointTimer;

	void Start()
	{
		GetComponent<SpriteRenderer> ().sprite = itemImages [(int) itemType];
	}

	void Update()
	{
		if (IsPaused)
			return;

		if (pickedUp)
		{
			FollowPlayer ();

			if (pointTimer >= pointTime)
			{
				//Disabled for now to migrate this to game manager.
				//GameManager.instance.AddPointsToPlayer (this.player, pointValue);
				pointTimer = 0.0f;
			}
			pointTimer += Time.deltaTime;
		} 
		else 
		{
			pointTimer = 0.0f;
		}
	}

	//Get picked up.
	public void GetPickedUpByPlayer(Player player)
	{
		pickedUp = true;
		this.player = player;

		GetComponent<Collider2D> ().enabled = false;
	}

	//Get dropped.
	public void GetDropped()
	{
		//In the future, maybe throw the object in a direction.
		pickedUp = false;
		player = null;

		GetComponent<Collider2D> ().enabled = true;
	}

	//Get placed in a cart.
	public void GetPlacedInCart(GameObject cartObj)
	{
		pickedUp = false;
		player = null;

		transform.SetParent (cartObj.transform);
		transform.position = cartObj.transform.position;

		GetComponent<Collider2D> ().enabled = false;
	}

	//Get removed from a cart.
	public void GetRemovedFromCart()
	{
		pickedUp = false;
		player = null;

		transform.SetParent (null);

		GetComponent<Collider2D> ().enabled = true;
	}

	void FollowPlayer()
	{
		float distance = Vector3.Distance (player.transform.position, transform.position);

		//Handle follow distance based on how many objects are in players cart. 0(X|.|.) => follow distance 1.
		if (distance > followDistance) 
		{
			Vector3 newPos = Vector3.Lerp (transform.position, player.transform.position, speed * Time.deltaTime);
			transform.position = newPos;
		}
		/*
		else if (distance < 2.0f) //Move to one unit away from the player.
		{
			Vector3 dir = (transform.position - player.transform.position).normalized;
			Vector3 newPos = Vector3.Lerp (transform.position, dir, Time.deltaTime);
			transform.position = newPos;
		}
		*/
		/*
		if(distance < 4.5f)
		{
			transform.RotateAround (player.transform.position, Vector3.forward, 15f * Time.deltaTime);
		}
		*/
	}
}
