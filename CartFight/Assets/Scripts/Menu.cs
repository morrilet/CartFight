using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using XInputDotNetPure;

/// <summary>
/// A menu superclass that helps generalize menu code.
/// </summary>
public class Menu : MonoBehaviour 
{
	/// <summary>
	/// The buttons in the menu.
	/// </summary>
	private Button[] buttons;

    /// <summary>
    /// The method header for a 'back' behaviour.
    /// </summary>
    public delegate void backBehaviour();
    /// <summary>
    /// An event that triggers when the player presses a dedicated
    /// back button, such as the 'B' button or 'Esc'. This does not
    /// trigger when the player clicks a button that takes them back.
    /// </summary>
    public event backBehaviour onBackButtonPressed;

    /// <summary>
    /// The current state of all 4 (potential) gamepads. Used for checking for 
    /// back button input.
    /// </summary>
    private GamePadState[] gamepadsCurrent;
    /// <summary>
    /// The previous state of all 4 (potential) gamepads. Used for checking for
    /// back button input.
    /// </summary>
    private GamePadState[] gamepadsPrevious;

    public virtual void Awake()
    {
        buttons = this.transform.GetComponentsInChildren<Button>();
        SetUpButtons();
    }

    public virtual void Start()
	{
        gamepadsCurrent = new GamePadState[] {
            GamePad.GetState(PlayerIndex.One),
            GamePad.GetState(PlayerIndex.Two),
            GamePad.GetState(PlayerIndex.Three),
            GamePad.GetState(PlayerIndex.Four) };
        gamepadsPrevious = gamepadsCurrent;
	}

    public virtual void Update()
    {
        //Update gamepad states for current.
        gamepadsCurrent = new GamePadState[] {
            GamePad.GetState(PlayerIndex.One),
            GamePad.GetState(PlayerIndex.Two),
            GamePad.GetState(PlayerIndex.Three),
            GamePad.GetState(PlayerIndex.Four) };

        bool backPressed = false;
        for(int i = 0; i < gamepadsCurrent.Length; i++)
        {
            //Hah... gross.
            if(gamepadsCurrent[i].IsConnected && 
                ((gamepadsCurrent[i].Buttons.B == ButtonState.Pressed 
                && gamepadsPrevious[i].Buttons.B == ButtonState.Released)
                || (gamepadsCurrent[i].Buttons.Back == ButtonState.Pressed
                && gamepadsPrevious[i].Buttons.Back == ButtonState.Released)))
            {
                backPressed = true;
            }
        }

        if(backPressed || Input.GetKeyDown(KeyCode.Escape))
        {
            if(onBackButtonPressed != null)
            {
                onBackButtonPressed();
            }
        }

        gamepadsPrevious = gamepadsCurrent;
    }
    

    //This will catch when a button is held, too. That's a problem! Should just fire the event
    //ONCE if there's a button pressed event. Don't keep firing the event!!!
    public void CheckForBackEvent()
    {
        for (int i = 0; i < 4; i++)
        {
            if(GamePad.GetState((PlayerIndex)i).IsConnected
                && (GamePad.GetState((PlayerIndex)i).Buttons.B == ButtonState.Pressed
                || GamePad.GetState((PlayerIndex)i).Buttons.Back == ButtonState.Pressed))
            {
                onBackButtonPressed();
            }
        }
    }

	/// <summary>
	/// Sets up all the buttons in the menu.
	/// </summary>
	public void SetUpButtons()
	{
        Debug.Log(this.gameObject.name + " :: Button Length -> " + buttons.Length);

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
    /// Deselects whatever is currently selected in the scenes event system.
    /// </summary>
    public void DeselectCurrentlySelected()
    {
        EventSystem system = GameObject.FindObjectOfType<EventSystem>();
        system.SetSelectedGameObject(null);
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
