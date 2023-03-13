using DragonLens.Configs;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Magnet : Tool
	{
		public static bool active = false;

		public static bool voidActive = false;

		public override string IconKey => "Magnet";

		public override string DisplayName => "Item magnet";

		public override string Description => "Toggle to pull all items in the world to you. Right click to enable the void magnet, which will automatically destroy items instead.";

		public override bool HasRightClick => true;

		public override string RightClickName => "Void magnet (destroys items on pickup)";

		public override void OnActivate()
		{
			active = !active;

			if (active)
				voidActive = false;
		}

		public override void OnRightClick()
		{
			voidActive = !voidActive;

			if (voidActive)
				active = false;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			if (voidActive)
			{
				Texture2D icon = ThemeHandler.GetIcon("VoidMagnet");

				float scale = 1;

				if (icon.Width > position.Width || icon.Height > position.Height)
					scale = icon.Width > icon.Height ? position.Width / icon.Width : position.Height / icon.Height;

				spriteBatch.Draw(icon, position.Center(), null, Color.White, 0, icon.Size() / 2f, scale, 0, 0);
			}
			else
			{
				base.DrawIcon(spriteBatch, position);
			}

			if (active || voidActive)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				Color color = voidActive ? Color.Purple : Color.White;
				color.A = 0;
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["active"] = active;
			tag["voidActive"] = voidActive;
		}

		public override void LoadData(TagCompound tag)
		{
			active = tag.GetBool("active");
			voidActive = tag.GetBool("voidActive");
		}
	}

	internal class MagnetPlayer : ModPlayer
	{
		public override void PostUpdate()
		{
			if (Magnet.active)
			{
				foreach (Item item in Main.item)
				{
					item.Center = Player.Center;
				}
			}
			else if (Magnet.voidActive)
			{
				foreach (Item item in Main.item)
				{
					item.active = false;
				}
			}
		}
	}
}
