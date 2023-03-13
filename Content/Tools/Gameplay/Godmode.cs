using DragonLens.Configs;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Godmode : Tool
	{
		public static bool godMode = false;

		public static bool dogMode = false;

		public override string IconKey => "GodMode";

		public override string DisplayName => "God mode";

		public override string Description => "You cannot be hit, lose mana, or die while active. Right click to allow being hit but disallow dying instead";

		public override bool HasRightClick => true;

		public override string RightClickName => "Dogmode (Godmode + hits allowed)";

		public override void OnActivate()
		{
			godMode = !godMode;

			if (godMode)
				dogMode = false;
		}

		public override void OnRightClick()
		{
			dogMode = !dogMode;

			if (dogMode)
				godMode = false;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			if (godMode)
			{
				base.DrawIcon(spriteBatch, position);

				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				var color = new Color(255, 220, 100)
				{
					A = 0
				};
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
			else if (dogMode)
			{
				Texture2D icon = ThemeHandler.GetIcon("DogMode");

				float scale = 1;

				if (icon.Width > position.Width || icon.Height > position.Height)
					scale = icon.Width > icon.Height ? position.Width / icon.Width : position.Height / icon.Height;

				spriteBatch.Draw(icon, position.Center(), null, Color.White, 0, icon.Size() / 2f, scale, 0, 0);

				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				Color color = Color.White;
				color.A = 0;

				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
			else
			{
				base.DrawIcon(spriteBatch, position);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["godMode"] = godMode;
			tag["dogMode"] = dogMode;
		}

		public override void LoadData(TagCompound tag)
		{
			godMode = tag.GetBool("godMode");
			dogMode = tag.GetBool("dogMode");
		}
	}

	internal class GodModePlayer : ModPlayer
	{
		public override void PostUpdate()
		{
			if (Godmode.godMode)
			{
				Player.statLife = Player.statLifeMax2;
				Player.statMana = Player.statManaMax2;
			}
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
		{
			if (Godmode.godMode)
				return false;

			return true;
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (Godmode.godMode || Godmode.dogMode)
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
