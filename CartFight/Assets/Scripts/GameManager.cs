using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

/// <summary>
/// Sets up rounds, chooses player/item spawns, keeps track of respawn times and locations,
/// and handles opening/closing the pause menu.
/// 
/// A note about player spawning: dead players are assigned a spawn point from the available
/// spawn points list (which is then unavailable to the other dead players for the duration
/// of the respawn.)
/// </summary>
public class GameManager : MonoBehaviour
{
	///////// Variables //////////

	private float gameTimer;

	private List<SpawnPoint> playerSpawnPoints; //The player spawn points on the map.
	private List<SpawnPoint> availablePlayerSpawnPoints; //The player spawn points currently accepting respawns.

	private List<SpawnPoint> itemSpawnPoints; //The spawn points for items.
	private List<SpawnPoint> availableItemSpawnPoints;

	public Canvas pauseMenu; //The pause menu GUI canvas.
	private bool isPaused;

	public float pointInterval; //How often we give out points, measured in seconds.
	private float pointTimer = 0.0f;

	private PlayerData[] players; //The active players in the scene.
	private GameSettings settings; //The settings for the game.

	public static GameManager instance;

	private KeyCode[] pauseKeys; //A list of all the players pause keys. Used to let the manager pause the game.
	private GamePadState[] currPauseStates; //A list of the current gamepad states of all the gamepad players. Used for pausing.
	private GamePadState[] prevPauseStates; //A list of the previous gamepad states of all the gamepad players. Used for pausing.

	///////// Custom Data //////////

	public enum GameLevels
	{
		FaceOff,
		FastTrack,
		Convenience,
		LevelCount //The number of levels in this enum. Must be last.
	};

	public struct PlayerData
	{
		public Player player;
		public int points;
	}

	public struct GameSettings
	{
		private int timeLimit;
		private int scoreLimit;
		private int itemCount;

		private bool useTimeLimit;
		private bool useScoreLimit;

		private GameManager.GameLevels level;

		public enum GameModes
		{
			Score,
			Time,
			ScoreAndTime
		};
		private GameModes gameMode;

		public int TimeLimit { get { return this.timeLimit; } set { timeLimit = value; 
				if (timeLimit <= 0) { timeLimit = 0; } } }
		public int ScoreLimit { get { return this.scoreLimit; } set { scoreLimit = value; 
				if (scoreLimit <= 0) { scoreLimit = 0; } } }
		public int ItemCount { get { return this.itemCount; } set { itemCount = value; 
				if (itemCount <= 0) { itemCount = 0; } } }
		public bool UseTimeLimit { get { return this.useTimeLimit; } }
		public bool UseScoreLimit { get { return this.useScoreLimit; } }
		public GameLevels Level { get { return this.level; } set { level = value; } }
		public GameModes Mode { 
			get { return this.gameMode; } 
			set 
			{ 
				switch (value) 
				{
				case GameModes.Score:
					useTimeLimit = false;
					useScoreLimit = true;
					break;
				case GameModes.Time:
					useTimeLimit = true;
					useScoreLimit = false;
					break;
				case GameModes.ScoreAndTime:
					useTimeLimit = true;
					useScoreLimit = true;
					break;
				}
				gameMode = value; 
			}
		}
	}

	///////// Accessors //////////

	public PlayerData[] Players() { return this.players; }
	public bool IsPaused { get { return this.isPaused; } }
	public GameSettings Settings { get { return this.settings; } }

	///////// Primary Methods //////////

	void Awake()
	{
		if (instance == null) 
		{
			instance = this;
		} 
		else 
		{
			Destroy (this.gameObject);
		}
	}

