using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Thanks to answers.unity3d.com/questions/461425/trail-rendering-for-car-skid-marks.html for this!
public class TrailEmitter : MonoBehaviour 
{
	//Store all live trails.
	private LinkedList<Trail> trails = new LinkedList<Trail> ();

	//Parameters.
	public float width = 0.1f;
	public float decayTime = 1f;
	public Material material;
	public int roughness = 0;
	public bool softSourceEnd = false;

	//Check if the most recent trail is active or not.
	public bool Active
	{
		get 
		{
			return (trails.Count == 0 ? false : (!trails.Last.Value.Finished));
		}
	}

	//Update once per frame.
	private void Update()
	{
		if (trails.Count == 0)
			return;

		LinkedListNode<Trail> t = trails.First;
		LinkedListNode<Trail> n;
		do 
		{
			n = t.Next;
			t.Value.Update ();
			if (t.Value.Dead)
				trails.Remove (t);
			t = n;
		} while(n != null);
	}

	//Make a new trail.
	public void NewTrail()
	{
		//Stop the old trail and start a new one.
		EndTrail ();
		trails.AddLast (new Trail(transform, material, decayTime, roughness, softSourceEnd, width));
	}

	public void EndTrail()
	{
		if (!Active)
			return;
		trails.Last.Value.Finish ();
	}
}
