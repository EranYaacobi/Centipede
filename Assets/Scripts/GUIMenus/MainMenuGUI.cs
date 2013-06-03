using System;
using System.Collections.Generic;
using UnityEngine;
using GUIDesigner.Scripts;
using GUIDesigner.Scripts.Controls;

public partial class MainMenuGUI : MonoBehaviour
{
	/// <summary>
	/// Performs custom initialization of the GUIFrame.
	/// Use this function instead of Start().
	/// </summary>
	private void Initialize()
	{
	}

	private void Multiplayer(GUIControlEventArgs EventArgs)
	{
		GameObject.Find("MultiplayerMenu").GetComponent<GUIFrame>().enabled = true;
		GetComponent<GUIFrame>().enabled = false;
	}

	private void Quit(GUIControlEventArgs EventArgs)
	{
		Application.Quit();
	}
}
