using UnityEngine;
using System.Collections;
using XInputDotNetPure;

/// <summary>
/// This class contains the key/gamepad info that is used to determine inputs
/// for different players.
/// </summary>
public class ControlScheme 
{
	///////// Variables //////////

	private bool isGamePad = false;
	private KeyCode upKey, downKey, rightKey, leftKey, throwKey, pauseKey; //The keys that create input values if not using a gamepad.
	private GamePadState state, prevState; //The states to get input from if we're using a gamepad.
	private PlayerIndex playerIndex; //The gamepad from which to get state info from if we're using a gamepad.
	private float horizontal, vertical; //The input axes. -1 = left/down, 0 = no input, 1 = right/up.
	private bool throwKeyDown = false;
    private bool throwKeyHeld = false;
	private bool pauseKeyDown = false;
    private bool isVibrating = false; //Whether or not we're currently vibrating the gamepad (if we use one).

	public enum GamepadControlStick //Which stick to use for gathering input. (For both, right = horiz, left = vert.)
	{
		BOTH,
		LEFT,
		RIGHT
	};
	private GamepadControlStick gamepadControlStick;

	///////// Accessors //////////

	public bool IsGamePad { get { return this.isGamePad; } }
	public KeyCode UpKey { get { return this.upKey; } }
	public KeyCode DownKey { get { return this.downKey; } }
	public KeyCode RightKey { get { return this.rightKey; } }
	public KeyCode LeftKey { get { return this.leftKey; } }
	public KeyCode ThrowKey { get { return this.throwKey; } }
	public KeyCode PauseKey { get { return this.pauseKey; } }

	public float Horizontal { get { return this.horizontal; } }
	public float Vertical { get { return this.vertical; } }
	public bool ThrowKeyDown { get { return this.throwKeyDown; } }
    public bool ThrowKeyHeld { get { return this.throwKeyHeld; } }
	public bool PauseKeyDown { get { return this.pauseKeyDown; } }
    public bool IsVibrating {
        get { return this.isVibrating; }
        set { this.isVibrating = value; } }

	public PlayerIndex PIndex { get { return this.playerIndex; } }

	public GamepadControlStick GamepadControls
	{ 
		get { return this.gamepadControlStick; } 
		set { this.gamepadControlStick = value; } 
	}

	///////// Constructors //////////

	public ControlScheme (PlayerIndex playerIndex)
	{
		isGamePad = true;
		this.playerIndex = playerIndex;

		//Default controls are both.
		gamepadControlStick = GamepadControlStick.BOTH;
	}

	public ControlScheme (KeyCode upKey, KeyCode downKey, KeyCode rightKey, KeyCode leftKey, 
		KeyCode throwKey, KeyCode pauseKey)
	{
		isGamePad = false;
		this.upKey = upKey;
		this.downKey = downKey;
		this.rightKey = rightKey;
		this.leftKey = leftKey;
		this.throwKey = throwKey;
		this.pauseKey = pauseKey;
	}

	///////// Primary Methods //////////

	public void Start()
	{
		horizontal = 0;
		vertical = 0;

		if (isGamePad) 
		{
			state = GamePad.GetState (playerIndex);
			prevState = state;
		}
	}

	public void Update()
	{
		if (isGamePad) //Gamepad input.
		{
			//Input axes...
			switch (gamepadControlStick) 
			{
				case GamepadControlStick.BOTH:
					horizontal = state.ThumbSticks.Right.X;
					vertical = state.ThumbSticks.Left.Y;
					break;
				case GamepadControlStick.LEFT:
					horizontal = state.ThumbSticks.Left.X;
					vertical = state.ThumbSticks.Left.Y;
					break;
				case GamepadControlStick.RIGHT:
					horizontal = state.ThumbSticks.Right.X;
					vertical = state.ThumbSticks.Right.Y;
					break;
				default:
					horizontal = 0;
					vertical = 0;
					Debug.Log ("Invalid GamepadControlStick!");
					break;
			}
				
			//Use raw 0-1 values, nothing in between.
			int currentTriggerRight = Mathf.CeilToInt (state.Triggers.Right);
			int prevTriggerRight = Mathf.CeilToInt (prevState.Triggers.Right);

			throwKeyDown = ((currentTriggerRight == 1) && (prevTriggerRight == 0));
            throwKeyHeld = (currentTriggerRight == 1);
			pauseKeyDown = ((state.Buttons.Start == ButtonState.Pressed) 
				&& (prevState.Buttons.Start == ButtonState.Released));

			prevState = state;
			state = GamePad.GetState (playerIndex);
		} 
		else //Keyboard input.
		{
			//Horizontal input detection.
			if (Input.GetKey (leftKey) && !Input.GetKey (rightKey)) //Only left key is down...
			{
				horizontal = -1;
			} 
			else if (Input.GetKey (rightKey) && !Input.GetKey (leftKey)) //Only right key is down...
			{
				horizontal = 1;
			} 
			else //Neither are down...
			{
				horizontal = 0;
			}

			//Vertical input detection.
			if (Input.GetKey (upKey) && !Input.GetKey (downKey)) //Only up key is down...
			{
				vertical = 1;
			} 
			else if (Input.GetKey (downKey) && !Input.GetKey (upKey)) //Only down key is down... 
			{
				vertical = -1;
			} 
			else //Neither are down...
			{
				vertical = 0;
			}

			if (Input.GetKeyDown (throwKey)) 
			{
				throwKeyDown = true;
			}
			else
			{
				throwKeyDown = false;
			}

            throwKeyHeld = Input.GetKey(throwKey);

			if (Input.GetKeyDown (pauseKey)) 
			{
				pauseKeyDown = true;
			}
			else 
			{
				pauseKeyDown = false;
			}
		}
	}
}
