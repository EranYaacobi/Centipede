using System;
using System.Collections.Generic;
using UnityEngine;
public partial class MainMenuGUIFrame
{
	[HideInInspector]
	public GUIFrameArea Area_946817090;
	[HideInInspector]
	public GUIFrameArea InnerArea;
	[HideInInspector]
	public GUIFrameButton StartCampaignButton;
	[HideInInspector]
	public GUIFrameButton ContinueCampaignButton;
	[HideInInspector]
	public GUIFrameButton LoadCampaignButton;
	[HideInInspector]
	public GUIFrameButton MultiplayerButton;
	[HideInInspector]
	public GUIFrameButton EditorButton;
	[HideInInspector]
	public GUIFrameFlexibleSpace FlexibleSpace_874724264;
	[HideInInspector]
	public GUIFrameButton QuitButton;
	public void InitializeComponents()
	{
		// Control Area_946817090:
		Area_946817090 = ScriptableObject.CreateInstance<GUIFrameArea>();
		Area_946817090.Name = "Area_946817090";
		Area_946817090.GUIStyle = "box";
		Area_946817090.Parent = null;
		#if UNITY_EDITOR
			Area_946817090.InspectorFoldout = true;
		#endif
		Area_946817090.RelativeMargins = new Vector2(0.1F, 0.1F);
		Area_946817090.MaximumMargins = new Vector2(100F, 100F);
		#if UNITY_EDITOR
			Area_946817090.MarginsInspectorFoldout = false;
		#endif
		Area_946817090.Controls = new List<GUIFrameControl>();
		// Control InnerArea:
		InnerArea = ScriptableObject.CreateInstance<GUIFrameArea>();
		InnerArea.Name = "InnerArea";
		InnerArea.GUIStyle = "box";
		InnerArea.Parent = Area_946817090;
		#if UNITY_EDITOR
			InnerArea.InspectorFoldout = true;
		#endif
		InnerArea.RelativeMargins = new Vector2(0.25F, 0.1F);
		InnerArea.MaximumMargins = new Vector2(400F, 100F);
		#if UNITY_EDITOR
			InnerArea.MarginsInspectorFoldout = true;
		#endif
		InnerArea.Controls = new List<GUIFrameControl>();
		// Control StartCampaignButton:
		StartCampaignButton = ScriptableObject.CreateInstance<GUIFrameButton>();
		StartCampaignButton.Name = "StartCampaignButton";
		StartCampaignButton.GUIStyle = "button";
		StartCampaignButton.Parent = InnerArea;
		#if UNITY_EDITOR
			StartCampaignButton.InspectorFoldout = true;
		#endif
		StartCampaignButton.Text = "Start Campaign";
		StartCampaignButton.Condition = null;
		StartCampaignButton.Action = null;
		StartCampaignButton.Size = new Vector2(0F, 0F);
		StartCampaignButton.MaximumSize = new Vector2(0F, 130F);
		#if UNITY_EDITOR
			StartCampaignButton.SizesInspectorFoldout = false;
		#endif
		InnerArea.Controls.Add(StartCampaignButton);
		// Control ContinueCampaignButton:
		ContinueCampaignButton = ScriptableObject.CreateInstance<GUIFrameButton>();
		ContinueCampaignButton.Name = "ContinueCampaignButton";
		ContinueCampaignButton.GUIStyle = "button";
		ContinueCampaignButton.Parent = InnerArea;
		#if UNITY_EDITOR
			ContinueCampaignButton.InspectorFoldout = true;
		#endif
		ContinueCampaignButton.Text = "Continue";
		ContinueCampaignButton.Condition = null;
		ContinueCampaignButton.Action = null;
		ContinueCampaignButton.Size = new Vector2(0F, 0F);
		ContinueCampaignButton.MaximumSize = new Vector2(0F, 130F);
		#if UNITY_EDITOR
			ContinueCampaignButton.SizesInspectorFoldout = false;
		#endif
		InnerArea.Controls.Add(ContinueCampaignButton);
		// Control LoadCampaignButton:
		LoadCampaignButton = ScriptableObject.CreateInstance<GUIFrameButton>();
		LoadCampaignButton.Name = "LoadCampaignButton";
		LoadCampaignButton.GUIStyle = "button";
		LoadCampaignButton.Parent = InnerArea;
		#if UNITY_EDITOR
			LoadCampaignButton.InspectorFoldout = true;
		#endif
		LoadCampaignButton.Text = "Load";
		LoadCampaignButton.Condition = null;
		LoadCampaignButton.Action = null;
		LoadCampaignButton.Size = new Vector2(0F, 0F);
		LoadCampaignButton.MaximumSize = new Vector2(0F, 130F);
		#if UNITY_EDITOR
			LoadCampaignButton.SizesInspectorFoldout = false;
		#endif
		InnerArea.Controls.Add(LoadCampaignButton);
		// Control MultiplayerButton:
		MultiplayerButton = ScriptableObject.CreateInstance<GUIFrameButton>();
		MultiplayerButton.Name = "MultiplayerButton";
		MultiplayerButton.GUIStyle = "button";
		MultiplayerButton.Parent = InnerArea;
		#if UNITY_EDITOR
			MultiplayerButton.InspectorFoldout = true;
		#endif
		MultiplayerButton.Text = "Multiplayer";
		MultiplayerButton.Condition = null;
		MultiplayerButton.Action = null;
		MultiplayerButton.Size = new Vector2(0F, 0F);
		MultiplayerButton.MaximumSize = new Vector2(0F, 130F);
		#if UNITY_EDITOR
			MultiplayerButton.SizesInspectorFoldout = false;
		#endif
		InnerArea.Controls.Add(MultiplayerButton);
		// Control EditorButton:
		EditorButton = ScriptableObject.CreateInstance<GUIFrameButton>();
		EditorButton.Name = "EditorButton";
		EditorButton.GUIStyle = "button";
		EditorButton.Parent = InnerArea;
		#if UNITY_EDITOR
			EditorButton.InspectorFoldout = true;
		#endif
		EditorButton.Text = "Editor";
		EditorButton.Condition = null;
		EditorButton.Action = null;
		EditorButton.Size = new Vector2(0F, 0F);
		EditorButton.MaximumSize = new Vector2(0F, 130F);
		#if UNITY_EDITOR
			EditorButton.SizesInspectorFoldout = false;
		#endif
		InnerArea.Controls.Add(EditorButton);
		// Control FlexibleSpace_874724264:
		FlexibleSpace_874724264 = ScriptableObject.CreateInstance<GUIFrameFlexibleSpace>();
		FlexibleSpace_874724264.Name = "FlexibleSpace_874724264";
		FlexibleSpace_874724264.GUIStyle = "";
		FlexibleSpace_874724264.Parent = InnerArea;
		#if UNITY_EDITOR
			FlexibleSpace_874724264.InspectorFoldout = true;
		#endif
		FlexibleSpace_874724264.MinimumSize = 75F;
		FlexibleSpace_874724264.MinimumSizeRate = 0.2F;
		InnerArea.Controls.Add(FlexibleSpace_874724264);
		// Control QuitButton:
		QuitButton = ScriptableObject.CreateInstance<GUIFrameButton>();
		QuitButton.Name = "QuitButton";
		QuitButton.GUIStyle = "button";
		QuitButton.Parent = InnerArea;
		#if UNITY_EDITOR
			QuitButton.InspectorFoldout = true;
		#endif
		QuitButton.Text = "Quit";
		QuitButton.Condition = null;
		QuitButton.Action = null;
		QuitButton.Size = new Vector2(0F, 0F);
		QuitButton.MaximumSize = new Vector2(0F, 130F);
		#if UNITY_EDITOR
			QuitButton.SizesInspectorFoldout = false;
		#endif
		InnerArea.Controls.Add(QuitButton);
		Area_946817090.Controls.Add(InnerArea);
		Area = Area_946817090;
		Name = "MainMenu";
	}
}
