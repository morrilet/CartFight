using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sets up rounds, chooses player/item spawns, keeps track of respawn times, etc.
/// </summary>
public class GameManager : MonoBehaviour
{
	public int scoreLimit = 100;

	public Canvas pauseMenu; //The pause menu GUI.
	private bool isPaused;

	//When a player dies, they are assigned a respawn point that becomes unavailable to all other
	//players until they've been respawned.
	public List<SpawnPoint> playerSpawnPoints; //The player spawn points on the map.
	public List<SpawnPoint> availablePlayerSpawnPoints; //The player spawn points currently accepting respawns.

	public List<SpawnPoint> itemSpawnPoints; //The spawn points for items.

	public struct PlayerData
	{
		public Player player;
		public int points;
	}
	private PlayerData[] players; //The active players in the scene.
	public PlayerData[] getPlayers() { return players; }

	public float pointInterval; //How often we apply points.
	private float pointTimer = 0.0f;

	public static GameManager instance;
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
		availablePlayerSpawnPoints = playerSpawnPoints;

		//This player setup code is just here for now. Use a lobby in the future to link
		//players to the appropriate player number and input method.
		players = new PlayerData[LobbyManager.JoinedPlayerData.Count];
		for (int i = 0; i < players.Length; i++) 
		{
			players [i].player = SpawnPlayer (LobbyManager.JoinedPlayerData[i].PlayerNumber, 
				LobbyManager.JoinedPlayerData[i].Controls, 0.0f);
			//Debug.Log (players[i].player.playerNumber.ToString() + " " + players[i].points);
		}
			
		SetPaused (false);
		AudioManager.instance.PlayMusic ("Videogame2");
	}

	void Update()
	{
		//Debug.LogFormat ("{0} : {1} points | {2} : {3} points.", players[0].player.playerNumber, 
		//	players[0].points, players[1].player.playerNumber, players[1].points);
		//Debug.Log (players[0].player.playerNumber + " : " + "points");

		//Check for pause...
		for (int i = 0; i < players.Length; i++) 
		{
			if (players [i].player.controlScheme.PauseKeyDown) 
			{
				//players [i].player.controlScheme.Update ();
				SetPaused (!isPaused);
				break;
			}
		}

		if (isPaused)
			return;

		for (int i = 0; i < players.Length; i++)
		{
			if (players [i].points >= scoreLimit)
			{
				players [i].points = scoreLimit;
				GUI.instance.UpdateScoreTexts ();
				SceneManager.LoadScene ("Results");
			}
		}

		if (pointTimer >= pointInterval) 
		{
			pointTimer = 0.0f;
			for (int i = 0; i < players.Length; i++) 
			{
				AddPointsToPlayer (players [i].player, players [i].player.carriedItems.Count);
				GUI.instance.UpdateScoreTexts ();
			}
		}

		pointTimer += Time.deltaTime;
	}

	//Adds points to a player in the players list.
	public void AddPointsToPlayer(Player player, int value)
	{
		bool success = false;
		for (int i = 0; i < players.Length; i++) 
		{
			if (players [i].player.Equals(player)) 
			{
				players [i].points += value;
				success = true;
			}
		}
		if (!success) 
		{
			//This could be because the player reference isn't the same one as that stored in the game managers
			//player list after being spawned!
			Debug.LogErrorFormat ("Adding {0} points to player ({1}) has failed!", value, player.playerNumber);
		}
	}

	//Public for now. Need to implement singleton.
	public Player SpawnPlayer(Player.PlayerNumber pNumber, ControlScheme controls, float seconds)
	{
		Player newPlayer;

		UpdateAvailablePlayerSpawns (); //Ensure that our avalable spawns list is accurate.
		int spawnerIndex = Random.Range (0, availablePlayerSpawnPoints.Count); //Choose a random index from available spawns.
		newPlayer = availablePlayerSpawnPoints [spawnerIndex].SpawnPlayer (pNumber, seconds);
		newPlayer.controlScheme = controls;
		//UpdateAvailablePlayerSpawns (); //While not strictly necessary, this keeps the list clean between uses.

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

	public void SetPaused(bool isPaused)
	{
		this.isPaused = isPaused;

		//Pause all objects derived from pausable object.
		PausableObject[] pauseObjs = GameObject.FindObjectsOfType<PausableObject> ();
		for (int i = 0; i < pauseObjs.Length; i++) 
		{
			pauseObjs [i].setPaused (isPaused);
		}

		//Set the pause menu to an apropriate state.
		pauseMenu.gameObject.SetActive(isPaused);
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
