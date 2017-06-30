using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HintText_Controller : MonoBehaviour 
{
	private Text hint_Text;
	private Coroutine effectCoroutine;
	private Coroutine fadeCoroutine;

	void Start()
	{
		hint_Text = this.GetComponent<Text> ();
		effectCoroutine = null;

		Color startColor = hint_Text.color;
		startColor.a = 0.0f;
		hint_Text.color = startColor;
	}

	public void FadeIn(float duration)
	{
		if (fadeCoroutine != null) 
		{
			StopCoroutine (fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine (Fade_Coroutine (1.0f, duration, true));
	}

	public void FadeOut(float duration)
	{
		if (fadeCoroutine != null) 
		{
			StopCoroutine (fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine (Fade_Coroutine (0.0f, duration, false));
	}

	private IEnumerator Fade_Coroutine(float opacity, float duration, bool startEffectAfter)
	{
		if (effectCoroutine != null) 
		{
			StopCoroutine (effectCoroutine);
		}

		float startOpacity = hint_Text.color.a;

		float timer = 0.0f;
		while (timer < duration) 
		{
			float a = Mathf.Lerp(startOpacity, opacity, timer / duration);
			Color newColor = hint_Text.color;
			newColor.a = a;
			hint_Text.color = newColor;

			timer += Time.deltaTime;
			yield return null;
		}

		Color finalColor = hint_Text.color;
		finalColor.a = opacity;
		hint_Text.color = finalColor;

		if (startEffectAfter) 
		{
			effectCoroutine = StartCoroutine (Effect_Coroutine (2.0f));
		}
	}

	private IEnumerator Effect_Coroutine(float period)
	{
		float timer = 90.0f / period; //This ensures that the effect always starts at full opacity.

		while (true) 
		{
			float angle = (timer % 360) * period;
			float a = Mathf.Abs (Mathf.Sin (angle));

			Color newColor = hint_Text.color;
			newColor.a = a;
			hint_Text.color = newColor;

			timer += Time.deltaTime;

			timer = timer % 360;
			yield return null;
		}
	}
}
