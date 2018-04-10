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

    private List<SpawnPoint> bombSpawnPoints; //The spawn points for bombs.
    private List<SpawnPoint> availableBombSpawnPoints;
    private List<Bomb> currentBombs; //All the bombs in play at the moment.

    private Dictionary<GameObject, float> abandonedCarts; //The abandoned carts in the game, paired with their age.

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
    
    //TODO: Add a random option. The issue here comes from selecting retry on a random level.
	public enum GameLevels
	{
		FaceOff,
		FastTrack,
		Convenience,
        Arena,
		LevelCount //The number of levels in this enum. Must be last.
	};

	public struct PlayerData
	{
		public Player player;
		public int points;
	}

    /// <summary>
    /// A list of settings that are used to determine gameplay parameters.
    /// </summary>
	public struct GameSettings
    {
        //Declarations//
        //Normal stuff.
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

        //Mutator stuff.
        private bool useSoulboundCarts; //Whether or not to use soulbound carts.
        private bool useCartLimit; //Do we limit the (abandoned) carts in the game?
        private int cartLimit; //How many abandoned carts are we limited to?
        private int bombCount; //How many bombs will be in play at once. 
        private int killValue; //How much each kill is worth (if they're being tracked).

        /// <summary>
        /// How we handle kills in terms of score.
        /// None: Kills do not count towards score.
        /// Include: Kills are included in score calculations.
        /// Solo: Kills are the only way to score points. Regular score calculation is ignored.
        /// </summary>
        public enum KillsMode
        {
            None,
            Include,
            Solo
        };
        private KillsMode killsMode;
        //End declarations//

        //Accessors//
        //Normal stuff.
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

        //Mutator stuff.
        public bool UseSoulboundCarts { get { return this.useSoulboundCarts; } set { this.useSoulboundCarts = value; } }
        public bool UseCartLimit { get { return this.useCartLimit; } set { this.useCartLimit = value; } }
        public int CartLimit
        {
            get { return this.cartLimit; }
            set
            {
                this.cartLimit = value;
                if (cartLimit <= 1) { cartLimit = 1; }
                if (cartLimit >= 100) { cartLimit = 100; }
            }
        }
        public int BombCount { get { return this.bombCount; } set { this.bombCount = value;
                if (this.bombCount <= 0) this.bombCount = 0; } }
        public int KillValue { get { return this.killValue; } set { this.killValue = value;
                if (this.killValue <= 0) this.killValue = 0; } }
        public KillsMode Kills { get { return this.killsMode; } set { this.killsMode = value; } }
        //End accessors//

        public override string ToString()
        {
            return string.Format("Normal Settings: " +
                "\n\tTime Limit: {0}" +
                "\n\tScore Limit: {1}" +
                "\n\tItem Count: {2}" +
                "\n\tUse Time Limit: {3}" +
                "\n\tUse Score Limit: {4}" +
                "\n\tLevel: {5}" +
                "\n\tGame Mode: {6}" +
                "\nMutators: " +
                "\n\tUse Soulbound Carts: {7}" +
                "\n\tUse Cart Limit: {8}" +
                "\n\tCart Limit: {9}" +
                "\n\tBomb Count: {10}" +
                "\n\tKills Mode: {11}" +
                "\n\tKill Value: {12}",
                this.timeLimit, this.scoreLimit, this.itemCount, this.useTimeLimit, this.useScoreLimit,
                this.level.ToString(), this.gameMode.ToString(), this.useSoulboundCarts, this.useCartLimit,
                this.cartLimit, this.bombCount, this.killsMode.ToString(), this.killValue);
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
        GetBombSpawnPoints();

        //At start, all player/item spawn points are available.
        availablePlayerSpawnPoints = new List<SpawnPoint>();
		availableItemSpawnPoints = new List<SpawnPoint>();
        availableBombSpawnPoints = new List<SpawnPoint>();

        //Update spawn points.
        UpdateAvailablePlayerSpawns();
        UpdateAvailableItemSpawns();
        UpdateAvailableBombSpawns();

        //Instantiate as empty.
        abandonedCarts = new Dictionary<GameObject, float>();

        //Set up players from the static joined players list from the lobby manager.
        players = new PlayerData[LobbyManager.JoinedPlayerData.Count];
		for (int i = 0; i < players.Length; i++) 
		{
			players [i].player = SpawnPlayer (LobbyManager.JoinedPlayerData[i].PlayerNumber, 
				LobbyManager.JoinedPlayerData[i].Controls, 0.0f);
		}

        //Spawn the items.
        SpawnStartingItems(settings.ItemCount);

        currentBombs = new List<Bomb>();

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
		//This is handled here (not in ControlScheme.cs) because we want to
		//be able to pause even when the player trying to pause is dead.
		for (int i = 0; i < pauseKeys.Length; i++)
		{
			if (Input.GetKeyDown (pauseKeys [i])) 
			{
				SetPaused (!isPaused);
				break;
			}
		}

		//Handle pausing for gamepads outside of the (volatile) control scheme class.
		for (int i = 0; i < currPauseStates.Length; i++) 
		{
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
        if (settings.Kills != GameSettings.KillsMode.Solo)
        {
            if (pointTimer >= pointInterval)
            {
                pointTimer = 0.0f;
                for (int i = 0; i < players.Length; i++)
                {
                    AddPointsToPlayer(players[i].player, players[i].player.carriedItems.Count);
                }
            }
        }
        
        //Check to see if anyone has won the game.
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].points >= settings.ScoreLimit && settings.UseScoreLimit)
            {
                players[i].points = settings.ScoreLimit;
                GUI.instance.UpdateScoreTexts();
                GUI.instance.TimeText.gameObject.SetActive(false);

                GoToResultsMenu();
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

    //Do list updating here if it's not super important. Keeps us from scrubbing lists > 60 times a second.
    private void FixedUpdate()
    {
        //Update the abandoned carts list and act on the results.
        GameObject[] tmp = GameObject.FindGameObjectsWithTag("Cart");
        foreach(GameObject pc in tmp)
        {
            if (pc.transform.parent == null && !abandonedCarts.ContainsKey(pc)) //Cart is abandoned and not in the list.
            {
                abandonedCarts.Add(pc, 0.0f);
            }
            else if (pc.transform.parent == null && abandonedCarts.ContainsKey(pc)) //Cart is abandoned and in the list.
            {
                abandonedCarts[pc] += Time.deltaTime;
            }
            else if (pc.transform.parent != null && abandonedCarts.ContainsKey(pc)) //Cart is not abandoned and in the list.
            {
                abandonedCarts.Remove(pc);
            }
        }

        //If we need to remove a cart, only remove them if they're the oldest 
        //and have been alive for more than a second.
        if (abandonedCarts.Count > settings.CartLimit && settings.UseCartLimit)
        {
            if (abandonedCarts.Count > 0)
            {
                KeyValuePair<GameObject, float> oldestPair =
                    new KeyValuePair<GameObject, float>(null, float.NegativeInfinity);
                foreach (KeyValuePair<GameObject, float> pair in abandonedCarts)
                {
                    if(pair.Value > oldestPair.Value)
                    {
                        oldestPair = pair;
                    }
                }
                if(oldestPair.Value >= 1.0f)
                {
                    //TODO: Make an actual method in cart or some shit. Pretty this up with fading or explosions.
                    //Also note that this creates a huge issue with soulbound carts when the player is recalling their 
                    //cart after it's been destroyed. Fix that shit.
                    GameObject particles = GameObject.Instantiate(Resources.Load("RemoveCartParticles")) as GameObject;
                    particles.transform.position = oldestPair.Key.transform.position;
                    foreach(Item i in oldestPair.Key.GetComponentsInChildren<Item>())
                    {
                        i.GetRemovedFromCart();
                    }
                    abandonedCarts.Remove(oldestPair.Key);
                    GameObject.Destroy(oldestPair.Key);
                }
            }
        }

        //Add new bombs, remove when null (exploded).
        currentBombs = currentBombs.Where(x => x != null).ToList();
        if(currentBombs.Count < settings.BombCount)
        {
            //Magic number: The length of time to wait before showing the bomb because of the
            //hardcoded particle system times.
            currentBombs.Add(SpawnBomb(2.25f));
        }
    }

    ///////// Custom Methods //////////

    //Opens the results menu, which in turn marks the end of the current game.
    private void GoToResultsMenu()
	{
		AudioManager.instance.StopMusic ();
		SceneManager.LoadScene ("Results");
	}

	//Adds points to a player in the players list.
	//(Be sure that our players list always stays up to date to avoid errors...)
	public void AddPointsToPlayer(Player player, int value)
	{
		for (int i = 0; i < players.Length; i++) 
		{
			if (players [i].player.Equals(player)) 
			{
				players [i].points += value;
			}
		}
        GUI.instance.UpdateScoreTexts();
    }

    //Spawns all of the items for the start (and duration) of the game.
    //This doesn't define any spawn behaviours, it simply calls them.
    private void SpawnStartingItems(int count)
	{
		//Pick a random number from 0 to the number of item types, then we'll use this
		//as the offset to cycle item types when spawning items.
		int numItemTypes = System.Enum.GetNames(typeof(Item.ItemType)).Length;
		int itemTypeCounter = Random.Range (0, numItemTypes - 1); //- 1 because bombs will always be the last option.

        for (int i = 0; i < count; i++) 
		{
            //Pick an item type. Just grab the next type in the list, cycling through it.
            Item.ItemType type = (Item.ItemType)(itemTypeCounter);
			SpawnItem (type, 0.0f); //Spawn it immediately.

            //Increment.
            itemTypeCounter++;
            //Clamp.
			itemTypeCounter = itemTypeCounter % (numItemTypes - 1); //- 1 because bombs will always be the last option.
        }
	}

    #region SpawningMethods
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
        //^THIS.
        UpdateAvailablePlayerSpawns (); //Ensure that our available spawns list is accurate.
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
    
    public Bomb SpawnBomb(float seconds)
    {
        Bomb newBomb;

        UpdateAvailableBombSpawns();
        int spawnerIndex = Random.Range(0, availableBombSpawnPoints.Count);
        newBomb = availableBombSpawnPoints[spawnerIndex].SpawnBomb(seconds);
        
        return newBomb;
    }
    #endregion

    #region GetSpawnPoints
    private void GetPlayerSpawnPoints()
	{
		GameObject[] temp = GameObject.FindGameObjectsWithTag("SpawnPoint");
		playerSpawnPoints = new List<SpawnPoint> ();
		for (int i = 0; i < temp.Length; i++)
		{
			SpawnPoint spawnPoint = temp [i].GetComponent<SpawnPoint> ();
			if (spawnPoint.isPlayerSpawn && !spawnPoint.isBombSpawn) 
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
			if (!spawnPoint.isPlayerSpawn && !spawnPoint.isBombSpawn) 
			{
				itemSpawnPoints.Add (spawnPoint);
			}
		}
	}

    private void GetBombSpawnPoints()
    {
        GameObject[] temp = GameObject.FindGameObjectsWithTag("SpawnPoint");
        bombSpawnPoints = new List<SpawnPoint>();
        for(int i = 0; i < temp.Length; i++)
        {
            SpawnPoint spawnPoint = temp[i].GetComponent<SpawnPoint>();
            if(!spawnPoint.isPlayerSpawn && spawnPoint.isBombSpawn)
            {
                bombSpawnPoints.Add(spawnPoint);
            }
        }
    }
    #endregion

    #region UpdateSpawnPoints
    //Checks updates the list of spawn points available for player respawns.
    private void UpdateAvailablePlayerSpawns()
	{
		for (int i = 0; i < playerSpawnPoints.Count; i++) 
		{
			if (playerSpawnPoints [i].isAvailable) 
			{
				//If an available spawn point isn't listed in the available list...
				if (!availablePlayerSpawnPoints.Contains(playerSpawnPoints[i])) 
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

        //Debugging...
        //Debug.Log("Player Spawn Points = " + playerSpawnPoints.Count);
        //Debug.Log("Available Player Spawn Points = " + availablePlayerSpawnPoints.Count);
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

    private void UpdateAvailableBombSpawns()
    {
        int availableCount = 0;
        for (int i = 0; i < bombSpawnPoints.Count; i++)
        {
            if (bombSpawnPoints[i].isAvailable)
            {
                //If an available spawn point isn't listed in the available list...
                if (!availableBombSpawnPoints.Contains(bombSpawnPoints[i]))
                {
                    availableBombSpawnPoints.Add(bombSpawnPoints[i]);
                }

                availableCount++;
            }
            else
            {
                //If an unavailable spawn point is listed in the available list...
                if (availableBombSpawnPoints.Contains(bombSpawnPoints[i]))
                {
                    availableBombSpawnPoints.Remove(bombSpawnPoints[i]);
                }
            }
        }

        //No available spawners...
        if (availableCount == 0)
        {
            Debug.Log("We've run out of bomb spawners! Create more or lower the bomb count.");
        }
    }
    #endregion

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
        GetBombSpawnPoints ();

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

        //Draw bomb spawn points...
        if (bombSpawnPoints.Count > 0)
        {
            for (int i = 0; i < bombSpawnPoints.Count; i++)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(bombSpawnPoints[i].transform.position, 0.25f);
            }
        }
    }
}