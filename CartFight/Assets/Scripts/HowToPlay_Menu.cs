using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class HowToPlay_Menu : Menu 
{
	public List<MovieTexture> movies;
	public RectTransform[] instructionsElements;
	public RectTransform controlsGrid;

	public Button backButton; //The button that returns us to the main menu.
	public Button switchButton; //The button that changes between controls and gameplay.
	private bool controlMenu = false; //Whether or not we're in the control menu. Alternative is the gameplay menu.

	private bool settingInstructionsActive = false;
	private bool settingControlsActive = false;

	public override void Start()
	{
		base.Start ();

		for (int i = 0; i < movies.Count; i++) 
		{
			movies [i].loop = true;
			movies [i].Play ();
		}

		base.SelectSilently (switchButton);

        base.onBackButtonPressed += this.ReturnToMenu;

        StartCoroutine(SetControlsActive(false, 0.0f));
    }

	public override void Update()
	{
        base.Update();

		if (controlMenu) 
		{
			switchButton.GetComponentInChildren<Text> ().text = "Gameplay";
		} 
		else
		{
			switchButton.GetComponentInChildren<Text>().text = "Controls";
		}
	}

	public void ReturnToMenu()
	{
		SceneManager.LoadScene ("MainMenu");
	}

	public void SwitchMenu()
	{
		StartCoroutine (SwitchMenu_Coroutine (.4f, .25f));
	}

	private IEnumerator SwitchMenu_Coroutine(float instructionDuration, float controlDuration)
	{
		//Switch the menu bool.
		controlMenu = !controlMenu;

		if (controlMenu) 
		{
			//Set instructions.
			if (settingInstructionsActive) 
			{
				StopCoroutine ("SetInstructionsActive");
			}
			StartCoroutine (SetInstructionsActive (!controlMenu, instructionDuration));
		}
		else
		{
			//Set controls.
			if (settingControlsActive) 
			{
				StopCoroutine ("SetControlsActive");
			}
			StartCoroutine (SetControlsActive (controlMenu, controlDuration));
		}

		//Wait a bit.
		float waitTime = (controlMenu) ? instructionDuration : controlDuration;
		yield return new WaitForSeconds(waitTime);

		//And a bit more.
		yield return new WaitForSeconds(.1f);

		if (!controlMenu) 
		{
			//Set instructions.
			if (settingInstructionsActive) 
			{
				StopCoroutine ("SetInstructionsActive");
			}
			StartCoroutine (SetInstructionsActive (!controlMenu, instructionDuration));
		}
		else
		{
			//Set controls.
			if (settingControlsActive) 
			{
				StopCoroutine ("SetControlsActive");
			}
			StartCoroutine (SetControlsActive (controlMenu, controlDuration));
		}
	}

	private IEnumerator SetInstructionsActive(bool active, float duration)
	{
		settingInstructionsActive = true; //For controlling state.

		//Get the starting positions of the instruction elements.
		Vector3[] startingPositions = new Vector3[instructionsElements.Length];
		for (int i = 0; i < startingPositions.Length; i++) 
		{
			startingPositions [i] = instructionsElements [i].anchoredPosition;
		}

		//Get the end positions of the gameplay elements.
		Vector3[] endPositions = new Vector3 [instructionsElements.Length];
		for (int i = 0; i < endPositions.Length; i++) 
		{
			if (active) 
			{
				Vector3 temp = startingPositions [i];
				temp.x = 0.0f;
				endPositions [i] = temp;
			} 
			else 
			{
				float temp = instructionsElements [i].rect.width;
				temp *= ((i + 1) % 2 == 0) ? 1.0f : -1.0f; //Every other elements goes to the left.

				endPositions [i] = startingPositions[i];
				endPositions [i].x += temp;
			}
		}

		//Lerp instruction elements.
		float timer = 0.0f;
		while (timer <= duration) 
		{
			float percent = timer / duration;
			for (int i = 0; i < instructionsElements.Length; i++) 
			{
				instructionsElements [i].anchoredPosition = 
					Vector3.Lerp (startingPositions [i], endPositions [i], percent);
			}

			timer += Time.deltaTime;
			yield return null;
		}

		//Set instruction elements to their end values.
		for (int i = 0; i < instructionsElements.Length; i++) 
		{
			instructionsElements [i].anchoredPosition = endPositions [i];
		}

		settingInstructionsActive = false;
	}

	private IEnumerator SetControlsActive(bool active, float duration)
	{
		settingControlsActive = true; //For controlling state.

		//Get the starting opacity of the controls elements.
		Text[] controlsTexts = controlsGrid.GetComponentsInChildren<Text> ();
		Image[] controlsImages = controlsGrid.GetComponentsInChildren<Image> ();
		float[] textStartingAlphas = new float[controlsTexts.Length];
		float[] imageStartingAphas = new float[controlsImages.Length];
		for (int i = 0; i < textStartingAlphas.Length; i++) 
		{
			textStartingAlphas [i] = controlsTexts [i].color.a;
		}
		for (int i = 0; i < imageStartingAphas.Length; i++) 
		{
			imageStartingAphas [i] = controlsImages [i].color.a;
		}

		//Get the end opacity of the controls elements.
		float[] textEndAlphas = new float[controlsTexts.Length];
		float[] imageEndAlphas = new float[controlsImages.Length];
		for (int i = 0; i < textEndAlphas.Length; i++) 
		{
			textEndAlphas [i] = (active) ? 1.0f : 0.0f;
		}
		for (int i = 0; i < imageEndAlphas.Length; i++) 
		{
			imageEndAlphas [i] = (active) ? 1.0f : 0.0f;
		}

		//Lerp control elements.
		float timer = 0.0f;
		while (timer < duration) 
		{
			float percent = timer / duration;
			for (int i = 0; i < controlsTexts.Length; i++) 
			{
				Color temp = controlsTexts [i].color;
				temp.a = Mathf.Lerp (textStartingAlphas [i], textEndAlphas [i], percent);
				controlsTexts [i].color = temp;
			}
			for (int i = 0; i < controlsImages.Length; i++) 
			{
				Color temp = controlsImages [i].color;
				temp.a = Mathf.Lerp (imageStartingAphas [i], imageEndAlphas [i], percent);
				controlsImages [i].color = temp;
			}

			timer += Time.deltaTime;
			yield return null;
		}
        
		//Set control elements to their end values.
		for (int i = 0; i < controlsTexts.Length; i++) 
		{
			Color temp = controlsTexts [i].color;
			temp.a = textEndAlphas [i];
			controlsTexts [i].color = temp;
		}
		for (int i = 0; i < controlsImages.Length; i++) 
		{
			Color temp = controlsImages [i].color;
			temp.a = imageEndAlphas [i];
			controlsImages [i].color = temp;
		}

		settingControlsActive = false;
	}
}
