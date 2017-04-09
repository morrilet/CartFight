using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sets up rounds, chooses player/item spawns, keeps track of respawn times, etc.
/// </summary>
public class NetworkedGameManager : NetworkManager
{
	//When a player dies, they are assigned a respawn point that becomes unavailable to all other
	//players until they've been respawned.
	public List<SpawnPoint> playerSpawnPoints; //The player spawn points on the map.
	public List<SpawnPoint> availablePlayerSpawnPoints; //The player spawn points currently accepting respawns.

	//public List<SpawnPoint> itemSpawnPoints; //The spawn points for items.

	//Client, PlayerData(PlayerObj and Points).
	//It may be worth it in the future to make this a list and add a connection to the PlayerData struct.
	private Dictionary<NetworkConnection, PlayerData> clients = new Dictionary<NetworkConnection, PlayerData> ();
	public Dictionary<NetworkConnection, PlayerData> Clients { get { return clients; } }

	public struct PlayerData
	{
		public NetworkedPlayer player;
		public int points;
		/*public PlayerData(NetworkedPlayer player, int points)
		{
			this.player = player;
			this.points = points;
		}*/
	}
	//private PlayerData[] players; //The active players in the scene.
	//public PlayerData[] getPlayers() { return players; }

	public float pointInterval; //How often we apply points.
	private float pointTimer = 0.0f;

	public static NetworkedGameManager instance;
	void Awake()
	{
		//DontDestroyOnLoad (this.gameObject);
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
		//At start, all player spawn points are available.
		//availablePlayerSpawnPoints = playerSpawnPoints;

		//This player setup code is just here for now. Use a lobby in the future to link
		//players to the appropriate player number and input method.
		//players = new PlayerData[2];
		//for (int i = 0; i < players.Length; i++) 
		//{
			//players [i].player = SpawnPlayer ((Player.PlayerNumber)i, 0.0f);

			//NetworkServer.Spawn (players[i].player.gameObject);
			//Debug.Log (players[i].player.playerNumber.ToString() + " " + players[i].points);
		//}
	}

	/*
	public override void OnServerConnect(NetworkConnection conn)
	{
		base.OnServerConnect (conn);

		Debug.Log (conn.address + " JOINED");

		PlayerData newData = new PlayerData ();//new PlayerData (conn.playerControllers[0].gameObject.GetComponent<NetworkedPlayer>(), 0);
		newData.player = conn.playerControllers[0].gameObject.GetComponent<NetworkedPlayer>();
		newData.points = 0;
		clients.Add (conn, newData);
	}
	*/

