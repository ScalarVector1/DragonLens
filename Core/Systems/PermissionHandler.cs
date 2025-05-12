using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems
{
	internal class PermissionHandler : ModSystem
	{
		public static string worldID = Guid.NewGuid().ToString();

		/// <summary>
		/// Admin list. On the server contains all admins, on a local client will contain your ID if you are adming
		/// and will not if you are not.
		/// </summary>
		public static List<string> admins = new();

		/// <summary>
		/// List of player indicies who are admins, used to sync this data visually without sending actual IDs
		/// </summary>
		public static List<int> visualAdmins = new();

		/// <summary>
		/// Determines if a player can use tools or not, based on their netmode and admin status.
		/// </summary>
		/// <param name="player">The player to query for usability.</param>
		/// <returns>If that player should be able to use tools or not.</returns>
		public static bool CanUseTools(Player player)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
				return true;
			else
				return admins.Contains(player.GetModPlayer<PermissionPlayer>().currentServerID);
		}

		/// <summary>
		/// If a player is an admin or not. Use this to tell if they are an admin from a foreign client.
		/// Should only be used for display purposes.
		/// </summary>
		/// <param name="player">The player to check</param>
		/// <returns>If they are an admin or not</returns>
		public static bool LooksLikeAdmin(Player player)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
				return true;
			else
				return visualAdmins.Contains(player.whoAmI);
		}

		/// <summary>
		/// This method adds a player to the list of admins, which can use tools.
		/// </summary>
		/// <param name="player">The player to add. They must not already be an admin.</param>
		public static void AddAdmin(Player player)
		{
			ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
			packet.Write("AdminUpdate");
			packet.Write(0);
			packet.Write(player.whoAmI);
			packet.Send();

			if (Main.netMode == NetmodeID.Server)
			{
				admins.Add(player.GetModPlayer<PermissionPlayer>().currentServerID);
				visualAdmins.Add(player.whoAmI);

				SendVisualAdmins();
			}
		}

		/// <summary>
		/// This method removes a player from the list of admins, preventing them from using tools.
		/// </summary>
		/// <param name="player">The player to remove. They must be an admin.</param>
		public static void RemoveAdmin(Player player)
		{
			ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
			packet.Write("AdminUpdate");
			packet.Write(1);
			packet.Write(player.whoAmI);
			packet.Send();

			if (Main.netMode == NetmodeID.Server)
			{
				admins.Remove(player.GetModPlayer<PermissionPlayer>().currentServerID);
				visualAdmins.Remove(player.whoAmI);

				SendVisualAdmins();
			}
		}

		/// <summary>
		/// Called by the server to send the servers current tool data to new clients
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="sender"></param>
		public static void SendToolData(int sender)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ModLoader.GetMod("DragonLens").Logger.Error("Client recieved a request intended for the server!");
				return;
			}

			foreach (Tool tool in ModContent.GetContent<Tool>())
			{
				if (tool.SyncOnClientJoint)
					tool.NetSend(sender, -1);
			}
		}

		private static void SendVisualAdmins()
		{
			ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
			packet.Write("AdminUpdate");
			packet.Write(2);
			packet.Write(visualAdmins.Count);

			foreach (int admin in visualAdmins)
			{
				packet.Write(admin);
			}

			packet.Send();
		}

		/// <summary>
		/// Handles an admin status update packet from the net
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="sender"></param>
		public static void HandlePacket(BinaryReader reader, int whoAmI)
		{
			int operation = reader.ReadInt32(); //First read the operation type
			ModLoader.GetMod("DragonLens").Logger.Info("Recieved permission packet: " + operation);

			if (operation == 0) //Set admin
			{
				if (Main.netMode == NetmodeID.Server)
				{
					Player player = Main.player[reader.ReadInt32()];

					if (!admins.Contains(player.GetModPlayer<PermissionPlayer>().currentServerID))
						admins.Add(player.GetModPlayer<PermissionPlayer>().currentServerID);

					visualAdmins.Add(player.whoAmI);

					ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
					packet.Write("AdminUpdate");
					packet.Write(0);
					packet.Send(player.whoAmI);

					SendVisualAdmins();
				}
				else
				{
					if (!admins.Contains(Main.LocalPlayer.GetModPlayer<PermissionPlayer>().currentServerID))
						admins.Add(Main.LocalPlayer.GetModPlayer<PermissionPlayer>().currentServerID);

					Main.NewText($"You are now an admin.", Color.Yellow);
				}
			}
			else if (operation == 1) //Remove admin
			{
				if (Main.netMode == NetmodeID.Server)
				{
					Player player = Main.player[reader.ReadInt32()];
					admins.RemoveAll(n => n == player.GetModPlayer<PermissionPlayer>().currentServerID);
					visualAdmins.Remove(player.whoAmI);

					ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
					packet.Write("AdminUpdate");
					packet.Write(1);
					packet.Send(player.whoAmI);

					SendVisualAdmins();
				}
				else
				{
					admins.RemoveAll(n => n == Main.LocalPlayer.GetModPlayer<PermissionPlayer>().currentServerID);
					Main.NewText($"You are no longer an admin.", Color.Yellow);

					foreach (SmartUIState item in UILoader.UIStates)
					{
						if (item is ToolbarState)
							continue;

						item.Visible = false;
					}
				}
			}
			else if (operation == 2) //Sync visual only
			{
				if (Main.netMode == NetmodeID.Server)
				{
					SendVisualAdmins();
				}
				else
				{
					visualAdmins.Clear();
					int count = reader.ReadInt32();

					for (int k = 0; k < count; k++)
					{
						visualAdmins.Add(reader.ReadInt32());
					}
				}
			}
			else if (operation == 3) //Send server ID
			{
				if (Main.netMode == NetmodeID.Server) // When the server gets this packet, send the world ID back to the client
				{
					ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
					packet.Write("AdminUpdate");
					packet.Write(3);
					packet.Write(worldID);
					packet.Send(whoAmI);
				}
				else // When the client gets this packet, send their ID for that world back to the server
				{
					worldID = reader.ReadString();

					PermissionPlayer mp = Main.LocalPlayer.GetModPlayer<PermissionPlayer>();

					// Safety check to make sure we dont generate a duplicate ID since that seems to be possible in rare cases?
					if (mp.IDs.Count <= 0)
						mp.LoadIDs();

					if (!mp.IDs.ContainsKey(worldID))
						mp.GenerateID();

					ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
					packet.Write("AdminUpdate");
					packet.Write(4);
					packet.Write(mp.IDs[worldID]);
					packet.Send();
				}
			}
			else if (operation == 4) //Validate ID
			{
				if (Main.netMode == NetmodeID.Server)
				{
					string ID = reader.ReadString();
					Main.player[whoAmI].GetModPlayer<PermissionPlayer>().currentServerID = ID;
				}
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["worldID"] = worldID;

			// safety check to make sure we never try to save nulls?
			if (admins is null)
				return;

			admins.RemoveAll(n => n is null);

			tag["admins"] = admins.ToList();
		}

		public override void LoadWorldData(TagCompound tag)
		{
			worldID = tag.GetString("worldID");

			if (string.IsNullOrEmpty(worldID))
				worldID = Guid.NewGuid().ToString();

			admins = (List<string>)tag.GetList<string>("admins");
		}
	}

	/// <summary>
	/// Handles clients getting the correct info on connection
	/// </summary>
	public class PermissionPlayer : ModPlayer
	{
		/// <summary>
		/// Dictionary of all IDs, stored in memory only on the local client. ONLY THE RELEVANT
		/// ID SHOULD EVER BE SENT TO A SERVER!!!
		/// </summary>
		public Dictionary<string, string> IDs = new();

		public string currentServerID;

		public void LoadIDs()
		{
			IDs.Clear();
			string dir = Path.Join(Main.SavePath, "DragonLensID");

			// Create ID file if it does not exist
			if (!File.Exists(dir))
			{
				File.WriteAllText(dir,
					"#========================= [ WARNING ] =========================\n" +
					"#This file contains your DragonLens IDs, which are what\n" +
					"#identify you across various servers you visit. These are\n" +
					"#essentially your passwords for these servers! YOU SHOULD\n" +
					"#NOT SHARE THESE WITH ANYONE, EVER, FOR ANY REASON! If you\n" +
					"#are being asked for these, someone is trying to impersonate\n" +
					"#you to gain admin permissions on a server!\n" +
					"#===============================================================\n");
			}

			// Read all IDs on local client
			string[] lines = File.ReadAllLines(dir);

			foreach (string line in lines.Where(n => !n.StartsWith("#")))
			{
				if (line.Contains(":"))
				{
					string[] parts = line.Split(":");
					IDs.TryAdd(parts[0], parts[1]);
				}
			}
		}

		public void GenerateID()
		{
			string dir = Path.Join(Main.SavePath, "DragonLensID");

			string newID = Guid.NewGuid().ToString();
			IDs.Add(PermissionHandler.worldID, newID);
			File.AppendAllText(dir, $"{PermissionHandler.worldID}:{newID}\n");
		}

		public override void OnEnterWorld() // Send an admin list sync request on entering the server
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about admins
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				if (IDs.Count <= 0)
					LoadIDs();

				// Send request for world ID and to send my ID back
				ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
				packet.Write("AdminUpdate");
				packet.Write(3);
				packet.Send();

				// Send request for visual admin info
				packet = ModLoader.GetMod("DragonLens").GetPacket();
				packet.Write("AdminUpdate");
				packet.Write(2);
				packet.Send();
			}

			if (Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost()) // The host is automatically an admin!
			{
				PermissionHandler.AddAdmin(Player);

				foreach (Tool tool in ModContent.GetContent<Tool>()) // The hosts settings get applied
				{
					if (tool.SyncOnClientJoint)
						tool.NetSend();
				}
			}
			else // Otherwise ask for the servers tool data
			{
				ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
				packet.Write("ToolDataRequest");
				packet.Send();
			}

			// Close all windows to make sure non-admins cant have tools already open
			foreach (SmartUIState item in UILoader.UIStates)
			{
				if (item is ToolbarState)
					continue;

				item.Visible = false;
			}
		}
	}
}