using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolSystem
{
	internal static class ToolHandler
	{
		private static readonly List<Tool> tools = new();

		public static ReadOnlyCollection<Tool> Tools => tools.AsReadOnly();
		
		internal static void AddTool(Tool tool)
		{
			tools.Add(tool);
		}
	}

	internal class ToolPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			foreach(Tool tool in ToolHandler.Tools)
			{
				if (tool.keybind.JustPressed)
					tool.OnActivate();
			}
		}
	}
}
