using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// This class is attached to a button and handles playing appropriate sounds
/// for it's events.
/// </summary>
public class ButtonSoundController : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
	private bool selected = false;
	private bool silent = false;
	public bool Silent { get { return this.silent; } set { this.silent = value; } }

	private void Start()
	{
		this.GetComponent<Button> ().onClick.AddListener (PlayClickEffect);
	}

	private void PlayClickEffect()
	{
		if(!silent)
			AudioManager.instance.PlayEffect ("Button_Click");
	}

	public void OnSelect(BaseEventData eventData)
	{
		selected = true;

		if(!silent)
			AudioManager.instance.PlayEffect ("Button_Hover");
	}

	public void OnDeselect(BaseEventData eventData)
	{
		selected = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		//if (!silent)
		//	AudioManager.instance.PlayEffect ("Button_Hover");

		this.GetComponent<Button> ().Select ();
	}
}