	void Start()
	{
		//Get the settings from the settings menu in the lobby.
		settings = LobbyManager.GameSettings;

		//Get the item/player spawn points procedurally.
		GetPlayerSpawnPoints();
		GetItemSpawnPoints ();

		//At start, all player/item spawn points are available.
		availablePlayerSpawnPoints = playerSpawnPoints;
		availableItemSpawnPoints = itemSpawnPoints;

		//Set up players from the static joined players list from the lobby manager.
		players = new PlayerData[LobbyManager.JoinedPlayerData.Count];
		for (int i = 0; i < players.Length; i++) 
		{
			players [i].player = SpawnPlayer (LobbyManager.JoinedPlayerData[i].PlayerNumber, 
				LobbyManager.JoinedPlayerData[i].Controls, 0.0f);
		}

		//Spawn the items.
		SpawnStartingItems (settings.ItemCount);
			
		//Grab all of the pause keys.
		pauseKeys = new KeyCode[players.Length];
		for (int i = 0; i < players.Length; i++) 
		{
			pauseKeys [i] = (players [i].player.controlScheme.PauseKey);
		}

		//Get all the gamepad states for pausing.
		int gamepadPlayerCount = 0;
		for (int i = 0; i < players.Length; i++) 
		{
			if (players [i].player.controlScheme.IsGamePad) 
			{
				gamepadPlayerCount++;
			}
		}
		currPauseStates = new GamePadState[gamepadPlayerCount];
		int gamepadPlayerNumber = 0;
		for (int i = 0; i < players.Length; i++)
		{
			if (players [i].player.controlScheme.IsGamePad) 
			{
				currPauseStates [gamepadPlayerNumber] = GamePad.GetState (players [i].player.controlScheme.PIndex);
				gamepadPlayerNumber++;
			}
		}
		prevPauseStates = new GamePadState[currPauseStates.Length];
			
		//Ignore collisions with invulnerable objects. This is because abandoned carts use the
		//built in physics system and not our custom event handlers, so we need to stop collisions
		//within the built in physics system. Blah. That's a nuisance. (6/17/17)
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Cart"), LayerMask.NameToLayer("Invulnerable"));
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Driver"), LayerMask.NameToLayer("Invulnerable"));

		pauseMenu.GetComponent<PauseMenu> ().Start ();
		SetPaused (false);
		AudioManager.instance.PlayMusic ("Videogame2");

		GUI.instance.Start ();
	}

	void Update()
	{
		//Pause the game if any players pause button is down.
		//This is handed here (not in ControlScheme.cs) because we want to
		//be able to pause even when the player trying to pause is dead.
		for (int i = 0; i < pauseKeys.Length; i++)
		{
			if (Input.GetKeyDown (pauseKeys [i])) 
			{
				SetPaused (!isPaused);
				break;
			}
		}
		///Handle pausing for gamepads outside of the (volatile) control scheme class.
		for (int i = 0; i < currPauseStates.Length; i++) 
		{
			if (players [i].player.playerNumber == Player.PlayerNumber.P1) 
			{
				Debug.Log ("CurrStart : " + currPauseStates[i].Buttons.Start);
				Debug.Log ("PrevStart : " + prevPauseStates[i].Buttons.Start + "\n\n\n\n");
			}

			if (currPauseStates [i].Buttons.Start == ButtonState.Pressed
			   && prevPauseStates [i].Buttons.Start == ButtonState.Released) 
			{
				SetPaused (!isPaused);
				break;
			}
		}

		//Update pause states.
		currPauseStates.CopyTo(prevPauseStates, 0);
		int gamepadPlayerNumber = 0;
		for (int i = 0; i < players.Length; i++)
		{
			if (players [i].player.controlScheme.IsGamePad) 
			{
				currPauseStates [gamepadPlayerNumber] = GamePad.GetState (players [i].player.controlScheme.PIndex);
				gamepadPlayerNumber++;
			}
		}

		if (isPaused)
			return;

		//Hand out points to all players with items.
		if (pointTimer >= pointInterval) 
		{
			pointTimer = 0.0f;
			for (int i = 0; i < players.Length; i++) 
			{
				AddPointsToPlayer (players [i].player, players [i].player.carriedItems.Count);
				GUI.instance.UpdateScoreTexts ();
			}

			//Check to see if anyone has won the game.
			for (int i = 0; i < players.Length; i++) 
			{
				if (players [i].points >= settings.ScoreLimit && settings.UseScoreLimit) 
				{
					players [i].points = settings.ScoreLimit;
					GUI.instance.UpdateScoreTexts ();
					GUI.instance.TimeText.gameObject.SetActive (false);

					GoToResultsMenu ();
				}
			}
		}

		if (settings.UseTimeLimit) 
		{
			//If time is up, end the game.
			if (gameTimer >= settings.TimeLimit) 
			{
				GUI.instance.UpdateScoreTexts ();
				GoToResultsMenu ();
			}

			gameTimer += Time.deltaTime;
			GUI.instance.FormatAndUpdateTimer (settings.TimeLimit - gameTimer);
		}

		if (!settings.UseTimeLimit) { GUI.instance.TimeText.gameObject.SetActive (false); }

		pointTimer += Time.deltaTime;
	}

