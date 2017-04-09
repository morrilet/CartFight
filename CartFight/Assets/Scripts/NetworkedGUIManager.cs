using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Contains a list of values pulled from the NetworkedGameManager that is used to
/// update the LocalGUIManagers in a scene.
/// </summary>
public class NetworkedGUIManager : NetworkBehaviour
{
	public GameObject scoreTextPrefab;

	public struct ScoreText
	{
		public int playerId;
		public int score;
	}
	List<ScoreText> scoreTexts;

	List<LocalGUIManager> clientGUIs;

	public static NetworkedGUIManager instance;
	void Awake()
	{
		if(instance == null)
			instance = this;
	}

	void Start()
	{
		scoreTexts = new List<ScoreText> ();
		clientGUIs = new List<LocalGUIManager> ();
	}
		
	public void AddScoreText(int playerId, int score)
	{
		ScoreText newScoreText = new ScoreText ();
		newScoreText.playerId = playerId;
		newScoreText.score = score;
		scoreTexts.Add (newScoreText);

		RpcAddClientGUI ();
	}

	public void RemoveScoreText(int playerId)
	{
		for (int i = 0; i < scoreTexts.Count; i++) 
		{
			if (scoreTexts [i].playerId == playerId) 
			{
				scoreTexts.RemoveAt (i);
			}
		}
	}

	[ClientRpc]
	public void RpcUpdateGUIs ()
	{
		foreach (KeyValuePair<NetworkConnection, NetworkedGameManager.PlayerData> pair in NetworkedGameManager.instance.Clients) 
		{
			Debug.LogFormat ("P{0} :: {1}", pair.Key.connectionId + 1, pair.Value.points);
		}
	}

	[ClientRpc]
	public void RpcAddClientGUI()
	{
		Text newText = Instantiate (scoreTextPrefab, Vector3.zero, Quaternion.identity) as Text;
		newText.rectTransform.SetParent (GameObject.Find("GUI").transform);
		newText.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, 300);
		newText.rectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, 75);
		newText.rectTransform.localScale = new Vector3 (1, 1, 1);
	}

	[ClientRpc]
	public void RpcFormatScoreTexts()
	{
		Text[] texts = GameObject.FindObjectsOfType<Text> ();
		for(int i = 0; i < texts.Length; i++)
		{
			texts[i].rectTransform.anchoredPosition = 
				new Vector2 (-50, Mathf.Abs(100 * (i - (texts.Length - 1))));
		}
	}
}
