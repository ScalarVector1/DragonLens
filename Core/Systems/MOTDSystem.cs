using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems
{
	/// <summary>
	/// Displays a welcome message to the user
	/// </summary>
	internal class MOTDPlayer : ModPlayer
	{
		public override void OnEnterWorld(Player player)
		{
			string MOTD = $"Thank you for using DragonLens V.{Mod.Version}!\n\n" +
				$"Be sure to check the customize tool (wrench icon) to set up the mod with the tools you need.\n\n" +
				$"Additional documentation can be found at https://github.com/ScalarVector1/DragonLens/wiki";

			Main.NewText(MOTD, new Color(200, 235, 255));
		}
	}
}
