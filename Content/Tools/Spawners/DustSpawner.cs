using DragonLens.Content.Filters;
using DragonLens.Content.Filters.DustFilters;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class DustSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/DustSpawner";

		public override string DisplayName => "Dust spawner";

		public override string Description => "Spawn dust, with options to preview different spawning methods and parameters";

		public override void OnActivate()
		{
			DustBrowser state = UILoader.GetUIState<DustBrowser>();
			state.visible = !state.visible;

			if (!state.initialized)
			{
				UILoader.GetUIState<DustBrowser>().Refresh();
				state.initialized = true;
			}
		}
	}

	internal class DustBrowser : Browser
	{
		public static Dust selected;

		public Vector2 velocity;

		public override string Name => "Dust spawner";

		public override string IconTexture => "DragonLens/Assets/Tools/DustSpawner";

		public override Vector2 DefaultPosition => new(0.5f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<DustButton>();
			for (int k = 0; true; k++)
			{
				if (k > DustID.Count && DustLoader.GetDust(k) is null)
					break;

				var dust = new Dust
				{
					type = k
				};

				buttons.Add(new DustButton(dust, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Mod filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Vanilla", "Vanilla", "Dusts from the base game", n => !(n is DustButton && (n as DustButton).dust.type <= DustID.Count)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModDust>().Count() > 0))
			{
				filters.AddFilter(new DustModFilter(mod));
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			if (selected != null)
			{
				Main.LocalPlayer.mouseInterface = true;

				if (Main.mouseLeft)
				{
					Dust.NewDust(Main.MouseWorld, 16, 16, selected.type);
				}
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (selected != null)
				selected = null;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (selected != null)
			{

			}

			base.Draw(spriteBatch);
		}
	}

	internal class DustButton : BrowserButton
	{
		public Dust dust;

		public override string Identifier => dust.type <= DustID.Count ? $"Dust {dust.type}" : DustLoader.GetDust(dust.type).Name;

		public DustButton(Dust dust, Browser browser) : base(browser)
		{
			this.dust = dust;

			if (dust.type > DustID.Count)
			{
				ModDust md = DustLoader.GetDust(dust.type);
				try
				{
					md.OnSpawn(dust);
					md.Update(dust);
				}
				catch
				{
					dust.frame = new Rectangle(0, 0, 8, 8);
				}
			}
			else
			{
				dust.frame.X = 10 * dust.type;
				dust.frame.Y = 10;
				dust.shader = null;
				dust.customData = null;
				int row = dust.type / 100;
				dust.frame.X -= 1000 * row;
				dust.frame.Y += 30 * row;
				dust.frame.Width = 8;
				dust.frame.Height = 8;
			}

			if (dust.frame.Width == 0)
				dust.frame = new Rectangle(0, 0, 8, 8);
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Texture2D tex;

			if (dust.type <= DustID.Count)
				tex = Terraria.GameContent.TextureAssets.Dust.Value;
			else
				tex = DustLoader.GetDust(dust.type).Texture2D.Value;

			Rectangle frame = dust.frame;

			float scale = 1;
			if (frame.Width > 32 || frame.Height > 32)
				scale = 32f / Math.Max(frame.Width, frame.Height);

			spriteBatch.Draw(tex, iconBox.Center(), frame, Color.White, 0, new Vector2(frame.Width, frame.Height) / 2, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip($"Type: {dust.type}");
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			DustBrowser.selected = dust;
			Main.NewText($"{Identifier} selected, click anywhere in the world to spawn. Right click to deselect.");
		}

		public override void RightClick(UIMouseEvent evt)
		{

		}

		public override int CompareTo(object obj)
		{
			return dust.type - (obj as DustButton).dust.type;
		}
	}
}
