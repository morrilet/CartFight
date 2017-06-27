using UnityEngine;
using System.Collections;

/// <summary>
/// This handles spawning players or items.
/// </summary>
public class SpawnPoint : MonoBehaviour 
{
	public GameObject doorsObject; //The doors used for spawning a player.
	public GameObject safeZone; //The area in which the player doesn't collide with obstacles.

	public bool isPlayerSpawn = false;
	public bool isAvailable = true;

	//Use these to set up the player/item to spawn, then set them back to null.
	Player spawnedPlayer = null;
	Item spawnedItem = null;

	private void Start()
	{
		if(isPlayerSpawn)
		{
			safeZone.SetActive (false);
			doorsObject.SetActive (false);
		}
	}

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
			//Set up and return playerr for game manager before we do the effect...
			Player newPlayer = null; //The player we're spawning.

			//Create the player using spawnedPlayer. This is because of some reference
			//based errors from a month ago that I forgot about... TODO: Write better docs.
			GameObject temp = (GameObject)Instantiate (Resources.Load ("Player"), 
				transform.position, this.transform.rotation);
			spawnedPlayer = temp.GetComponent<Player> ();
			spawnedPlayer.playerNumber = pNumber;

			//Pass spawnedPlayer to new player and nullify it.
			newPlayer = spawnedPlayer;
			spawnedPlayer = null;

			//Deactivate the new player until the coroutine finishes waiting and reactivates it.
			newPlayer.gameObject.SetActive (false);

			StartCoroutine (SpawnPlayerEffect_Coroutine (newPlayer, seconds));

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

	//Pulls the doors up, opens them, spawns/throws out the player, then removes the doors.
	private IEnumerator SpawnPlayerEffect_Coroutine(Player player, float seconds)
	{
		isAvailable = false;

		//Wait until it's time to start the spawn.
		yield return PausableWaitForSeconds_Coroutine (seconds);

		//Activate and open the doors.
		doorsObject.SetActive (true);
		doorsObject.GetComponent<Animator> ().SetTrigger ("Activated");

		yield return PausableWaitForSeconds_Coroutine(0.9f);

		//Activate the player and throw them out the doors.
		player.gameObject.SetActive (true);
		player.Velocity = player.transform.right * player.maxVelocity;

		//Activate the safe zone.
		StartCoroutine(SafezoneHandler_Coroutine(player));

		yield return PausableWaitForSeconds_Coroutine (.5f);

		//Close the doors.
		doorsObject.GetComponent<Animator> ().SetTrigger ("Activated");

		yield return PausableWaitForSeconds_Coroutine (1.0f);

		player.EnableObstacleCollisions (true); //Just in case it hasn't already been done.
		doorsObject.SetActive (false);

		isAvailable = true;
	}

	private IEnumerator SafezoneHandler_Coroutine(Player player)
	{
		safeZone.SetActive (true);
		if (safeZone.GetComponent<Collider2D>().bounds.Contains (player.transform.position)) 
		{
			player.Start ();
			//player.EnableObstacleCollisions (false);
		}

		while (safeZone.GetComponent<Collider2D>().bounds.Contains (player.transform.position)) 
		{
			yield return null;
		}

		player.EnableObstacleCollisions (true);
		safeZone.SetActive (false);
	}

	private IEnumerator PausableWaitForSeconds_Coroutine(float seconds)
	{
		float timer = 0.0f;
		while (timer <= seconds) 
		{
			if (!GameManager.instance.IsPaused) 
			{
				timer += Time.deltaTime;
			}
			yield return null;
		}
	}
}
