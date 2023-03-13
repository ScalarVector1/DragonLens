using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems.ToolSystem
{
	/// <summary>
	/// Maintains a collection of all loaded tools.
	/// </summary>
	internal class ToolHandler : ModSystem
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

		/// <summary>
		/// Checks every tool if it has saved data, and if it does, loads it for that tool (remember, tools are singletons!)
		/// </summary>
		/// <param name="path">The path to the tool data file</param>
		private void LoadToolData(string path)
		{
			TagCompound tag = TagIO.FromFile(path);

			if (tag is null)
				return;

			foreach (Tool tool in Tools)
			{
				if (tag.ContainsKey(tool.Name))
					tool.LoadData(tag.Get<TagCompound>(tool.Name));
			}
		}

		/// <summary>
		/// Saves the data for every tool
		/// </summary>
		/// <param name="path">The path to the tool data file</param>
		private void SaveToolData(string path)
		{
			var tag = new TagCompound();

			foreach (Tool tool in Tools)
			{
				var toolTag = new TagCompound();
				tool.SaveData(toolTag);

				if (toolTag.Count > 0)
					tag[tool.Name] = toolTag;
			}

			if (!File.Exists(path))
			{
				FileStream stream = File.Create(path);
				stream.Close();
			}

			TagIO.ToFile(tag, path);
		}

		/// <summary>
		/// Load or create the tool data file and directory
		/// </summary>
		public override void OnModLoad()
		{
			string currentPath = Path.Join(Main.SavePath, "DragonLensLayouts", "ToolData", "ToolData");

			if (File.Exists(currentPath))
			{
				LoadToolData(currentPath);
			}
			else
			{
				string dir = Path.Join(Main.SavePath, "DragonLensLayouts", "ToolData");
				Directory.CreateDirectory(dir);

				File.Create(currentPath);
			}
		}

		/// <summary>
		/// Save tool data on world save
		/// </summary>
		public override void OnWorldUnload()
		{
			string currentPath = Path.Join(Main.SavePath, "DragonLensLayouts", "ToolData", "ToolData");

			SaveToolData(currentPath);
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
