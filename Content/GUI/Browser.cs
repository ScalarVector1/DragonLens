using DragonLens.Configs;
using DragonLens.Content.Filters;
using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using FixedUIScrollbar = Terraria.GameContent.UI.Elements.FixedUIScrollbar;

namespace DragonLens.Content.GUI
{
	/// <summary>
	/// A draggable UI state used for the various spawners of the mod. Allows the user to browse and select entries easily.
	/// </summary>
	internal abstract class Browser : DraggableUIState
	{
		private UIGrid options;
		private FixedUIScrollbar scrollBar;
		private FilterPanel filters;
		private ToggleButton listButton;
		private ToggleButton filterButton;

		internal SearchBar searchBar;

		public bool initialized;
		public bool listMode;
		public bool filtersVisible;
		public int buttonSize = 36;

		public abstract string Name { get; }

		public virtual string IconTexture => "TestTool";

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 500, 64);

		public event FilterDelegate FilterEvent;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		/// <summary>
		/// How the grid should be filled out with BrowserButton elements. Add all BrowserButton instances to the grid here.
		/// </summary>
		/// <param name="grid">The grid to populate</param>
		public abstract void PopulateGrid(UIGrid grid);

		/// <summary>
		/// Forces an update on the order of the elements in the browser.
		/// </summary>
		public void SortGrid()
		{
			options.UpdateOrder();
		}

		/// <summary>
		/// Determines if a button should be filtered out based on the current active filters.
		/// </summary>
		/// <param name="button">The button to check</param>
		/// <returns>true if the button should be hidden, false otherwise</returns>
		public bool ShouldBeFiltered(BrowserButton button)
		{
			bool result = false;

			if (FilterEvent is null)
				return false;

			foreach (FilterDelegate del in FilterEvent?.GetInvocationList())
			{
				result |= del(button);
			}

			return result;
		}

		/// <summary>
		/// Add filters to the browser here, by calling AddFilter and AddSeperator on the FilterPanel parameter.
		/// </summary>
		/// <param name="filters">The FilterPanel instance to add your filters to.</param>
		public virtual void SetupFilters(FilterPanel filters) { }

		/// <summary>
		/// Any initialization you need to do should be done here so it is appropriately refrehed.
		/// </summary>
		public virtual void PostInitialize() { }

		public sealed override void SafeOnInitialize()
		{
			width = 500;
			height = 600;

			scrollBar = new(UserInterface);
			scrollBar.Width.Set(24, 0);
			scrollBar.Height.Set(480, 0);
			Append(scrollBar);

			options = new();
			options.Width.Set(460, 0);
			options.Height.Set(480, 0);
			options.SetScrollbar(scrollBar);
			options.ListPadding = 0;
			Append(options);

			searchBar = new();
			searchBar.Width.Set(200, 0);
			searchBar.Height.Set(32, 0);
			Append(searchBar);

			listButton = new("DragonLens/Assets/GUI/Play", () => listMode, "List view");
			listButton.OnClick += (n, k) => listMode = !listMode;
			Append(listButton);

			filterButton = new("DragonLens/Assets/GUI/Filter", () => filtersVisible, "Filters");
			filterButton.OnClick += (n, k) =>
			{
				filtersVisible = !filtersVisible;
				if (filtersVisible)
					filters.Width.Set(220, 0);
				else
					filters.Width.Set(0, 0);
			};
			Append(filterButton);

			filters = new(this);
			filters.Width.Set(0, 0);
			filters.Height.Set(420, 0);
			Append(filters);

			SetupFilters(filters);
			PostInitialize();
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			scrollBar.Left.Set(newPos.X + 464, 0);
			scrollBar.Top.Set(newPos.Y + 110, 0);

			options.Left.Set(newPos.X + 10, 0);
			options.Top.Set(newPos.Y + 110, 0);

			searchBar.Left.Set(newPos.X + 10, 0);
			searchBar.Top.Set(newPos.Y + 66, 0);

			listButton.Left.Set(newPos.X + 220, 0);
			listButton.Top.Set(newPos.Y + 66, 0);

			filterButton.Left.Set(newPos.X + 262, 0);
			filterButton.Top.Set(newPos.Y + 66, 0);

			filters.Left.Set(newPos.X + width + 10, 0);
			filters.Top.Set(newPos.Y, 0);
		}

