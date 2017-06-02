using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class FadeOutAndDestroy : MonoBehaviour 
{
	bool coroutineStarted = false;

	void Update()
	{
		if (this.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("Driver_Die_1")
		   || this.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("Driver_Die_2")
		   || this.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("Driver_Die_3")
		   || this.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).IsName ("Driver_Die_4")) 
		{
			if (this.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).normalizedTime >= 1.0f 
				&& !coroutineStarted) 
			{
				StartCoroutine (FadeAndDestroy_Coroutine (0.65f));
			}
		}
	}

	private IEnumerator FadeAndDestroy_Coroutine(float duration)
	{
		coroutineStarted = true;
		Color newColor = this.GetComponent<SpriteRenderer> ().color;

		for (float i = 0.0f; i < duration; i += Time.deltaTime) 
		{
			newColor.a = 1 - (i / duration);
			this.GetComponent<SpriteRenderer> ().color = newColor;
			yield return null;
		}

		Destroy (this.gameObject);
	}
}
