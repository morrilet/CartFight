using UnityEngine;
using System.Collections;

/// <summary>
/// This class will be attached to the driver and cart parts of the player. It calls events that the
/// player is subscribed to when it is hit by something on the appropriate layer.
/// </summary>
public class PlayerComponent : MonoBehaviour 
{
	//public LayerMask layerMask; //The objects that can hit us.
	//Collider2D coll; //The collider attached to this game object.

	public delegate void OnHitAction (Collision2D collision);
	public event OnHitAction OnHitCart; //An event for when we hit a cart.
	public event OnHitAction OnHitDriver; //An event for when we hit a driver.
	public event OnHitAction OnHitObstacle; //An event for when we hit an obstacle.

	void Start () 
	{
		//coll = GetComponent<Collider2D> ();
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		//Bit shift operator.
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Cart")) 
		{
			if (OnHitCart != null) 
			{
				OnHitCart(other);
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Driver")) 
		{
			if (OnHitDriver != null) 
			{
				OnHitDriver(other);
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Obstacle")) 
		{
			if (OnHitObstacle != null) 
			{
				OnHitObstacle(other);
			}
		}
	}

	void OnCollisionStay2D(Collision2D other)
	{
		//Bit shift operator.
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Cart")) 
		{
			if (OnHitCart != null) 
			{
				OnHitCart(other);
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Driver")) 
		{
			if (OnHitDriver != null) 
			{
				OnHitDriver(other);
			}
		}
		if ((1 << other.gameObject.layer) == 1 << LayerMask.NameToLayer("Obstacle")) 
		{
			if (OnHitObstacle != null) 
			{
				OnHitObstacle(other);
			}
		}
	}
}
