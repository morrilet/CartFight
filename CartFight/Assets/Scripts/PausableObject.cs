using UnityEngine;
using System.Collections;

public class PausableObject : MonoBehaviour 
{
	private bool isPaused = false;
	private bool isPausedPrev = false;
	public bool IsPaused { get { return this.isPaused; } }
	public bool IsPausedPrev { get { return this.isPausedPrev; } }

	public void setPaused(bool isPaused)
	{
		this.isPaused = isPaused;
		StartCoroutine (setPausedPrev_Coroutine());
	}

	//Waits one frame then sets paused previous to the current paused state.
	private IEnumerator setPausedPrev_Coroutine()
	{
		yield return new WaitForEndOfFrame ();
		this.isPausedPrev = this.isPaused;
	}
}
