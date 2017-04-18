using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	public void Play()
	{
		SceneManager.LoadScene ("Lobby");
	}

	public void Quit()
	{
		Application.Quit ();
	}
}
