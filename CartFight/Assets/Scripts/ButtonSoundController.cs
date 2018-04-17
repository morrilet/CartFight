using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// This class is attached to a button and handles playing appropriate sounds
/// for it's events.
/// </summary>
public class ButtonSoundController : SelectableSoundController
{
    public string clickSound = "Button_Click";

	private void Start()
	{
		this.GetComponent<Button> ().onClick.AddListener (PlayClickEffect);
	}

	private void PlayClickEffect()
    {
        if (!silent && clickSound.Length > 0)
			AudioManager.instance.PlayEffect (clickSound);
	}
}
