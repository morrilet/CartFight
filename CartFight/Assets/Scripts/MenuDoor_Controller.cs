using UnityEngine;
using System.Collections;

public class MenuDoor_Controller : MonoBehaviour 
{
	[SerializeField]
	private AnimationCurve openCurve, closeCurve; //The curves to use for opening/closing the doors.
	[SerializeField]
	private float openDuration, closeDuration; //How long it takes to open/close the doors.
	[SerializeField]
	private float openOffset; //How far to move the doors to open them. Close offset is the starting position.

	[SerializeField]
	private AnimationCurve openScaleCurve, closeScaleCurve; //The curves to use for scaling the doors on open/close
	[SerializeField]
	private float openScaleDuration, closeScaleDuration;
	[SerializeField]
	private Vector3 openScale;

	public RectTransform doorRight, doorLeft;
	private Vector3 doorRight_StartPos, doorLeft_StartPos;
	private Vector3 doorRight_StartScale, doorLeft_StartScale;

	private bool doorsOpen = false;
	public bool DoorsOpen { get { return this.doorsOpen; } }

	private void Start()
	{
		doorRight_StartPos = doorRight.anchoredPosition;
		doorLeft_StartPos = doorLeft.anchoredPosition;

		doorRight_StartScale = doorRight.localScale;
		doorLeft_StartScale = doorLeft.localScale;
	}

	public void OpenDoors()
	{
		//Ensure the doors are in the right spot to begin with...
		doorRight.anchoredPosition = doorRight_StartPos;
		doorLeft.anchoredPosition = doorLeft_StartPos;

		StartCoroutine (MoveDoors_Coroutine(openCurve, openOffset, openDuration, true));
		//StartCoroutine (ScaleDoors_Coroutine (openScaleCurve, doorLeft_StartScale, openScale, openScaleDuration));
	}

	public void CloseDoors()
	{
		//Ensure the doors are in the right spot to begin with...
		doorRight.anchoredPosition = (Vector2)doorRight_StartPos + new Vector2 (openOffset, 0.0f);
		doorLeft.anchoredPosition = (Vector2)doorLeft_StartPos + new Vector2 (-openOffset, 0.0f);

		float closeOffset = Mathf.Abs (doorRight_StartPos.x - doorRight.anchoredPosition.x);
		StartCoroutine (MoveDoors_Coroutine (closeCurve, closeOffset, closeDuration, false));
		//StartCoroutine (ScaleDoors_Coroutine (closeScaleCurve, openScale, doorLeft_StartScale, closeScaleDuration));
	}

	private IEnumerator MoveDoors_Coroutine(AnimationCurve curve, float offset, float duration, bool doorsOpenNext)
	{
		float timer = 0;

		Vector2 doorLeftNewPos = doorLeft.anchoredPosition;
		Vector2 doorRightNewPos = doorRight.anchoredPosition;

		while (timer <= duration) 
		{
			doorLeftNewPos.x = (-offset * curve.Evaluate (timer / duration) + doorLeft_StartPos.x);
			doorRightNewPos.x = (offset * curve.Evaluate (timer / duration) + doorRight_StartPos.x);

			doorRight.anchoredPosition = doorRightNewPos;
			doorLeft.anchoredPosition = doorLeftNewPos;

			timer += Time.deltaTime;
			yield return null;
		}

		if (doorsOpenNext == true) 
		{
			doorLeft.anchoredPosition = (Vector2)doorLeft_StartPos + new Vector2 (-offset, 0.0f);
			doorRight.anchoredPosition = (Vector2)doorRight_StartPos + new Vector2 (offset, 0.0f);
		}
		if (doorsOpenNext == false) 
		{
			doorLeft.anchoredPosition = (Vector2)doorLeft_StartPos;
			doorRight.anchoredPosition = (Vector2)doorRight_StartPos;
		}
		doorsOpen = doorsOpenNext;
	}

	//This is pretty broken right now... Dang.  Oh well, the effect was only OK.
	private IEnumerator ScaleDoors_Coroutine(AnimationCurve curve, Vector3 oldScale, Vector3 newScale, float duration)
	{
		float timer = 0.0f;

		Vector3 newLeftScale = doorLeft.localScale;
		Vector3 newRightScale = doorRight.localScale;

		Vector3 rightOldScale = oldScale;
		Vector3 rightNewScale = newScale;

		rightOldScale.x *= -1f;
		rightNewScale.x *= -1f;

		while (timer <= duration) 
		{
			newLeftScale = Vector3.Lerp (oldScale, newScale, curve.Evaluate (timer / duration));
			newRightScale = Vector3.Lerp (rightOldScale, rightNewScale, curve.Evaluate (timer / duration));

			doorLeft.localScale = newLeftScale;
			doorRight.localScale = newRightScale;

			timer += Time.deltaTime;
			yield return null;
		}

		doorLeft.localScale = newScale;
		doorRight.localScale = rightNewScale;
	}
}
