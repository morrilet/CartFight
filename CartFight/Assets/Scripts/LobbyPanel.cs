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
    [Space]
    public Text playerNumberText;
    public Text playerJoinedText;

    [HideInInspector]
    public bool canPing = true;

    private bool throwKeyDown, throwKeyDownPrev;
    private float pingTrauma = 0.0f;

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
        
        StartCoroutine(Ping_Coroutine());
	}

	void Update()
	{
        if(joinedPlayer.PlayerNumber != Player.PlayerNumber.None)
        {
            throwKeyDown = joinedPlayer.Controls.ThrowKeyDown;
            joinedPlayer.Controls.Update();
        }

        //Listen for the throw key of each player. If they press it, we ping!
        if(throwKeyDown && !throwKeyDownPrev)
        {
            Ping(0.3f);
        }
        
        throwKeyDownPrev = throwKeyDown;
	}


	////////// Custom Methods //////////
	//Creates a player object to pass into the next scene. Used when a player joins on this panel. 
	public void AddPlayer(Player.PlayerNumber playerNumber, ControlScheme controlScheme) 
	{
		//Create the player info to be passed to the GameManager in the gameplay scene.
		LobbyPlayerData newPlayerData = new LobbyPlayerData(playerNumber, controlScheme);
		joinedPlayer = newPlayerData;
		playerNumberText.text = "P" + ((int)playerNumber + 1);
        playerJoinedText.text = "Press 'throw' to ping this panel!";

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

    private void Ping(float trauma)
    {
        //Add.
        pingTrauma += trauma;

        //Clamp.
        if (pingTrauma < 0)
            pingTrauma = 0;
        if (pingTrauma > 1)
            pingTrauma = 1;
    }

    private IEnumerator Ping_Coroutine()
    {
        while (true)
        {
            float angleMax = 10.0f; //Max rotational angle.
            float newAngle = 0.0f;

            if (pingTrauma <= 0 || joinedPlayer.PlayerNumber == Player.PlayerNumber.None || !canPing)
            {
                //Lerp into normal place.
                newAngle = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 7.5f).eulerAngles.z;
            }
            else
            {
                newAngle = transform.rotation.eulerAngles.z;
                newAngle += ((Mathf.PerlinNoise(Time.time * pingTrauma, 
                    Time.time + ((int)joinedPlayer.PlayerNumber / 4.0f)) * angleMax)
                    * 2.0f) - angleMax;
                if (newAngle >= angleMax && newAngle <= 180.0f)
                {
                    newAngle = angleMax;
                }
                else if (newAngle <= 360.0f - angleMax && newAngle >= 180f)
                {
                    newAngle = 360.0f - angleMax;
                }

                if (joinedPlayer.PlayerNumber == Player.PlayerNumber.P1)
                    Debug.Log(newAngle);
            }

            transform.rotation = Quaternion.Euler(0.0f, 0.0f, newAngle);
            Ping(-Time.deltaTime);

            yield return null;
        }
    }
}
