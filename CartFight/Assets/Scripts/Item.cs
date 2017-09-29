using UnityEngine;
using System.Collections;

public class Item : PausableObject
{
	public enum ItemType
	{
		Meat, Veggie, Cheese
	};
	public ItemType itemType;

	public Sprite[] itemImages;
	public Material[] itemMaterials;

	//Perhaps make there
	[HideInInspector]
	public float followDistance;
	[HideInInspector]
	public float speed;

	private bool pickedUp = false;
	private Player player = null; //The player that's carrying us.

	public bool isPickedUp() { return pickedUp; }

	private Vector3 startingScale; //The size of the item.

	private Coroutine movingCoroutine; //This houses the coroutine that we use for lerping to a position.

	void Start()
	{
		GetComponent<SpriteRenderer> ().sprite = itemImages [(int) itemType];
		//GetComponent<SpriteRenderer> ().material = itemMaterials [(int)itemType];
		startingScale = this.transform.localScale;
	}

	void Update()
	{
		if (IsPaused)
			return;

		if (pickedUp) 
		{
			FollowPlayer ();
		}

		if (!pickedUp && this.player == null && !this.IsPaused) 
		{
			HandleCollision ();
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
	public void GetPlacedInCart(GameObject cartObj, float offset)
	{
		pickedUp = false;
		player = null;

		Destroy (this.GetComponent<Rigidbody2D> ());

		transform.localScale = startingScale * 0.75f; //Make the items a little smaller. Do this before parenting it.
		transform.SetParent (cartObj.transform);
		transform.position = cartObj.transform.position + (offset * cartObj.transform.up);

		GetComponent<Collider2D> ().enabled = false;
	}

	//Get removed from a cart.
	public void GetRemovedFromCart()
	{
		pickedUp = false;
		player = null;

		transform.SetParent (null);
		transform.localScale = startingScale; //Return the items to normal size.
		transform.rotation = Quaternion.identity;

		this.gameObject.AddComponent<Rigidbody2D> ();
		this.GetComponent<Rigidbody2D> ().gravityScale = 0.0f;
		this.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;

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

	/// <summary>
	/// Handles obstacle collisions. This is used to keep the item from being left in an obstacle
	/// where it can't be retreived.
	/// </summary>
	private void HandleCollision()
	{
		Collider2D collider = this.GetComponent <Collider2D> ();
		int obstacleLayer = LayerMask.NameToLayer ("Obstacle");

		//Here is the procedure for moving us away from the object that we're inside of...
		// 1: Detect the collision. If no collision, return.
		// 2: Raycast outward from the item in several directions and find the nearest
		//    point outside the obstacle.
		// 3: Move the item in that direction until it's no longer colliding with the obstacle.

		if (Physics2D.IsTouchingLayers(collider, LayerMask.NameToLayer("Obstacle")))
		{
			Debug.Log ("Here!");

			//Raycast outward.
			int numRaycasts = 16;
			RaycastHit2D[] hits = new RaycastHit2D [numRaycasts];
			for (int i = 0; i < numRaycasts; i++) 
			{
				//Get the angle of the raycast.
				float angle = ((float)i / (float)numRaycasts) * 360f;

				//Raycast toward that angle.
				Vector2 direction = ((Vector2)transform.right) * 
					Quaternion.Angle(Quaternion.identity, Quaternion.Euler(0.0f, 0.0f, angle));
				hits [i] = Physics2D.Raycast ((Vector2)transform.position, direction, 
					Mathf.Infinity, 1 << LayerMask.NameToLayer ("Obstacle"));

				//Visualization.
				Debug.DrawRay (transform.position, (Vector3)direction, Color.red);
			}

			//Search through hits and find the nearest point that isn't in an obstacle.
			Vector2 nearestPoint = new Vector2(Mathf.Infinity, Mathf.Infinity);
			for (int i = 0; i < hits.Length; i++) 
			{
			}
		}
	}
}
