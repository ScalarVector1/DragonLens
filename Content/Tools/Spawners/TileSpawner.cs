using DragonLens.Content.Filters;
using DragonLens.Content.Filters.TileFilters;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.ObjectData;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class TileSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/TileSpawner";

		public override string DisplayName => "Tile spawner";

		public override string Description => "Place tiles without items!";

		public override void OnActivate()
		{
			TileBrowser state = UILoader.GetUIState<TileBrowser>();
			state.visible = !state.visible;

			if (!state.initialized)
			{
				UILoader.GetUIState<TileBrowser>().Refresh();
				state.initialized = true;
			}
		}
	}

	internal class TileBrowser : Browser
	{
		public static int selected = -1;

		public static int variant = 0;

		public override string Name => "Tile spawner";

		public override string IconTexture => "DragonLens/Assets/Tools/TileSpawner";

		public override Vector2 DefaultPosition => new(0.5f, 0.3f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<TileButton>();
			// Unlike a lot of content, `0` actually corresponds to TileID.Dirt,
			// so that works out fine.
			for (int k = TileID.Dirt; k < TileLoader.TileCount; k++)
			{
				buttons.Add(new TileButton(k, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Mod filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Vanilla", "Vanilla", "Tiles from the base game", n => !(n is TileButton && (n as TileButton).tileType <= TileID.Count)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModTile>().Count() > 0))
			{
				filters.AddFilter(new TileModFilter(mod));
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			if (Main.mouseLeft && selected != -1)
			{
				if (Main.tileFrameImportant[selected])
				{
					TileObject.CanPlace((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16, selected, variant, 0, out TileObject to);
					TileObject.Place(to);
				}
				else
				{
					Tile tilePtr = Framing.GetTileSafely((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
					tilePtr.HasTile = true;
					tilePtr.TileType = (ushort)selected;
					tilePtr.Slope = 0;

					WorldGen.SquareTileFrame((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
				}
			}

			if (selected != -1)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			if (selected == -1)
				return;

			int maxVariants = 1;

			var tod = TileObjectData.GetTileData(selected, 0, 0);
			Texture2D tex = Terraria.GameContent.TextureAssets.Tile[selected].Value;

			if (tod != null)
			{
				if (tod.StyleHorizontal)
					maxVariants = tex.Width / tod.CoordinateFullWidth - 1;
				else
					maxVariants = tex.Height / tod.CoordinateFullHeight - 1;
			}

			if (evt.ScrollWheelValue < 1)
			{
				if (variant < maxVariants)
					variant++;
				else
					variant = 0;
			}
			else
			{
				if (variant > 0)
					variant--;
				else
					variant = maxVariants;
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (selected != -1)
			{
				selected = -1;
				variant = 0;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (selected != -1)
			{
				Main.instance.LoadTiles(selected);
				Texture2D tex = Terraria.GameContent.TextureAssets.Tile[selected].Value;

				Vector2 pos = new((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
				int width = 16;
				int height = 16;

				var tod = TileObjectData.GetTileData(selected, 0, 0);

				if (tod != null)
				{
					pos -= new Vector2(tod.Origin.X, tod.Origin.Y);
					width = tod.Width * 16;
					height = tod.Height * 16;
				}

				var sourcePos = new Vector2(0, 0);

				if (tod != null && Main.tileFrameImportant[selected])
				{
					if (tod.StyleHorizontal)
						sourcePos.X = tod.CoordinateFullWidth * variant;
					else
						sourcePos.Y = tod.CoordinateFullHeight * variant;
				}

				spriteBatch.Draw(tex, Main.MouseScreen + Vector2.One * 32, new Rectangle((int)sourcePos.X, (int)sourcePos.Y, 16, 16), Color.White, 0, Vector2.One * 8, 1, 0, 0);

				var point = (pos * 16 - Main.screenPosition).ToPoint16();
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(point.X, point.Y, width, height), Color.White);
			}

			base.Draw(spriteBatch);
		}
	}

	internal class TileButton : BrowserButton
	{
		public int tileType;

		public override string Identifier => ProcessName(TileID.Search.GetName(tileType));

		public TileButton(int tileType, Browser browser) : base(browser)
		{
			this.tileType = tileType;
		}

		private string ProcessName(string input)
		{
			input = Regex.Replace(input, "(.*)/(.*)", "$2");
			input = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
			return input;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Main.instance.LoadTiles(tileType);
			Texture2D tex = Terraria.GameContent.TextureAssets.Tile[tileType].Value;

			var sourcePos = new Vector2(0, 0);

			var tod = TileObjectData.GetTileData(tileType, 0, 0);

			if (TileBrowser.selected == tileType && tod != null && Main.tileFrameImportant[tileType])
			{
				if (tod.StyleHorizontal)
					sourcePos.X = tod.CoordinateFullWidth * TileBrowser.variant;
				else
					sourcePos.Y = tod.CoordinateFullHeight * TileBrowser.variant;
			}

			spriteBatch.Draw(tex, iconBox.Center(), new Rectangle((int)sourcePos.X, (int)sourcePos.Y, 16, 16), Color.White, 0, Vector2.One * 8, 1, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip($"Type: {tileType}");
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			TileBrowser.selected = tileType;
			TileBrowser.variant = 0;
			Main.NewText($"{Identifier} selected, click anywhere in the world to place. Right click to deselect.");
		}

		public override void RightClick(UIMouseEvent evt)
		{

		}

		public override int CompareTo(object obj)
		{
			return tileType - (obj as TileButton).tileType;
		}
	}
}