		/// <summary>
		/// Reload the entire browser. Note this may cause a bit of lag due to re-populating the entire grid.
		/// </summary>
		public void Refresh()
		{
			RemoveAllChildren();
			OnInitialize();
			PopulateGrid(options);
			SortGrid();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var target = new Rectangle((int)basePos.X, (int)basePos.Y, 500, 600);

			GUIHelper.DrawBox(spriteBatch, target, ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D gridBack = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			var gridBackTarget = options.GetDimensions().ToRectangle();
			gridBackTarget.Inflate(4, 4);
			spriteBatch.Draw(gridBack, gridBackTarget, Color.Black * 0.25f);

			Texture2D icon = ThemeHandler.GetIcon(IconTexture);
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, Name, basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			base.Draw(spriteBatch);
		}
	}

	internal abstract class BrowserButton : UIElement
	{
		public Browser parent;

		public static int drawDelayTimer = 2; //Here so we dont draw on the first frame of the grid populating, causing a lag bonanza since every single button tries to draw.

		public abstract string Identifier { get; }

		public BrowserButton(Browser parent)
		{
			int size = (int)MathHelper.Clamp(ModContent.GetInstance<GUIConfig>().browserButtonSize, 36, 108);

			Width.Set(size, 0);
			Height.Set(size, 0);

			this.parent = parent;
		}

		public override void Update(GameTime gameTime)
		{
			//Will likely need a better solution to optimize when not constantly searching
			if (!Identifier.ToLower().Contains(parent.searchBar.currentValue.ToLower()) || parent.ShouldBeFiltered(this))
			{
				Width.Set(0, 0);
				Height.Set(0, 0);

				MarginLeft = 0;
				MarginRight = 0;
				MarginTop = 0;
				MarginBottom = 0;
				return;
			}

			if (parent.listMode)
				UpdateAsList();
			else
				UpdateAsGrid();

			base.Update(gameTime);
		}

		private void UpdateAsGrid()
		{
			int size = (int)MathHelper.Clamp(ModContent.GetInstance<GUIConfig>().browserButtonSize, 36, 108);

			if (GetDimensions().Width != size || MarginLeft == 0)
			{
				Width.Set(size, 0);
				Height.Set(size, 0);

				MarginLeft = 2;
				MarginRight = 2;
				MarginTop = 2;
				MarginBottom = 2;
			}
		}

		private void UpdateAsList()
		{
			int size = (int)MathHelper.Clamp(ModContent.GetInstance<GUIConfig>().browserButtonSize, 36, 108);

			Width.Set(Parent.GetDimensions().Width - 24, 0);
			Height.Set(size, 0);

			MarginLeft = 2;
			MarginRight = 2;
			MarginTop = 2;
			MarginBottom = 2;
		}

		public virtual void SafeDraw(SpriteBatch spriteBatch, Rectangle iconArea) { }

		public sealed override void Draw(SpriteBatch spriteBatch)
		{
			if (GetDimensions().Width <= 0)
				return;

			if (!parent.GetDimensions().ToRectangle().Intersects(GetDimensions().ToRectangle()))
				return;

			if (drawDelayTimer > 0)
				return;

			int size = (int)MathHelper.Clamp(parent.buttonSize, 36, 108);

			var drawBox = GetDimensions().ToRectangle();

			if (parent.listMode)
				drawBox.Width = size;

			if (parent.listMode)
			{
				GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().backgroundColor);
				Utils.DrawBorderStringBig(spriteBatch, Identifier, GetDimensions().Position() + new Vector2(size + 10, size / 2f + 4), Color.White, size / 36f / 3f, 0, 0.5f);
			}

			GUIHelper.DrawBox(spriteBatch, drawBox, ModContent.GetInstance<GUIConfig>().buttonColor);
			SafeDraw(spriteBatch, drawBox);

			base.Draw(spriteBatch);
		}
	}

	internal class SearchBar : TextField
	{
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (updated)
			{
				BrowserButton.drawDelayTimer = 2;
				(Parent as Browser)?.SortGrid();
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			if (typing)
			{
				GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());
			}

			Vector2 pos = GetDimensions().Position() + Vector2.One * 4;

			string displayed = currentValue;

			if (typing && Main.GameUpdateCount % 20 < 10)
				displayed += "|";

			Utils.DrawBorderString(spriteBatch, displayed, pos, Color.White);
		}
	}
}
