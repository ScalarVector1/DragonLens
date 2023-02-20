using DragonLens.Content.Tools.Gameplay;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems
{
	internal class MapTeleportSystem : ModSystem
	{
		public override void Load()
		{
			Main.OnPostFullscreenMapDraw += MapTeleport;
		}

		private void MapTeleport(Vector2 arg1, float arg2)
		{
			if (Main.mouseRight)
			{
				var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
				Vector2 target = ((Main.MouseScreen - screenSize / 2) / 16 * (16 / Main.mapFullscreenScale) + Main.mapFullscreenPos) * 16;

				Main.LocalPlayer.Center = target;

				if (NoClip.active)
					NoClip.desiredPos = target;
			}
		}
	}
}
