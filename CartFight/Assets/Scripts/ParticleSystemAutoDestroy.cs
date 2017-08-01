using UnityEngine;
using System.Collections;

public class ParticleSystemAutoDestroy : MonoBehaviour 
{
	private ParticleSystem system;

	private void Start()
	{
		system = GetComponent<ParticleSystem> ();
	}

	private void Update()
	{
		if (system != null) 
		{
			if (!system.IsAlive ()) 
			{
				Destroy (this.gameObject);
			}
		}
	}
}
