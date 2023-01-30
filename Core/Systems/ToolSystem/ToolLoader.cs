using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolSystem
{
	/// <summary>
	///	Handles loaded <see cref="Tool"/> instances.
	/// </summary>
	internal static class ToolLoader
	{
		private static List<Tool> tools = new();

		public static ReadOnlyCollection<Tool> Tools => tools.AsReadOnly();

		internal static void AddTool(Tool tool)
		{
			tools.Add(tool);
		}

		internal static void Unload()
		{
			tools.Clear();
			tools = null;
		}
	}

	internal sealed class ToolLoaderSystem : ModSystem
	{
		public override void Unload()
		{
			ToolLoader.Unload();
		}
	}
}