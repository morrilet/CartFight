using UnityEngine;
using System.Collections;

/// <summary>
/// This handles spawning players or items.
/// </summary>
public class SpawnPoint : MonoBehaviour 
{
	public Vector3 rotation;

	public bool isPlayerSpawn = false;
	public bool isAvailable = true;

	public string testString = "";

	//Use these to set up the player/item to spawn, then set them back to null.
	Player spawnedPlayer = null;
	Item spawnedItem = null;

	public void SpawnItem(Item.ItemType iType, float seconds)
	{
	}

	private IEnumerator SpawnItem_Coroutine(Item.ItemType iType, float seconds)
	{
		return null;
	}

	public Player SpawnPlayer(Player.PlayerNumber pNumber, float seconds)
	{
		Player newPlayer = null; //The player we're spawning.

		if (isAvailable && isPlayerSpawn)
		{
			//Create the player using spawnedPlayer.
			GameObject temp = (GameObject)Instantiate (Resources.Load ("Player"), 
				transform.position, Quaternion.Euler (rotation));
			spawnedPlayer = temp.GetComponent<Player> ();
			spawnedPlayer.playerNumber = pNumber;

			//Pass spawnedPlayer to new player and nullify it.
			newPlayer = spawnedPlayer;
			spawnedPlayer = null;

			//Deactivate the new player until the coroutine finishes waiting and reactivates it.
			newPlayer.gameObject.SetActive (false);

			StartCoroutine (ActivatePlayer_Coroutine (newPlayer.gameObject, seconds));

			return newPlayer;
		} 
		else
		{
			Debug.LogError ("Error! Tried to spawn a player at an unusable spawn point.");
		}

		//Debug.Log ("Temp = " + tempPlayer.ToString() + " :: From SpawnPlayer.");
		//Debug.Log ("New = " + newPlayer.ToString() + " :: From SpawnPlayer.");
		return newPlayer;
	}

	//Waits for seconds then activates the player object. Also sets isAvailable to the appropriate value
	//based on whether the spawner has finished spawning (activating) the player object.
	private IEnumerator ActivatePlayer_Coroutine(GameObject player, float seconds)
	{
		isAvailable = false;

		yield return new WaitForSeconds (seconds);

		player.SetActive (true);
		isAvailable = true;
	}
}
