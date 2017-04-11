using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Testing...
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour 
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

	//This is here if needed, but it'd be best to handle this in game manager.
	//private int points;
	//public void AddPoints(int value) { points += value; }
	//public int GetPoints() { return points; }

	private Vector3 driverLocalPosition;
	private Vector3 cartLocalPosition;

	private GameObject driverObj; //The driver part of the player.
	private PlayerComponent driver; //The player component for the driver.
	private GameObject cartObj; //The cart part of the player.
	private PlayerComponent cart; //The player component for the cart.

	//private bool colliding = false; //Whether or not we're in any sort of collision right now.
	private bool isAlive = true;

	[HideInInspector]
	public List<Item> carriedItems; //The items that the player is carrying.

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

		//Ignore collisions between our two components...
		Physics2D.IgnoreCollision(driverObj.GetComponent<Collider2D>(), cartObj.GetComponent<Collider2D>());

		driverLocalPosition = driverObj.transform.localPosition;
		cartLocalPosition = cartObj.transform.localPosition;

		HookUpEvents ();
	}

	public void Update()
	{
		if (isAlive) 
		{
			controlScheme.Update ();

			Move ();

			//Continually place driver and cart at their starting local positions.
			driverObj.transform.localPosition = driverLocalPosition;
			cartObj.transform.localPosition = cartLocalPosition;
		}
	}

	private void Die()
	{
		isAlive = false;

		//Unhook the player from the player components... Eventually make an init and unhook method for these...
		UnhookEvents();

		//Remove all collected objects.
		Debug.Log(carriedItems.Count);
		for (int i = 0; i < carriedItems.Count; i++)
		{
			Debug.Log (carriedItems[i].gameObject.name);
			carriedItems [i].Drop ();
		}
		carriedItems.RemoveRange (0, carriedItems.Count);

		GameManager.instance.SpawnPlayer (playerNumber, controlScheme, 5f);
		Destroy (driverObj);
		Destroy (this.gameObject, 3f);
	}

	public void HitItem(Collision2D other)
	{
		if(!other.gameObject.GetComponent<Item>().isPickedUp())
		{
			other.gameObject.GetComponent<Item> ().PickUp (this.GetComponent<Player> ());

			//Do this before adding to count...
			other.gameObject.GetComponent<Item> ().followDistance = 2.5f + (1.25f * carriedItems.Count);
			other.gameObject.GetComponent<Item> ().speed = 4.0f / (1.0f + carriedItems.Count);

			carriedItems.Add (other.gameObject.GetComponent<Item> ());
		}
	}

	public void HitCart(Collision2D other)
	{
		//Debug.Log ("~Event triggered :: Hit a cart~");

		//Reflect current velocity with little dampening.
		velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.75f;
		//Push the other cart.
		other.transform.parent.GetComponent<Player>().velocity -= velocity * 0.25f;

		//HandleCollision (other, 1 << LayerMask.NameToLayer("Cart"));
	}

	public void HitDriver(Collision2D other)
	{
		//Debug.Log ("~Event triggered :: Hit a driver~");
		//Debug.Log(other.gameObject.name);
		//Debug.DrawLine (other.contacts[0].point, other.gameObject.transform.position, Color.black);

		//Reflect current velocity with moderate dampening.
		velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.5f;
		//HandleCollision (other, 1 << LayerMask.NameToLayer("Driver"));

		//If we're going fast enough, kill the other driver.
		if (velocity.magnitude >= 5f)
		{
			Debug.Log ("-----Driver killed-----");

			//Kill the driver.
			other.transform.parent.gameObject.GetComponent<Player>().Die();
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
		HandleCollision (other, 1 << LayerMask.NameToLayer("Obstacle"));
	}

	public void HitCart_Stay(Collision2D other)
	{
		HandleCollision (other, 1 << LayerMask.NameToLayer ("Cart"));
	}

	public void HitDriver_Stay(Collision2D other)
	{
		HandleCollision (other, 1 << LayerMask.NameToLayer ("Driver"));
	}

	//Keeps an object from passing into another object.
	private void HandleCollision (Collision2D other, LayerMask mask)
	{
		//Get the obj out of the obstacle if we're inside it.
		if (other.collider.bounds.Contains (other.contacts [0].point)) 
		{
			//Find the closest point on the obstacle to move to.
			Vector2 direction = other.contacts [0].point - (Vector2)transform.position;
			//Stopped using distance because it was continually inside the bounds
			//of our components. Opted to use 25 because that guarantees multiple hits
			//and a more accurate shortest distance and nearest point.
			float distance = Vector2.Distance ((Vector2)transform.position, other.contacts [0].point);
			RaycastHit2D[] hits = Physics2D.RaycastAll ((Vector2)transform.position, direction, 
				25f, mask);

			float shortestDistance = float.PositiveInfinity;
			Vector2 nearestPoint = Vector2.zero; //The nearest point (on the other object) between 
			                                     //transform.position and the collision point.
			for (int i = 0; i < hits.Length; i++) 
			{
				//If the collider isn't one of ours...
				bool validHit = true;
				if (cartObj != null && cartObj.GetComponent<Collider2D> () != null) 
				{
					if (hits [i].collider == cartObj.GetComponent<Collider2D> ()) 
					{
						validHit = false;
					}
				}
				if(driverObj != null && driverObj.GetComponent<Collider2D> () != null)
				{
					if (hits [i].collider == driverObj.GetComponent<Collider2D> ())
					{
						validHit = false;
					}
				}
				if (validHit) 
				{
					//Check the raycast hit against the current shortest distance.
					if (Vector2.Distance(hits [i].point, transform.position) < shortestDistance)
					{
						shortestDistance = Vector2.Distance(hits[i].point, transform.position);
						nearestPoint = hits [i].point;
					}
				}
			}

			//Pause for easy debugging...
			//UnityEditor.EditorApplication.isPaused = true;
			//Trace the raycast.
			Debug.DrawLine (other.contacts[0].point, nearestPoint, Color.blue);
			//Note the origin...
			Debug.DrawLine(other.contacts[0].point + new Vector2(0, .15f), other.contacts[0].point - new Vector2(0, .15f));
			Debug.DrawLine(other.contacts[0].point + new Vector2(.15f, 0), other.contacts[0].point - new Vector2(.15f, 0));
			//Note the nearest point...
			//Debug.Log(nearestPoint);
			Debug.DrawLine(nearestPoint + new Vector2(0, .15f), nearestPoint - new Vector2(0, .15f));
			Debug.DrawLine(nearestPoint + new Vector2(.15f, 0), nearestPoint - new Vector2(.15f, 0));

			//Move the collision point to this point.
			Vector3 offset = transform.position;
			velocity = (Vector2)nearestPoint - (Vector2)other.contacts [0].point;
			offset += (Vector3)nearestPoint - (Vector3)other.contacts [0].point;
			transform.position = offset;
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere (transform.position, 0.25f);
	}

	public void Move()
	{
		//Pull the input from our desired control scheme.
		Vector2 input = new Vector2 (controlScheme.Horizontal, controlScheme.Vertical);

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

		//Cap velocity.
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

	//Rotates a vector by an angle.
	private Vector2 RotateVector(Vector2 inputVector, float degrees)
	{
		Vector2 output = Vector2.zero;
		output.x = inputVector.x * Mathf.Cos (degrees) - inputVector.y * Mathf.Sin (degrees);
		output.y = inputVector.x * Mathf.Sin (degrees) + inputVector.y * Mathf.Cos (degrees);
		return output;
	}

	//Sets up events.
	private void HookUpEvents()
	{
		cart.OnHitCart += HitCart;
		cart.OnHitObstacle += HitObstacle;
		cart.OnHitDriver += HitDriver;

		//Driver handles everything as an obstacle collision.
		driver.OnHitObstacle += HitObstacle;
		driver.OnHitDriver += HitObstacle;
		driver.OnHitCart += HitObstacle;

		driver.OnHitObstacle_Stay += HitObstacle_Stay;
		driver.OnHitDriver_Stay += HitDriver_Stay;
		driver.OnHitCart_Stay += HitCart_Stay;

		cart.OnHitObstacle_Stay += HitObstacle_Stay;
		cart.OnHitDriver_Stay += HitDriver_Stay;
		cart.OnHitCart_Stay += HitCart_Stay;

		cart.OnHitItem += HitItem;
		driver.OnHitItem += HitItem;
	}

	//Removes our functions from all events.
	private void UnhookEvents()
	{
		cart.OnHitObstacle -= HitObstacle;
		cart.OnHitDriver -= HitDriver;
		cart.OnHitCart -= HitObstacle;
		cart.OnHitObstacle_Stay -= HitObstacle_Stay;
		cart.OnHitDriver_Stay -= HitDriver_Stay;
		cart.OnHitCart_Stay -= HitCart_Stay;

		driver.OnHitObstacle -= HitObstacle;
		driver.OnHitDriver -= HitObstacle;
		driver.OnHitCart -= HitObstacle;
		driver.OnHitObstacle_Stay -= HitObstacle_Stay;
		driver.OnHitDriver_Stay -= HitDriver_Stay;
		driver.OnHitCart_Stay -= HitCart_Stay;

		driver.OnHitItem -= HitItem;
		cart.OnHitItem -= HitItem;
	}
}