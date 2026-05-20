using DragonLens.Content.Tools.Multiplayer.Trackers;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace DragonLens.Content.Tools.Multiplayer
{
	internal static class PingTrackerNetHandler
	{
		public static void HandlePacket(BinaryReader reader, int sender)
		{
			string type = reader.ReadString();

			if (type == "PingRequest")
				ReceivePingRequest(reader, sender);

			if (type == "PingResponse")
				ReceivePingResponse(reader);

			if (type == "PingValue")
				ReceivePingValue(reader, sender);

			if (type == "FullSync")
				ReceiveFullSync(reader);
		}

		private static ModPacket GetPacket(string type)
		{
			ModPacket packet = ModLoader.GetMod("DragonLens").GetPacket();
			packet.Write("PingTracker");
			packet.Write(type);
			return packet;
		}

		public static void SendPingRequest(long pingId, long sentTicks)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			ModPacket packet = GetPacket("PingRequest");
			packet.Write(pingId);
			packet.Write(sentTicks);
			packet.Send();
		}

		private static void ReceivePingRequest(BinaryReader reader, int sender)
		{
			if (Main.netMode != NetmodeID.Server)
				return;

			long pingId = reader.ReadInt64();
			long sentTicks = reader.ReadInt64();

			ModPacket packet = GetPacket("PingResponse");
			packet.Write(pingId);
			packet.Write(sentTicks);
			packet.Send(toClient: sender);
		}

		private static void ReceivePingResponse(BinaryReader reader)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			long pingId = reader.ReadInt64();
			long sentTicks = reader.ReadInt64();
			PingTracker.ReceivePingResponse(pingId, sentTicks);
		}

		public static void SendPingValue(int playerIndex, int pingMs)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			ModPacket packet = GetPacket("PingValue");
			packet.Write(playerIndex);
			packet.Write(pingMs);
			packet.Send();
		}

		private static void ReceivePingValue(BinaryReader reader, int sender)
		{
			if (Main.netMode != NetmodeID.Server)
				return;

			int playerIndex = reader.ReadInt32();
			int pingMs = reader.ReadInt32();

			PingTracker.SetPing(playerIndex, pingMs);
			SendFullSync();
		}

		public static void SendFullSync(int toClient = -1)
		{
			if (Main.netMode != NetmodeID.Server)
				return;

			ModPacket packet = GetPacket("FullSync");
			packet.Write(PingTracker.Pings.Count);

			foreach ((int playerIndex, int pingMs) in PingTracker.Pings)
			{
				packet.Write(playerIndex);
				packet.Write(pingMs);
			}

			packet.Send(toClient);
		}

		private static void ReceiveFullSync(BinaryReader reader)
		{
			PingTracker.Pings.Clear();

			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int playerIndex = reader.ReadInt32();
				int pingMs = reader.ReadInt32();
				PingTracker.Pings[playerIndex] = pingMs;
			}
		}
	}
}