	///////// Custom Methods //////////

	//Opens the results menu, which in turn marks the end of the current game.
	private void GoToResultsMenu()
	{
		AudioManager.instance.StopMusic ();
		SceneManager.LoadScene ("ResultsMenu");
	}

	//Adds points to a player in the players list.
	//(Be sure that our players list always stays up to date to avoid errors...)
	private void AddPointsToPlayer(Player player, int value)
	{
		for (int i = 0; i < players.Length; i++) 
		{
			if (players [i].player.Equals(player)) 
			{
				players [i].points += value;
			}
		}
	}

	//Spawns all of the items for the start (and duration) of the game.
	private void SpawnStartingItems(int count)
	{
		//Pick a random number from 0 to the number of item types, then we'll use this
		//as the offset to cycle item types when spawning items.
		int numItemTypes = System.Enum.GetNames(typeof(Item.ItemType)).Length;
		int itemTypeCounter = Random.Range (0, numItemTypes);

		for (int i = 0; i < count; i++) 
		{
			//Pick an item type. Just grab the next type in the list, cycling through it.
			Item.ItemType type = (Item.ItemType)(itemTypeCounter % numItemTypes);
			SpawnItem (type, 0.0f); //Spawn it immediately.

			itemTypeCounter++;
		}
	}

	private Item SpawnItem(Item.ItemType itemType, float seconds)
	{
		Item newItem;

		UpdateAvailableItemSpawns ();
		int spawnerIndex = Random.Range (0, availableItemSpawnPoints.Count);
		newItem = availableItemSpawnPoints [spawnerIndex].SpawnItem (itemType, seconds);

		return newItem;
	}

	public Player SpawnPlayer(Player.PlayerNumber pNumber, ControlScheme controls, float seconds)
	{
		Player newPlayer;

		//There may be an issue with choosing a random spawn point when two players die in
		//quick succession.
		UpdateAvailablePlayerSpawns (); //Ensure that our avalable spawns list is accurate.
		int spawnerIndex = Random.Range (0, availablePlayerSpawnPoints.Count); //Choose a random index from available spawns.
		newPlayer = availablePlayerSpawnPoints [spawnerIndex].SpawnPlayer (pNumber, seconds);
		newPlayer.controlScheme = controls;

		//Replace the player of the same player number in our players array with the new player.
		for (int i = 0; i < players.Length; i++)
		{
			if (players [i].player != null) 
			{
				if (players [i].player.playerNumber == newPlayer.playerNumber) 
				{
					PlayerData newPlayerData = new PlayerData ();
					newPlayerData.player = newPlayer;
					newPlayerData.points = players [i].points;
					players.SetValue (newPlayerData, i);
				}
			}
		}

		return newPlayer;
	}

	private void GetPlayerSpawnPoints()
	{
		GameObject[] temp = GameObject.FindGameObjectsWithTag("SpawnPoint");
		playerSpawnPoints = new List<SpawnPoint> ();
		for (int i = 0; i < temp.Length; i++)
		{
			SpawnPoint spawnPoint = temp [i].GetComponent<SpawnPoint> ();
			if (spawnPoint.isPlayerSpawn) 
			{
				playerSpawnPoints.Add (spawnPoint);
			}
		}
	}

