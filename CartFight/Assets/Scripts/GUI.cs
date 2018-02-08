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
		private Coroutine stressEffect; //A reference to the coroutine running our stress FX.
		private bool stressEffectActive;

		public ScoreText(Text text, Player.PlayerNumber playerNumber)
		{
			this.text = text;
			this.playerNumber = playerNumber;
			this.stressEffectActive = false;
			this.stressEffect = null;
		}

		public Text getText() { return this.text; }
		public void setText(Text text) { this.text.text = text.text; }
		public Player.PlayerNumber getPlayerNumber() { return this.playerNumber; }
		public Coroutine StressEffect { 
			get { return this.stressEffect; } 
			set { this.stressEffect = value; } }
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
    //TODO: Add a setting for Arena!
	public void Start()
	{
		InitializeScoreTexts ();
		switch (GameManager.instance.Settings.Level) 
		{
		case GameManager.GameLevels.FaceOff:
			FormatScoreTexts (new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2 (-200f, 75f), true);
			break;
		case GameManager.GameLevels.FastTrack:
			FormatScoreTexts (new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2 (-200f, 75f), true);
			break;
		case GameManager.GameLevels.Convenience:
			FormatScoreTexts (new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2 (150f, -75f), false);
			break;
		default:
			Debug.Log ("WARNING: Level " + GameManager.instance.Settings.Level.ToString () +
			" is not defined in the GUI. Setting score texts XY to default value.");
			FormatScoreTexts (new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2 (-200f, 75f), true);
			break;
		}
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

	/// <summary>
	/// Formats the score texts.
	/// </summary>
	/// <param name="scoresXY">Score text starting position: scores go UP from here for each player.</param>
	/// <param name="anchorMin">The minimum anchor to use for the RectTransform.</param>
	/// <param name="anchorMax">The maximum anchor to use for the RectTransform.</param>
	/// <param name="stackOnTop">Whether or not to stack scores on top of each other 
	/// or below each other from the origin.</param>
	private void FormatScoreTexts(Vector2 anchorMin, Vector2 anchorMax, Vector2 scoresXY, bool stackOnTop)
	{
		for (int i = scoreTexts.Length - 1; i >= 0; i--) 
		{
			Text tempText = scoreTexts [i].getText (); //Create a text obj to modify then feed back into scoreTexts.

			//Set the anchors.
			tempText.rectTransform.anchorMin = anchorMin;
			tempText.rectTransform.anchorMax = anchorMax;

			//Set the size and position.
			tempText.rectTransform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			tempText.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, 300.0f);
			tempText.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, 75.0f);

			if (stackOnTop) 
			{
				//We use scoreTexts.Length - i - 1 to make keep P1 at the top of the list
				//while the last player is at the origin. 75 is just the height.
				tempText.rectTransform.anchoredPosition = new Vector2 (scoresXY.x, 
					scoresXY.y + ((scoreTexts.Length - i - 1) * 75f));
			}
			else
			{
				//Use plain ol' i and half the height to move each text down.
				//This ensures that P1 is on the top (the origin) and that all other scores go below it.
				tempText.rectTransform.anchoredPosition = new Vector2 (scoresXY.x, 
					scoresXY.y + (i * -75f));
			}

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
						scoreTexts [j].StressEffect = ApplyStressEffect (scoreTexts [i].getText ());
						scoreTexts [j].StressEffectActive = true;
					}

					//If the player has no carried items at the moment,
					//remove any stress effect from their score text.
					if (scoreTexts [i].StressEffectActive && players [j].player.carriedItems.Count <= 0) 
					{
						//Manually stop the score texts stress FX coroutine.
						StopCoroutine (scoreTexts [i].StressEffect);
						RemoveStressEffect (scoreTexts [i].getText ());
						scoreTexts [i].StressEffectActive = false;
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

	//Returns a reference to the stress effect coroutine.
	private Coroutine ApplyStressEffect(Text text)
	{
		StopCoroutine (RemoveStressEffect_Coroutine (text));
		return StartCoroutine (ApplyStressEffect_Coroutine (text));
	}

	//Flashes the input text red and scales it along a sine wave.
	private IEnumerator ApplyStressEffect_Coroutine(Text text)
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
				if(!GameManager.instance.IsPaused)
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

	private void RemoveStressEffect(Text text)
	{
		//FX coroutine is stopped first where this is called.
		StartCoroutine (RemoveStressEffect_Coroutine (text));
	}

	//Lerps the text from its current color to its normal color over 1 second.
	private IEnumerator RemoveStressEffect_Coroutine(Text text)
	{
		Color startColor = text.color;
		Vector3 startScale = text.gameObject.GetComponent<RectTransform> ().localScale;

		//TODO: Don't hardcode these values. Eew.
		Color normalColor = new Color(50f/255f, 50f/255f, 50f/255f, 1);
		Vector3 normalScale = new Vector3(1.0f, 1.0f, 1.0f);

		float t = 0.0f; //Timer.

		while (t <= 1.0f)
		{
			float percent = t / 1.0f;
			percent = Mathf.Clamp01 (Mathf.Abs (percent));

			Color tempColor = Color.Lerp (startColor, normalColor, percent);
			Vector3 tempScale = Vector3.Lerp (startScale, normalScale, percent);

			text.color = tempColor;
			text.GetComponent<RectTransform> ().localScale = tempScale;

			//Is this line needed? Dunno. Ethan 8/18/17.
			yield return new WaitForEndOfFrame ();

			//Increment timer.
			t += Time.deltaTime;

			yield return null;
		}

		text.color = normalColor;
		text.rectTransform.localScale = normalScale;
	}
}
