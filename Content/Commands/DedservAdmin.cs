using DragonLens.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			if (input.Length < Command.Length + 1)
			{
				throw new UsageException("You must provide a players name!");
			}

			string name = input[(Command.Length + 1)..];

			Player player = Main.player.FirstOrDefault(n => n.name.ToLower() == name);

			if (player != null)
			{
				if (PermissionHandler.CanUseTools(player))
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"The server made {name} not an admin!"), Color.Orange);
					PermissionHandler.RemoveAdmin(player);
				}
				else
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"The server made {name} an admin!"), Color.Yellow);
					PermissionHandler.AddAdmin(player);
				}
			}
			else
			{
				throw new UsageException($"A player by the name {name} was not found.");
			}
		}
	}
}