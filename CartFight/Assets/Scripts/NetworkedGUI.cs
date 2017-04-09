using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This is mega broken. The whole ScoreText struct thing is backfiring. I think the best course of action is to scrap it
/// and start again using a SyncListString that is set up by the game manager. The strings will be what the score texts should
/// display (eg "P1 :: 32"), and this class will simply create a text ui object for each text, set the text to the text found
/// in the game manager list, and position the ui elements properly.
/// </summary>

public class NetworkedGUI : NetworkBehaviour 
{
	//public Text p1ScoreText;
	//public Text p2ScoreText;

	public Text scoreTextPrefab;


	public struct GUITextObj
	{
		public Text text;
		public NetworkedGameManager.PlayerData client;
	};
	List<GUITextObj> GUITexts;

	/*
	public static NetworkedGUI instance;
	void Awake()
	{
		//if (instance == null)
		//	instance = this;
		//else
		//	Destroy (instance.gameObject);
	}
	*/

	void Start()
	{
		/*
		if (hasAuthority)
			Debug.Log ("AUTH PRE: TRUE: " + connectionToClient.address);
		
		//this.GetComponent<NetworkIdentity> ().AssignClientAuthority (connectionToClient);

		if (hasAuthority)
			Debug.Log ("AUTH POST: TRUE: " + connectionToClient.address);
		else
			Debug.Log ("AUTH FALSE ALL: " + connectionToClient.address);
		*/

		GUITexts = new List<GUITextObj> ();
		CreateGUITexts ();
		FormatGUITexts ();

		//scoreStrings.Callback += OnScoreStringsChanged;

		/*
		for (int i = 0; i < NetworkServer.connections.Count; i++) 
		{
			//AddScoreText (i);
			//Debug.Log (NetworkedGameManager.instance.Clients[NetworkServer.connections[i]].points);
		}
		*/
		//InstantiateScoreTexts ();

		//Text testText = Instantiate (scoreTextPrefab, Vector3.zero, Quaternion.identity) as Text;
	}

	public void UpdateGUITexts()
	{
		for (int i = 0; i < GUITexts.Count; i++) 
		{
			//client.points isn't a ref to GameManager client, only a stored state of it, methinks.
			GUITexts [i].text.text = "P" + i + " :: " + GUITexts [i].client.points;
		}
	}

	public void CreateGUITexts()
	{
		GUITexts.Clear ();
		int index = 1;
		foreach (KeyValuePair<NetworkConnection, NetworkedGameManager.PlayerData> pair 
			in NetworkedGameManager.instance.Clients) 
		{
			GUITextObj newTextObj = new GUITextObj();
			newTextObj.client = pair.Value;

			newTextObj.text = Instantiate (scoreTextPrefab, Vector3.zero, Quaternion.identity) as Text;
			newTextObj.text.rectTransform.SetParent (this.transform);
			newTextObj.text.text = "P" + index + " :: " + pair.Value.points;

			GUITexts.Add (newTextObj);

			Debug.Log ("Adding GUI object...");

			index++;
		}
	}

