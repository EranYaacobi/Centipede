using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public partial class MultiplayerMenuGUIFrame
{
	[HideInInspector]
	public GUIFrameArea GUIFrameArea_537596899;
	[HideInInspector]
	public GUIFrameHorizontal GUIFrameHorizontal_1962052501;
	[HideInInspector]
	public GUIFrameVertical GUIFrameVertical_1495339226;
	[HideInInspector]
	public GUIFrameList GamesList;
	[HideInInspector]
	public GUIFrameVertical GUIFrameVertical_1674000384;
	[HideInInspector]
	public GUIFrameButton ConnectButton;
	[HideInInspector]
	public GUIFrameButton HostButton;
	public void InitializeComponents()
	{
		// Control GUIFrameArea_537596899:
		GUIFrameArea_537596899 = ScriptableObject.CreateInstance<GUIFrameArea>();
		GUIFrameArea_537596899.Name = "GUIFrameArea_537596899";
		GUIFrameArea_537596899.GUIStyle = "box";
		GUIFrameArea_537596899.Parent = null;
		#if UNITY_EDITOR
			GUIFrameArea_537596899.InspectorFoldout = true;
		#endif
		GUIFrameArea_537596899.RelativeMargins = new Vector2(0.1F, 0.1F);
		GUIFrameArea_537596899.MaximumMargins = new Vector2(100F, 100F);
		#if UNITY_EDITOR
			GUIFrameArea_537596899.MarginsInspectorFoldout = true;
		#endif
		GUIFrameArea_537596899.Controls = new List<GUIFrameControl>();
		// Control GUIFrameHorizontal_1962052501:
		GUIFrameHorizontal_1962052501 = ScriptableObject.CreateInstance<GUIFrameHorizontal>();
		GUIFrameHorizontal_1962052501.Name = "GUIFrameHorizontal_1962052501";
		GUIFrameHorizontal_1962052501.GUIStyle = "";
		GUIFrameHorizontal_1962052501.Parent = GUIFrameArea_537596899;
		#if UNITY_EDITOR
			GUIFrameHorizontal_1962052501.InspectorFoldout = true;
		#endif
		GUIFrameHorizontal_1962052501.Controls = new List<GUIFrameControl>();
		// Control GUIFrameVertical_1495339226:
		GUIFrameVertical_1495339226 = ScriptableObject.CreateInstance<GUIFrameVertical>();
		GUIFrameVertical_1495339226.Name = "GUIFrameVertical_1495339226";
		GUIFrameVertical_1495339226.GUIStyle = "";
		GUIFrameVertical_1495339226.Parent = GUIFrameHorizontal_1962052501;
		#if UNITY_EDITOR
			GUIFrameVertical_1495339226.InspectorFoldout = true;
		#endif
		GUIFrameVertical_1495339226.Controls = new List<GUIFrameControl>();
		// Control GamesList:
		GamesList = ScriptableObject.CreateInstance<GUIFrameList>();
		GamesList.Name = "GamesList";
		GamesList.GUIStyle = "label";
		GamesList.Parent = GUIFrameVertical_1495339226;
		#if UNITY_EDITOR
			GamesList.InspectorFoldout = true;
		#endif
		GamesList.Action = null;
		GamesList.BoxGUIStyle = "box";
		GUIFrameVertical_1495339226.Controls.Add(GamesList);
		GUIFrameHorizontal_1962052501.Controls.Add(GUIFrameVertical_1495339226);
		// Control GUIFrameVertical_1674000384:
		GUIFrameVertical_1674000384 = ScriptableObject.CreateInstance<GUIFrameVertical>();
		GUIFrameVertical_1674000384.Name = "GUIFrameVertical_1674000384";
		GUIFrameVertical_1674000384.GUIStyle = "";
		GUIFrameVertical_1674000384.Parent = GUIFrameHorizontal_1962052501;
		#if UNITY_EDITOR
			GUIFrameVertical_1674000384.InspectorFoldout = true;
		#endif
		GUIFrameVertical_1674000384.Controls = new List<GUIFrameControl>();
		// Control ConnectButton:
		ConnectButton = ScriptableObject.CreateInstance<GUIFrameButton>();
		ConnectButton.Name = "ConnectButton";
		ConnectButton.GUIStyle = "button";
		ConnectButton.Parent = GUIFrameVertical_1674000384;
		#if UNITY_EDITOR
			ConnectButton.InspectorFoldout = true;
		#endif
		ConnectButton.Text = "Connect";
		ConnectButton.Condition = null;
		ConnectButton.Action = null;
		ConnectButton.Size = new Vector2(0F, 0F);
		ConnectButton.MaximumSize = new Vector2(0F, 100F);
		#if UNITY_EDITOR
			ConnectButton.SizesInspectorFoldout = true;
		#endif
		GUIFrameVertical_1674000384.Controls.Add(ConnectButton);
		// Control HostButton:
		HostButton = ScriptableObject.CreateInstance<GUIFrameButton>();
		HostButton.Name = "HostButton";
		HostButton.GUIStyle = "button";
		HostButton.Parent = GUIFrameVertical_1674000384;
		#if UNITY_EDITOR
			HostButton.InspectorFoldout = true;
		#endif
		HostButton.Text = "Host";
		HostButton.Condition = null;
		HostButton.Action = null;
		HostButton.Size = new Vector2(0F, 0F);
		HostButton.MaximumSize = new Vector2(0F, 100F);
		#if UNITY_EDITOR
			HostButton.SizesInspectorFoldout = true;
		#endif
		GUIFrameVertical_1674000384.Controls.Add(HostButton);
		GUIFrameHorizontal_1962052501.Controls.Add(GUIFrameVertical_1674000384);
		GUIFrameArea_537596899.Controls.Add(GUIFrameHorizontal_1962052501);
		Area = GUIFrameArea_537596899;
		Name = "MultiplayerMenu";
	}
}
