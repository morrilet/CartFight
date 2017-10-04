using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ResultsMenu : Menu 
{
	////////// Variables //////////
	public GameObject resultsPanelPrefab;
	private ResultsPanel[] resultsPanels;

	////////// Custom Data Types //////////
	private struct ResultsPanel
	{
		private RectTransform rectTransform;
		private Player.PlayerNumber playerNumber;
		private int score;
		private Text playerTextObj;
		private Text scoreTextObj;
		private Image[] stars;
		private Image outline;

		public RectTransform RectTransformComponent { 
			get { return this.rectTransform; } set { this.rectTransform = value; } }
		public Player.PlayerNumber PlayerNumber { get { return this.playerNumber; } }
		public int Score { get { return this.score; } }
		public Text PlayerTextObj { get { return this.playerTextObj; } set { this.playerTextObj = value; } }
		public Text ScoreTextObj { get { return this.scoreTextObj; } set { this.scoreTextObj = value; } }

		public ResultsPanel (RectTransform rectTransform, Player.PlayerNumber playerNumber, int score)
		{
			this.rectTransform = rectTransform;
			this.playerNumber = playerNumber;
			this.score = score;

			this.playerTextObj = this.rectTransform.FindChild ("PlayerNumber").GetComponent<Text> ();
			this.playerTextObj.text = "P" + ((int)this.playerNumber + 1);

			this.scoreTextObj = this.rectTransform.FindChild ("Score").GetComponent<Text> ();
			this.scoreTextObj.text = this.score.ToString ();

			this.outline = rectTransform.FindChild("Outline").GetComponent<Image>();
			this.outline.color = new Color32((byte)50, (byte)50, (byte)50, (byte)0); //Gray.

			stars = new Image[2];
			this.stars[0] = rectTransform.FindChild("Star_1").GetComponent<Image>();
			this.stars[1] = rectTransform.FindChild("Star_2").GetComponent<Image>();
			SetStarsActive(false);
		}

		public void SetStarsActive(bool active)
		{
			for (int i = 0; i < stars.Length; i++)
			{
				stars[i].gameObject.SetActive(active);
			}
		}

		//Sets the color of the outline.
		public void SetBorderColor(Color32 color)
		{
			outline.color = color;
		}
	}

	////////// Primary Methods //////////
	public override void Start()
	{
		base.Start ();

		InstantiateResultsPanels ();
		FormatResultsPanels ();
	}

	////////// Custom Methods //////////
	private void InstantiateResultsPanels()
	{
		GameManager.PlayerData[] players = GameManager.instance.Players ();
		List<GameManager.PlayerData> winners = GetWinners ();

		resultsPanels = new ResultsPanel[players.Length];
		for (int i = 0; i < players.Length; i++) 
		{
			GameObject tempObj = Instantiate (resultsPanelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			ResultsPanel tempPanel = new ResultsPanel (tempObj.GetComponent<RectTransform> (), 
				                         players [i].player.playerNumber, players [i].points);
			if (winners.Contains (players [i])) 
			{
				tempPanel.SetStarsActive (true);
				tempPanel.SetBorderColor (new Color32 ((byte)205, (byte)190, (byte)115, (byte)255)); //Gold.
			}
			tempPanel.RectTransformComponent.SetParent (this.transform);
			tempPanel.RectTransformComponent.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			resultsPanels [i] = tempPanel;
		}
	}

	//Set the final positions of the results panels. This could be done mathematically,
	//but for only three possible cases I figured it'd be fastest to just use if statements.
	private void FormatResultsPanels()
	{
		int startPos = 0;
		if (resultsPanels.Length == 2) 
		{
			startPos = -150;
		}
		if (resultsPanels.Length == 3)
		{
			startPos = -300;
		}
		if (resultsPanels.Length == 4) 
		{
			startPos = -450;
		}

		for (int i = 0; i < resultsPanels.Length; i++) 
		{
			RectTransform temp = resultsPanels [i].RectTransformComponent;
			temp.offsetMin = new Vector2 (-125, 274);
			temp.offsetMax = new Vector2 (125, -224);
			temp.anchoredPosition = new Vector2 (startPos + (300 * i), 25);
			resultsPanels [i].RectTransformComponent = temp;

			switch(resultsPanels[i].PlayerNumber)
			{
			case Player.PlayerNumber.P1:
				resultsPanels[i].RectTransformComponent.gameObject.GetComponent<Image>().color 
					= new Color32((byte)138, (byte)69, (byte)69, (byte)255); //P1 red.
				break;
			case Player.PlayerNumber.P2:
				resultsPanels[i].RectTransformComponent.gameObject.GetComponent<Image>().color 
					= new Color32((byte)69, (byte)95, (byte)138, (byte)255); //P2 blue.
				break;
			case Player.PlayerNumber.P3:
				resultsPanels[i].RectTransformComponent.gameObject.GetComponent<Image>().color 
					= new Color32((byte)69, (byte)138, (byte)84, (byte)255); //P3 green.
				break;
			case Player.PlayerNumber.P4:
				resultsPanels[i].RectTransformComponent.gameObject.GetComponent<Image>().color 
					= new Color32((byte)138, (byte)69, (byte)126, (byte)255); //P4 purple.
				break;
			default:
				resultsPanels[i].RectTransformComponent.gameObject.GetComponent<Image>().color 
					= new Color32((byte)50, (byte)50, (byte)50, (byte)255); //Default gray.
				break;
			}
		}
	}

	//Figure out which player(s) won the game. (There can technically be a tie.)
	private List<GameManager.PlayerData> GetWinners()
	{
		GameManager.PlayerData[] players = GameManager.instance.Players ();
		List<GameManager.PlayerData> winners = new List<GameManager.PlayerData>();

		//Get the highest score. In a game with a set score limit, this will (obviously) be that score limit.
		int highestScore = 0;
		for (int i = 0; i < players.Length; i++) 
		{
			if (players [i].points > highestScore) 
			{
				highestScore = players [i].points;
			}
		}

		//Figure out which players got that score and add them to the winners list.
		for (int i = 0; i < players.Length; i++) 
		{
			if (players [i].points >= highestScore) 
			{
				winners.Add (players [i]);
			}
		}

		return winners;
	}

	public void Retry()
	{
		SceneManager.LoadScene (GameSettingsMenu.Settings.Level.ToString());
	}

	public void Quit()
	{
		SceneManager.LoadScene ("MainMenu");
	}
}
