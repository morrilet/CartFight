﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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

	public int scoreLimit = 100;
	public int itemCount = 1;

	[SerializeField]
	private List<SpawnPoint> playerSpawnPoints; //The player spawn points on the map.
	private List<SpawnPoint> availablePlayerSpawnPoints; //The player spawn points currently accepting respawns.

	[SerializeField]
	private List<SpawnPoint> itemSpawnPoints; //The spawn points for items.
	private List<SpawnPoint> availableItemSpawnPoints;

	public Canvas pauseMenu; //The pause menu GUI canvas.
	private bool isPaused;

	public float pointInterval; //How often we give out points, measured in seconds.
	private float pointTimer = 0.0f;

	private PlayerData[] players; //The active players in the scene.

	public static GameManager instance;

	///////// Custom Data //////////

	public struct PlayerData
	{
		public Player player;
		public int points;
	}

	///////// Accessors //////////

	public PlayerData[] Players() { return players; }

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
		//TODO: Get the item/player spawn points procedurally with tags.

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
		SpawnStartingItems (itemCount);
			
		SetPaused (false);
		AudioManager.instance.PlayMusic ("Videogame2");
	}

	void Update()
	{
		//Check if any players are trying to pause the game.
		//TODO: Find a way to do this even for players who are dead.
		for (int i = 0; i < players.Length; i++)
		{
			if (players [i].player.controlScheme.PauseKeyDown)
			{
				SetPaused (!isPaused);
				break;
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
				if (players [i].points >= scoreLimit)
				{
					players [i].points = scoreLimit;
					GUI.instance.UpdateScoreTexts ();

					SceneManager.LoadScene ("Results");
				}
			}
		}

		pointTimer += Time.deltaTime;
	}

	///////// Custom Methods //////////

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

		//Set the pause menu to an apropriate state.
		pauseMenu.gameObject.SetActive(isPaused);
	}

	///////// Gizmos //////////

	//TODO: Automate the process of finding spawns. Don't rely on the list.
	void OnDrawGizmos ()
	{
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