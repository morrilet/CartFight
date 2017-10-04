using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

public class LobbyManager : MonoBehaviour 
{
	////////// Variables //////////

	public string gameSceneName; //The scene to load when the game is started.
	public string mainMenuName;

	public LobbyPanel[] lobbyPanels = new LobbyPanel[4]; //The panels that have joined the game. 
														 //(Each panel represents one player)
	private List<ControlScheme> availableControlSchemes; //The control schemes still available for use.
	private ControlScheme wasd_Controls;
	private ControlScheme ijkl_Controls;
	private ControlScheme arrow_Controls;
	private ControlScheme gamepad1_Controls;
	private ControlScheme gamepad2_Controls;
	private ControlScheme gamepad3_Controls;
	private ControlScheme gamepad4_Controls;
	private static List<LobbyPanel.LobbyPlayerData> joinedPlayerData; //This is a list of all joinePlayer objects from 
																	  //the panels. Because static objects persist between
																	  //scenes, this is used by the game manager to start
																	  //the game.

	private GamePadState[] gamepad_States; //Houses the current states for gamepads 1-4 (0-3).
	private GamePadState[] gamepad_PrevStates; //Houses the previous states for gamepads 1-4 (0-3).

	private int currentJoinedPlayerCount;

	[SerializeField]
	private GameSettingsMenu settingsMenu; //The settings menu to use.

	////////// Accessors //////////

	public static List<LobbyPanel.LobbyPlayerData> JoinedPlayerData
	{
		get { return joinedPlayerData; }
	}

	public static GameManager.GameSettings GameSettings
	{
		get { return GameSettingsMenu.Settings; }
	}

	public int CurrentJoinedPlayerCount
	{
		get { return this.currentJoinedPlayerCount; }
	}

	////////// Primary Methods //////////

	void Start()
	{
		gamepad_States = new GamePadState[4];
		gamepad_PrevStates = new GamePadState[4];

		InitializeControlSchemes ();
		InitializeAvailableControlSchemes ();

		//Get the settings menu to pull settings from.
		//settingsMenu = GameObject.Find ("GameSettings_Menu").GetComponent<GameSettingsMenu>();
	}

	void Update()
	{
		//Updating gamepad states. Should work if players plug in during lobby screen.
		for (int i = 0; i < gamepad_States.Length; i++) 
		{
			gamepad_States [i] = GamePad.GetState ((PlayerIndex)i);
		}

		if (settingsMenu.GetComponent<GameSettingsMenu> ().MenuActive) 
		{
			return;
		}

		//Logic for adding/removing players from panels.
		ListenForControlSchemes ();

		//Set the ready button interactable state to true or false based on how many players are joined.
		currentJoinedPlayerCount = 0;
		for (int i = 0; i < lobbyPanels.Length; i++) 
		{
			if (lobbyPanels [i].JoinedPlayer.PlayerNumber != Player.PlayerNumber.None) 
			{
				currentJoinedPlayerCount++;
			}
		}

		//Update previous gamepad states.
		for(int i = 0; i < gamepad_PrevStates.Length; i++)
		{
			gamepad_PrevStates [i] = gamepad_States [i];
		}

		//Testing...
		string debugOutput = "Available Controls :: ";
		for (int i = 0; i < availableControlSchemes.Count; i++) 
		{
			debugOutput += " (" + availableControlSchemes[i].UpKey.ToString() + ") ";
		}
		//Debug.Log (debugOutput);
	}

	////////// Custom Methods //////////

	public void StartGame()
	{
		joinedPlayerData = new List<LobbyPanel.LobbyPlayerData> ();

		//Add copies of all the players that have joined to the joinedPlayerData list so that
		//they can be spawned in the game scene by the game manager.
		for (int i = 0; i < lobbyPanels.Length; i++) 
		{
			if (lobbyPanels [i].JoinedPlayer.PlayerNumber != Player.PlayerNumber.None) 
			{
				joinedPlayerData.Add (new LobbyPanel.LobbyPlayerData(
					lobbyPanels[i].JoinedPlayer.PlayerNumber, lobbyPanels[i].JoinedPlayer.Controls));
			}
		}

		Debug.Log ("Joined: " + joinedPlayerData.Count);

		//Load the main game scene.
		SceneManager.LoadScene(GameSettingsMenu.Settings.Level.ToString());
	}

	public void ReturnToMenu()
	{
		SceneManager.LoadScene (mainMenuName);
	}

