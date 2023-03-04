using DragonLens.Configs;
using DragonLens.Content.Tools.Gameplay;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Map
{
	internal class MapTeleport : Tool
	{
		public static bool active = true;

		public override string IconKey => "MapTeleport";

		public override string DisplayName => "Map teleportation";

		public override string Description => "Toggles the ability to teleport on the map. Right click anywhere on the map to teleport.";

		public override void OnActivate()
		{
			active = !active;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle((int)position.X - 7, (int)position.Y - 7, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				var color = new Color(255, 100, 200)
				{
					A = 0
				};
				var target = new Rectangle((int)position.X, (int)position.Y, 32, 32);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class MapTeleportSystem : ModSystem
	{
		public override void Load()
		{
			Main.OnPostFullscreenMapDraw += TeleportFromMap;
		}

		public override void Unload()
		{
			Main.OnPostFullscreenMapDraw -= TeleportFromMap;
		}

		private void TeleportFromMap(Vector2 arg1, float arg2)
		{
			if (MapTeleport.active && Main.mouseRight)
			{
				Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight) * Main.UIScale;
				Vector2 target = ((Main.MouseScreen - screenSize / 2) / 16 * (16 / Main.mapFullscreenScale) + Main.mapFullscreenPos) * 16;

				if (WorldGen.InWorld((int)target.X / 16, (int)target.Y / 16))
				{
					Main.LocalPlayer.Center = target;

					if (NoClip.active)
						NoClip.desiredPos = target;

					Main.LocalPlayer.fallStart = (int)Main.LocalPlayer.position.Y;
				}
				else
				{
					Main.NewText("You cant teleport outside of the world!");
				}
			}
		}
	}
}
