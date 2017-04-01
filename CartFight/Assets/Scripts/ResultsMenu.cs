using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ResultsMenu : MonoBehaviour 
{
	public Text p1_scoreText;
	public Text p2_scoreText;

	void Start()
	{
		p1_scoreText.text = GameManager.instance.getPlayers () [0].points.ToString ();
		p2_scoreText.text = GameManager.instance.getPlayers () [1].points.ToString ();
	}

	void Update()
	{
		Debug.Log (Time.timeScale);
	}

	public void Retry()
	{
		SceneManager.LoadScene ("Main");
	}

	public void Quit()
	{
		SceneManager.LoadScene ("MainMenu");
	}
}
