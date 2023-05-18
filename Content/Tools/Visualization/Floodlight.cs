using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Visualization
{
	internal class Floodlight : Tool
	{
		public static float strength = 0f;

		public override string IconKey => "Lighting";

		public override bool HasRightClick => true;

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

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			if (strength > 0)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				Color color = new Color(255, 215, 150) * strength;
				color.A = 0;
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["strength"] = strength;
		}

		public override void LoadData(TagCompound tag)
		{
			strength = tag.GetFloat("strength");
		}
	}

	internal class LightingSystem : ModSystem
	{

		public override void Load()
		{
			Terraria.Graphics.Light.On_TileLightScanner.GetTileLight += HackLight;
		}

		private void HackLight(Terraria.Graphics.Light.On_TileLightScanner.orig_GetTileLight orig, Terraria.Graphics.Light.TileLightScanner self, int x, int y, out Vector3 outputColor)
		{
			orig(self, x, y, out outputColor);

			if (Floodlight.strength > 0)
				outputColor += Vector3.One * Floodlight.strength;
		}
	}
}