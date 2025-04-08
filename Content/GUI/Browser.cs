using DragonLens.Content.Filters;
using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Content.Sorts;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using ReLogic.Localization.IME;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	/// <summary>
	/// A draggable UI state used for the various spawners of the mod. Allows the user to browse and select entries easily.
	/// </summary>
	internal abstract class Browser : DraggableUIState
	{
		public BrowserTool tool;

		private UIGrid options;
		private StyledScrollbar scrollBar;
		private ToggleButton listButton;
		private ToggleButton filterButton;
		private ToggleButton sortButton;
		public FilterPanel filters;

		internal SearchBar searchBar;
		internal ButtonSizeSlider sizeSlider;

		public bool initialized;
		public bool listMode;
		public bool filtersVisible;
		public int buttonSize = 36;

		public abstract string Name { get; }

		public virtual string IconTexture => "TestTool";

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 500, 64);

		public event FilterDelegate FilterEvent;
		public int sortIndex = 0;
		public List<Sort> SortModes = [];
		public Func<BrowserButton, BrowserButton, int> SortFunction;

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
			foreach (UIElement item in options._items)
			{
				if (item is BrowserButton button)
					button.ShrinkIfFiltered();
			}

			options.UpdateOrder();

			// UIGrid is funny and wants this.
			Recalculate();
			Recalculate();
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
		/// Initialize sorting functions for this browser here
		/// </summary>
		public virtual void SetupSorts()
		{
			// "sensible" default sort based on key prop
			SortFunction = (a, b) => a.Key.CompareTo(b.Key);
		}

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

			sizeSlider = new(this);
			Append(sizeSlider);

			listButton = new("DragonLens/Assets/GUI/Play", () => listMode, LocalizationHelper.GetGUIText("Browser.ListView"));
			listButton.OnLeftClick += (n, k) =>
			{
				listMode = !listMode;
				Recalculate();
				Recalculate();
			};
			Append(listButton);

			filterButton = new("DragonLens/Assets/GUI/Filter", () => filtersVisible, LocalizationHelper.GetGUIText("Browser.Filters"));
			filterButton.OnLeftClick += (n, k) =>
			{
				filtersVisible = !filtersVisible;
				if (filtersVisible)
					filters.Width.Set(220, 0);
				else
					filters.Width.Set(0, 0);

				filters.Recalculate();
				Recalculate();
			};
			Append(filterButton);

			sortButton = new("DragonLens/Assets/GUI/Sort", () => false, LocalizationHelper.GetGUIText("Browser.Sorts"), () => LocalizationHelper.GetGUIText($"Browser.Sort.{SortModes[sortIndex].Name}"));
			sortButton.OnLeftClick += (n, k) =>
			{
				sortIndex++;
				sortIndex %= SortModes.Count;
				SortFunction = SortModes[sortIndex].Function;
				SortGrid();
			};
			Append(sortButton);

			filters = new(this);
			filters.Width.Set(0, 0);
			filters.Height.Set(420, 0);

			if (filtersVisible)
				filters.Width.Set(220, 0);
			else
				filters.Width.Set(0, 0);

			Append(filters);

			SetupFilters(filters);
			SetupSorts();
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

			sizeSlider.Left.Set(newPos.X + 354, 0);
			sizeSlider.Top.Set(newPos.Y + 74, 0);

			listButton.Left.Set(newPos.X + 220, 0);
			listButton.Top.Set(newPos.Y + 66, 0);

			filterButton.Left.Set(newPos.X + 262, 0);
			filterButton.Top.Set(newPos.Y + 66, 0);

			sortButton.Left.Set(newPos.X + 304, 0);
			sortButton.Top.Set(newPos.Y + 66, 0);

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

			Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var target = new Rectangle((int)basePos.X, (int)basePos.Y, 500, 600);

			GUIHelper.DrawBox(spriteBatch, target, ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
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

	internal abstract class BrowserButton : SmartUIElement
	{
		public Browser parent;

		public static int drawDelayTimer = 2; //Here so we dont draw on the first frame of the grid populating, causing a lag bonanza since every single button tries to draw.
		public bool filtered;

		public abstract string Identifier { get; }
		public abstract string Key { get; } // Key used for favorites

		public bool Favorite => parent?.tool?.Favorites?.Contains(Key) ?? false;

		public BrowserButton(Browser parent)
		{
			int size = (int)MathHelper.Clamp(parent.buttonSize, 36, 108);

			Width.Set(size, 0);
			Height.Set(size, 0);

			this.parent = parent;
		}

		public void ShrinkIfFiltered()
		{
			if (!Identifier.ToLower().Contains(parent.searchBar.currentValue.ToLower()) || parent.ShouldBeFiltered(this))
			{
				Width.Set(0, 0);
				Height.Set(0, 0);

				MarginLeft = 0;
				MarginRight = 0;
				MarginTop = 0;
				MarginBottom = 0;

				filtered = true;
			}
			else
			{
				filtered = false;
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (filtered)
				return;

			if (IsMouseHovering && !Main.oldKeyState.IsKeyDown(Main.FavoriteKey) && Main.keyState.IsKeyDown(Main.FavoriteKey))
			{
				if (Favorite)
					parent.tool.Favorites.Remove(Key);
				else
					parent.tool.Favorites.Add(Key);

				parent.SortGrid();
			}
		}

		private void UpdateAsGrid()
		{
			int size = (int)MathHelper.Clamp(parent.buttonSize, 36, 108);

			Width.Set(size, 0);
			Height.Set(size, 0);

			MarginLeft = 2;
			MarginRight = 2;
			MarginTop = 2;
			MarginBottom = 2;
		}

		private void UpdateAsList()
		{
			int size = (int)MathHelper.Clamp(parent.buttonSize, 36, 108);

			Width.Set(Parent.GetDimensions().Width - 24, 0);
			Height.Set(size, 0);

			MarginLeft = 2;
			MarginRight = 2;
			MarginTop = 2;
			MarginBottom = 2;
		}

		public override void Recalculate()
		{
			if (!filtered)
			{
				if (parent.listMode)
					UpdateAsList();
				else
					UpdateAsGrid();
			}

			base.Recalculate();
		}

		public virtual void SafeDraw(SpriteBatch spriteBatch, Rectangle iconArea) { }

		public sealed override void Draw(SpriteBatch spriteBatch)
		{
			if (filtered)
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
				GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.BackgroundColor);
				Utils.DrawBorderStringBig(spriteBatch, Identifier, GetDimensions().Position() + new Vector2(size + 10, size / 2f + 4), Color.White, size / 36f / 3f, 0, 0.5f);
			}

			GUIHelper.DrawBox(spriteBatch, drawBox, ThemeHandler.ButtonColor);
			SafeDraw(spriteBatch, drawBox);

			if (Favorite)
			{
				Texture2D tex = Assets.GUI.Star.Value;
				spriteBatch.Draw(tex, drawBox.TopLeft(), null, Color.White, 0, new Vector2(2, 4), 1, 0, 0);
			}

			base.Draw(spriteBatch);
		}

		public sealed override int CompareTo(object obj)
		{
			if (obj is not BrowserButton other)
				return base.CompareTo(obj);

			if (Favorite != other.Favorite)
				return Favorite.CompareTo(other.Favorite) * -1;

			return parent.SortFunction(this, other);
		}
	}

	internal class SearchBar : TextField
	{
		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			if (updated)
				(Parent as Browser)?.SortGrid();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			if (typing)
			{
				GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor.InvertColor());
				HandleText();

				// draw ime panel, note that if there's no composition string then it won't draw anything
				Main.instance.DrawWindowsIMEPanel(GetDimensions().Position());
			}

			Vector2 pos = GetDimensions().Position() + Vector2.One * 4;

			const float scale = 1;
			string displayed = currentValue;

			Utils.DrawBorderString(spriteBatch, displayed, pos, Color.White, scale);

			// composition string + cursor drawing below
			if (!typing)
				return;

			pos.X += FontAssets.MouseText.Value.MeasureString(displayed).X * scale;
			string compositionString = Platform.Get<IImeService>().CompositionString;

			if (compositionString is { Length: > 0 })
			{
				Utils.DrawBorderString(spriteBatch, compositionString, pos, new Color(255, 240, 20), scale);
				pos.X += FontAssets.MouseText.Value.MeasureString(compositionString).X * scale;
			}

			if (Main.GameUpdateCount % 20 < 10)
				Utils.DrawBorderString(spriteBatch, "|", pos, Color.White, scale);
		}
	}

	internal class ButtonSizeSlider : SmartUIElement
	{
		public bool dragging;
		public float progress;
		public Browser parent;

		public ButtonSizeSlider(Browser parent)
		{
			Width.Set(100, 0);
			Height.Set(16, 0);

			this.parent = parent;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);

				parent.buttonSize = (int)(36 + progress * (108 - 36));

				if (!Main.mouseLeft)
					dragging = false;

				parent.Recalculate();
			}
			else
			{
				progress = (parent.buttonSize - 36) / (108 - 36f);
			}
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			dragging = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			Texture2D tex = Assets.GUI.AlphaScale.Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ThemeHandler.ButtonColor);

			if (IsMouseHovering && !Main.mouseLeft)
			{
				Tooltip.SetName(LocalizationHelper.GetGUIText("Browser.ButtonSizeSlider.Name"));
				Tooltip.SetTooltip(LocalizationHelper.GetGUIText("Browser.ButtonSizeSlider.Tooltip"));
			}
		}
	}
}