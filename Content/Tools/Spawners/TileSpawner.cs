using DragonLens.Content.Filters;
using DragonLens.Content.Filters.TileFilters;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class TileSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/TileSpawner";

		public override string DisplayName => "Tile spawner";

		public override string Description => "Place tiles without items! Spawns tile entities aswell";

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

		public override string Name => "Tile spawner";

		public override string IconTexture => "DragonLens/Assets/Tools/TileSpawner";

		public override Vector2 DefaultPosition => new(0.5f, 0.3f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<TileButton>();
			for (int k = 0; k < TileLoader.TileCount; k++)
			{
				buttons.Add(new TileButton(k, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Mod filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Vanilla", "Vanilla", "Projectiles from the base game", n => !(n is TileButton && (n as TileButton).tileType <= TileID.Count)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModTile>().Count() > 0))
			{
				filters.AddFilter(new TileModFilter(mod));
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			if (selected != -1)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);

			if (selected != -1)
			{
				WorldGen.PlaceTile((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16, selected);
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (selected != -1)
				selected = -1;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (selected != -1)
			{
				Main.instance.LoadTiles(selected);
				Texture2D tex = Terraria.GameContent.TextureAssets.Tile[selected].Value;

				spriteBatch.Draw(tex, Main.MouseScreen + Vector2.One * 8 + tex.Size(), new Rectangle(0, 0, 16, 16), Color.White * 0.5f, 0, Vector2.One * 8, 1, 0, 0);
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

			spriteBatch.Draw(tex, iconBox.Center(), new Rectangle(0, 0, 16, 16), Color.White, 0, Vector2.One * 8, 1, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip($"Type: {tileType}");
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			TileBrowser.selected = tileType;
			Main.NewText($"{tileType} selected, click anywhere in the world to place. Right click to deselect.");
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
