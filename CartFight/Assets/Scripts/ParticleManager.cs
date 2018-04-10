using UnityEngine;
using System.Collections;

/// <summary>
/// Controls particle spawning and system parameters.
/// </summary>
public class ParticleManager : MonoBehaviour 
{
	////////// VARIABLES //////////

	public static ParticleManager instance;

	////////// PRIMARY METHODS //////////

	private void Awake()
	{
		if (instance != null && instance != this) 
		{
			Destroy (this.gameObject);
		} 
		else if (instance == null)
		{
			instance = this;
		}
	}

	////////// CUSTOM METHODS //////////

	public void CreateParticles(string name, Vector3 position, Quaternion rotation)
	{
		ParticleSystem system = ((GameObject)Instantiate (Resources.Load (name), 
			position, rotation)).GetComponent<ParticleSystem> ();
		system.transform.SetParent (instance.transform);
		system.Play ();
	}

	public void CreateParticles(string name, Transform parent, Vector3 position, Quaternion rotation)
	{
		ParticleSystem system = ((GameObject)Instantiate (Resources.Load (name), 
			position, rotation)).GetComponent<ParticleSystem> ();
		system.transform.SetParent (parent);
		system.Play ();
    }

    public void CreateParticles(string name, Vector3 position, Quaternion rotation, Color startColor)
	{
		ParticleSystem system = ((GameObject)Instantiate (Resources.Load (name), 
			position, rotation)).GetComponent<ParticleSystem> ();
		system.transform.SetParent (this.transform);
		system.startColor = startColor;
		system.Play ();
	}

	public void CreateParticles(string name, Transform parent, Vector3 position, Quaternion rotation, Color startColor)
	{
		ParticleSystem system = ((GameObject)Instantiate (Resources.Load (name), 
			position, rotation)).GetComponent<ParticleSystem> ();
		system.transform.SetParent (parent);
		system.startColor = startColor;
		system.Play ();
	}

	//Creates two sets of sparks orthogonal to the normal of a collision point.
	public void CreateSparksAtCollision(Collision2D other)
	{
		Quaternion angle_1 = Quaternion.LookRotation (other.contacts [0].normal * -1f, -Vector3.forward);
		Quaternion angle_2 = Quaternion.LookRotation (other.contacts [0].normal, -Vector3.forward);
		ParticleManager.instance.CreateParticles ("Sparks", other.contacts [0].point, angle_1);
		ParticleManager.instance.CreateParticles ("Sparks", other.contacts [0].point, angle_2);
	}

	/*
	public void CreateDecal(string name, Vector3 position, Quaternion rotation)
	{
		GameObject decal = (GameObject)Instantiate (Resources.Load (name), position, rotation);
		decal.transform.SetParent (this.transform);
	}
	*/
}
