﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Testing...
using UnityEngine.SceneManagement;

public class Player : PausableObject 
{
	[HideInInspector]
	public ControlScheme controlScheme; //The control scheme to use for movement.
	public enum PlayerNumber
	{
		P1, P2, P3, P4, None
	};
	public PlayerNumber playerNumber;

	public float maxVelocity; //How fast we can go.
	public float acceleration; //How much is added to velocity.
	public float deceleration; //How quickly we can manually slow down.
	public float turnSpeed; //How quickly we turn.
	public float dampening; //How quickly we lose velocity.

	private Vector2 velocity;
	public Vector2 Velocity { get { return this.velocity; } }

	//This is here if needed, but it'd be best to handle this in game manager.
	//private int points;
	//public void AddPoints(int value) { points += value; }
	//public int GetPoints() { return points; }

	private Vector3 driverLocalPosition;
	private Vector3 cartLocalPosition;
	private Quaternion cartLocalRotation;

	private GameObject driverObj; //The driver part of the player.
	private PlayerComponent driver; //The player component for the driver.
	private GameObject cartObj; //The cart part of the player.
	private PlayerComponent cart; //The player component for the cart.

	private GameObject cartObjPrev; //The last cart we used.

	//private bool colliding = false; //Whether or not we're in any sort of collision right now.
	private bool isAlive = true;

	[HideInInspector]
	public List<Item> carriedItems; //The items that the player is carrying.

	private Animator driverAnimator; //The animator of the driver component.

	public Sprite[] cartImages;

	public void Start()
	{
		controlScheme.Start ();

		velocity = Vector2.zero;
		carriedItems = new List<Item> ();

		//Get driver and cart components...
		for (int i = 0; i < transform.childCount; i++) 
		{
			if (transform.GetChild (i).name == "Driver") 
			{
				driverObj = transform.GetChild (i).gameObject;
			}
			if (transform.GetChild (i).name == "Cart") 
			{
				cartObj = transform.GetChild (i).gameObject;
			}
		}
		if (driverObj == null)
		{
			Debug.LogError ("The driverObj is missing from the player! Is the name correct?");
		}
		if (cartObj == null)
		{
			Debug.LogError ("The cartObj is missing from the player! Is the name correct?");
		}
		driver = driverObj.GetComponent<PlayerComponent> ();
		cart = cartObj.GetComponent<PlayerComponent> ();
		cartObjPrev = cartObj;

		//Ignore collisions between our two components...
		Physics2D.IgnoreCollision(driverObj.GetComponent<Collider2D>(), cartObj.GetComponent<Collider2D>());

		driverLocalPosition = driverObj.transform.localPosition;
		cartLocalPosition = cartObj.transform.localPosition;
		cartLocalRotation = cartObj.transform.localRotation;

		//Set the driver animation up.
		driverAnimator = driverObj.GetComponent<Animator> ();
		driverAnimator.SetInteger ("PlayerNumber", (int)playerNumber + 1);

		//Set the cart to the right image
		cartObj.GetComponent<SpriteRenderer> ().sprite = cartImages [(int)playerNumber];

		HookupCartEvents ();
		HookupDriverEvents ();
	}

	void Update()
	{
		if (IsPaused)
			return;

		if (isAlive) 
		{
			controlScheme.Update ();

			if (cartObj != null) 
			{
				Move ();
			}
			else
			{
				MoveWithoutCart ();
			}

			if (controlScheme.ThrowKeyDown) 
			{
				if (cartObj != null) 
				{
					RemoveCart ((Vector3)velocity + (transform.right * 20f));
				}
				else //if (driverObj.GetComponent<Collider2D>().IsTouchingLayers(1 << LayerMask.NameToLayer("Cart")))
				{
					Debug.Log ("Trying to add cart...");
					foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Cart"))
					{
						if (obj.transform.parent == null)
						{
							foreach (Collider2D coll in driver.Touching)
							{
								if (coll.gameObject.transform.Equals (obj.transform)) 
								{
									Debug.Log("Cart added! :: " + obj.name);
									if (cartObj == null) //Double check this.
									{
										AddCart (obj);
									}
								}
							}
						}
					}
				}
			}

			//Continually place driver and cart at their starting local positions.
			driverObj.transform.localPosition = driverLocalPosition;
			if (cartObj != null) 
			{
				cartObj.transform.localPosition = cartLocalPosition;
				cartObj.transform.localRotation = cartLocalRotation;
			}

			//Update the drivers animation speed based on our current velocity.
			float clampedSpeed = velocity.magnitude / maxVelocity; //Our speed from 0-1.
			driverAnimator.SetFloat("AnimationSpeed", clampedSpeed);
		}
	}

