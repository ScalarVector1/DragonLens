using DragonLens.Configs;
using DragonLens.Content.Filters;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Helpers;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class FilterPanel : SmartUIElement
	{
		public Browser parent;

		public UIGrid filters;
		public FixedUIScrollbar bar;

		/// <summary>
		/// We track an order so the order in which filters are added in code is the order in which they appear
		/// </summary>
		public int lastOrder = 0;

		public FilterPanel(Browser parent)
		{
			this.parent = parent;

			bar = new(parent.UserInterface);
			bar.Width.Set(16, 0);
			bar.Height.Set(400, 0);
			bar.Left.Set(194, 0);
			bar.Top.Set(10, 0);
			Append(bar);

			filters = new();
			filters.Width.Set(180, 0);
			filters.Height.Set(400, 0);
			filters.Left.Set(10, 0);
			filters.Top.Set(10, 0);
			filters.SetScrollbar(bar);
			Append(filters);

		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawBox = GetDimensions().ToRectangle();

			if (drawBox.Width == 0)
				return;

			drawBox.Height = (int)MathHelper.Min(420, filters.GetTotalHeight() + 20);

			bar.Height.Set(drawBox.Height - 20, 0);

			GUIHelper.DrawBox(spriteBatch, drawBox, ModContent.GetInstance<GUIConfig>().backgroundColor);

			base.Draw(spriteBatch);
		}

		public void AddFilter(Filter filter)
		{
			var button = new FilterButton(parent, filter);
			button.Width.Set(32, 0);
			button.Height.Set(32, 0);
			button.order = lastOrder++;
			filters.Add(button);
		}

		public void AddSeperator(string localizationKey)
		{
			var seperator = new FilterSeperator(localizationKey)
			{
				order = lastOrder++
			};
			filters.Add(seperator);
		}
	}

	internal class FilterButton : SmartUIElement
	{
		public Browser parent;
		public Filter filter;

		public bool active;

		public int order;

		public FilterButton(Browser parent, Filter filter)
		{
			this.parent = parent;
			this.filter = filter;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			active = !active;

			if (active)
				parent.FilterEvent += filter.shouldFilter;
			else
				parent.FilterEvent -= filter.shouldFilter;

			BrowserButton.drawDelayTimer = 2;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawBox = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, drawBox, ModContent.GetInstance<GUIConfig>().buttonColor);

			if (active)
				GUIHelper.DrawOutline(spriteBatch, drawBox, ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

			drawBox.Inflate(-4, -4);
			filter.Draw(spriteBatch, drawBox);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Tooltip.SetName(filter.Name);
				Tooltip.SetTooltip(filter.Description);
			}
		}

		public override int CompareTo(object obj)
		{
			if (obj is FilterButton)
				return order - (obj as FilterButton).order;
			else if (obj is FilterSeperator)
				return order - (obj as FilterSeperator).order;
			else
				return 0;
		}
	}

	internal class FilterSeperator : SmartUIElement
	{
		public string localizationKey;

		public int order;

		public FilterSeperator(string localizationKey)
		{
			this.localizationKey = localizationKey;

			Width.Set(180, 0);
			Height.Set(24, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = GetDimensions().ToRectangle();
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetText(localizationKey), GetDimensions().Position() + new Vector2(12, 4), Color.White, 0.8f);
		}

		public override int CompareTo(object obj)
		{
			if (obj is FilterButton)
				return order - (obj as FilterButton).order;
			else if (obj is FilterSeperator)
				return order - (obj as FilterSeperator).order;
			else
				return 0;
		}
	}
}