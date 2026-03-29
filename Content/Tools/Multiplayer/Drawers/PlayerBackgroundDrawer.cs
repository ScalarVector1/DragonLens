using DragonLens.Core.Systems;
using ReLogic.Graphics;
using System;
using Terraria.GameContent;
using Biomes = Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes; // keep this, it's important for filter setup

namespace DragonLens.Content.Tools.Multiplayer.Drawers
{
	internal class PlayerBackgroundDrawer
	{
		public static void DrawMapFullscreenBackground(SpriteBatch sb, Rectangle rect, Player player, bool listMode)
		{
			var biome = BiomeHelper.GetBiomeVisual(player);

			Texture2D tex;
			Color color = biome.BackgroundColor;

			if (biome.BestiaryBiome == BiomeHelper.ShimmerBiome)
			{
				tex = Assets.GUI.ShimmerBackground.Value;
				color = new Color(180, 140, 255);
			}
			else
			{
				Asset<Texture2D> asset = biome.BestiaryBiome.GetBackgroundImage();

				if (Main.bloodMoon && biome.BestiaryBiome == Biomes.Surface)
					asset = TextureAssets.MapBGs[24];

				if (asset?.Value == null)
					return;

				tex = asset.Value;
			}

			int padding = 4;
			rect.X += padding;
			rect.Y += padding;
			rect.Width -= padding * 2;
			rect.Height -= padding * 2;

			if (listMode)
			{
				// If you want perfect aspect ratio, keep these 3 lines.
				//int drawWidth = (int)(rect.Height * (tex.Width / (float)tex.Height));
				//Rectangle drawRect = new(rect.X, rect.Y, drawWidth, rect.Height);
				//DrawHorizontalFade(sb, tex, drawRect, tex.Bounds, color, 0.5f, 1);

				// If you want stretched out, keep this line.
				DrawHorizontalFade(sb, tex, rect, tex.Bounds, color, 0.5f, 1);
				return;
			}

			float sourceWidth = tex.Height == 0 ? tex.Width : rect.Width * (tex.Height / (float)rect.Height);
			int croppedWidth = Math.Min(tex.Width, (int)Math.Round(sourceWidth));
			int sourceX = (tex.Width - croppedWidth) / 2;
			Rectangle source = new(sourceX, 0, croppedWidth, tex.Height);

			DrawHorizontalFade(sb, tex, rect, source, color, 1f, 1);
		}
		
		public static void DrawTeamColorBackground(SpriteBatch sb, Rectangle rect, Player player)
		{
			//if (player.team <= 0 || player.team >= Main.teamColor.Length)
				//return;

			// Slight inset so it matches your biome background padding style
			const int padding = 6;
			rect.Inflate(-padding, -padding);
			if (rect.Width <= 0 || rect.Height <= 0)
				return;

			Color teamColor = Main.teamColor[player.team];

			// Keep it subtle so stats remain readable
			teamColor *= 0.9f;

			// Either solid fill:
			 //sb.Draw(TextureAssets.MagicPixel.Value, rect, teamColor);

			// Or reuse your fade helper for a nicer “background” look:
			DrawHorizontalFade(sb,TextureAssets.MagicPixel.Value,rect,new Rectangle(0, 0, 1, 1),teamColor,
				solidPortion: 0.55f,
				sliceWidth: 1
			);
		}

