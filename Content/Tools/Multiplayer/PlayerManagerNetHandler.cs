using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Multiplayer
{
	internal class PlayerManagerNetHandler
	{
		/// <summary>
		/// Handles an incomping ModPacket for the player manager
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="sender"></param>
		public static void HandlePacket(BinaryReader reader, int sender)
		{
			string type = reader.ReadString();

			if (type == "Item")
				RecieveItem(reader, sender);

			if (type == "Kick")
				RecieveKick(reader);
		}

		/// <summary>
		/// Helper to get a packet which write the appropriate prefixes for this handler
		/// </summary>
		/// <param name="type">string key for what we should do with this packet</param>
		/// <returns>a ModPacket to send</returns>
		public static ModPacket GetPacket(string type)
		{
			ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
			packet.Write("PlayerManager");
			packet.Write(type);

			return packet;
		}

		/// <summary>
		/// Sends an item update for a player's inventory, forcibly updating it from the inventory manager
		/// </summary>
		/// <param name="player">The WhoAmI of the player to manipulate the inventory of</param>
		/// <param name="index">The index of the item in the inventory to change</param>
		/// <param name="inventory">The ID of the inventory to change, see switch case inside for valid values</param>
		/// <param name="item">The item to replace that slot with</param>
		public static void SendItem(int player, int index, int inventory, Item item)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
				return;

			if (item is null)
				return;

			ModPacket packet = GetPacket("Item");

			packet.Write(player);
			packet.Write(index);
			packet.Write(inventory);
			ItemIO.Send(item, packet, true);

			packet.Send();
		}

		private static void RecieveItem(BinaryReader reader, int sender)
		{
			int pIndex = reader.ReadInt32();
			Player player = Main.player[pIndex];

			int index = reader.ReadInt32();
			int invIndex = reader.ReadInt32();

			Item[] inventory = invIndex switch
			{
				0 => player.inventory,
				2 => player.armor,
				4 => player.bank.item,
				6 => player.bank2.item,
				8 => player.bank3.item,
				10 => player.bank4.item,
				_ => player.inventory
			};

			Item item = ItemIO.Receive(reader, true);
			inventory[index] = item.Clone();

			if (Main.netMode == NetmodeID.Server)
				SendItem(pIndex, index, invIndex, item, sender);
		}

		/// <summary>
		/// Sends a packet to ban a player
		/// </summary>
		/// <param name="playerToKick">The WhoAmI of the player to ban</param>
		public static void SendKick(int playerToKick)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
				return;

			ModPacket packet = GetPacket("Kick");
			packet.Write(playerToKick);
			packet.Send();
		}

		private static void RecieveKick(BinaryReader reader)
		{
			NetMessage.SendData(MessageID.Kick, reader.ReadInt32(), -1, NetworkText.FromLiteral("You were kicked by a DragonLens admin."));
		}
	}
}