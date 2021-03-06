﻿using UnityEngine;
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

    //The last known non-null parent player. 
    //Used to keep track of previous owners of abandoned carts.
    private Player storedPlayer;

    public delegate void OnHitAction (Collision2D collision);

	public event OnHitAction OnHitCart; //An event for when we hit a cart.
	public event OnHitAction OnHitDriver; //An event for when we hit a driver.
	public event OnHitAction OnHitObstacle; //An event for when we hit an obstacle.

	public event OnHitAction OnHitCart_Stay;
	public event OnHitAction OnHitDriver_Stay;
	public event OnHitAction OnHitObstacle_Stay;

	public event OnHitAction OnHitItem;

	private List<Collider2D> touching; //The triggers we're currently touching.
	public List<Collider2D> Touching { get { return this.touching; } }

	public bool invulnerable;
	public bool collidesWithObstacles;

	//Used for pausing.
	private Vector2 storedVelocity;
	private float storedAngularVelocity;

    private float safetyTimer = 0.0f;
    private float safetyTime = 0.1f;

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

        if(this.transform.parent != null)
        {
            storedPlayer = this.transform.parent.GetComponent<Player>();
            safetyTimer = 0.0f;
        }
        else
        {
            if (safetyTimer <= safetyTime)
            {
                safetyTimer += Time.deltaTime;
            }
        }

        //Filter the touching list...
        for (int i = 0; i < touching.Count; i++)
        {
            if (!GetComponent<Collider2D>().IsTouching(touching[i]))
            {
                touching.Remove(touching[i]);
            }
        }

        if (this.transform.parent == null) //Abandoned cart.
		{
			//Ignore collisions with anything that's invulnerable.
			//PlayerComponent[] invulnerableComponents = GameObject.fin
            //(4/6/2018) What...? Gameobject.fin? God, sometimes I fucking hate you, past Ethan.

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

	//The trigger methods are only used by carts, and allow 
	//for an easier cart grab for the player.
	private void OnTriggerEnter2D(Collider2D other)
	{
		touching.Add (other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		touching.Remove (other);
	}

	IEnumerator OnCollisionEnter2D(Collision2D other)
	{
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
					//Debug.Log (this.gameObject.name + " has hit " + other.gameObject.name + "!");
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
					//If the other object has a player (not an abandoned cart)
					if (other.transform.parent != null && other.transform.parent.GetComponent<Player> () != null)
					{
						//If neither of us are invulnerable
						if (!invulnerable && !other.gameObject.GetComponent<PlayerComponent> ().invulnerable) 
						{
                            //If we're not using soulbound carts, just kill the player.
                            //If we're using soulbound carts then we kill the player if...
                            //    -We aren't their souldbound cart
                            //    OR
                            //    -We are their soulbound cart AND they're not calling us.
                            if (!GameManager.instance.Settings.UseSoulboundCarts
                                || !other.transform.parent.GetComponent<Player>().SoulboundCart.Equals(this.gameObject)
                                || (other.transform.parent.GetComponent<Player>().SoulboundCart.Equals(this.gameObject)
                                    && !other.transform.parent.GetComponent<Player>().AttractingCart))
                            {
                                //Check to make sure our cart can't kill us for a short time after it's thrown.
                                //This, as well as the safetyTime(r) were added to fix a bug that causes a thrown cart
                                //to instantly kill its thrower. (4/13/2018)
                                if (other.gameObject.transform.parent.gameObject != storedPlayer.gameObject 
                                    || (safetyTimer >= safetyTime))
                                {
                                    //Debug.Log("HERE DEAD! Stored player is " + storedPlayer.playerNumber);
                                    other.transform.parent.GetComponent<Player>().Die(storedPlayer);
                                }
                            }
						}
					}
				}
			}
		}

		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Obstacle")) 
		{
			if (OnHitObstacle != null && collidesWithObstacles) 
			{
				//Spark and shake effects if we're a cart.
				if (this.tag == "Cart") 
				{
					//If we're attached to a player and going more than half of our max speed...
					if (this.transform.parent != null 
						&& this.transform.parent.GetComponent<Player>().Velocity.magnitude 
						>= this.transform.parent.GetComponent<Player>().maxVelocity / 2f) 
					{
                        AudioManager.instance.PlayEffect("CartHit");
						ParticleManager.instance.CreateSparksAtCollision (other);
						Camera_Controller.instance.Shake (.1f, .075f);

                        //Controller rumble.
                        this.transform.parent.GetComponent<Player>().TryVibrateGamepad(0.2f, 0.5f);
                    }
				}

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
