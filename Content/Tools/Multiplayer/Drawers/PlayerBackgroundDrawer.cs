using System;
using Terraria.GameContent;

namespace DragonLens.Content.Tools.Multiplayer.Drawers
{
	internal class PlayerBackgroundDrawer
	{
		public static Asset<Texture2D> GetBackground(Player player)
		{
			if (ModContent.RequestIfExists(player.CurrentSceneEffect.mapBackground, out Asset<Texture2D> modTexture, AssetRequestMode.ImmediateLoad))
			{
				ModSceneEffect modSceneEffect = player.CurrentSceneEffect.mapBackgroundSceneEffect;
				Color modMapColor = player.Center.Y <= Main.worldSurface * 16.0 ? Main.ColorOfTheSkies : Color.White;

				if (modSceneEffect?.MapBackgroundFullbright == true)
					modMapColor = Color.White;

				modSceneEffect?.MapBackgroundColor(ref modMapColor);

				return modTexture;
			}

			Asset<Texture2D> asset = TextureAssets.MapBGs[0];
			int num = -1;

			int wall = Main.tile[(int)(player.Center.X / 16f), (int)(player.Center.Y / 16f)].wall;
			if (Main.screenPosition.Y > (Main.maxTilesY - 232) * 16)
			{
				num = 2;
			}
			else if (player.ZoneDungeon)
			{
				num = 4;
			}
			else if (wall == 87)
			{
				num = 13;
			}
			else if (Main.screenPosition.Y > Main.worldSurface * 16.0)
			{
				num = wall switch
				{
					86 or 108 => 15,
					180 or 184 => 16,
					178 or 183 => 17,
					62 or 263 => 18,
					_ => (!player.ZoneGlowshroom) ? ((!player.ZoneCorrupt) ? ((!player.ZoneCrimson) ? ((!player.ZoneHallow) ? ((!player.ZoneSnow) ? ((!player.ZoneJungle) ? ((!player.ZoneDesert) ? ((!player.ZoneRockLayerHeight) ? 1 : 31) : 14) : 12) : 3) : ((!player.ZoneDesert) ? ((!player.ZoneSnow) ? 21 : 35) : 41)) : ((!player.ZoneDesert) ? ((!player.ZoneSnow) ? 23 : 34) : 40)) : ((!player.ZoneDesert) ? ((!player.ZoneSnow) ? 22 : 33) : 39)) : 20,
				};
			}
			else if (player.ZoneGlowshroom)
			{
				num = 19;
			}
			else
			{
				int num2 = (int)((Main.screenPosition.X + Main.screenWidth / 2) / 16f);
				if (player.ZoneSkyHeight)
				{
					num = 32;
				}
				else if (player.ZoneCorrupt)
				{
					num = (!player.ZoneDesert) ? 5 : 36;
				}
				else if (player.ZoneCrimson)
				{
					num = (!player.ZoneDesert) ? 6 : 37;
				}
				else if (player.ZoneHallow)
				{
					num = (!player.ZoneDesert) ? 7 : 38;
				}
				else if ((double)(Main.screenPosition.Y / 16f) < Main.worldSurface + 10.0 && (num2 < 380 || num2 > Main.maxTilesX - 380))
				{
					num = 10;
				}
				else if (player.ZoneSnow)
				{
					num = 11;
				}
				else if (player.ZoneJungle)
				{
					num = 8;
				}
				else if (player.ZoneDesert)
				{
					num = 9;
				}
				else if (Main.bloodMoon)
				{
					num = 25;
				}
				else if (player.ZoneGraveyard)
				{
					num = 26;
				}
			}

			if (num > -1)
				asset = TextureAssets.MapBGs[num];

			return asset;
		}

		public static void DrawPlayerFull(SpriteBatch sb, Rectangle rect, Player player)
		{
			sb.End();
			sb.Begin(default, default, SamplerState.PointClamp, default, Main.Rasterizer, null, Main.UIScaleMatrix);

			Main.Rasterizer.ScissorTestEnable = true;
			Rectangle oldRect = Main.graphics.graphicsDevice.ScissorRectangle;

			Matrix matrix = Main.UIScaleMatrix;

			var topLeft = Vector2.Transform(new Vector2(rect.Left, rect.Top), matrix);
			var topRight = Vector2.Transform(new Vector2(rect.Right, rect.Top), matrix);
			var bottomLeft = Vector2.Transform(new Vector2(rect.Left, rect.Bottom), matrix);
			var bottomRight = Vector2.Transform(new Vector2(rect.Right, rect.Bottom), matrix);

			float minX = MathF.Min(topLeft.X, MathF.Min(topRight.X, MathF.Min(bottomLeft.X, bottomRight.X)));
			float minY = MathF.Min(topLeft.Y, MathF.Min(topRight.Y, MathF.Min(bottomLeft.Y, bottomRight.Y)));
			float maxX = MathF.Max(topLeft.X, MathF.Max(topRight.X, MathF.Max(bottomLeft.X, bottomRight.X)));
			float maxY = MathF.Max(topLeft.Y, MathF.Max(topRight.Y, MathF.Max(bottomLeft.Y, bottomRight.Y)));

			var scissor = new Rectangle(
				(int)MathF.Floor(minX),
				(int)MathF.Floor(minY),
				(int)MathF.Ceiling(maxX - minX),
				(int)MathF.Ceiling(maxY - minY)
			);

			Main.graphics.graphicsDevice.ScissorRectangle = scissor;

			float playerDrawX = rect.Center.X - player.width / 2;
			float playerDrawY = rect.Center.Y - player.height / 2;
			var basePos = new Vector2(playerDrawX, playerDrawY);

			Vector2 playerPos = basePos + Main.screenPosition;

			bool oldDisplay = player.isDisplayDollOrInanimate;
			player.isDisplayDollOrInanimate = true;
			ModifyPlayerDrawInfo.ForceFullBrightOnce = true;

			Main.PlayerRenderer.DrawPlayer(null, player, playerPos, 0f, Vector2.Zero, 0f, 1);

			player.isDisplayDollOrInanimate = oldDisplay;
			ModifyPlayerDrawInfo.ForceFullBrightOnce = false;

			sb.End();
			sb.Begin(default, default, SamplerState.LinearClamp, default, Main.Rasterizer, null, Main.UIScaleMatrix);

			Main.graphics.graphicsDevice.ScissorRectangle = oldRect;
		}
	}
}