using UnityEngine;
using System.Collections;

public class Camera_Controller : MonoBehaviour 
{
	public static Camera_Controller instance;

	private Coroutine shakeCoroutine;
	private Vector3 startPos;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		startPos = this.transform.position;
	}

	public void Shake(float duration, float intensity)
	{
		if (shakeCoroutine != null) 
		{
			StopCoroutine (shakeCoroutine);
			this.transform.position = startPos;
		}
		shakeCoroutine = StartCoroutine (Shake_Coroutine (duration, intensity));
	}

	private IEnumerator Shake_Coroutine(float duration, float intensity)
	{
		float timer = 0.0f;
		int ticks = 0;
		while (timer < duration) 
		{
			Vector3 newPos = startPos;

			//Every 4th tick we return to startPos. Keeps us from moving too much.
			if (ticks % 4 == 0) 
			{
				ticks = 0;
				newPos = startPos;
			} 
			else 
			{
				newPos = transform.position + (Vector3)(Random.insideUnitCircle * intensity);
				newPos.z = startPos.z;
			}

			this.transform.position = newPos;

			ticks ++;
			timer += Time.deltaTime;
			yield return null;
		}
		transform.position = startPos;
	}
}
