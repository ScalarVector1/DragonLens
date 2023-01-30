using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolSystem
{
	internal class ToolHandler : ModSystem
	{
		private readonly static List<Tool> tools = new();

		public static ReadOnlyCollection<Tool> Tools => tools.AsReadOnly();

		internal static void AddTool(Tool tool)
		{
			tools.Add(tool);
		}
	}
}
