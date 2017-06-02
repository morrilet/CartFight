using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour 
{
	public void LoadMainMenu()
	{
		SceneManager.LoadScene ("MainMenu");
	}

	public void UnpauseGame()
	{
		GameManager.instance.SetPaused (false);
	}
}
