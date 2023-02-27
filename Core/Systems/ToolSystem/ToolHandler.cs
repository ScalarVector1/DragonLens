using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolSystem
{
	/// <summary>
	/// Maintains a collection of all loaded tools.
	/// </summary>
	internal static class ToolHandler
	{
		private static readonly List<Tool> tools = new();

		/// <summary>
		/// All tools currently loaded
		/// </summary>
		public static ReadOnlyCollection<Tool> Tools => tools.AsReadOnly();

		/// <summary>
		/// Adds a tool to the collection of loaded tools
		/// </summary>
		/// <param name="tool">The tool to add</param>
		internal static void AddTool(Tool tool)
		{
			tools.Add(tool);
		}
	}

	/// <summary>
	/// Used to handle the hotkeys for tools.
	/// </summary>
	internal class ToolPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			foreach (Tool tool in ToolHandler.Tools)
			{
				if (tool.keybind.JustPressed)
					tool.OnActivate();

				if (tool.HasRightClick && tool.altKeybind.JustPressed)
					tool.OnRightClick();
			}
		}
	}
}
