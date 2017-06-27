using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUI : MonoBehaviour 
{
	////////// Variables //////////
	private ScoreText[] scoreTexts;
	[SerializeField]
	private Text scoreTextPrefab;
	[SerializeField]
	private Text timeText;

	////////// Accessors //////////
	public ScoreText[] ScoreTexts { get { return this.scoreTexts; } }
	public Text TimeText { get { return this.timeText; } }

	////////// Custom Data Types //////////
	public struct ScoreText
	{
		private Text text;
		private Player.PlayerNumber playerNumber;

		public ScoreText(Text text, Player.PlayerNumber playerNumber)
		{
			this.text = text;
			this.playerNumber = playerNumber;
		}

		public Text getText() { return this.text; }
		public void setText(Text text) { this.text = text; }
		public Player.PlayerNumber getPlayerNumber() { return this.playerNumber; }
	}
		
	////////// Singleton //////////
	public static GUI instance;
	void Awake()
	{
		if (instance == null) 
		{
			instance = this;
		} 
		else 
		{
			Destroy (this);
		}
	}


	////////// Primary Methods //////////
	void Start()
	{
		InitializeScoreTexts ();
		FormatScoreTexts ();
		UpdateScoreTexts ();
	}

	////////// Custom Methods //////////
	private void InitializeScoreTexts() //Sets up the score texts array.
	{
		GameManager.PlayerData[] players = GameManager.instance.Players ();
		scoreTexts = new ScoreText[players.Length];
		for (int i = 0; i < scoreTexts.Length; i++) 
		{
			Text newText = Instantiate (scoreTextPrefab, Vector3.zero, Quaternion.identity) as Text;
			newText.rectTransform.SetParent (this.transform); //Assuming GUI.cs is on the GUI canvas.
			newText.text = "{N/A} :: {N/A}";

			scoreTexts [i] = new ScoreText (newText, players[i].player.playerNumber);
		}
	}

	private void FormatScoreTexts() //Sets the location of each score text
	{
		for (int i = scoreTexts.Length - 1; i >= 0; i--)
		{
			Text tempText = scoreTexts [i].getText (); //Create a text obj to modify then feed back into scoreTexts.
			tempText.rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			tempText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300.0f);
			tempText.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, 75.0f);
			tempText.rectTransform.anchoredPosition = new Vector2 (-200, 75 + ((scoreTexts.Length - i - 1) * 75));

			scoreTexts [i].setText (tempText);
		}
	}

	public void UpdateScoreTexts() //Updates the score texts to reflect the scores kept in GameManager.
	{
		GameManager.PlayerData[] players = GameManager.instance.Players ();
		for (int i = 0; i < scoreTexts.Length; i++) 
		{
			Text tempText = scoreTexts [i].getText ();

			//Check the players to find the right player number.
			for (int j = 0; j < players.Length; j++) 
			{
				if (scoreTexts [i].getPlayerNumber () == players [j].player.playerNumber) 
				{
					tempText.text = scoreTexts [i].getPlayerNumber ().ToString ()
					+ " :: " + players [j].points;
				}
			}

			scoreTexts [i].setText (tempText);
		}
	}

	public void FormatAndUpdateTimer(float seconds)
	{
		if (!timeText.IsActive ()) 
		{
			timeText.gameObject.SetActive (true);
		}


		string mins = Mathf.Floor (seconds / 60.0f).ToString ();
		string secs = Mathf.Floor (seconds % 60.0f).ToString ();

		if (seconds <= 0) 
		{
			mins = "0";
			secs = "0";
		}

		if (int.Parse(secs) < 10) 
		{
			secs = "0" + secs;
		}

		timeText.text = mins + ":" + secs;
	}
}
