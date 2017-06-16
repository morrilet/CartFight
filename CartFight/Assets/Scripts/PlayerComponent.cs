using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class will be attached to the driver and cart parts of the player. It calls events that the
/// player is subscribed to when it is hit by something on the appropriate layer.
/// </summary>
public class PlayerComponent : PausableObject 
{
	//public LayerMask layerMask; //The objects that can hit us.
	//Collider2D coll; //The collider attached to this game object.

	public delegate void OnHitAction (Collision2D collision);

	public event OnHitAction OnHitCart; //An event for when we hit a cart.
	public event OnHitAction OnHitDriver; //An event for when we hit a driver.
	public event OnHitAction OnHitObstacle; //An event for when we hit an obstacle.

	public event OnHitAction OnHitCart_Stay;
	public event OnHitAction OnHitDriver_Stay;
	public event OnHitAction OnHitObstacle_Stay;

	public event OnHitAction OnHitItem;

	private List<Collider2D> touching; //The colliders we're currently touching.
	public List<Collider2D> Touching { get { return this.touching; } }

	//Used for pausing.
	private Vector2 storedVelocity;
	private float storedAngularVelocity;

	void Start () 
	{
		//coll = GetComponent<Collider2D> ();
		touching = new List<Collider2D>();
	}

	void Update()
	{
		if (this.GetComponent<Animator> () != null) 
		{
			this.GetComponent<Animator> ().speed = (IsPaused) ? 0 : 1;
		}

		if (this.transform.parent == null) //Abandoned cart.
		{
			if (IsPaused && !IsPausedPrev) //Just paused...
			{
				storedAngularVelocity = this.GetComponent<Rigidbody2D> ().angularVelocity;
				storedVelocity = this.GetComponent<Rigidbody2D> ().velocity;

				this.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
				this.GetComponent<Rigidbody2D> ().angularVelocity = 0.0f;
			} 
			else if (!IsPaused && IsPausedPrev) //Just unpaused...
			{
				this.GetComponent<Rigidbody2D> ().velocity = storedVelocity;
				this.GetComponent<Rigidbody2D> ().angularVelocity = storedAngularVelocity;
			}
		}
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		touching.Add (other.collider);

		//Bit shift operator.
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Cart")) 
		{
			if (OnHitCart != null) 
			{
				//Debug.Log (this.gameObject.name + " has hit " + other.gameObject.name + "!");
				OnHitCart(other);
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Driver")) 
		{
			if (OnHitDriver != null) 
			{
				//Debug.Log (this.gameObject.name + " has hit " + other.gameObject.name + "!");
				OnHitDriver(other);
			}

			//Temporary!!! Hack-y solution. THIS IS CREATING ISSUES!!!
			if (this.transform.parent == null && this.tag == "Cart") //Abandoned cart
			{
				if (this.GetComponent<Rigidbody2D> ().velocity.magnitude >= 10.0f) 
				{
					other.transform.parent.GetComponent<Player> ().Die ();
				}
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Obstacle")) 
		{
			if (OnHitObstacle != null) 
			{
				//Debug.Log (this.gameObject.name + " has hit " + other.gameObject.name + "!");
				OnHitObstacle(other);
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer ("Item")) 
		{
			if (OnHitItem != null)
			{
				//Debug.Log (this.gameObject.name + " has hit " + other.gameObject.name + "!");
				OnHitItem (other);
			}
		}
	}

	void OnCollisionStay2D(Collision2D other)
	{
		//Bit shift operator.
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Cart")) 
		{
			if (OnHitCart_Stay != null) 
			{
				OnHitCart_Stay(other);
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Driver")) 
		{
			if (OnHitDriver_Stay != null)
			{
				OnHitDriver_Stay(other);
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Obstacle")) 
		{
			if (OnHitObstacle_Stay != null) 
			{
				OnHitObstacle_Stay(other);
			}
		}
	}

	void OnCollisionExit2D(Collision2D other)
	{
		touching.Remove (other.collider);
	}
}
