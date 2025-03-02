﻿using DragonLens.Content.Tools.Gameplay;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Map
{
	internal class MapTeleport : Tool
	{
		public static bool active = true;

		public override string IconKey => "MapTeleport";

		public override void OnActivate()
		{
			active = !active;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

				Texture2D tex = Assets.Misc.GlowAlpha.Value;
				var color = new Color(255, 100, 200)
				{
					A = 0
				};
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["active"] = active;
		}

		public override void LoadData(TagCompound tag)
		{
			active = tag.GetBool("active");
		}

		public override void SendPacket(BinaryWriter writer)
		{
			writer.WriteVector2(MapTeleportSystem.lastTarget);
			writer.Write(MapTeleportSystem.whoTeleported);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			Vector2 target = reader.ReadVector2();
			int who = reader.ReadInt32();

			if (who != -1)
			{
				Main.player[who].Center = target;

				if (Main.netMode == NetmodeID.Server)
				{
					MapTeleportSystem.lastTarget = target;
					MapTeleportSystem.whoTeleported = who;

					NetSend(-1, sender);
				}
			}
		}
	}

	internal class MapTeleportSystem : ModSystem
	{
		public static Vector2 lastTarget; //These are here for MP sync purposes
		public static int whoTeleported = -1;

		public override void Load()
		{
			Main.OnPostFullscreenMapDraw += TeleportFromMap;
		}

		public override void Unload()
		{
			Main.OnPostFullscreenMapDraw -= TeleportFromMap;
		}

		public override void OnWorldLoad()
		{
			whoTeleported = -1;
		}

		private void TeleportFromMap(Vector2 arg1, float arg2)
		{
			if (MapTeleport.active && PermissionHandler.CanUseTools(Main.LocalPlayer) && Main.mouseRight)
			{
				Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight) * Main.UIScale;
				Vector2 target = ((Main.MouseScreen - screenSize / 2) / 16 * (16 / Main.mapFullscreenScale) + Main.mapFullscreenPos) * 16;

				if (WorldGen.InWorld((int)target.X / 16, (int)target.Y / 16))
				{
					Main.LocalPlayer.Center = target;

					if (Main.LocalPlayer.GetModPlayer<NoClipPlayer>().active)
						Main.LocalPlayer.GetModPlayer<NoClipPlayer>().desiredPos = target;

					lastTarget = target;
					whoTeleported = Main.LocalPlayer.whoAmI;

					Main.LocalPlayer.fallStart = (int)Main.LocalPlayer.position.Y;
				}
				else
				{
					Main.NewText(LocalizationHelper.GetToolText("MapTeleport.OutsideWorldWarning"));
				}
			}
		}
	}
}