	//Takes the cart off of the player object, making it a separate entity.
	private void RemoveCart(Vector2 force)
	{
		//Separate the cart from the rest of the player.
		cartObj.gameObject.name = "Cart";
		cartObj.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.None;
		cartObj.transform.SetParent (null);
		cartObj.GetComponent<Rigidbody2D> ().AddForce (force, ForceMode2D.Impulse);
		PutItemsInCart (cartObj); //Place all carried items into the cart.
		UnhookCartEvents ();

		//Ignore collisions with items and the cart.
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Item")) 
		{
			Physics2D.IgnoreCollision (cartObj.GetComponent<Collider2D> (), obj.GetComponent<Collider2D> (), true);
		}
		//Don't ignore collisions between the player and the cart.
		Physics2D.IgnoreCollision (driverObj.GetComponent<Collider2D>(), cartObj.GetComponent<Collider2D>(), false);

		//Empty our references.
		cartObj = null;
		cart = null;
	}

	//Adds a cart to this player object.
	private void AddCart(GameObject cartObj)
	{
		//Attach the cart to the rest of the player.
		this.cartObj = cartObj;
		this.cart = cartObj.GetComponent<PlayerComponent> ();
		this.cartObj.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
		this.cartObj.transform.SetParent (this.transform);
		this.cartObjPrev = this.cartObj;
		TakeItemsFromCart (cartObj); ///Get all the objects from the cart.
		HookupCartEvents ();

		//Stop ignoring collisions between the cart and items.
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Item"))
		{
			Physics2D.IgnoreCollision (cart.GetComponent<Collider2D> (), obj.GetComponent<Collider2D> (), false);
		}
		//Ignore collisions between the player and the cart.
		Physics2D.IgnoreCollision (driverObj.GetComponent<Collider2D> (), this.cartObj.GetComponent<Collider2D> ());
	}

	//Places our currently carried items into a cart.
	private void PutItemsInCart(GameObject cartObj)
	{
		//++i makes no copy of i and is faster. No bugs because of the limited use case here.
		for (int i = 0; i < carriedItems.Count; ++i) 
		{
			carriedItems [i].GetDropped ();
			carriedItems [i].GetPlacedInCart (cartObj);
		}
		carriedItems = new List<Item> ();
	}

	//Takes the items from a cart and places them into the carried object list.
	private void TakeItemsFromCart(GameObject cartObj)
	{
		Item[] cartItems = cartObj.GetComponentsInChildren<Item> ();
		for (int i = 0; i < cartItems.Length; ++i) 
		{
			cartItems [i].GetRemovedFromCart ();
			PickupItem (cartItems [i]);
		}
	}

	//Adds an item to the carried items list.
	private void PickupItem(Item item)
	{
		item.GetPickedUpByPlayer (this.GetComponent<Player> ());

		item.followDistance = 2.5f + (1.25f * carriedItems.Count);
		item.speed = 4.0f / (1.0f + carriedItems.Count);

		carriedItems.Add (item);

		AudioManager.instance.PlayEffect ("PickupFood");
	}

	public void Die()
	{
		isAlive = false;

		//Unhook the player from the player components...
		//(Not doing this for cart because it's handled in remove cart.)
		UnhookDriverEvents ();

		//Remove all collected objects.
		Debug.Log(carriedItems.Count);
		for (int i = 0; i < carriedItems.Count; i++)
		{
			Debug.Log (carriedItems[i].gameObject.name);
			carriedItems [i].GetDropped ();
		}
		carriedItems.RemoveRange (0, carriedItems.Count);

		GameManager.instance.SpawnPlayer (playerNumber, controlScheme, 5f);
		if (cartObj != null)
		{
			RemoveCart (transform.right * 5.0f);
		}

		//Remove the driver and start the death animation. Driver is destroyed after this.
		driverObj.GetComponent<Animator> ().SetBool ("isAlive", false);
		driverObj.GetComponent<Animator> ().speed = 1.0f;
		driverObj.GetComponent<SpriteRenderer> ().sortingOrder = -1;
		driverObj.AddComponent<FadeOutAndDestroy> ();
		driverObj.GetComponent<Collider2D> ().enabled = false;
		driverObj.transform.SetParent (null);

		driverObj.GetComponent<SpriteRenderer>().sortingLayerName = "DeadPlayers";

		Destroy (this.gameObject);
		//Destroy (this.gameObject, 3f);
	}

