using UnityEngine;
using System.Collections;

public class Item : PausableObject
{
	public enum ItemType
	{
		Meat, Veggie, Cheese, Bomb
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

	protected Vector3 startingScale; //The size of the item.

	private Coroutine movingCoroutine; //This houses the coroutine that we use for lerping to a position.

	public virtual void Start()
	{
		GetComponent<SpriteRenderer> ().sprite = itemImages [(int) itemType];
		//GetComponent<SpriteRenderer> ().material = itemMaterials [(int)itemType];
		startingScale = this.transform.localScale;
	}

	public virtual void Update()
	{
		if (IsPaused)
			return;

		if (pickedUp) 
		{
			FollowPlayer ();
		}

		if (!pickedUp && this.player == null && !this.IsPaused) 
		{
            //This method isn't even finished... Wasted raycasts.
			//HandleCollision ();
		}
	}

    public void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            ClearObject(collision.gameObject);
        }
    }

    //Get picked up.
    public virtual void GetPickedUpByPlayer(Player player)
	{
		pickedUp = true;
		this.player = player;

        //GetComponent<Collider2D> ().enabled = false;
        GetComponent<Rigidbody2D>().isKinematic = true;
	}

	//Get dropped.
	public void GetDropped()
    {
        pickedUp = false;
		player = null;

        //GetComponent<Collider2D> ().enabled = true;
        GetComponent<Rigidbody2D>().isKinematic = false;
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
        this.GetComponent<Rigidbody2D>().drag = 0.65f;
		this.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;

		GetComponent<Collider2D> ().enabled = true;
	}

	void FollowPlayer()
	{
		float distance = Vector3.Distance (player.DriverObj.transform.position, transform.position);

		//Handle follow distance based on how many objects are in players cart. 0(X|.|.) => follow distance 1.
		if (distance > followDistance) 
		{
			Vector3 newPos = Vector3.Lerp (transform.position, player.DriverObj.transform.position, speed * Time.deltaTime);
			transform.position = newPos;
		}
	}

    /// <summary>
    /// Moves away from other items so there's no overlapping.
    /// </summary>
    private void ClearObject(GameObject other)
    {
        Vector2 dir = other.transform.position - this.transform.position;
        if (dir == Vector2.zero)
        {
            dir = Random.insideUnitCircle;
        }

        dir = dir.normalized * Time.deltaTime;
        this.GetComponent<Rigidbody2D>().AddForce(dir, ForceMode2D.Force);
    }
}
