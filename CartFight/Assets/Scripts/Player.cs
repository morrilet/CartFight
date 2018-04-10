using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

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
	public Vector2 Velocity { get { return this.velocity; } set { this.velocity = value; } }

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

	private bool invulnerable = true; //Invulnerable for a few seconds at the start.
	public float invulnerableTime;
	private float invulnerableTimer = 0.0f;
	public bool Invulnerable { get { return this.invulnerable; } }

	//These help ensure that the collision handling is only performed once per frame.
	private bool hitObstacleCalledThisFrame = false;
	private bool hitCartCalledThisFrame = false;
	private bool hitDriverCalledThisFrame = false;

	[HideInInspector]
	public List<Item> carriedItems; //The items that the player is carrying.

	private Animator driverAnimator; //The animator of the driver component.

	public Sprite[] cartImages;

	private Coroutine invulnerableEffect;
	private bool cartLerpEffectActive; //The carts lerp-to-position effect. 

    //Testing this right now. (1/29/18)
    GameObject soulboundCart = null;
    bool attractingCart = false;

    public GameObject SoulboundCart { get { return this.soulboundCart; } }
    public bool AttractingCart { get { return this.attractingCart; } }

    public GameObject CartObj { get { return this.cartObj; } }
    public GameObject DriverObj { get { return this.driverObj; } }

    public void Start()
	{
		controlScheme.Start ();

		//velocity = Vector2.zero;
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

		//This is later handled by the spawner.
		SetInvulnerable (true);
		EnableObstacleCollisions (false);

        //Testing for now. (1/29/18)
        soulboundCart = cartObj;
        attractingCart = false;
    }

	void Update()
	{
		controlScheme.Update ();

		if (IsPaused)
			return;

        carriedItems.RemoveAll(item => item == null);

		if (isAlive) 
		{
			if (invulnerable && invulnerableTimer >= invulnerableTime) 
			{
				SetInvulnerable (false);
			} 
			else if(!IsPaused)
			{
				invulnerableTimer += Time.deltaTime;
			}

			if (cartObj != null) 
			{
				Move ();
			}
			else
			{
				MoveWithoutCart ();
			}
            
            //TODO: Add this to a soulbound carts check. Throw logic should be separate, but pickup should all be in one if.
			if (controlScheme.ThrowKeyDown && driver.collidesWithObstacles)
			{
				if (cartObj != null && !cartLerpEffectActive && cart.collidesWithObstacles)
				{
                    if (GameManager.instance.Settings.UseSoulboundCarts)
                        attractingCart = false;

					RemoveCart ((Vector3)velocity + (transform.right * 20f));
				}
				else
				{
                    if (GameManager.instance.Settings.UseSoulboundCarts)
                        attractingCart = true;

                    //Debug.Log ("Trying to add cart...");
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Cart"))
                    {
                        if (obj.transform.parent == null)
                        {
                            foreach (Collider2D coll in driver.Touching)
                            {
                                if (coll.gameObject.transform.Equals(obj.transform))
                                {
                                    //Debug.Log("Cart added! :: " + obj.name);
                                    if (cartObj == null) //Double check this.
                                    {
                                        //We can only add our own soulbound cart, assuming soulbound carts are enabled.
                                        if (!GameManager.instance.Settings.UseSoulboundCarts || obj.Equals(soulboundCart))
                                        {
                                            AddCart(obj);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
				}
			}
            
            if (GameManager.instance.Settings.UseSoulboundCarts)
            {
                if (attractingCart)
                {
                    //Attract the cart first, so that it can't just snap into a stupid position.
                    if (soulboundCart != null)
                    {
                        soulboundCart.GetComponent<Rigidbody2D>().AddForce(
                            (driverObj.transform.position - soulboundCart.transform.position).normalized * 0.75f,
                            ForceMode2D.Impulse);

                        //Grab the (soulbound) cart if it's touching us.
                        foreach (Collider2D coll in driver.Touching)
                        {
                            if (coll.gameObject.Equals(soulboundCart))
                            {
                                AddCart(soulboundCart);
                                attractingCart = false;
                                break;
                            }
                        }
                    }
                }

                //Maintain the attraction state if it's already been set (throw key pressed).
                if(controlScheme.ThrowKeyHeld && attractingCart)
                {
                    attractingCart = true;
                }
                else
                {
                    attractingCart = false;
                }
            }

            //Continually place driver and cart at their starting local positions.
            driverObj.transform.localPosition = driverLocalPosition;
			if (cartObj != null && !cartLerpEffectActive) 
			{
				cartObj.transform.localPosition = cartLocalPosition;
				cartObj.transform.localRotation = cartLocalRotation;
			}

			//Update the drivers animation speed based on our current velocity.
			float clampedSpeed = velocity.magnitude / maxVelocity; //Our speed from 0-1.
			//If we're moving backwards, we should reverse the clamped speed.
			//Recall that transform.right is our forward direction.
			if (Vector2.Dot ((Vector2)this.transform.right, velocity.normalized) < 0) 
			{
				clampedSpeed *= -1f;
			}
			driverAnimator.SetFloat("AnimationSpeed", clampedSpeed);
		}
	}

	void LateUpdate()
	{
		hitObstacleCalledThisFrame = false;
		hitCartCalledThisFrame = false;
		hitDriverCalledThisFrame = false;
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
		//UnhookCartEvents ();
		cart.UnhookEvents();

		//Ignore collisions with items and the cart.
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Item")) {
			Physics2D.IgnoreCollision (cartObj.GetComponent<Collider2D> (), obj.GetComponent<Collider2D> (), true);
		}
		//Don't ignore collisions between the player and the cart.
		Physics2D.IgnoreCollision (driverObj.GetComponent<Collider2D> (), cartObj.GetComponent<Collider2D> (), false);

		//Handle invulnerability by removing it.
		if (invulnerable) 
		{
			SetInvulnerable (false);
		}

		//Empty our references.
		cartObj = null;
		cart = null;

		//Set the animator info.
		driverAnimator.SetBool("hasCart", false);
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

		//Lerp the cart to the right position.
		StartCoroutine(LerpCart_Coroutine(cartLocalPosition, cartLocalRotation, 0.15f));

		//Stop ignoring collisions between the cart and items.
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Item"))
		{
			Physics2D.IgnoreCollision (cart.GetComponent<Collider2D> (), obj.GetComponent<Collider2D> (), false);
		}
		//Ignore collisions between the player and the cart.
		Physics2D.IgnoreCollision (driverObj.GetComponent<Collider2D> (), this.cartObj.GetComponent<Collider2D> ());

		//Set the animator info.
		driverAnimator.SetBool("hasCart", true);
	}

	private IEnumerator LerpCart_Coroutine(Vector3 position, Quaternion rotation, float duration)
	{
		Vector3 startingPos = cartObj.transform.localPosition;
		Quaternion startingRot = cartObj.transform.localRotation;
		cartLerpEffectActive = true;

		float timer = 0.0f;
		while (timer < duration) 
		{
			Vector3 newPos = Vector3.Lerp (startingPos, position, timer / duration);
			cartObj.transform.localPosition = newPos;

			Quaternion newRot = Quaternion.Slerp (startingRot, rotation, timer / duration);
			cartObj.transform.localRotation = newRot;

			timer += Time.deltaTime;
			yield return null;
		}
			
		cartObj.transform.localPosition = position;
		cartObj.transform.localRotation = rotation;
		cartLerpEffectActive = false;
	}
    
	//Places our currently carried items into a cart.
	private void PutItemsInCart(GameObject cartObj)
	{
		//++i makes no copy of i and is faster. No bugs because of the limited use case here.
		for (int i = 0; i < carriedItems.Count; ++i) 
		{
            if (carriedItems[i] != null && carriedItems[i].isPickedUp())
            {
                Item tmp = carriedItems[i];

                //Note that carts are ~3 units long, but items are ~1 unit so we use 2 here.
                float offset = (2f / carriedItems.Count) * i;
                offset = (carriedItems.Count > 1) ? offset - 0.5f : offset;

                tmp.GetDropped();
                tmp.GetPlacedInCart(cartObj, offset); //This has caused a null reference exception in the past. Why?
            }
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

    #region ItemManipulation
    //Adds an item to the carried items list.
    public void PickupItem(Item item)
	{
        //Take the chance to scrub our carried items list before we add more to it.
        carriedItems = carriedItems.Where(x => x != null).ToList();

		item.GetPickedUpByPlayer (this.GetComponent<Player> ());

        //Sort of magic number-y. These relate to the size of items in the game, and
        //create a comfortable buffer between objects.
		item.followDistance = 2.5f + (1.25f * carriedItems.Count);
		item.speed = 4.0f / (1.0f + carriedItems.Count);

		carriedItems.Add (item);

		//Particles.
		Color32 particleColor = new Color32((byte)255, (byte)255, (byte)255, (byte)255);
		switch (playerNumber) 
		{
		case PlayerNumber.P1:
			particleColor = new Color32 ((byte)138, (byte)69, (byte)69, (byte)255);
			break;
		case PlayerNumber.P2:
			particleColor = new Color32 ((byte)69, (byte)95, (byte)138, (byte)255);
			break;
		case PlayerNumber.P3:
			particleColor = new Color32 ((byte)69, (byte)138, (byte)84, (byte)255);
			break;
		case PlayerNumber.P4:
			particleColor = new Color32 ((byte)138, (byte)69, (byte)126, (byte)255);
			break;
		default:
			Debug.Log ("Item grabbed by invalid player number!");
			break;
		}
		ParticleManager.instance.CreateParticles ("PickupParticles",
			item.transform.position, Quaternion.identity, (Color)particleColor);

		//Audio.
		AudioManager.instance.PlayEffect ("PickupFood");
	}

    //Remove an item from the carried items list.
    public void DropItem(Item i)
    {
        //Scrub our carried items list.
        carriedItems = carriedItems.Where(x => x != null).ToList();

        carriedItems.Remove(i);
        UpdateItemFollowDistances();
    }

    private void UpdateItemFollowDistances()
    {
        try
        {
            for (int i = 0; i < carriedItems.Count; i++)
            {
                carriedItems[i].followDistance = 2.5f + (1.25f * i);
                carriedItems[i].speed = 4.0f / (1.0f + i);
            }
        }
        catch(System.NullReferenceException e)
        {
            throw new System.NullReferenceException("Tried to update the follow distance of a null item. " +
                "Be sure that the list is scrubbed before you try to update follow distances.");
        }
    }
    #endregion

    public void Die(Player killer)
	{
		isAlive = false;

        if(GameManager.instance.Settings.Kills != GameManager.GameSettings.KillsMode.None)
        {
            if(killer != null && killer != this)
            {
                GameManager.instance.AddPointsToPlayer(killer, GameManager.instance.Settings.KillValue);
            }
            else if (killer == null)
            {
                Debug.LogWarning("A player has been killed by a null killer. Ensure that it was a play-area death.");
            }
        }

		//Moderate screen shake.
		Camera_Controller.instance.Shake(.35f, .5f);

        //Play the death sound.
        AudioManager.instance.PlayEffect("Die");

		//Unhook the player from the player components...
		//(Not doing this for cart because it's handled in remove cart.)
		driver.UnhookEvents();

		//Remove all collected objects.
		//Debug.Log(carriedItems.Count);
		for (int i = 0; i < carriedItems.Count; i++)
		{
            //Debug.Log (carriedItems[i].gameObject.name);
            if (carriedItems[i] != null)
            {
                carriedItems[i].GetDropped();
            }
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
		driverObj.GetComponent<SpriteRenderer>().sortingLayerName = "DeadPlayers";
		driverObj.GetComponent<SpriteRenderer> ().sortingOrder = 0;
		driverObj.AddComponent<FadeOutAndDestroy> ();
		driverObj.GetComponent<Collider2D> ().enabled = false;
		driverObj.transform.SetParent (null);

		Destroy (this.gameObject);
	}

	public void HitItem(Collision2D other)
	{
		//Debug.Log ("Hit item.");
		if(!other.gameObject.GetComponent<Item>().isPickedUp() && this.cartObj != null)
		{
			//Pickup the item.
			PickupItem (other.gameObject.GetComponent<Item> ());
		}
	}

	public void HitCart(Collision2D other)
	{
		//Debug.Log ("~Event triggered :: Hit a cart~");

		if (hitCartCalledThisFrame)
			return;
		hitCartCalledThisFrame = true;

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

			//If we hit it hard enough, shake the items out.
			if (velocity.magnitude >= (maxVelocity / 10f) * 7.5f) //75% of max speed or higher.
			{
				//Remove the childed items.
				Item[] cartItems = other.gameObject.GetComponentsInChildren<Item> ();
				for (int i = 0; i < cartItems.Length; i++) 
				{
					cartItems [i].GetRemovedFromCart ();
				}
			}
		}

		AudioManager.instance.PlayEffect ("CartHit");

		//Effect stuff.
		if (velocity.magnitude >= (maxVelocity / 10f) * 7.5f) //75% of max speed or higher.
		{
			ParticleManager.instance.CreateSparksAtCollision (other);

			if (other.transform.parent != null) //Other cart isn't abandoned.
			{
				//Slight screen shake.
				Camera_Controller.instance.Shake(.2f, .35f);

                //Controller rumble.
                this.TryVibrateGamepad(0.3f, 0.8f);
                other.transform.parent.GetComponent<Player>().TryVibrateGamepad(0.3f, 0.8f);
			}
			else
			{
				//Minimal screen shake, unless there are items in the cart.
				if (other.gameObject.GetComponentInChildren<Item> () != null) 
				{
					Camera_Controller.instance.Shake (.15f, .25f);
				}
				else
				{
					Camera_Controller.instance.Shake (.1f, .1f);
				}

                //Controller rumble.
                this.TryVibrateGamepad(0.3f, 0.8f);
            }
		}
	}

	public void HitDriver(Collision2D other)
	{
		if (hitDriverCalledThisFrame)
			return;
		hitDriverCalledThisFrame = true;

		if (driverObj == null || !other.transform.Equals (driverObj.transform))
		{
			//Debug.Log ("~Event triggered :: Hit a driver~ Called By : " + playerNumber.ToString());
			//Debug.Log(other.gameObject.name);
			//Debug.DrawLine (other.contacts[0].point, other.gameObject.transform.position, Color.black);
			float impactForce = 0.0f;

			//If we have a cart and the other object has a player parent.
			if (cartObj != null && other.transform.parent != null && other.transform.parent.GetComponent<Player>() != null)
			{
				impactForce = (velocity /*- other.transform.parent.GetComponent<Player> ().velocity*/).magnitude;
			}
			//Or if we don't have a cart and the other object has a player parent.
			else if(other.transform.parent != null && other.transform.parent.GetComponent<Player>() != null)
			{
				impactForce = (cartObjPrev.GetComponent<Rigidbody2D> ().velocity.magnitude); //- 
					//other.transform.parent.GetComponent<Player> ().velocity).magnitude;
			}

			//Reflect current velocity with moderate dampening.
			velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * 0.5f;

			//If we're going fast enough, kill the other driver.
			if (impactForce >= 10f) 
			{
                //Debug.Log ("-----Driver killed-----");

                //Kill the driver.
                other.transform.parent.gameObject.GetComponent<Player>().Die(this);

                //Controller rumble.
                this.TryVibrateGamepad(0.5f, 1.0f);
                if(other.transform.parent != null && other.transform.parent.GetComponent<Player>() != null)
                    other.transform.parent.GetComponent<Player>().TryVibrateGamepad(0.5f, 1.0f);
            }
		}
	}

	public void HitObstacle(Collision2D other)
	{
		//Debug.Log ("~Event triggered :: Hit an obstacle~");
		//colliding = true;

		if (hitObstacleCalledThisFrame)
			return;
		hitObstacleCalledThisFrame = true;

		//Reflect current velocity with low dampening.
		//velocity = -velocity * (Mathf.Abs(Vector2.Dot(transform.right, other.contacts[0].normal)));
		velocity = Vector2.Reflect (velocity, other.contacts [0].normal) * .75f;
		Debug.DrawRay (other.contacts [0].point, other.contacts [0].normal, Color.red);
		Debug.DrawRay(transform.position, (Vector3)velocity / maxVelocity * 5f, Color.yellow);
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
	//4.5 :: Repeat steps 3 and 4 for the -normal, then choose the shortest distance
	//	   to use for step 5. This ensures that we never choose to move the player
	//	   to the other side of an object we're colliding with.
	//5 :: Move the appropriate distance in the appropriate direction in order to
	//     get outside of the object, and set velocity to that same value so that
	//     the movement is less jerky.
	private void HandleCollisionNew(Collision2D other, LayerMask mask)
	{
		if (other.collider.bounds.Contains (other.contacts [0].point)) 
		{
			Vector2 normal = other.contacts [0].normal;

			//Test the normal in both directions...
			//First test it along the normal.
			Vector2 pointOutsideCollider_1 = (Vector2)other.contacts[0].point + (normal.normalized * 5f);
			Vector2 nearestPoint_1 = Vector2.zero;
			RaycastHit2D[] hits_1 = Physics2D.RaycastAll (pointOutsideCollider_1, -normal, float.PositiveInfinity);
			for (int i = 0; i < hits_1.Length; i++) 
			{
				if (hits_1 [i].transform.Equals (other.transform)) 
				{
					nearestPoint_1 = hits_1 [i].point;
				}
			}
			//Then test it along -normal.
			Vector2 pointOutsideCollider_2 = (Vector2)other.contacts[0].point + (-normal.normalized * 5f);
			Vector2 nearestPoint_2 = Vector2.zero;
			RaycastHit2D[] hits_2 = Physics2D.RaycastAll (pointOutsideCollider_2, normal, float.PositiveInfinity);
			for (int i = 0; i < hits_2.Length; i++) 
			{
				if (hits_2 [i].transform.Equals (other.transform)) 
				{
					nearestPoint_2 = hits_2 [i].point;
				}
			}

			//See which point is nearest to the point of impact.
			Vector2 nearestPoint = Vector2.zero;
			Vector2 pointOutsideCollider = Vector2.zero;
			if (Vector2.Distance (nearestPoint_1, other.contacts [0].point)
			   < Vector2.Distance (nearestPoint_2, other.contacts [0].point)) 
			{
				nearestPoint = nearestPoint_1;
				pointOutsideCollider = pointOutsideCollider_1;
			}
			else
			{
				nearestPoint = nearestPoint_2;
				pointOutsideCollider = pointOutsideCollider_2;
				normal *= -1;
			}

			//For debugging...
			DrawCross (nearestPoint, .3f, Color.white);
			DrawCross (pointOutsideCollider, .25f, Color.red);
			DrawCross (other.contacts [0].point, .25f, Color.blue);
			Debug.DrawRay (other.contacts [0].point, normal);
			//Debug.Break ();
			if (Vector2.Distance (nearestPoint, other.contacts[0].point) > 1) 
			{
				nearestPoint = (nearestPoint - other.contacts [0].point).normalized * 1f;
				Debug.Break();
			}

			float distance = Vector2.Distance (other.contacts [0].point, nearestPoint);
			//Debug.Log (distance);

			Vector3 offset = transform.position;
			velocity = distance * normal.normalized;
			offset += distance * (Vector3)normal.normalized;
			transform.position = offset;
		}
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
		/*
		if (controlScheme.IsGamePad) 
		{
			Debug.Log ("Controller inputs... " + input);
		}
		*/

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
				//Debug.DrawLine (transform.position, transform.position + (Vector3)dir);

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

		//Debug.DrawRay (transform.position, velocity.normalized, Color.red);
		//Debug.DrawRay (transform.position, transform.right * 2f, Color.yellow);

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

		//Debug.DrawRay (transform.position, velocity.normalized, Color.red);
		//Debug.DrawRay (transform.position, transform.right * 2f, Color.yellow);
			
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

	public void EnableObstacleCollisions(bool value)
	{
		driver.collidesWithObstacles = value;
		cart.collidesWithObstacles = value;
	}

	private void SetInvulnerable(bool value)
	{
		invulnerable = value;
		
		if (value)
		{
			invulnerableEffect = StartCoroutine (InvulnerableEffect_Coroutine (invulnerableTime, 10.0f));
			//cartObj.tag = "Invulnerable";
			//driverObj.tag = "Invulnerable";
			cartObj.layer = LayerMask.NameToLayer("Invulnerable");
			driverObj.layer = LayerMask.NameToLayer ("Invulnerable");
		}
		else
		{
			//cartObj.tag = "Cart";
			//driverObj.tag = "Driver";
			cartObj.layer = LayerMask.NameToLayer("Cart");
			driverObj.layer = LayerMask.NameToLayer ("Driver");

			StopCoroutine (invulnerableEffect);

			Color cartColor = cartObj.GetComponent<SpriteRenderer> ().color;
			Color driverColor = driverObj.GetComponent<SpriteRenderer> ().color;

			cartColor.a = 1.0f;
			driverColor.a = 1.0f;

			cartObj.GetComponent<SpriteRenderer> ().color = cartColor;
			driverObj.GetComponent<SpriteRenderer> ().color = driverColor;
		}

		cart.invulnerable = value;
		driver.invulnerable = value;
	}

	private IEnumerator InvulnerableEffect_Coroutine(float duration, float period)
	{
		float timer = 0.0f;

		Color newCartColor = cartObj.GetComponent<SpriteRenderer> ().color;
		Color newDriverColor = driverObj.GetComponent<SpriteRenderer> ().color;

		while (timer <= duration)
		{
			if (!this.IsPaused) 
			{
				if (invulnerable) 
				{
					float angle = ((timer % 360.0f) * period);
					float alpha = Mathf.Abs (Mathf.Sin (angle));

					newCartColor.a = alpha;
					newDriverColor.a = alpha;

					cartObj.GetComponent<SpriteRenderer> ().color = newCartColor;
					driverObj.GetComponent<SpriteRenderer> ().color = newDriverColor;
				}
				timer += Time.deltaTime;
			}
			yield return null;
		}

		newCartColor.a = 1.0f;
		newDriverColor.a = 1.0f;

		cartObj.GetComponent<SpriteRenderer> ().color = newCartColor;
		driverObj.GetComponent<SpriteRenderer> ().color = newDriverColor;
	}

    /// <summary>
    /// Attempts to vibrate the gamepad. Does nothing if we're not using
    /// a gamepad or if we're already vibrating the gamepad.
    /// </summary>
    /// <param name="duration">The length of the vibration in seconds.</param>
    /// <param name="vibrationStrength">The strength of the vibration from 0 to 1.</param>
    public void TryVibrateGamepad(float duration, float vibrationStrength)
    {
        //If we're using a gamepad...
        if (this.controlScheme.IsGamePad)
        {
            //Set vibrating to true. If it's already true, stop the coroutine
            //and start a new vibration coroutine.
            if (this.controlScheme.IsVibrating)
            {
                StopCoroutine("VibrateGamepad_Coroutine");
                GamePad.SetVibration(this.controlScheme.PIndex, 0.0f, 0.0f);
            }
            else
            {
                this.controlScheme.IsVibrating = true;
            }

            StartCoroutine(VibrateGamepad_Coroutine(duration, vibrationStrength));
        }
    }

    /// <summary>
    /// Vibrates the gamepad associated with this player for a specified duration
    /// and with a specified strength. Assumes that we are using a gamepad.
    /// </summary>
    /// <param name="duration">The length of the vibration in seconds.</param>
    /// <param name="vibrationStrength">The strength of the vibration from 0 to 1.</param>
    /// <returns></returns>
    private IEnumerator VibrateGamepad_Coroutine(float duration, float vibrationStrength)
    {
        GamePad.SetVibration(this.controlScheme.PIndex, vibrationStrength, vibrationStrength);

        float timer = 0.0f;
        while (timer < duration)
        {
            //If the game gets paused, halt the vibration and resume it after the pause.
            if (this.IsPaused && !this.IsPausedPrev)
            {
                GamePad.SetVibration(this.controlScheme.PIndex, 0.0f, 0.0f);
            }
            if(!this.IsPaused && this.IsPausedPrev)
            {
                GamePad.SetVibration(this.controlScheme.PIndex, vibrationStrength, vibrationStrength);
            }

            if(!this.IsPaused)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }

        GamePad.SetVibration(this.controlScheme.PIndex, 0.0f, 0.0f);

        this.controlScheme.IsVibrating = false;
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