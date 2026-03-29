using System;
using System.Collections.Generic;
using Terraria.ID;

namespace DragonLens.Content.Tools.Multiplayer.Trackers
{
	internal class SessionTracker : ModSystem
	{
		internal static readonly Dictionary<int, DateTime> Sessions = [];

		public override void OnWorldLoad()
		{
			Sessions.Clear();
		}

		public override void OnWorldUnload()
		{
			Sessions.Clear();
		}

		public override void PostUpdatePlayers()
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Player player = Main.LocalPlayer;

				if (player.active && !Sessions.ContainsKey(player.whoAmI))
					Sessions[player.whoAmI] = DateTime.UtcNow;

				if (!player.active)
					Sessions.Clear();

				return;
			}

			if (Main.netMode != NetmodeID.Server)
				return;

			bool changed = false;
			DateTime now = DateTime.UtcNow;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				bool tracked = Sessions.ContainsKey(i);

				if (player.active)
				{
					if (!tracked)
					{
						Sessions[i] = now;
						changed = true;
					}
				}
				else if (tracked)
				{
					Sessions.Remove(i);
					changed = true;
				}
			}

			if (changed)
				SessionTrackerNetHandler.SendFullSync();
		}

		public static string GetSessionDuration(int playerIndex)
		{
			if (!Sessions.TryGetValue(playerIndex, out DateTime start))
				return "-";

			TimeSpan span = DateTime.UtcNow - start;

			if (span.TotalHours >= 1)
				return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";

			return $"{(int)span.TotalMinutes:D2}:{span.Seconds:D2}";
		}

		public static long GetSessionDurationTicks(int playerIndex)
		{
			if (!Sessions.TryGetValue(playerIndex, out DateTime start))
				return 0;

			return (DateTime.UtcNow - start).Ticks;
		}
	}
}