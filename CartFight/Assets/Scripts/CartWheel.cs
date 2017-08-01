using UnityEngine;
using System.Collections;

public class CartWheel : MonoBehaviour 
{
	private TrailRenderer trail;
	private Collider2D coll;
	private float bloodDensity = 0f; //0-1.
	private float bloodDensityDampening = .1f;
	
	private void Start()
	{
		trail = this.GetComponent<TrailRenderer> ();
		coll = this.GetComponent<Collider2D> ();

		trail.sortingLayerName = "DeadPlayers";
		trail.sortingOrder = -1;
	}

	private void Update()
	{
		//Reduce density with time and dampening...
		if (bloodDensity > 0f) 
		{
			bloodDensity -= bloodDensityDampening * Time.deltaTime;
		} 
		else 
		{
			bloodDensity = 0f;
		}

		//Refill blood density if we're touching a blood pool.
		if (coll.IsTouchingLayers (LayerMask.NameToLayer ("BloodDecal"))) 
		{
			Debug.Log ("Touching BloodDecal!");
			bloodDensity = 1.0f;
		}

		//Apply the blood density to the trail renderer.
		Color newColor = trail.material.color;
		newColor.a = bloodDensity;
		trail.material.color = newColor;
	}
}