	public void HitItem(Collision2D other)
	{
		Debug.Log ("Hit item.");
		if(!other.gameObject.GetComponent<Item>().isPickedUp() && this.cartObj != null)
		{
			PickupItem (other.gameObject.GetComponent<Item> ());
		}
	}

	public void HitCart(Collision2D other)
	{
		//Debug.Log ("~Event triggered :: Hit a cart~");

		//Push the other cart depending on whether or not it's being used by a player.
		if (other.transform.parent != null) //Cart is in use...
		{
			if (other.transform.parent.gameObject.GetComponent<Player> () != null) 
			{
				other.transform.parent.GetComponent<Player> ().velocity -= velocity * 0.25f;
			}
				
			//Reflect current velocity with little dampening.
			velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.75f;
		}
		else //Cart is abandoned...
		{
			other.gameObject.GetComponent<Rigidbody2D> ().AddForceAtPosition (
				velocity / 2.0f, other.contacts [0].point, ForceMode2D.Impulse);
		}

		AudioManager.instance.PlayEffect ("CartHit");
	}

	public void HitDriver(Collision2D other)
	{
		if (driverObj == null || !other.transform.Equals (driverObj.transform))
		{
			Debug.Log ("~Event triggered :: Hit a driver~ Called By : " + transform.name);
			//Debug.Log(other.gameObject.name);
			//Debug.DrawLine (other.contacts[0].point, other.gameObject.transform.position, Color.black);
			float impactForce = 0.0f;
			if (cartObj != null) 
			{
				impactForce = (velocity - other.transform.parent.GetComponent<Player> ().velocity).magnitude;
				Debug.LogFormat ("Player Velocity :: {0} --- Cart Velocity :: {1}", 
					velocity, cartObj.GetComponent<Rigidbody2D> ().velocity);
			}
			else
			{
				impactForce = (cartObjPrev.GetComponent<Rigidbody2D> ().velocity - 
					other.transform.parent.GetComponent<Player> ().velocity).magnitude;
				Debug.LogFormat ("Player Velocity :: {0} --- Cart Velocity :: {1}", 
					velocity, cartObjPrev.GetComponent<Rigidbody2D> ().velocity);
			}

			//Reflect current velocity with moderate dampening.
			velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.5f;

			//If we're going fast enough, kill the other driver.
			if (impactForce >= 10f) 
			{
				//Debug.Log ("-----Driver killed-----");
				Debug.LogFormat ("{0} killed {1}", playerNumber.ToString (), 
					other.transform.parent.gameObject.GetComponent<Player> ().playerNumber.ToString ());

				//Kill the driver.
				other.transform.parent.gameObject.GetComponent<Player> ().Die ();
			}
		}
	}

	public void HitObstacle(Collision2D other)
	{
		//Debug.Log ("~Event triggered :: Hit an obstacle~");
		//colliding = true;

		//Reflect current velocity with low dampening.
		//velocity = -velocity * (Mathf.Abs(Vector2.Dot(transform.right, other.contacts[0].normal)));
		velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.75f;
		//	* (1 - Mathf.Abs(Vector2.Dot(transform.right, other.contacts[0].normal)));

		//HandleCollision (other, 1 << LayerMask.NameToLayer("Obstacle"));
	}

	public void HitObstacle_Stay(Collision2D other)
	{
		//Debug.Log ("~Event triggered :: Hit an obstacle (stay)~");
		HandleCollisionNew (other, 1 << LayerMask.NameToLayer("Obstacle"));
	}

	public void HitCart_Stay(Collision2D other)
	{
		HandleCollisionNew (other, 1 << LayerMask.NameToLayer ("Cart"));
	}

	public void HitDriver_Stay(Collision2D other)
	{
		HandleCollisionNew (other, 1 << LayerMask.NameToLayer ("Driver"));
	}

