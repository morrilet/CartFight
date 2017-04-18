using UnityEngine;
using System.Collections;

public class SpinUIObject : MonoBehaviour 
{
	////////// Variables //////////
	public float rotationSpeed;
	private RectTransform rectTransform;

	////////// Primary Methods //////////
	void Start()
	{
		rectTransform = this.GetComponent<RectTransform> ();
	}

	void Update()
	{
		rectTransform.RotateAround (rectTransform.position, Vector3.forward, rotationSpeed * Time.deltaTime);
	}
}