	private void GetItemSpawnPoints()
	{
		GameObject[] temp = GameObject.FindGameObjectsWithTag("SpawnPoint");
		itemSpawnPoints = new List<SpawnPoint> ();
		for (int i = 0; i < temp.Length; i++)
		{
			SpawnPoint spawnPoint = temp [i].GetComponent<SpawnPoint> ();
			if (!spawnPoint.isPlayerSpawn) 
			{
				itemSpawnPoints.Add (spawnPoint);
			}
		}
	}

	//Checks updates the list of spawn points available for player respawns.
	private void UpdateAvailablePlayerSpawns()
	{
		for (int i = 0; i < playerSpawnPoints.Count; i++) 
		{
			if (playerSpawnPoints [i].isAvailable) 
			{
				//If an available spawn point isn't listed in the available list...
				if (!availablePlayerSpawnPoints.Contains (playerSpawnPoints [i])) 
				{
					availablePlayerSpawnPoints.Add (playerSpawnPoints [i]);
				}
			}
			else
			{
				//If an unavailable spawn point is listed in the available list...
				if (availablePlayerSpawnPoints.Contains (playerSpawnPoints [i])) 
				{
					availablePlayerSpawnPoints.Remove (playerSpawnPoints [i]);
				}
			}
		}
	}

	private void UpdateAvailableItemSpawns()
	{
		int availableCount = 0;
		for (int i = 0; i < itemSpawnPoints.Count; i++) 
		{
			if (itemSpawnPoints [i].isAvailable) 
			{
				//If an available spawn point isn't listed in the available list...
				if (!availableItemSpawnPoints.Contains (itemSpawnPoints [i])) 
				{
					availableItemSpawnPoints.Add (itemSpawnPoints [i]);
				}

				availableCount++;
			}
			else
			{
				//If an unavailable spawn point is listed in the available list...
				if (availableItemSpawnPoints.Contains (itemSpawnPoints [i])) 
				{
					availableItemSpawnPoints.Remove (itemSpawnPoints [i]);
				}
			}
		}

		//No available spawners...
		if (availableCount == 0) 
		{
			Debug.Log ("We've run out of item spawners! Create more or lower the item count.");
		}
	}

	public void SetPaused(bool isPaused)
	{
		this.isPaused = isPaused;

		//Pause all objects derived from pausable object.
		PausableObject[] pauseObjs = GameObject.FindObjectsOfType<PausableObject> ();
		for (int i = 0; i < pauseObjs.Length; i++) 
		{
			pauseObjs [i].setPaused (isPaused);
		}

		//Pause or unpause the pause menu appropriately.
		if (isPaused) 
		{
			pauseMenu.GetComponent<PauseMenu> ().OpenMenu ();
		} 
		else 
		{
			pauseMenu.GetComponent<PauseMenu> ().CloseMenu ();
		}
	}

	///////// Gizmos //////////

	void OnDrawGizmos ()
	{
		GetPlayerSpawnPoints ();
		GetItemSpawnPoints ();

		//Draw player spawn points...
		if (playerSpawnPoints.Count > 0) 
		{
			for (int i = 0; i < playerSpawnPoints.Count; i++) 
			{
				if (playerSpawnPoints [i].isAvailable) 
				{
					Gizmos.color = Color.gray;
					Gizmos.DrawWireSphere (playerSpawnPoints [i].transform.position, 0.25f);
				}
				else
				{	
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere (playerSpawnPoints [i].transform.position, 0.25f);
				}
				Gizmos.DrawLine (playerSpawnPoints[i].transform.position, 
					playerSpawnPoints[i].transform.position + playerSpawnPoints[i].transform.right);
			}
		}

		//Draw item spawn points...
		if (itemSpawnPoints.Count > 0) 
		{
			for (int i = 0; i < itemSpawnPoints.Count; i++) 
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere (itemSpawnPoints [i].transform.position, 0.25f);
			}
		}
	}
}