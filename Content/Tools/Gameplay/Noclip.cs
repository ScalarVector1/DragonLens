using DragonLens.Content.Tools.Visualization;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using Terraria.ID;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class NoClip : Tool
	{
		public static int syncingPlayer;

		public bool Active
		{
			get => Main.LocalPlayer.GetModPlayer<NoClipPlayer>().active;

			set
			{
				Main.LocalPlayer.GetModPlayer<NoClipPlayer>().active = value;
				syncingPlayer = Main.LocalPlayer.whoAmI;
				NetSend();
			}
		}

		public override string IconKey => "Noclip";

		public override bool IsHighlighted => Active;

		public override void OnActivate()
		{
			Active = !Active;

			if (Active)
				Main.LocalPlayer.GetModPlayer<NoClipPlayer>().desiredPos = Main.LocalPlayer.Center;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);
		}

		public override void SendPacket(BinaryWriter writer)
		{
			if (Main.netMode != NetmodeID.Server)
				NoClip.syncingPlayer = Main.LocalPlayer.whoAmI;

			NoClipPlayer mp = Main.player[syncingPlayer].GetModPlayer<NoClipPlayer>();
			writer.Write(syncingPlayer);
			writer.Write(mp.desiredPos.X);
			writer.Write(mp.desiredPos.Y);
			writer.Write(mp.active);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			syncingPlayer = reader.ReadInt32();
			NoClipPlayer mp = Main.player[syncingPlayer].GetModPlayer<NoClipPlayer>();
			mp.desiredPos.X = reader.ReadSingle();
			mp.desiredPos.Y = reader.ReadSingle();
			mp.active = reader.ReadBoolean();

			if (Main.netMode == NetmodeID.Server)
			{
				NetSend(-1, sender);
			}
		}

		public override void ResetForNonAdmin(Player player)
		{
			Active = false;
		}
	}

	internal class NoClipPlayer : ModPlayer
	{
		public Vector2 desiredPos;
		public bool active;

		public override void PostUpdate()
		{
			if (active && Main.LocalPlayer == Player && !PermissionHandler.CanUseTools(Player))
			{
				active = false;
				ModContent.GetInstance<NoClip>().NetSend(-1, -1);
			}

			if (active)
			{
				// Check free cam to stop movement while freecam is active
				if (!FreeCamera.active)
				{
					// Custom speeds with shift / ctrl keys:
					// Ctrl+shift -> 120f
					// Ctrl -> 7.5f
					// Shift -> 60f
					// (normal) -> 15f

					bool shift = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
					bool ctrl = Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl);

					float speed = ctrl && shift ? 120f : ctrl ? 7.5f : shift ? 60f : 15f;

					if (Player.controlLeft)
						desiredPos.X -= speed;
					if (Player.controlRight)
						desiredPos.X += speed;
					if (Player.controlUp)
						desiredPos.Y -= speed;
					if (Player.controlDown)
						desiredPos.Y += speed;
				}

				if (Main.netMode == NetmodeID.MultiplayerClient &&
					Main.LocalPlayer == Player &&
					Player.Center != desiredPos + Vector2.UnitY * 0.4f &&  // account for gravity
					Main.GameUpdateCount % 20 == 0) // Bootleg throttle for this to prevent it from spamming
				{
					NoClip.syncingPlayer = Main.LocalPlayer.whoAmI;
					ModContent.GetInstance<NoClip>().NetSend(-1, -1);
				}

				Player.Center = desiredPos;
				Player.velocity *= 0;
				Player.gfxOffY = 0;
			}
		}
	}
}