		public static void DrawPlayerFull(SpriteBatch sb, Rectangle rect, Player player, bool listMode)
		{
			Player drawPlayer = CreateDrawPlayer(player);

			// Slight inset so it matches your biome background padding style
			const int padding = 4;
			const float nameScale = 1f;
			const float listPlayerX = 26f;
			const float nameOffsetY = 26f;
			const int minHeightForName = 64;

			// Debug draw to ensure its drawing something
			//sb.Draw(TextureAssets.MagicPixel.Value, rect, Color.Red * 0.35f);

			rect.Inflate(-padding, -padding);
			if (rect.Width <= 0 || rect.Height <= 0 || drawPlayer == null)
				return;

			// Restart spritebatch with point sampling to ensure player draws!
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
				DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

			Vector2 playerPos;
			float playerDrawX;
			float playerDrawY;
			if (listMode)
			{
				playerDrawX = rect.X + listPlayerX;
				playerDrawY = rect.Center.Y - drawPlayer.height * 0.5f;
				playerPos = new Vector2(playerDrawX, playerDrawY) + Main.screenPosition;
			}
			else
			{
				playerPos = rect.Center.ToVector2() - new Vector2(drawPlayer.width, drawPlayer.height) * 0.5f + Main.screenPosition;
				playerDrawX = playerPos.X - Main.screenPosition.X;
				playerDrawY = playerPos.Y - Main.screenPosition.Y;
			}

			// Force fullbright
			bool oldDisplay = drawPlayer.isDisplayDollOrInanimate;
			drawPlayer.isDisplayDollOrInanimate = true;
			ModifyPlayerDrawInfo.ForceFullBrightOnce = true;

			try
			{
				Main.PlayerRenderer.DrawPlayer(Main.Camera, drawPlayer, playerPos, 0f, Vector2.Zero, 0f, 1f);
			}
			finally
			{
				drawPlayer.isDisplayDollOrInanimate = oldDisplay;
				ModifyPlayerDrawInfo.ForceFullBrightOnce = false;
			}

			// Restart spritebatch with linear sampling for text and other UI elements to avoid blurriness.
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

			// Draw player name in list mode if there's enough vertical space for it.
			if (rect.Height >= minHeightForName)
			{
				DynamicSpriteFont font = FontAssets.MouseText.Value;
				float maxNameWidth = drawPlayer.width + 55f;
				string fittedName = PlayerStatDrawer.FitStatText(drawPlayer.name, maxNameWidth, nameScale);

				if (!string.IsNullOrEmpty(fittedName))
				{
					Vector2 textSize = font.MeasureString(fittedName) * nameScale;
					float playerCenterX = playerDrawX + drawPlayer.width * 0.5f;
					float textX = playerCenterX - textSize.X * 0.5f;
					float textY = playerDrawY - nameOffsetY;

					if (textY >= rect.Y)
					{
						Color nameColor = PermissionHandler.LooksLikeAdmin(player) ? new Color(100, 235, 235) : Color.White;
						if (player.team > 0)
							nameColor = Main.teamColor[player.team];

						Utils.DrawBorderString(sb, fittedName, new Vector2(textX, textY), nameColor, nameScale);
					}
				}
			}
		}

		private static Player CreateDrawPlayer(Player player)
		{
			Player drawPlayer = (Player)player.Clone();

			drawPlayer.position = player.position;
			drawPlayer.velocity = player.velocity;
			drawPlayer.direction = player.direction;
			drawPlayer.gravDir = player.gravDir;
			drawPlayer.fullRotation = player.fullRotation;
			drawPlayer.fullRotationOrigin = player.fullRotationOrigin;
			drawPlayer.selectedItem = player.selectedItem;
			drawPlayer.itemAnimation = player.itemAnimation;
			drawPlayer.itemAnimationMax = player.itemAnimationMax;
			drawPlayer.itemRotation = player.itemRotation;
			drawPlayer.heldProj = player.heldProj;
			drawPlayer.isDisplayDollOrInanimate = true;

			return drawPlayer;
		}

		#region Draw helpers
		/// <summary>
		/// Used to draw an image that fades opacity slowly to 0 from left to right.
		/// </summary>
		internal static void DrawHorizontalFade(SpriteBatch sb, Texture2D texture, Rectangle target, Rectangle source, Color color, float solidPortion = 0.5f, int sliceWidth = 1)
		{
			if (texture == null || target.Width <= 0 || target.Height <= 0 || source.Width <= 0 || source.Height <= 0)
				return;

			solidPortion = MathHelper.Clamp(solidPortion, 0f, 1f);
			sliceWidth = Math.Max(1, sliceWidth);

			int solidWidth = (int)(target.Width * solidPortion);
			int fadeWidth = Math.Max(1, target.Width - solidWidth);

			for (int x = 0; x < target.Width; x += sliceWidth)
			{
				int currentSliceWidth = Math.Min(sliceWidth, target.Width - x);

				Rectangle dest = new(target.X + x, target.Y, currentSliceWidth, target.Height);

				int srcX = source.X + (int)(x / (float)target.Width * source.Width);
				int srcWidth = Math.Max(1, (int)(currentSliceWidth / (float)target.Width * source.Width));
				if (srcX + srcWidth > source.Right)
					srcWidth = source.Right - srcX;

				Rectangle src = new(srcX, source.Y, srcWidth, source.Height);

				float alpha = 1f;
				if (x >= solidWidth)
				{
					float fadeT = (x - solidWidth) / (float)fadeWidth;
					alpha = 1f - MathHelper.Clamp(fadeT, 0f, 1f);
				}

				sb.Draw(texture, dest, src, color * alpha);
			}
		}

		#endregion
	}
}
