using Microsoft.Xna.Framework;
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
			try
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
			catch
			{
				Mod.Logger.Error("Tool data file could not be read!");
				return;
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

			CreateOrLoadData();
		}

		/// <summary>
		/// Load our data when we enter worlds, so that it resets after a multiplayer session
		/// </summary>
		public override void OnWorldLoad()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
				return;

			CreateOrLoadData();
		}

		/// <summary>
		/// Checks if the tool data path exists, and if it does not, creates it and saves default data. If it does, the data from the tool path is loaded.
		/// </summary>
		private void CreateOrLoadData()
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
				SaveToolData(currentPath);
			}
		}

		/// <summary>
		/// Save tool data on world save
		/// </summary>
		public override void OnWorldUnload()
		{
			if (Main.netMode != NetmodeID.SinglePlayer) // We dont want to lift the tool data from our last multiplayer session, that would be silly...
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
		/// <summary>
		/// Handles the triggers for all tool hotkeys. Also checks against admin status in multiplayer so non-admins cant use hotkeys to access tools.
		/// </summary>
		/// <param name="triggersSet"></param>
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			foreach (Tool tool in ToolHandler.Tools)
			{
				if (tool.keybind.JustPressed)
				{
					if (!PermissionHandler.CanUseTools(Main.LocalPlayer))
					{
						Main.NewText("You are not an admin!", Color.Red);
						return;
					}

					tool.OnActivate();
				}

				if (tool.HasRightClick && tool.altKeybind.JustPressed)
				{
					if (!PermissionHandler.CanUseTools(Main.LocalPlayer))
					{
						Main.NewText("You are not an admin!", Color.Red);
						return;
					}

					tool.OnRightClick();
				}
			}
		}
	}
}
