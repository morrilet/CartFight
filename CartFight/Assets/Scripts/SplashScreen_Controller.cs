using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen_Controller : MonoBehaviour 
{
	Text mainText, subText;

	public float fadeInDuration, fadeOutDelay, fadeOutDuration, loadDelay;
	public string sceneToLoad;

	private void Start()
	{
		mainText = GameObject.Find ("Text").GetComponent<Text> ();
		subText = GameObject.Find ("SubText").GetComponent<Text> ();

		StartCoroutine (Splash_Coroutine ());
	}

	private IEnumerator Splash_Coroutine()
	{
		Color newMainTextColor = mainText.color;
		newMainTextColor.a = 0.0f;
		Color newSubTextColor = subText.color;
		newMainTextColor.a = 0.0f;

		//Fade in.
		float timer = 0f;
		while (timer < fadeInDuration) 
		{
			float percent = timer / fadeInDuration;
			newMainTextColor.a = percent;
			mainText.color = newMainTextColor;
			newSubTextColor.a = percent;
			subText.color = newSubTextColor;

			timer += Time.deltaTime;
			yield return null;
		}
		newMainTextColor.a = 1.0f;
		mainText.color = newMainTextColor;
		newSubTextColor.a = 1.0f;
		subText.color = newSubTextColor;

		//Wait until it's time to fade out.
		yield return new WaitForSeconds (fadeOutDelay);

		//Fade out.
		timer = 0f;
		while (timer < fadeOutDuration) 
		{
			float percent = timer / fadeOutDuration;
			newMainTextColor.a = Mathf.Abs (percent - 1f);
			mainText.color = newMainTextColor;
			newSubTextColor.a = Mathf.Abs (percent - 1f);
			subText.color = newSubTextColor;

			timer += Time.deltaTime;
			yield return null;
		}

		//Wait to load the next scene.
		yield return new WaitForSeconds (loadDelay);

		//Load the next scene.
		SceneManager.LoadScene (sceneToLoad);
	}
}