	//This is called when the client joins and gets their player, not on a respawn.
	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerID)
	{
		base.OnServerAddPlayer (conn, playerControllerID);

		//Debug.Log ("Adding player for " + conn.address);

		PlayerData newData = new PlayerData ();//new PlayerData (conn.playerControllers[0].gameObject.GetComponent<NetworkedPlayer>(), 0);
		newData.player = conn.playerControllers[0].gameObject.GetComponent<NetworkedPlayer>();
		newData.points = 0;
		clients.Add (conn, newData);
		Debug.Log (newData.player.name);

		//NetworkedGUI.instance.RpcSyncScoreTexts (NetworkedGUI.instance.GenerateConnectionsArray());
		Debug.Log("Attempting to add score text for " + conn.address + "...");
		//NetworkedGUI.instance.RpcAddToScoreTexts(newData.player.gameObject, newData.player.GetComponent<NetworkIdentity>().netId);
		foreach (KeyValuePair<NetworkConnection, PlayerData> pair in Clients) 
		{
			//Should call NetworkedGUI.CreateGUITexts() here, but can't RPC from a networkManager object. GAAAHHH!!!
		}
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController playerController)
	{
		base.OnServerRemovePlayer (conn, playerController);

		Debug.Log ("Removing player for " + conn.address);
	}

	public void UpdatePlayerDataForConnection(NetworkConnection conn)
	{
		Debug.Log ("Updating player data for " + conn.address + "...");

		PlayerData newData = new PlayerData ();
		newData.player = conn.playerControllers [0].gameObject.GetComponent<NetworkedPlayer> ();
		newData.points = clients [conn].points;
		clients [conn] = newData;

		Debug.LogFormat ("Done! Player ({0}), Points ({1})", clients [conn].player.gameObject.name, clients [conn].points);
	}

	void Update()
	{
		//Debug.LogFormat ("{0} : {1} points | {2} : {3} points.", players[0].player.playerNumber, 
		//	players[0].points, players[1].player.playerNumber, players[1].points);
		//Debug.Log (players[0].player.playerNumber + " : " + "points");

		//Check for endgame conditions and respond appropriately.
		foreach (PlayerData playerData in clients.Values)
		{
			if (playerData.points >= 100)
			{
				Debug.Log("GAME HAS BEEN WON");
				//score = 100; Find a way to do this without editing score in this loop. Use another list.
				//SceneManager.LoadScene("Results");
			}
		}

		//Give players points at an interval based on the items they carry.
		if (pointTimer >= pointInterval)
		{
			pointTimer = 0.0f;
			foreach (PlayerData playerData in clients.Values.ToArray())
			{
				
				//playerData.AddPoints (playerData.Player.carriedItems.Count);
				AddPointsToPlayer (playerData.player, playerData.player.carriedItems.Count);
				//if(NetworkServer.active)
					//NetworkedGUIManager.instance.RpcUpdateGUIs ();
			}
				
			foreach (KeyValuePair<NetworkConnection, PlayerData> pair in clients) 
			{
				//Debug.LogFormat ("-- {0} :: {1} pts --", pair.Key.address, pair.Value.points);
			}
			//Debug.Log ("-------------------------------");
		}
		pointTimer += Time.deltaTime;
	}

	//Adds points to a player in the players list.
	public void AddPointsToPlayer(NetworkedPlayer player, int value)
	{
		bool foundPlayer = false;
		//Find the KeyValuePair in the dictionary...
		KeyValuePair<NetworkConnection, PlayerData> pairToChange = new KeyValuePair<NetworkConnection, PlayerData>();
		foreach (KeyValuePair<NetworkConnection, PlayerData> pair in clients) 
		{
			if (pair.Value.player.Equals(player)) 
			{
				pairToChange = pair;
				foundPlayer = true;
			}
		}
		//Change the value of the value in the dictionary outside of the foreach loop.
		if (foundPlayer) 
		{
			PlayerData updatedData = clients [pairToChange.Key];
			updatedData.points += value;
			clients [pairToChange.Key] = updatedData;
			//clients [pairToChange.Key].points += value;
		}
		else 
		{
			//This could be because the player reference isn't the same one as that stored in the game managers
			//player list after being spawned!
			Debug.LogErrorFormat ("Adding {0} points to player ({1}) has failed!", value, player.playerNumber);
		}
	}

	//Public for now. Need to implement singleton.
	/*
	public Player SpawnPlayer(Player.PlayerNumber pNumber, float seconds)
	{
		Player newPlayer;

		UpdateAvailablePlayerSpawns (); //Ensure that our avalable spawns list is accurate.
		int spawnerIndex = Random.Range (0, availablePlayerSpawnPoints.Count); //Choose a random index from available spawns.
		newPlayer = availablePlayerSpawnPoints [spawnerIndex].SpawnPlayer (pNumber, seconds);
		//UpdateAvailablePlayerSpawns (); //While not strictly necessary, this keeps the list clean between uses.

		//Replace the player of the same player number in our players list with the new player.
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
	*/

	//Checks updates the list of spawn points available for player respawns.
	void UpdateAvailablePlayerSpawns()
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

	void OnDrawGizmos ()
	{
		//Draw spawn points...
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
				{	Gizmos.color = Color.red;
					Gizmos.DrawWireSphere (playerSpawnPoints [i].transform.position, 0.25f);
				}
			}
		}
	}
}
