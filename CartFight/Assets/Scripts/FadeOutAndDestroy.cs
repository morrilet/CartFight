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

		float timer = 0.0f;

		while (timer <= duration) 
		{
			newColor.a = 1 - (timer / duration);
			this.GetComponent<SpriteRenderer> ().color = newColor;

			if (!GameManager.instance.IsPaused) 
			{
				timer += Time.deltaTime;
			}

			yield return null;
		}

		Destroy (this.gameObject);
	}
}
