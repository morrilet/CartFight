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
	private Image background;

	private float resumeOpenPos, resumeClosePos;
	private float backOpenPos, backClosePos;
	private float titleOpenPos, titleClosePos;
	private float backgroundOpenA, backgroundCloseA;

	void Start()
	{
		resumeElement = this.transform.FindChild ("Resume").GetComponent<RectTransform> ();
		backElement = this.transform.FindChild ("Quit").GetComponent<RectTransform> ();
		titleElement = this.transform.FindChild ("PauseText").GetComponent<RectTransform> ();
		background = this.transform.FindChild ("Background").GetComponent<Image> ();

		resumeOpenPos = 475f; resumeClosePos = -50f;
		backOpenPos = 325f; backClosePos = -200f;
		titleOpenPos = -300f; titleClosePos = 75f;
		backgroundOpenA = 200f; backgroundCloseA = 0f;

		SetChildrenActive (false);
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
		float backgroundStartA = background.color.a;

		Vector2 newResumePos = new Vector2 (0f, resumeStartPos);
		Vector2 newBackPos = new Vector2 (0f, backStartPos);
		Vector2 newTitlePos = new Vector2 (0f, titleStartPos);
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

			resumeElement.anchoredPosition = newResumePos;
			backElement.anchoredPosition = newBackPos;
			titleElement.anchoredPosition = newTitlePos;

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
		newBackgroundA = backgroundOpenA;

		resumeElement.anchoredPosition = newResumePos;
		backElement.anchoredPosition = newBackPos;
		titleElement.anchoredPosition = newTitlePos;

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
		float backgroundStartA = background.color.a;

		Vector2 newResumePos = new Vector2 (0f, resumeStartPos);
		Vector2 newBackPos = new Vector2 (0f, backStartPos);
		Vector2 newTitlePos = new Vector2 (0f, titleStartPos);
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

			resumeElement.anchoredPosition = newResumePos;
			backElement.anchoredPosition = newBackPos;
			titleElement.anchoredPosition = newTitlePos;

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
		newBackgroundA = backgroundCloseA;

		resumeElement.anchoredPosition = newResumePos;
		backElement.anchoredPosition = newBackPos;
		titleElement.anchoredPosition = newTitlePos;

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
