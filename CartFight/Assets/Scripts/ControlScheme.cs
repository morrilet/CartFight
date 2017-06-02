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
	private KeyCode upKey, downKey, rightKey, leftKey, throwKey; //The keys that create input values if not using a gamepad.
	private GamePadState state, prevState; //The states to get input from if we're using a gamepad.
	private PlayerIndex playerIndex; //The gamepad from which to get state info from if we're using a gamepad.
	private float horizontal, vertical; //The input axes. -1 = left/down, 0 = no input, 1 = right/up.
	private bool throwKeyDown = false;

	///////// Accessors //////////

	public bool IsGamePad { get { return this.isGamePad; } }
	public KeyCode UpKey { get { return this.upKey; } }
	public KeyCode DownKey { get { return this.downKey; } }
	public KeyCode RightKey { get { return this.rightKey; } }
	public KeyCode LeftKey { get { return this.leftKey; } }
	public KeyCode ThrowKey { get { return this.throwKey; } }

	public float Horizontal { get { return this.horizontal; } }
	public float Vertical { get { return this.vertical; } }
	public bool ThrowKeyDown { get { return this.throwKeyDown; } }

	///////// Constructors //////////

	public ControlScheme (PlayerIndex playerIndex)
	{
		isGamePad = true;
		this.playerIndex = playerIndex;
	}

	public ControlScheme (KeyCode upKey, KeyCode downKey, KeyCode rightKey, KeyCode leftKey, KeyCode throwKey)
	{
		isGamePad = false;
		this.upKey = upKey;
		this.downKey = downKey;
		this.rightKey = rightKey;
		this.leftKey = leftKey;
		this.throwKey = throwKey;
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
		if (isGamePad) 
		{
			horizontal = state.ThumbSticks.Right.X;
			vertical = state.ThumbSticks.Left.Y;
			throwKeyDown = ((state.Triggers.Right == 1) && (prevState.Triggers.Right == 0));

			prevState = state;
			state = GamePad.GetState (playerIndex);
		} 
		else 
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
		}
	}
}
