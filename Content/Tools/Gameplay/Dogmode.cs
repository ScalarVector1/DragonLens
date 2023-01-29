using DragonLens.Configs;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Dogmode : Tool
	{
		public static bool active = false;

		public override string Texture => "DragonLens/Assets/Tools/DogMode";

		public override string Name => "Dog mode";

		public override string Description => "You can be hit, but cannot die while active. Mutually exclusive with God mode.";

		public override void OnActivate()
		{
			active = !active;

			if (active)
				Godmode.active = false;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle((int)position.X - 7, (int)position.Y - 7, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				Color color = Color.White;
				color.A = 0;
				var target = new Rectangle((int)position.X, (int)position.Y, 32, 32);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class DogModePlayer : ModPlayer
	{
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (Dogmode.active)
			{
				playSound = false;
				genGore = false;

				Main.NewText("Dogmode prevented death with reason: " + damageSource.GetDeathText(Player.name));
				Player.statLife = Player.statLifeMax2;
				return false;
			}

			return true;
		}
	}
}
