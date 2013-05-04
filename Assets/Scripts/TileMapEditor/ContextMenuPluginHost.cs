using UnityEngine;

/// <summary>
/// Provides a component for context menu plugins
/// This class is not inside TileMapEditorPackage, because it causes problems when creating it via code.
/// </summary>
public class ContextMenuPluginHost : MonoBehaviour
{
	/// <summary>
	/// The actual plugin.
	/// </summary>
	public Object ContextMenuPlugin;
}
