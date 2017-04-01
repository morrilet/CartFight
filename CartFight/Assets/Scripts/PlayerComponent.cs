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

	public event OnHitAction OnHitCart_Stay;
	public event OnHitAction OnHitDriver_Stay;
	public event OnHitAction OnHitObstacle_Stay;

	public event OnHitAction OnHitItem;

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
}
