using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyMenu : Menu 
{
	private LobbyManager lobbyManager; //The class that manages the lobby state. We just handle the buttons.
	private Button backButton;
	private Button readyButton; //The button that starts the game.
	private bool prevReadyButtonInteract; //The last interactable state of the ready button. 
										  //Used to select it on making it interactable.


	//Do this eary to beat eventmanager to the punch.
	private void Awake()
	{
		base.Start ();
		base.SetAllButtonsSilent (true);
	}

	public override void Start()
	{
		base.SetAllButtonsSilent (false);

		lobbyManager = GameObject.Find ("LobbyManager").GetComponent<LobbyManager> ();

		backButton = this.transform.FindChild ("Back_Button").GetComponent<Button> ();
		readyButton = this.transform.FindChild ("StartGame_Button").GetComponent<Button> ();
		prevReadyButtonInteract = false;

		base.SelectSilently (backButton);
	}
		
	private void Update()
	{
		Debug.Log ("Joined players: " + lobbyManager.CurrentJoinedPlayerCount);
		readyButton.interactable = (lobbyManager.CurrentJoinedPlayerCount >= 2) ? true : false;
		base.SetButtonSilent (readyButton, !readyButton.interactable); //Silence the button if it's not interactable.

		//Select the ready button if it's interactable. Otherwise, select the back button.
		if (readyButton.interactable && !prevReadyButtonInteract) 
		{ 
			base.SelectSilently (readyButton);
		}
		else if (!readyButton.interactable && prevReadyButtonInteract) 
		{ 
			base.SelectSilently (backButton);
		}

		prevReadyButtonInteract = readyButton.interactable;
	}
}
