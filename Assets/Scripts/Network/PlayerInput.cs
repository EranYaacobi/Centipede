using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class PlayerInput : Photon.MonoBehaviour
{
	private const Single LocalInputDelay = 0;

	/// <summary>
	/// Holds references for all PlayerInputs.
	/// </summary>
	private static readonly Dictionary<PhotonPlayer, PlayerInput> PlayersInput = new Dictionary<PhotonPlayer, PlayerInput>();

	/// <summary>
	/// Holds the state of all the registered buttons.
	/// </summary>
	private readonly Dictionary<String, ButtonState> Buttons = new Dictionary<String, ButtonState>();

	/// <summary>
	/// Holds the value of the horizontal axis.
	/// </summary>
	private Single HorizontalAxis;

	/// <summary>
	/// The player whose input is recorded.
	/// </summary>
	private PhotonPlayer Owner;

	public static PlayerInput GetPlayerInput(PhotonPlayer Player)
	{
		return PlayersInput[Player];
	}

	/// <summary>
	/// Initializes the PlayerInput.
	/// </summary>
	public void Initialize(PhotonPlayer Owner)
	{
		this.Owner = Owner;
		PlayersInput[Owner] = this;
	}

	// Update is called once per frame
	void Update()
	{
		if (Owner == PhotonNetwork.player)
		{
			var NewHorizontalAxis = Input.GetAxis(Keys.Horizontal);

			if (NewHorizontalAxis != HorizontalAxis)
				photonView.RPC("UpdateHorizontalAxis", PhotonTargets.All, NewHorizontalAxis);

			foreach (var Button in Buttons.ToArray())
			{
				var NewButtonState = CheckButtonState(Button.Key);
				if (NewButtonState != Button.Value)
				{
					// Not sending update for Down an Up, as it may cause missing the Pressed and Released states.
					if ((NewButtonState != ButtonState.Down) && (NewButtonState != ButtonState.Up))
						photonView.RPC("UpdateButtonState", PhotonTargets.All, Button.Key, (Int32)NewButtonState);
				}
			}
		}

		foreach (var Button in Buttons.ToArray())
		{
			if (Button.Value == ButtonState.Released)
				Buttons[Button.Key] = ButtonState.Up;
			else if (Button.Value == ButtonState.Down)
				Buttons[Button.Key] = ButtonState.Pressed;
		}
	}

	private ButtonState CheckButtonState(String ButtonName)
	{
		if (Input.GetButtonDown(ButtonName))
			return ButtonState.Pressed;

		if (Input.GetButton(ButtonName))
			return ButtonState.Down;

		if (Input.GetButtonUp(ButtonName))
			return ButtonState.Released;

		return ButtonState.Up;
	}

	[RPC]
	private IEnumerator UpdateHorizontalAxis(Single NewHorizontalAxis)
	{
		if (Owner == PhotonNetwork.player)
			yield return new WaitForSeconds(LocalInputDelay);
		else
			yield return new WaitForEndOfFrame();

		HorizontalAxis = NewHorizontalAxis;
	}

	[RPC]
	private IEnumerator UpdateButtonState(String ButtonName, Int32 NewButtonState)
	{
		if (Owner == PhotonNetwork.player)
			yield return new WaitForSeconds(LocalInputDelay);
		else
			yield return new WaitForEndOfFrame();

		Buttons[ButtonName] = (ButtonState)NewButtonState;
	}

	/// <summary>
	/// Gets the value of the horizontal axis.
	/// </summary>
	/// <returns></returns>
	public Single GetHorizontalAxis()
	{
		return HorizontalAxis;
	}

	/// <summary>
	/// Registers a button for input.
	/// </summary>
	/// <param name="Button"></param>
	public void RegisterButton(String Button)
	{
		Buttons[Button] = CheckButtonState(Button);
	}

	/// <summary>
	/// Gets the state of the given button.
	/// </summary>
	/// <param name="Button"></param>
	/// <returns></returns>
	public ButtonState GetButtonState(String Button)
	{
		return Buttons[Button];
	}

	/// <summary>
	/// Returns whether the given button is pressed or held down.
	/// </summary>
	/// <param name="Button"></param>
	/// <returns></returns>
	public Boolean ButtonDown(String Button)
	{
		var ButtonState = Buttons[Button];
		return (ButtonState == ButtonState.Pressed) || (ButtonState == ButtonState.Down);
	}

	/// <summary>
	/// Returns whether the given button is up or released.
	/// </summary>
	/// <param name="Button"></param>
	/// <returns></returns>
	public Boolean ButtonUp(String Button)
	{
		var ButtonState = Buttons[Button];
		return (ButtonState == ButtonState.Up) || (ButtonState == ButtonState.Released);
	}

	public enum ButtonState
	{
		/// <summary>
		/// Indicates that the button is not pressed.
		/// </summary>
		Up,
		
		/// <summary>
		/// Indicates that the button was pressed this frame.
		/// </summary>
		Pressed,

		/// <summary>
		/// Indicates that the button is held down.
		/// </summary>
		Down,

		/// <summary>
		/// Indicates that the button is released.
		/// </summary>
		Released
	}
}
