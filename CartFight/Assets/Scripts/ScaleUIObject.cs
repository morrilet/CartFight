using UnityEngine;
using System.Collections;

public class ScaleUIObject : MonoBehaviour 
{
	////////// Variables //////////
	public float maxScale;
	public float minScale;
	private RectTransform rectTransform;

	////////// Primary Methods //////////
	void Start()
	{
		rectTransform = this.GetComponent<RectTransform> ();
	}

	void Update()
	{
		float newScale = Mathf.PingPong (Time.time, maxScale - minScale) + minScale;
		rectTransform.localScale = new Vector3 (newScale, newScale, 1.0f);
	}
}
