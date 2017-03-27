using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	public float maxVelocity; //How fast we can go.
	public float acceleration; //How much is added to velocity.
	public float deceleration; //How quickly we can manually slow down.
	public float turnSpeed; //How quickly we turn.
	public float dampening; //How quickly we lose velocity.

	private Vector2 velocity;

	private Vector3 driverLocalPosition;
	private Vector3 cartLocalPosition;

	private GameObject driverObj; //The driver part of the player.
	private PlayerComponent driver; //The player component for the driver.
	private GameObject cartObj; //The cart part of the player.
	private PlayerComponent cart; //The player component for the cart.

	private bool colliding = false; //Whether or not we're in any sort of collision right now.

	public void Start()
	{
		velocity = Vector2.zero;

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

		driverLocalPosition = driverObj.transform.localPosition;
		cartLocalPosition = cartObj.transform.localPosition;

		cart.OnHitCart += HitCart;
		cart.OnHitObstacle += HitObstacle;
		cart.OnHitDriver += HitDriver;

		driver.OnHitObstacle += HitObstacle;

		//cart.OnHitCart_Exit += HitObstacle_Exit;
	}

	public void Update()
	{
		Move ();

		//Continually place driver and cart at their starting local positions.
		driverObj.transform.localPosition = driverLocalPosition;
		cartObj.transform.localPosition = cartLocalPosition;
	}

	public void HitCart(Collision2D other)
	{
		Debug.Log ("~Event triggered :: Hit a cart~");

		//Reflect current velocity with little dampening.
		velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.75f;

		HandleCollision (other, 1 << LayerMask.NameToLayer("Cart"));
	}

	public void HitDriver(Collision2D other)
	{
		Debug.Log ("~Event triggered :: Hit a driver~");

		//Reflect current velocity with moderate dampening.
		velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.5f;
		HandleCollision (other, 1 << LayerMask.NameToLayer("Driver"));

		//If we're going fast enough, kill the other driver.
		if (velocity.magnitude >= 5f)
		{
			Debug.Log ("-----Driver killed-----");

			//Kill the driver. (Use a destroy method later on)
			Destroy(other.gameObject);
		}
	}

	public void HitObstacle(Collision2D other)
	{
		Debug.Log ("~Event triggered :: Hit an obstacle~");
		//colliding = true;

		//Reflect current velocity with high dampening.
		velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.2f;

		HandleCollision (other, 1 << LayerMask.NameToLayer("Obstacle"));
	}

	//Keeps an object from passing into another object.
	//NOTES: This is sort of broken in some cases right now. A possible fix for this
	//could be to have each PlayerComponent handle its own collisions.
	private void HandleCollision (Collision2D other, LayerMask mask)
	{
		//Get the obj out of the obstacle if we're inside it.
		if (other.collider.bounds.Contains (other.contacts [0].point)) 
		{
			//Find the closest point on the obstacle to move to.
			Vector2 direction = other.contacts [0].point - (Vector2)transform.position;
			float distance = Vector2.Distance ((Vector2)transform.position, other.contacts [0].point);
			RaycastHit2D hit = Physics2D.Raycast ((Vector2)transform.position, direction, 
				distance, mask);

			Debug.DrawLine (other.contacts[0].point, hit.point, Color.blue);

			//Move the collision point to this point.
			Vector3 offset = transform.position;
			offset += (Vector3)hit.point - (Vector3)other.contacts [0].point;
			transform.position = offset;
		}
	}

	public void Move()
	{
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

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
		if (Vector2.Angle(velocity, transform.right) <= 90)
		{
			if (Vector2.Dot (velocity, -transform.up) > 0) 
			{
				velocity = RotateVector (velocity, (Vector2.Angle (transform.right, velocity) / 25f) * Mathf.Deg2Rad);
			} 
			else if (Vector2.Dot (velocity, -transform.up) < 0) 
			{
				velocity = RotateVector (velocity, -(Vector2.Angle (transform.right, velocity) / 25f) * Mathf.Deg2Rad);
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
}