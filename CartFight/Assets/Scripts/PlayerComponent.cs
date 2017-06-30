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

	public bool invulnerable;
	public bool collidesWithObstacles;

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
			//Ignore collisions with anything that's invulnerable.
			//PlayerComponent[] invulnerableComponents = GameObject.fin

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

	IEnumerator OnCollisionEnter2D(Collision2D other)
	{
		touching.Add (other.collider);

		//This is a good idea I think, try to keep it in with the upcoming refactoring. (6/27/17)
		if (other.gameObject.tag == "Invulnerable")
			yield return null;

		if (other.contacts.Length == 0)
			yield return null;

		//yield return new WaitForEndOfFrame ();

		//Bit shift operator.
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Cart")) 
		{
			if (OnHitCart != null) 
			{
				if (!invulnerable && !other.gameObject.GetComponent<PlayerComponent> ().invulnerable) 
				{
					Debug.Log (this.gameObject.name + " has hit " + other.gameObject.name + "!");
					OnHitCart (other);
				}
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Driver")) 
		{
			if (OnHitDriver != null) 
			{
				if (!invulnerable && !other.gameObject.GetComponent<PlayerComponent> ().invulnerable) 
				{
					//Debug.Log (this.gameObject.name + " has hit " + other.gameObject.name + "!");
					OnHitDriver (other);
				}
			}

			//Temporary!!! Hack-y solution. THIS IS CREATING ISSUES!!!
			if (this.transform.parent == null && this.tag == "Cart") //If we're an abandoned cart
			{
				if (this.GetComponent<Rigidbody2D> ().velocity.magnitude >= 10.0f) //If we're going fast enough
				{
					//If the other object has a player (not abandoned)
					if (other.transform.parent != null && other.transform.parent.GetComponent<Player> () != null)
					{
						//If neither of us are invulnerable
						if (!invulnerable && !other.gameObject.GetComponent<PlayerComponent> ().invulnerable) 
						{
							other.transform.parent.GetComponent<Player> ().Die ();
						}
					}
				}
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Obstacle")) 
		{
			if (OnHitObstacle != null && collidesWithObstacles) 
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

		yield return new WaitForEndOfFrame ();
	}

	IEnumerator OnCollisionStay2D(Collision2D other)
	{
		if (other.gameObject.tag == "Invulnerable")
			yield return null;

		//yield return new WaitForFixedUpdate ();

		//Bit shift operator.
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Cart")) 
		{
			if (OnHitCart_Stay != null) 
			{
				if (!invulnerable && !other.gameObject.GetComponent<PlayerComponent> ().invulnerable) 
				{
					if (transform.parent != null) //If we're not an abandoned cart.
					{
						OnHitCart_Stay (other);
					}
				}
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Driver")) 
		{
			if (OnHitDriver_Stay != null)
			{
				if (!invulnerable && !other.gameObject.GetComponent<PlayerComponent> ().invulnerable)
				{
					if (transform.parent != null) //If we're not an abandoned cart.
					{
						OnHitDriver_Stay (other);
					}
				}
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Obstacle")) 
		{
			if (OnHitObstacle_Stay != null && collidesWithObstacles) 
			{
				if (transform.parent != null) //If we're not an abandoned cart.
				{
					OnHitObstacle_Stay (other);
				}
			}
		}
	}

	void OnCollisionExit2D(Collision2D other)
	{
		touching.Remove (other.collider);
	}

	public void UnhookEvents()
	{
		OnHitCart = null;
		OnHitDriver = null;
		OnHitObstacle = null;

		OnHitCart_Stay = null;
		OnHitDriver_Stay = null;
		OnHitObstacle_Stay = null;
	}
}