	private void FormatGUITexts()
	{
		for (int i = 0; i < GUITexts.Count; i++) 
		{
			GUITexts [i].text.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, 300);
			GUITexts [i].text.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, 75);
			GUITexts [i].text.rectTransform.anchoredPosition = 
				new Vector2 (-50, Mathf.Abs(100 * (i - (GUITexts.Count - 1))));
		}
	}

	/// <summary>
	/// Moves the score texts into descending order based on player number.
	/// </summary>
	/*
	public void FormatScoreTexts()
	{
		for (int i = 0; i < scoreStrings.Count; i++) 
		{
			scoreStrings [i].text.rectTransform.anchoredPosition = 
				new Vector2 (-50, Mathf.Abs(100 * (i - (scoreStrings.Count - 1))));
		}
	}

	public void AddScoreText(GameObject player, NetworkInstanceId netId)
	{
		if (!hasAuthority) 
		{
			this.gameObject.GetComponent<NetworkIdentity> ().AssignClientAuthority (connectionToClient);
			Debug.Log ("Assigned authority to " + hasAuthority + " for " + connectionToClient.address);
			CmdAddToScoreTexts (player, netId);
			this.gameObject.GetComponent<NetworkIdentity> ().RemoveClientAuthority (connectionToClient);
			Debug.Log ("Assigned authority to " + hasAuthority + " for " + connectionToClient.address);
		}
		else 
		{
			CmdAddToScoreTexts (player, netId);
		}
	}
		
	NetworkConnection tempConn;
	[Command]
	void CmdAddToScoreTexts(GameObject player, NetworkInstanceId netId)
	{
		RpcAddToScoreTexts (player, netId);
	}
	*/

	/*
	//Updates the text element of the UI objects for player scores.
	private void UpdateScoreTexts()
	{
		for (int i = 0; i < scoreTexts.Count; i++) 
		{
			scoreTexts [i].text = scoreStrings [i];
		}
	}

	public void UpdateScoreStrings()
	{
		NetworkedGameManager.PlayerData[] data = NetworkedGameManager.instance.Clients.Values.ToArray();
		for (int i = 0; i < data.Length; i++) 
		{
			scoreStrings [i] = "P" + i + " :: " + data [i].points;
		}
		UpdateScoreTexts ();
	}

	//Called when the score strings list elements are changed (added or removed, not edited).
	void OnScoreStringsChanged(SyncListInt.Operation op, int indexChanged)
	{
		//Update the score texts.
		scoreTexts.Clear();
		for(int i = 0; i < NetworkedGameManager.instance.Clients.Count; i++)
		{
			AddScoreText ();
		}
		FormatScoreTexts ();
	}

	//Creates a new score text object and adds it to the list.
	void AddScoreText()
	{
		Text newScoreText = Instantiate (scoreTextPrefab, Vector3.zero, Quaternion.identity) as Text;
		newScoreText.rectTransform.SetParent (this.gameObject.transform);

		newScoreText.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, 300);
		newScoreText.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, 75);
		newScoreText.rectTransform.localScale = new Vector3 (1, 1, 1);

		newScoreText.text = "";

		scoreTexts.Add (newScoreText);
		UpdateScoreStrings ();
	}

	//Formats the score text UI objects into a descending list.
	void FormatScoreTexts()
	{
		for(int i = 0; i < scoreTexts.Count; i++)
		{
			scoreTexts[i].rectTransform.anchoredPosition = 
				new Vector2 (-50, Mathf.Abs(100 * (i - (scoreStrings.Count - 1))));
		}
	}

	public void AddScoreString()
	{
		scoreStrings.Add ("");
	}

	/*
	[ClientRpc]
	public void RpcAddToScoreTexts()
	{
		Debug.Log ("RpcAddToScoreTexts has been called for player" + player.gameObject.name + "...");
		Debug.Log ("NetID = " + netId.Value);
		NetworkConnection conn = ClientScene.FindLocalObject(netId).GetComponent<NetworkIdentity>().connectionToServer;
		//Debug.LogFormat ("RpcAddToScoreTexts for (netId {0} : conn {1})", netId, conn.address);
		Debug.LogFormat("PlayerObj - {0} :: Conn - {1}", player.gameObject.name, conn.address);
		ScoreText newScoreText = new ScoreText ();

		newScoreText.text = Instantiate (scoreTextPrefab, Vector3.zero, Quaternion.identity) as Text;
		newScoreText.text.rectTransform.SetParent (this.transform);
		newScoreText.text.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, 300);
		newScoreText.text.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, 75);
		newScoreText.text.rectTransform.localScale = new Vector3 (1, 1, 1);
		newScoreText.text.text = "P" + (conn.connectionId + 1) + " :: {ERR}";
		//Gives a problem because we can only get connectionToServer...
		//newScoreText.text.text = "P" + (conn.connectionId + 1) + " :: " + 
		//	NetworkedGameManager.instance.Clients [conn].points.ToString ();

		newScoreText.conn = conn;

		scoreStrings.Add (newScoreText);
		Debug.Log ("Score text " + newScoreText.text.gameObject.name + " added!");
	}
	*/
}
