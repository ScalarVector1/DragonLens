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
	internal class Godmode : Tool
	{
		public static bool active = false;

		public override string Texture => "DragonLens/Assets/Tools/GodMode";

		public override string Name => "God mode";

		public override string Description => "You cannot be hit or die while active. Mutually exclusive with Dog mode";

		public override void OnActivate()
		{
			active = !active;

			if (active)
				Dogmode.active = false;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle((int)position.X - 7, (int)position.Y - 7, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				var color = new Color(255, 220, 100)
				{
					A = 0
				};
				var target = new Rectangle((int)position.X, (int)position.Y, 32, 32);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class GodModePlayer : ModPlayer
	{
		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
		{
			if (Godmode.active)
				return false;

			return true;
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (Godmode.active)
			{
				playSound = false;
				genGore = false;

				Main.NewText("Godmode prevented death with reason: " + damageSource.GetDeathText(Player.name));
				Player.statLife = Player.statLifeMax2;
				return false;
			}

			return true;
		}
	}
}
