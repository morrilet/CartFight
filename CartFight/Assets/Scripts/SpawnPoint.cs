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

	//Use these to set up the player/item to spawn, then set them back to null.
	Player spawnedPlayer = null;
	Item spawnedItem = null;

	public Item SpawnItem(Item.ItemType itemType, float seconds)
	{
		if (isAvailable && !isPlayerSpawn)
		{
			Item newItem = null;

			GameObject temp = (GameObject)Instantiate (Resources.Load ("Item"), 
				this.transform.position, this.transform.rotation);
			spawnedItem = temp.GetComponent<Item> ();
			spawnedItem.itemType = itemType;

			newItem = spawnedItem;
			spawnedItem = null;

			newItem.gameObject.SetActive (false);
			StartCoroutine (ActivateObject_Coroutine (newItem.gameObject, seconds));

			return newItem;
		}
		else
		{
			Debug.Log ("Error! Tried to spawn an item at an unusable spawn point.");
			return null;
		}
	}

	public Player SpawnPlayer(Player.PlayerNumber pNumber, float seconds)
	{
		if (isAvailable && isPlayerSpawn)
		{
			Player newPlayer = null; //The player we're spawning.

			//Create the player using spawnedPlayer. This is because of some reference
			//based errors from a month ago that I forgot about... TODO: Write better docs.
			GameObject temp = (GameObject)Instantiate (Resources.Load ("Player"), 
				transform.position, Quaternion.Euler (rotation));
			spawnedPlayer = temp.GetComponent<Player> ();
			spawnedPlayer.playerNumber = pNumber;

			//Pass spawnedPlayer to new player and nullify it.
			newPlayer = spawnedPlayer;
			spawnedPlayer = null;

			//Deactivate the new player until the coroutine finishes waiting and reactivates it.
			newPlayer.gameObject.SetActive (false);
			StartCoroutine (ActivateObject_Coroutine (newPlayer.gameObject, seconds));

			return newPlayer;
		} 
		else
		{
			Debug.LogError ("Error! Tried to spawn a player at an unusable spawn point.");
			return null;
		}
	}

	//Waits for seconds then activates the object. Also sets isAvailable to the appropriate value
	//based on whether the spawner has finished spawning (activating) the object.
	private IEnumerator ActivateObject_Coroutine(GameObject obj, float seconds)
	{
		isAvailable = false;

		//Basically making a pausable WaitForSeconds.
		float timer = 0.0f;

		while (timer <= seconds) 
		{
			if (!GameManager.instance.IsPaused) 
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}

		obj.SetActive (true);
		isAvailable = true;
	}
}
