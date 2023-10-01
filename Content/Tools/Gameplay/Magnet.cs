using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Magnet : Tool
	{
		public static bool active = false;

		public static bool voidActive = false;

		public override string IconKey => "Magnet";

		public override bool HasRightClick => true;

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
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

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

		public override void SendPacket(BinaryWriter writer)
		{
			writer.Write(active);
			writer.Write(voidActive);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			active = reader.ReadBoolean();
			voidActive = reader.ReadBoolean();

			if (Main.netMode == NetmodeID.Server)
				NetSend(-1, sender);
		}
	}

	internal class MagnetPlayer : ModPlayer
	{
		public override void PostUpdate()
		{
			if (Magnet.active && PermissionHandler.CanUseTools(Player))
			{
				foreach (Item item in Main.item)
				{
					item.Center = Player.Center;
				}
			}
			else if (Magnet.voidActive && PermissionHandler.CanUseTools(Player))
			{
				foreach (Item item in Main.item)
				{
					item.active = false;
				}
			}
		}
	}
}