	//Keeps us from passing into another object. The method we use is as follows:
	//1 :: Check that we're inside the object.
	//2 :: If yes, get the normal of the surface we're colliding with.
	//3 :: Get a point outside of the collider we're inside of, along the normal.
	//4 :: Cast a ray from the outside point toward the object, along the normal.
	//     This gets the point on the other objects edge that we want to move the 
	//     intruding part to (called the nearest point.)
	//5 :: Move the appropriate distance in the appropriate direction in order to
	//     get outside of the object, and set velocity to that same value so that
	//     the movement is less jerky.
	private void HandleCollisionNew(Collision2D other, LayerMask mask)
	{
		if (other.collider.bounds.Contains (other.contacts [0].point)) 
		{
			Vector2 normal = other.contacts [0].normal;
			Vector2 pointOutsideCollider = (Vector2)other.contacts[0].point + (normal.normalized * 5);

			Vector2 nearestPoint = Vector2.zero;
			RaycastHit2D[] hits = Physics2D.RaycastAll(pointOutsideCollider, -normal, float.PositiveInfinity);
			for (int i = 0; i < hits.Length; i++) 
			{
				if (hits [i].transform.Equals (other.transform)) 
				{
					//DrawCross (hits [i].point, .25f, Color.green);
					nearestPoint = hits[i].point;
				}
			}

			//For debugging...
			DrawCross (nearestPoint, .25f, Color.white);
			DrawCross (pointOutsideCollider, .25f, Color.red);
			DrawCross (other.contacts [0].point, .25f, Color.blue);
			Debug.DrawRay (other.contacts [0].point, normal);
			//Debug.Break ();

			float distance = Vector2.Distance (other.contacts [0].point, nearestPoint);

			Vector3 offset = transform.position;
			velocity = distance * normal.normalized;
			offset += distance * (Vector3)normal.normalized;
			transform.position = offset;
		}
	}

	void OnDrawGizmos()
	{
		//Gizmos.color = Color.green;
		//Gizmos.DrawSphere (transform.position, 0.25f);
	}

	public void MoveWithoutCart()
	{
		//Pull the input from our desired control scheme.
		Vector2 input = new Vector2 (controlScheme.Horizontal, controlScheme.Vertical);

		if (controlScheme.IsGamePad && 
			controlScheme.GamepadControls != ControlScheme.GamepadControlStick.LEFT) 
		{
			controlScheme.GamepadControls = ControlScheme.GamepadControlStick.LEFT;
		}

		//Debugging...
		if (controlScheme.IsGamePad) 
		{
			Debug.Log ("Controller inputs... " + input);
		}

		//Keyboard...
		if(!controlScheme.IsGamePad)
		{
			if (input.x != 0 && !controlScheme.IsGamePad) ///Turning...
			{
				transform.rotation = Quaternion.Euler (transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,
					transform.rotation.eulerAngles.z + (-input.x * (turnSpeed * 2.0f)) * Time.deltaTime);
			}
				
			if (input.y != 0) //Acceleration/deceleration
			{
				velocity = (maxVelocity / 1.5f) * transform.right * input.normalized.y;
			}
			else 
			{
				velocity *= dampening / 5.0f;
			}
		}
		else
		{
			if (input.x != 0 || input.y != 0) 
			{
				//Get direction of input...
				Vector2 dir = (((Vector2)transform.position + input) - (Vector2)transform.position).normalized;
				Debug.DrawLine (transform.position, transform.position + (Vector3)dir);

				//Point the character toward the direction with our turn speed(?)...
				float angle = Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg;
				Quaternion newRot = Quaternion.AngleAxis (angle, Vector3.forward);
				transform.rotation = Quaternion.RotateTowards (transform.rotation, newRot, turnSpeed);

				//Trim the rotation to only work on the Z axis...
				transform.eulerAngles = new Vector3 (0.0f, 0.0f, transform.rotation.eulerAngles.z);

				//Handle movement towards the current input direction...
				velocity = (maxVelocity / 1.5f) * input;
			}
			else
			{
				velocity *= dampening / 5.0f;
			}
		}
			
		//Debug.Log (input.normalized);
		//velocity = (maxVelocity / 1.5f) * input.normalized;
		//if (velocity.magnitude != 0)
		//	transform.rotation = Quaternion.AngleAxis(Mathf.Atan2 (velocity.y, velocity.x) * Mathf.Rad2Deg, transform.forward);

		ClampVelocity ();

		Debug.DrawRay (transform.position, velocity.normalized, Color.red);
		Debug.DrawRay (transform.position, transform.right * 2f, Color.yellow);

		transform.position = transform.position + (Vector3)velocity * Time.deltaTime;
	}