	private void ListenForControlSchemes()
	{
		if (Input.GetKeyDown (KeyCode.W))
		{
			if (availableControlSchemes.Contains (wasd_Controls))
			{
				Join (wasd_Controls);
			}
			else
			{
				Remove (wasd_Controls);
			}
		}
		if (Input.GetKeyDown (KeyCode.I))
		{
			if (availableControlSchemes.Contains (ijkl_Controls))
			{
				Join (ijkl_Controls);
			}
			else
			{
				Remove (ijkl_Controls);
			}
		}
		if (Input.GetKeyDown (KeyCode.UpArrow))
		{
			if (availableControlSchemes.Contains (arrow_Controls))
			{
				Join (arrow_Controls);
			}
			else
			{
				Remove (arrow_Controls);
			}
		}
		if (gamepad_States[0].Buttons.Start == ButtonState.Pressed &&
			gamepad_PrevStates[0].Buttons.Start != ButtonState.Pressed) 
		{
			if (availableControlSchemes.Contains (gamepad1_Controls)) 
			{
				Join (gamepad1_Controls);
			} 
			else 
			{
				Remove (gamepad1_Controls);
			}
		}
		if (gamepad_States[1].Buttons.Start == ButtonState.Pressed &&
			gamepad_PrevStates[1].Buttons.Start != ButtonState.Pressed) 
		{
			if (availableControlSchemes.Contains (gamepad2_Controls)) 
			{
				Join (gamepad2_Controls);
			} 
			else 
			{
				Remove (gamepad2_Controls);
			}
		}
		if (gamepad_States[2].Buttons.Start == ButtonState.Pressed &&
			gamepad_PrevStates[2].Buttons.Start != ButtonState.Pressed) 
		{
			if (availableControlSchemes.Contains (gamepad3_Controls)) 
			{
				Join (gamepad3_Controls);
			} 
			else 
			{
				Remove (gamepad3_Controls);
			}
		}
		if (gamepad_States[3].Buttons.Start == ButtonState.Pressed &&
			gamepad_PrevStates[3].Buttons.Start != ButtonState.Pressed) 
		{
			if (availableControlSchemes.Contains (gamepad4_Controls)) 
			{
				Join (gamepad4_Controls);
			} 
			else 
			{
				Remove (gamepad4_Controls);
			}
		}
	}

	private void Join(ControlScheme controls) //What control scheme was used to join? 
	{										  //A.K.A. What control scheme will this player use?
		availableControlSchemes.Remove(controls);

		//Add the new player with the correct control scheme and player number to the panel.
		for (int i = 0; i < lobbyPanels.Length; i++) 
		{
			//The first lobby panel w/o a joined player...
			if (lobbyPanels [i].JoinedPlayer.PlayerNumber == Player.PlayerNumber.None) 
			{
				lobbyPanels [i].AddPlayer ((Player.PlayerNumber)i, controls);
				break;
			}
		}
	}
		
	private void Remove(ControlScheme controls) //Remove a player based on the control scheme they use.
	{
		for (int i = 0; i < lobbyPanels.Length; i++) 
		{
			if (lobbyPanels [i].JoinedPlayer.Controls == controls) 
			{
				lobbyPanels [i].RemovePlayer ();
				availableControlSchemes.Add (controls);
			}
		}
	}
		
	private void InitializeControlSchemes() //Creates all of the control schemes.
	{
		wasd_Controls = new ControlScheme(KeyCode.W, KeyCode.S, KeyCode.D, KeyCode.A, KeyCode.Q, KeyCode.Escape);
		ijkl_Controls = new ControlScheme(KeyCode.I, KeyCode.K, KeyCode.L, KeyCode.J, KeyCode.U, KeyCode.Delete);
		arrow_Controls = new ControlScheme (KeyCode.UpArrow, KeyCode.DownArrow, 
			KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightControl, KeyCode.Delete);
		gamepad1_Controls = new ControlScheme (PlayerIndex.One);
		gamepad2_Controls = new ControlScheme (PlayerIndex.Two);
		gamepad3_Controls = new ControlScheme (PlayerIndex.Three);
		gamepad4_Controls = new ControlScheme (PlayerIndex.Four);
	}

	private void InitializeAvailableControlSchemes() //Add all control schemes to available control schemes.
	{
		availableControlSchemes = new List<ControlScheme> ();

		availableControlSchemes.Add (wasd_Controls);
		availableControlSchemes.Add (ijkl_Controls);
		availableControlSchemes.Add (arrow_Controls);
		availableControlSchemes.Add (gamepad1_Controls);
		availableControlSchemes.Add (gamepad2_Controls);
		availableControlSchemes.Add (gamepad3_Controls);
		availableControlSchemes.Add (gamepad4_Controls);
	}
}
