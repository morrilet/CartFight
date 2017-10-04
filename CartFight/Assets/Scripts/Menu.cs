using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// A menu superclass that helps generalize menu code.
/// </summary>
public class Menu : MonoBehaviour 
{
	/// <summary>
	/// The buttons in the menu.
	/// </summary>
	private Button[] buttons;

	public virtual void Start()
	{
		buttons = this.transform.GetComponentsInChildren<Button> ();
		SetUpButtons ();
	}

	/// <summary>
	/// Sets up all the buttons in the menu.
	/// </summary>
	public void SetUpButtons()
	{
		foreach (Button b in buttons) 
		{
			b.gameObject.AddComponent<ButtonSoundController> ();
		}
	}

	/// <summary>
	/// Sets every buttons silent bool to true/false.
	/// </summary>
	/// <param name="silent">If set to <c>true</c> silent.</param>
	public void SetAllButtonsSilent(bool silent)
	{
		foreach (Button b in buttons) 
		{
			b.GetComponent<ButtonSoundController> ().Silent = silent;
		}
	}

	/// <summary>
	/// Sets a button to silent or not silent.
	/// </summary>
	/// <param name="button">The button to set.</param>
	/// <param name="silent">If set to <c>true</c> silent.</param>
	public void SetButtonSilent(Button button, bool silent)
	{
		button.GetComponent<ButtonSoundController> ().Silent = silent;
	}

	/// <summary>
	/// Selects a button silently.
	/// </summary>
	/// <param name="button">Button.</param>
	public void SelectSilently(Button button)
	{
		SetButtonSilent (button, true);
		button.Select ();
		SetButtonSilent (button, false);
	}

	/// <summary>
	/// Selects the button in this scenes event system.
	/// </summary>
	/// <param name="button">Button to select.</param>
	public void SelectButtonInEventSystem(Button button)
	{
		StartCoroutine (SelectButtonInEventSystem_Coroutine (button));
	}

	//We use this so that we can wait a frame and force the event system to refresh.
	private IEnumerator SelectButtonInEventSystem_Coroutine(Button button)
	{
		EventSystem system = GameObject.FindObjectOfType<EventSystem> ();
		system.SetSelectedGameObject (null);
		yield return new WaitForEndOfFrame ();
		system.SetSelectedGameObject (button.gameObject);
	}
}