	public void Move()
	{
		//Pull the input from our desired control scheme.
		Vector2 input = new Vector2 (controlScheme.Horizontal, controlScheme.Vertical);

		if (controlScheme.IsGamePad && 
			controlScheme.GamepadControls != ControlScheme.GamepadControlStick.BOTH) 
		{
			controlScheme.GamepadControls = ControlScheme.GamepadControlStick.BOTH;
		}

		//Turning
		if (input.x != 0)
		{
			transform.rotation = Quaternion.Euler (transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,
				transform.rotation.eulerAngles.z + (-input.x * turnSpeed) * Time.deltaTime);
		}

		//Basic acceleration/deceleration
		if (input.y != 0) 
		{
			if (input.y > 0) //Forward
			{ 
				velocity += input.y * acceleration * (Vector2)transform.right;
			} 
			else if (input.y < 0) //Backward
			{ 
				velocity -= input.y * deceleration * -(Vector2)transform.right;
			}
		}
		else 
		{
			velocity *= dampening;
		}

		ClampVelocity ();

		Debug.DrawRay (transform.position, velocity.normalized, Color.red);
		Debug.DrawRay (transform.position, transform.right * 2f, Color.yellow);
			
		//Redirect velocity towards forward vector. Checks what side of transform.right (yes, we use transform.up)
		//velocity is on, then directs velocity toward transform.right based on the shortest path.
		//Only do this if we're not going backwards...
		if (Vector2.Angle(velocity, transform.right) <= 100)
		{
			//Dividing the angle by a lower number will make turns less bank-y.
			if (Vector2.Dot (velocity, -transform.up) > 0) 
			{
				velocity = RotateVector (velocity, (Vector2.Angle (transform.right, velocity) / 20f) * Mathf.Deg2Rad);
			} 
			else if (Vector2.Dot (velocity, -transform.up) < 0) 
			{
				velocity = RotateVector (velocity, -(Vector2.Angle (transform.right, velocity) / 20f) * Mathf.Deg2Rad);
			}
		}
		//Debug.Log(Vector2.Angle(velocity, transform.right));

		transform.position = transform.position + (Vector3)velocity * Time.deltaTime;
	}

	//Keeps velocity within the set bounds.
	private void ClampVelocity()
	{
		if (velocity.x > maxVelocity) 
		{
			velocity.x = maxVelocity;
		}
		else if (velocity.x < -maxVelocity) 
		{
			velocity.x = -maxVelocity;
		}
		if (velocity.y > maxVelocity) 
		{
			velocity.y = maxVelocity;
		}
		else if (velocity.y < -maxVelocity) 
		{
			velocity.y = -maxVelocity;
		}
	}

	//Rotates a vector by an angle.
	private Vector2 RotateVector(Vector2 inputVector, float degrees)
	{
		Vector2 output = Vector2.zero;
		output.x = inputVector.x * Mathf.Cos (degrees) - inputVector.y * Mathf.Sin (degrees);
		output.y = inputVector.x * Mathf.Sin (degrees) + inputVector.y * Mathf.Cos (degrees);
		return output;
	}

	private void HookupCartEvents ()
	{
		cart.OnHitCart += HitCart;
		cart.OnHitObstacle += HitObstacle;
		cart.OnHitDriver += HitDriver;

		cart.OnHitObstacle_Stay += HitObstacle_Stay;
		cart.OnHitDriver_Stay += HitDriver_Stay;
		cart.OnHitCart_Stay += HitCart_Stay;

		cart.OnHitItem += HitItem;
	}

	private void HookupDriverEvents ()
	{
		//Driver handles everything as an obstacle collision.
		driver.OnHitObstacle += HitObstacle;
		driver.OnHitDriver += HitObstacle;
		//driver.OnHitCart += HitObstacle; //This may have been causing some driver pushing issues (5/16/17)

		driver.OnHitObstacle_Stay += HitObstacle_Stay;
		driver.OnHitDriver_Stay += HitDriver_Stay;
		driver.OnHitCart_Stay += HitCart_Stay;

		driver.OnHitItem += HitItem;
	}

	private void UnhookCartEvents ()
	{
		cart.OnHitObstacle -= HitObstacle;
		cart.OnHitDriver -= HitDriver;
		cart.OnHitCart -= HitObstacle;

		cart.OnHitObstacle_Stay -= HitObstacle_Stay;
		cart.OnHitDriver_Stay -= HitDriver_Stay;
		cart.OnHitCart_Stay -= HitCart_Stay;

		cart.OnHitItem -= HitItem;
	}

	private void UnhookDriverEvents ()
	{
		driver.OnHitObstacle -= HitObstacle;
		driver.OnHitDriver -= HitObstacle;
		driver.OnHitCart -= HitObstacle;

		driver.OnHitObstacle_Stay -= HitObstacle_Stay;
		driver.OnHitDriver_Stay -= HitDriver_Stay;
		driver.OnHitCart_Stay -= HitCart_Stay;

		driver.OnHitItem -= HitItem;
	}

	//Draws a cross on a point for debugging.
	private void DrawCross(Vector3 point, float size, Color color)
	{
		//Vertical...
		Debug.DrawLine(new Vector3(point.x, point.y - size, 0), new Vector3(point.x, point.y + size, 0), color);
		//Horizontal...
		Debug.DrawLine(new Vector3(point.x - size, point.y, 0), new Vector3(point.x + size, point.y, 0), color);
	}
}