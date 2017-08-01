using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

//REWORK THIS! May need to handle the animation from code in order to get it to blend correctly.
//Also, be sure that the menu leaves before the game is unpaused.

public class PauseMenu : MonoBehaviour 
{
	public AnimationCurve elementCurve;
	public AnimationCurve backgroundCurve;

	public float backgroundOpenDuration;
	public float backgroundCloseDuration;
	public float elementOpenDuration;
	public float elementCloseDuration;

	private Coroutine animCoroutine;

	private RectTransform resumeElement;
	private RectTransform backElement;
	private RectTransform titleElement;
	private RectTransform musicVolElement;
	private RectTransform effectVolElement;
	private Image background;

	private Slider musicSlider, effectSlider;
	private Text musicText, effectText;

	private float resumeOpenPos, resumeClosePos;
	private float backOpenPos, backClosePos;
	private float titleOpenPos, titleClosePos;
	private float musicVolOpenPos, musicVolClosePos;
	private float effectVolOpenPos, effectVolClosePos;
	private float backgroundOpenA, backgroundCloseA;

	public void Start()
	{
		resumeElement = this.transform.FindChild ("Resume").GetComponent<RectTransform> ();
		backElement = this.transform.FindChild ("Quit").GetComponent<RectTransform> ();
		titleElement = this.transform.FindChild ("PauseText").GetComponent<RectTransform> ();
		musicVolElement = this.transform.FindChild ("MusicVolume_Setting").GetComponent<RectTransform> ();
		effectVolElement = this.transform.FindChild ("EffectVolume_Setting").GetComponent<RectTransform> ();
		background = this.transform.FindChild ("Background").GetComponent<Image> ();

		musicSlider = musicVolElement.GetComponentInChildren<Slider> ();
		musicText = musicVolElement.FindChild ("MusicSliderValue_Text").GetComponent<Text> ();
		effectSlider = effectVolElement.GetComponentInChildren<Slider> ();
		effectText = effectVolElement.FindChild ("EffectSliderValue_Text").GetComponent<Text> ();

		resumeOpenPos = 400f; resumeClosePos = -247.5f;
		backOpenPos = 250f; backClosePos = -397.5f;
		titleOpenPos = -300f; titleClosePos = 75f;
		musicVolOpenPos = 615f; musicVolClosePos = -32.5f;
		effectVolOpenPos = 540f; effectVolClosePos = -107.5f;
		backgroundOpenA = 200f; backgroundCloseA = 0f;

		musicSlider.value = AudioManager.instance.MusicVolume * 100f;
		effectSlider.value = AudioManager.instance.EffectVolume * 100f;
		musicText.text = musicSlider.value + "%";
		effectText.text = effectSlider.value + "%";

		SetChildrenActive (false);
	}

	private void Update ()
	{
		AudioManager.instance.MusicVolume = musicSlider.value / 100f;
		AudioManager.instance.EffectVolume = effectSlider.value / 100f;
		musicText.text = musicSlider.value + "%";
		effectText.text = effectSlider.value + "%";
	}

	public void ResumeGame()
	{
		GameManager.instance.SetPaused (false);
	}

	public void LoadMainMenu()
	{
		SceneManager.LoadScene ("MainMenu");
	}

	public void OpenMenu()
	{
		if(animCoroutine != null)
		{
			StopCoroutine (animCoroutine);
		}
		animCoroutine = StartCoroutine (OpenMenu_Coroutine ());

		resumeElement.GetComponentInChildren<Button> ().Select ();
	}

	public void CloseMenu()
	{
		if(animCoroutine != null)
		{
			StopCoroutine (animCoroutine);
		}
		animCoroutine = StartCoroutine (CloseMenu_Coroutine ());
	}

