using DragonLens.Configs;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Visualization
{
	internal class Floodlight : Tool
	{
		public static float strength = 0f;

		public override string Texture => "DragonLens/Assets/Tools/Lighting";

		public override string DisplayName => "Mass Illumination";

		public override string Description => "Light up the entire screen!";

		public override bool HasRightClick => true;

		public override string RightClickName => "Decrease Mass Illumination";

		public override void OnActivate()
		{
			if (strength < 1)
				strength += 0.25f;
			else
				strength = 0;
		}

		public override void OnRightClick()
		{
			if (strength > 0)
				strength -= 0.25f;
			else
				strength = 1;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
		{
			base.DrawIcon(spriteBatch, position);

			if (strength > 0)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle((int)position.X - 7, (int)position.Y - 7, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				Color color = new Color(255, 215, 150) * strength;
				color.A = 0;
				var target = new Rectangle((int)position.X, (int)position.Y, 32, 32);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class LightingSystem : ModSystem
	{

		public override void Load()
		{
			On.Terraria.Graphics.Light.TileLightScanner.GetTileLight += HackLight;
		}

		private void HackLight(On.Terraria.Graphics.Light.TileLightScanner.orig_GetTileLight orig, Terraria.Graphics.Light.TileLightScanner self, int x, int y, out Vector3 outputColor)
		{
			orig(self, x, y, out outputColor);

			if (Floodlight.strength > 0)
				outputColor += Vector3.One * Floodlight.strength;
		}
	}
}
