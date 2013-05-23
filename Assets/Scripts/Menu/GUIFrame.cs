using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GUIFrame : Photon.MonoBehaviour
{
	/// <summary>
	/// The name of the GUIFrame.
	/// This will also be the name of the created file and class.
	/// </summary>
	public String Name;

	/// <summary>
	/// The area of the GUIFrame.
	/// </summary>
	[HideInInspector]
	public GUIFrameArea Area;

	/// <summary>
	/// The frame to which to return on pressing the 'back' button.
	/// If null, the 'back' button isn't shown.
	/// </summary>
	[HideInInspector]
	public GUIFrame PreviousFrame;

	public void OnGUI()
	{
		if (PreviousFrame != null)
		{
			if (GUILayout.Button("back", GUILayout.Width(40), GUILayout.Height(40)))
				SwitchBack();
		}

		Area.Draw();
	}

	/// <summary>
	/// Switches to the given frame.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	protected void SwitchToFrame<T>() where T : GUIFrame
	{
		var NextMenu = GetComponent<T>();
		NextMenu.enabled = true;
		NextMenu.PreviousFrame = this;
		enabled = false;
	}

	/// <summary>
	/// Switches to the previous frame.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	protected virtual void SwitchBack()
	{
		PreviousFrame.enabled = true;
		enabled = false;
	}
}