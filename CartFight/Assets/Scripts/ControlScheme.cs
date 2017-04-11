using UnityEngine;
using System.Collections;
using XInputDotNetPure;

/// <summary>
/// This class contains the key/gamepad info that is used to determine inputs
/// for different player.
/// </summary>
public class ControlScheme 
{
	///////// Variables //////////

	private bool isGamePad = false;
	private KeyCode upKey, downKey, rightKey, leftKey; //The keys that create input values if not using a gamepad.
	private GamePadState state, prevState; //The states to get input from if we're using a gamepad.
	private PlayerIndex playerIndex; //The gamepad from which to get state info from if we're using a gamepad.
	private float horizontal, vertical; //The input axes. -1 = left/down, 0 = no input, 1 = right/up.

	///////// Accessors //////////

	public bool IsGamePad { get { return this.isGamePad; } set { this.isGamePad = value; } }
	//We may not need the keycode accessors, actually, because of the constructors.
	public KeyCode UpKey { get { return this.upKey; } set { this.upKey = value; } }
	public KeyCode DownKey { get { return this.downKey; } set { this.downKey = value; } }
	public KeyCode RightKey { get { return this.rightKey; } set { this.rightKey = value; } }
	public KeyCode LeftKey { get { return this.leftKey; } set { this.leftKey = value; } }
	public float Horizontal { get { return this.horizontal; } }
	public float Vertical { get { return this.vertical; } }


	///////// Constructors //////////

	public ControlScheme (PlayerIndex playerIndex)
	{
		isGamePad = true;
		this.playerIndex = playerIndex;
	}

	public ControlScheme (KeyCode upKey, KeyCode downKey, KeyCode rightKey, KeyCode leftKey)
	{
		isGamePad = false;
		this.upKey = upKey;
		this.downKey = downKey;
		this.rightKey = rightKey;
		this.leftKey = leftKey;
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
		}
	}
}
