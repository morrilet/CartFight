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

    //Keeps track of the frame that the game settings menu is opened.
    private bool settingsMenuOpen_Curr;
    private bool settingsMenuOpen_Prev;

	//Do this early to beat eventmanager to the punch.
	public override void Awake()
	{
        base.Awake();
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

        settingsMenuOpen_Curr = lobbyManager.SettingsMenu.MenuActive;
        settingsMenuOpen_Prev = settingsMenuOpen_Curr;

        base.onBackButtonPressed += this.lobbyManager.ReturnToMenu;
	}
		
	public override void Update()
	{
        base.Update();

        settingsMenuOpen_Curr = this.lobbyManager.SettingsMenu.MenuActive;

		//Debug.Log ("Joined players: " + lobbyManager.CurrentJoinedPlayerCount);
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

        //Settings menu just opened.
        if(settingsMenuOpen_Curr && !settingsMenuOpen_Prev)
        {
            base.onBackButtonPressed -= this.lobbyManager.ReturnToMenu;
        }

        //Settings menu just closed.
        if(!settingsMenuOpen_Curr && settingsMenuOpen_Prev)
        {
            base.onBackButtonPressed += this.lobbyManager.ReturnToMenu;
        }

		prevReadyButtonInteract = readyButton.interactable;
        settingsMenuOpen_Prev = settingsMenuOpen_Curr;
	}
}
