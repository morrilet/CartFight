using UnityEngine;
using System.Collections;

/// <summary>
/// Raycasts out vertically and horizontally from the object it's attached to.
/// If the raycasts hit anything, it calls the OnHitEnter and/or OnHitStay event(s).
/// </summary>
public class RaycastController : MonoBehaviour 
{
	////////// Variables //////////
	public int horizontalRaycasts;
	public int verticalRaycasts;
	public float skinWidth; //An inset to cast the rays from. Prevents rays from being cast inside other objects.
	private float width;
	private float height;

	////////// Events //////////
	public delegate void OnHitAction (Collider2D other);
	public event OnHitAction OnHitEnter; //Functions just like OnTriggerEnter2D(Collider2D other)
	public event OnHitAction OnHitStay; //Functions just like OnTriggerStay2D(Collider2D other)

	////////// Primary Methods //////////
	void Start()
	{
		width = this.GetComponent<BoxCollider2D> ().size.x;
		height = this.GetComponent<BoxCollider2D> ().size.y;
	}

	void Update()
	{
		GetRaycastHitsVerical (1 << LayerMask.NameToLayer ("Obstacle"));
	}

	////////// Custom Methods //////////
	private void GetRaycastHitsVerical(LayerMask mask)
	{
		RaycastHit2D[] hits = new RaycastHit2D[verticalRaycasts];

		//Raycast up...
		for (int i = 0; i < verticalRaycasts; i++) 
		{
			Vector2 direction = (Vector2)this.transform.up;

			Vector2 origin = (Vector2)transform.position;
			origin.y += height;

			//Rotate origin point around the transform position properly...
			Vector3 tempDir = (Vector3)origin - transform.position;
			tempDir = Quaternion.Euler (0, 0, Vector2.Dot (direction, Vector2.up)) * tempDir;
			origin = (Vector2)tempDir + (Vector2)transform.position;

			hits [i] = Physics2D.Raycast (origin, direction, 1.0f + skinWidth, mask);

			//Debugging...
			Debug.DrawRay((Vector3)origin, direction, Color.blue);
		}

		//Raycast down...
		for (int i = 0; i < verticalRaycasts; i++)
		{
			Vector2 origin = (Vector2)transform.position;
			origin.x -= width;
			origin.x += (i / verticalRaycasts) * width;
			origin.y -= height / 2.0f + skinWidth;

			Vector2 direction = -(Vector2)this.transform.up;

			hits [i] = Physics2D.Raycast (origin, direction, 1.0f + skinWidth, mask);

			//Debugging...
			Debug.DrawRay((Vector3)origin, direction, Color.red);
		}
	}

	private void GetRaycastHitsHorizontal(LayerMask mask)
	{
	}
}
