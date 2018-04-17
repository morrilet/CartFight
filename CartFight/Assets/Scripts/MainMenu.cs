using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : Menu
{
	private MenuDoor_Controller doorController;
	private HintText_Controller hintTextController;
	private static bool sceneLoadedBefore = false; //Whether or not we've loaded this scene before.

	private Button playButton;
	private Button howToPlayButton;
	private Button quitButton;
	private Button settingsButton;

	public override void Start()
	{
		base.Start ();

		doorController = transform.GetComponentInChildren<MenuDoor_Controller> ();
		hintTextController = transform.GetComponentInChildren<HintText_Controller> ();

		playButton = GameObject.Find ("Play").GetComponent<Button> ();
		howToPlayButton = GameObject.Find ("HowToPlay").GetComponent<Button> ();
		quitButton = GameObject.Find ("Quit").GetComponent<Button> ();
		settingsButton = GameObject.Find ("Settings").GetComponent<Button> ();

		if (!sceneLoadedBefore) 
		{
			sceneLoadedBefore = true;
            AudioManager.instance.PlayMusic("Traffic_Loop");
            StartCoroutine (StartScene_Coroutine ());
		}
		else
		{
			doorController.gameObject.SetActive (false);
		}

		//Select it silently b/c it's the default button to select.
		SelectSilently(playButton);
	}

	public void Settings()
	{
		SceneManager.LoadScene ("SettingsMenu");
	}

	public void Play()
	{
		SceneManager.LoadScene ("Lobby");
	}

	public void HowToPlay()
	{
		SceneManager.LoadScene ("HowToPlay");
	}

	public void Quit()
	{
		StartCoroutine (Quit_Coroutine ());
	}

	private IEnumerator Quit_Coroutine()
	{
		base.SetAllButtonsSilent (true);

		playButton.interactable = false;
		howToPlayButton.interactable = false;
		quitButton.interactable = false;
		settingsButton.interactable = false;

		doorController.gameObject.SetActive (true);

        AudioManager.instance.FadeInMusic("Traffic_Loop", 0.5f);
        AudioManager.instance.PlayEffect("Doors_Close");
        
		doorController.CloseDoors ();
		while (doorController.DoorsOpen == true) 
		{
			yield return null;
		}

        AudioManager.instance.PlayEffect("Doors_Slam");

		yield return new WaitForSeconds (0.65f);
		Application.Quit ();
	}

	private IEnumerator StartScene_Coroutine()
	{
		playButton.interactable = false;
		howToPlayButton.interactable = false;
		quitButton.interactable = false;
		settingsButton.interactable = false;

		Coroutine hintTextCoroutine = StartCoroutine (StartHintText_Coroutine (1.5f));

		//yield return new WaitForSeconds (.7f);
		while (!Input.anyKeyDown) 
		{
			yield return null;
		}

		if (hintTextCoroutine != null) 
		{
			StopCoroutine (hintTextCoroutine);
		}
		hintTextController.FadeOut (0.35f);

        AudioManager.instance.PlayEffect("Doors_Open");
        AudioManager.instance.FadeOutMusic(0.5f);

		doorController.OpenDoors ();
		while (doorController.DoorsOpen != true) 
		{
			yield return null;
		}

        playButton.interactable = true;
		howToPlayButton.interactable = true;
		quitButton.interactable = true;
		settingsButton.interactable = true;

        //Select it silently b/c it's the default button to select.
        SelectButtonInEventSystem(playButton, true);
	}

	private IEnumerator StartHintText_Coroutine(float entryDelay)
	{
		float timer = 0.0f;
		while (timer < entryDelay) 
		{
			timer += Time.deltaTime;
			yield return null;
		}
		hintTextController.FadeIn (.75f);
	}
}
