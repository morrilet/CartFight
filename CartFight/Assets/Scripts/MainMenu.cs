using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	private MenuDoor_Controller doorController;
	private HintText_Controller hintTextController;
	private static bool sceneLoadedBefore = false; //Whether or not we've loaded this scene before.

	private Button playButton;
	private Button quitButton;

	public void Start()
	{
		doorController = transform.GetComponentInChildren<MenuDoor_Controller> ();
		hintTextController = transform.GetComponentInChildren<HintText_Controller> ();

		playButton = GameObject.Find ("Play").GetComponent<Button> ();
		quitButton = GameObject.Find ("Quit").GetComponent<Button> ();

		if (!sceneLoadedBefore) 
		{
			sceneLoadedBefore = true;
			StartCoroutine (StartScene_Coroutine ());
		} 
		else 
		{
			doorController.gameObject.SetActive (false);
		}
	}

	public void Play()
	{
		SceneManager.LoadScene ("Lobby");
	}

	public void Quit()
	{
		StartCoroutine (Quit_Coroutine ());
	}

	private IEnumerator Quit_Coroutine()
	{
		doorController.gameObject.SetActive (true);

		doorController.CloseDoors ();
		while (doorController.DoorsOpen == true) 
		{
			yield return null;
		}
		yield return new WaitForSeconds (0.65f);
		Application.Quit ();
	}

	private IEnumerator StartScene_Coroutine()
	{
		playButton.interactable = false;
		quitButton.interactable = false;

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

		doorController.OpenDoors ();
		while (doorController.DoorsOpen != true) 
		{
			yield return null;
		}

		playButton.interactable = true;
		quitButton.interactable = true;

		playButton.Select ();
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
