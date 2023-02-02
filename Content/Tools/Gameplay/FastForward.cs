using DragonLens.Configs;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class FastForward : Tool
	{
		public static int speedup = 0;

		public override string Texture => "DragonLens/Assets/Tools/FastForward";

		public override string DisplayName => "Fast forward";

		public override string Description => "The game moves at increasingly fast speeds!";

		public override void OnActivate()
		{
			if (speedup < 4)
				speedup++;
			else
				speedup = 0;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
		{
			base.DrawIcon(spriteBatch, position);

			if (speedup > 0)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle((int)position.X - 7, (int)position.Y - 7, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				Color color = new Color(200, 255, 220) * (speedup / 4f);
				color.A = 0;
				var target = new Rectangle((int)position.X, (int)position.Y, 32, 32);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class FastForwardSystem : ModSystem
	{
		public override void Load()
		{
			On.Terraria.Main.DoUpdate += UpdateExtraTimes;
		}

		private void UpdateExtraTimes(On.Terraria.Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
		{
			orig(self, ref gameTime);

			for (int k = 0; k < FastForward.speedup; k++)
			{
				orig(self, ref gameTime);
			}
		}
	}
}
