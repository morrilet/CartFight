using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LobbyPanel : MonoBehaviour 
{
	////////// Variables //////////
	private LobbyPlayerData joinedPlayer; //The player that joined via this panel.
	public List<GameObject> inactiveObjects; //The panel objects shown when a player isn't connected.
	public List<GameObject> activeObjects; //The panel objects shown when a player is connected.

	////////// Custom Data Types //////////
	public struct LobbyPlayerData
	{
		private Player.PlayerNumber playerNumber;
		private ControlScheme controls;

		public Player.PlayerNumber PlayerNumber { get { return playerNumber; } }
		public ControlScheme Controls { get { return controls; } }

		public LobbyPlayerData(Player.PlayerNumber playerNumber, ControlScheme controls)
		{
			this.playerNumber = playerNumber;
			this.controls = controls;
		}
	}

	////////// Accessors //////////
	public LobbyPlayerData JoinedPlayer { get { return this.joinedPlayer; } set { this.joinedPlayer = value; }}

	////////// Primary Methods //////////
	void Start()
	{
		joinedPlayer = new LobbyPlayerData(Player.PlayerNumber.None, null);

		SetListActive (inactiveObjects, true);
		SetListActive (activeObjects, false);
	}

	void Update()
	{
		//Listen for out players back button (from their control scheme) and remove them as necessary.
		//...
	}


	////////// Custom Methods //////////
	//Creates a player object to pass into the next scene. Used when a player joins on this panel. 
	public void AddPlayer(Player.PlayerNumber playerNumber, ControlScheme controlScheme) 
	{
		//Create the player info to be passed to the GameManager in the gameplay scene.
		LobbyPlayerData newPlayerData = new LobbyPlayerData(playerNumber, controlScheme);
		joinedPlayer = newPlayerData;

		//Switch the panel objects to reflect that the panel has been taken.
		SetListActive (inactiveObjects, false);
		SetListActive (activeObjects, true);
	}

	//Removes the player object. Used when a player backs out of the lobby.
	public void RemovePlayer()
	{
		//Set the current player readied on this panel to null.
		joinedPlayer = new LobbyPlayerData(Player.PlayerNumber.None, null);

		//Switch the panel objects to reflect that the panel is unused.
		SetListActive (inactiveObjects, true);
		SetListActive (activeObjects, false);
	}

	private void SetListActive(List<GameObject> objs, bool active) //Sets a list of objects to be active or inactive.
	{
		foreach (GameObject o in objs) 
		{
			o.SetActive (active);
		}
	}
}
