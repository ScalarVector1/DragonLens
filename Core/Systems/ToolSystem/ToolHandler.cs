using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolSystem
{
	internal class ToolHandler : ModSystem
	{
		public readonly static List<Tool> tools = new();
		private readonly static Dictionary<Type, Tool> toolsByType = new();

		public static Tool GetTool<T>()
		{
			return toolsByType[typeof(T)];
		}

		public static Tool GetTool(string typeString)
		{
			Type type = DragonLens.instance.Code.GetType(typeString);
			return toolsByType[type];
		}

		public override void Load()
		{
			foreach (Type t in Mod.Code.GetTypes())
			{
				if (!t.IsAbstract && t.IsSubclassOf(typeof(Tool)))
				{
					var singleton = (Tool)Activator.CreateInstance(t);
					singleton.Load();

					tools.Add(singleton);
					toolsByType.Add(t, singleton);
				}
			}
		}
	}
}
