using System;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolSystem
{
	internal class ToolHandler : ModSystem
	{
		public readonly static List<Tool> tools = new();
		private readonly static Dictionary<Type, Tool> toolsByType = new();

		public static Tool GetTool<T>() where T : Tool
		{
			if (!toolsByType.ContainsKey(typeof(T)))
				return null;

			return toolsByType[typeof(T)];
		}

		public static Tool GetTool(string typeString)
		{
			Type type = ModContent.GetInstance<DragonLens>().Code.GetType(typeString);
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

					ModKeybind bind = KeybindLoader.RegisterKeybind(Mod, singleton.Name, Microsoft.Xna.Framework.Input.Keys.None);
					singleton.keybind = bind;

					tools.Add(singleton);
					toolsByType.Add(t, singleton);		
				}
			}
		}
	}

	internal class ToolPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			foreach(Tool tool in ToolHandler.tools)
			{
				if (tool.keybind.JustPressed)
					tool.OnActivate();
			}
		}
	}
}
