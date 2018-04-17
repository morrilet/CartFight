using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SelectableSoundController : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    protected bool selected = false;
    protected bool silent = false;
    public bool Silent { get { return this.silent; } set { this.silent = value; } }

    //Default sounds to play.
    public string selectSound = "Button_Hover";
    public string deselectSound = "";

    public void OnPointerEnter(PointerEventData eventData)
    {
		this.GetComponent<Selectable> ().Select ();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        if (this.GetComponent<Selectable>().interactable)
            if (!silent && deselectSound.Length > 0)
                AudioManager.instance.PlayEffect(deselectSound);
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;

        if (this.GetComponent<Selectable>().interactable)
            if (!silent && selectSound.Length > 0)
                AudioManager.instance.PlayEffect(selectSound);
    }
}
