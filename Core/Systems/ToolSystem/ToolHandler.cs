using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
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
		/// Helper method for quickly sending the packet of a given tool from outside of itself
		/// </summary>
		/// <typeparam name="T">The type of the tool to send the packet of</typeparam>
		public static void NetSend<T>(int toClient = -1, int ignoreClient = -1) where T : Tool
		{
			ModContent.GetInstance<T>().NetSend(toClient, ignoreClient);
		}

		/// <summary>
		/// Handles network packets, sending them to the correct tool automatically
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="sender"></param>
		public static void HandlePacket(BinaryReader reader, int sender)
		{
			string tool = reader.ReadString();
			Tool target = Tools.FirstOrDefault(n => n.Name == tool);

			ModLoader.GetMod("DragonLens").Logger.Info($"Recieved packet for tool {tool} from {sender}");

			if (target != null)
				target.RecievePacket(reader, sender);
			else
				ModLoader.GetMod("DragonLens").Logger.Warn($"Recieved a packet with an invalid tool path: '{tool}'! cannot route!");
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
			if (Main.netMode == NetmodeID.Server)
				return;

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
			if (Main.netMode == NetmodeID.Server)
				return;

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
