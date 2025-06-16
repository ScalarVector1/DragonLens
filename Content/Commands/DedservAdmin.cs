using System;
using DragonLens.Core.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;

namespace DragonLens.Content.Commands
{
	internal class DedservAdmin : ModCommand
	{
		public override string Command => "dladmin";
		public override string Description => "Toggles the admin status of a given user";
		public override string Usage => "dladmin <player>";
		public override CommandType Type => CommandType.Console;

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length < 1)
				throw new UsageException("You must provide a player's name!");

			string name = args[0];
			Player player = Array.Find(Main.player, p =>
				string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase)
			);

			if (player == null)
				throw new UsageException($"A player by the name '{name}' was not found.");

			if (PermissionHandler.CanUseTools(player))
			{
				PermissionHandler.RemoveAdmin(player);
				ChatHelper.BroadcastChatMessage(
					NetworkText.FromLiteral($"The server made {player.name} not an admin!"),
					Color.Orange
				);
			}
			else
			{
				PermissionHandler.AddAdmin(player);
				ChatHelper.BroadcastChatMessage(
					NetworkText.FromLiteral($"The server made {player.name} an admin!"),
					Color.Yellow
				);
			}
		}
	}
}