	private IEnumerator OpenMenu_Coroutine()
	{
		SetChildrenActive (true);

		//Get the starting values of each element.
		float resumeStartPos = resumeElement.anchoredPosition.y;
		float backStartPos = backElement.anchoredPosition.y;
		float titleStartPos = titleElement.anchoredPosition.y;
		float musicVolStartPos = musicVolElement.anchoredPosition.y;
		float effectVolStartPos = effectVolElement.anchoredPosition.y;
		float backgroundStartA = background.color.a;

		Vector2 newResumePos = new Vector2 (0f, resumeStartPos);
		Vector2 newBackPos = new Vector2 (0f, backStartPos);
		Vector2 newTitlePos = new Vector2 (0f, titleStartPos);
		Vector2 newMusicVolPos = new Vector2 (0f, musicVolStartPos);
		Vector2 newEffectVolPos = new Vector2 (0f, effectVolStartPos);
		float newBackgroundA = backgroundStartA;

		float timer = 0.0f;
		while (timer < elementOpenDuration || timer < backgroundOpenDuration) 
		{
			//Animate the elements.
			float elementPercent = timer / elementOpenDuration;
			newResumePos = new Vector2 (0f, 
				Mathf.Lerp (resumeStartPos, resumeOpenPos, elementCurve.Evaluate (elementPercent)));
			newBackPos = new Vector2 (0f, 
				Mathf.Lerp (backStartPos, backOpenPos, elementCurve.Evaluate (elementPercent)));
			newTitlePos = new Vector2 (0f, 
				Mathf.Lerp (titleStartPos, titleOpenPos, elementCurve.Evaluate (elementPercent)));
			newMusicVolPos = new Vector2 (0f,
				Mathf.Lerp (musicVolStartPos, musicVolOpenPos, elementCurve.Evaluate (elementPercent)));
			newEffectVolPos = new Vector2 (0f,
				Mathf.Lerp (effectVolStartPos, effectVolOpenPos, elementCurve.Evaluate (elementPercent)));

			resumeElement.anchoredPosition = newResumePos;
			backElement.anchoredPosition = newBackPos;
			titleElement.anchoredPosition = newTitlePos;
			musicVolElement.anchoredPosition = newMusicVolPos;
			effectVolElement.anchoredPosition = newEffectVolPos;

			//Animate the background.
			float backgroundPercent = timer / backgroundOpenDuration;
			newBackgroundA = Mathf.Lerp (backgroundStartA, backgroundOpenA, 
				backgroundCurve.Evaluate (backgroundPercent)) / 255f;

			Color newColor = background.color;
			newColor.a = newBackgroundA;
			background.color = newColor;

			timer += Time.deltaTime;
			yield return null;
		}

		newResumePos = new Vector2 (0f, resumeOpenPos);
		newBackPos = new Vector2 (0f, backOpenPos);
		newTitlePos = new Vector2 (0f, titleOpenPos);
		newMusicVolPos = new Vector2 (0f, musicVolOpenPos);
		newEffectVolPos = new Vector2 (0f, effectVolOpenPos);
		newBackgroundA = backgroundOpenA;

		resumeElement.anchoredPosition = newResumePos;
		backElement.anchoredPosition = newBackPos;
		titleElement.anchoredPosition = newTitlePos;
		musicVolElement.anchoredPosition = newMusicVolPos;
		effectVolElement.anchoredPosition = newEffectVolPos;

		Color finalColor = background.color;
		finalColor.a = newBackgroundA / 255f;
		background.color = finalColor;
	}

	private IEnumerator CloseMenu_Coroutine()
	{
		//Get the starting values of each element.
		float resumeStartPos = resumeElement.anchoredPosition.y;
		float backStartPos = backElement.anchoredPosition.y;
		float titleStartPos = titleElement.anchoredPosition.y;
		float musicVolStartPos = musicVolElement.anchoredPosition.y;
		float effectVolStartPos = effectVolElement.anchoredPosition.y;
		float backgroundStartA = background.color.a;

		Vector2 newResumePos = new Vector2 (0f, resumeStartPos);
		Vector2 newBackPos = new Vector2 (0f, backStartPos);
		Vector2 newTitlePos = new Vector2 (0f, titleStartPos);
		Vector2 newMusicVolPos = new Vector2 (0f, musicVolStartPos);
		Vector2 newEffectVolPos = new Vector2 (0f, effectVolStartPos);
		float newBackgroundA = backgroundStartA;

		float timer = 0.0f;
		while (timer < elementCloseDuration || timer < backgroundCloseDuration) 
		{
			//Animate the elements.
			float elementPercent = timer / elementCloseDuration;
			newResumePos = new Vector2 (0f, 
				Mathf.Lerp (resumeStartPos, resumeClosePos, elementCurve.Evaluate (elementPercent)));
			newBackPos = new Vector2 (0f, 
				Mathf.Lerp (backStartPos, backClosePos, elementCurve.Evaluate (elementPercent)));
			newTitlePos = new Vector2 (0f, 
				Mathf.Lerp (titleStartPos, titleClosePos, elementCurve.Evaluate (elementPercent)));
			newMusicVolPos = new Vector2 (0f,
				Mathf.Lerp (musicVolStartPos, musicVolClosePos, elementCurve.Evaluate (elementPercent)));
			newEffectVolPos = new Vector2 (0f,
				Mathf.Lerp (effectVolStartPos, effectVolClosePos, elementCurve.Evaluate (elementPercent)));

			resumeElement.anchoredPosition = newResumePos;
			backElement.anchoredPosition = newBackPos;
			titleElement.anchoredPosition = newTitlePos;
			musicVolElement.anchoredPosition = newMusicVolPos;
			effectVolElement.anchoredPosition = newEffectVolPos;

			//Animate the background.
			float backgroundPercent = timer / backgroundCloseDuration;
			newBackgroundA = Mathf.Lerp (backgroundStartA, backgroundCloseA, 
				backgroundCurve.Evaluate (backgroundPercent));

			Color newColor = background.color;
			newColor.a = newBackgroundA;
			background.color = newColor;

			timer += Time.deltaTime;
			yield return null;
		}

		newResumePos = new Vector2 (0f, resumeClosePos);
		newBackPos = new Vector2 (0f, backClosePos);
		newTitlePos = new Vector2 (0f, titleClosePos);
		newMusicVolPos = new Vector2 (0f, musicVolClosePos);
		newEffectVolPos = new Vector2 (0f, effectVolClosePos);
		newBackgroundA = backgroundCloseA;

		resumeElement.anchoredPosition = newResumePos;
		backElement.anchoredPosition = newBackPos;
		titleElement.anchoredPosition = newTitlePos;
		musicVolElement.anchoredPosition = newMusicVolPos;
		effectVolElement.anchoredPosition = newEffectVolPos;

		Color finalColor = background.color;
		finalColor.a = newBackgroundA / 255f;
		background.color = finalColor;

		SetChildrenActive (false);
	}

	private void SetChildrenActive(bool active)
	{
		for (int i = 0; i < this.transform.childCount; i++) 
		{
			this.transform.GetChild (i).gameObject.SetActive (active);
		}
	}
}
