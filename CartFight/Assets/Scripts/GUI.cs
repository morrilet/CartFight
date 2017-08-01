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
	private bool timeStressEffectActive = false;

	////////// Accessors //////////
	public ScoreText[] ScoreTexts { get { return this.scoreTexts; } }
	public Text TimeText { get { return this.timeText; } }

	////////// Custom Data Types //////////
	public struct ScoreText
	{
		private Text text;
		private Player.PlayerNumber playerNumber;
		private bool stressEffectActive;

		public ScoreText(Text text, Player.PlayerNumber playerNumber)
		{
			this.text = text;
			this.playerNumber = playerNumber;
			this.stressEffectActive = false;
		}

		public Text getText() { return this.text; }
		public void setText(Text text) { this.text = text; }
		public Player.PlayerNumber getPlayerNumber() { return this.playerNumber; }
		public bool StressEffectActive { 
			get { return this.stressEffectActive; } 
			set { this.stressEffectActive = value; } }
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
	public void Start()
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

					//If a player has 90% or more of the score limit, 
					//apply a stress effect to their score text.
					if ((float)(players [j].points / (float)GameManager.instance.Settings.ScoreLimit) >= 0.9f
						&& !scoreTexts [i].StressEffectActive && GameManager.instance.Settings.UseScoreLimit) 
					{
						ApplyStressEffect (scoreTexts [i].getText ());
						scoreTexts [j].StressEffectActive = true;
					}
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

		//If we're 90% or more through the time limit, 
		//apply the stress effect to the time text.
		if (seconds <= 10f
			&& !timeStressEffectActive) 
		{
			ApplyStressEffect (timeText);
			timeStressEffectActive = true;
		}

		string mins = Mathf.Floor (seconds / 60.0f).ToString ();
		string secs = Mathf.Floor (seconds % 60.0f).ToString ();

		if (seconds <= 0.0f) 
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

	private void ApplyStressEffect(Text text)
	{
		Debug.Log ("Here!");
		StartCoroutine (StressEffect_Coroutine (text));
	}

	//Flashes the input text red and scales it along a sine wave.
	private IEnumerator StressEffect_Coroutine(Text text)
	{
		Color startColor = text.color;
		Vector3 startScale = text.gameObject.GetComponent<RectTransform> ().localScale;

		Color stressColor = new Color(1, 0, 0, 1);
		Vector3 stressScale = startScale * 1.25f;

		float t = 0.0f; //Timer, AKA angle on the sine wave. 0-360.

		while (true)
		{
			float period = 2.5f;
			float percent = (Mathf.Sin(t * period) * Mathf.Rad2Deg) / (Mathf.Sin(360f) * Mathf.Rad2Deg);
			percent = Mathf.Clamp01 (Mathf.Abs (percent));

			Color tempColor = Color.Lerp (startColor, stressColor, percent);
			Vector3 tempScale = Vector3.Lerp (startScale, stressScale, percent);

			text.color = tempColor;
			text.GetComponent<RectTransform> ().localScale = tempScale;

			yield return new WaitForEndOfFrame ();

			//Increment/clamp timer.
			if (t <= 180f)
			{
				t += Time.deltaTime;
			}
			else
			{
				t = 0.0f;
			}

			Debug.Log ("From StressFX: t = " + t + "\nFrom StressFX: percent = " + percent);
			yield return null;
		}
	}
}
