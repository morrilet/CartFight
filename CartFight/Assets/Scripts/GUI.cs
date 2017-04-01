using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUI : MonoBehaviour 
{
	public Text p1ScoreText;
	public Text p2ScoreText;

	void Update()
	{
		p1ScoreText.text = "P1 : " + GameManager.instance.getPlayers () [0].points.ToString ();
		p2ScoreText.text = "P2 : " + GameManager.instance.getPlayers () [1].points.ToString ();
	}
}
