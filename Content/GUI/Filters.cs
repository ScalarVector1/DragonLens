using DragonLens.Configs;
using DragonLens.Content.Filters;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class FilterPanel : UIElement
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

		public void AddSeperator(string name)
		{
			var seperator = new FilterSeperator(name)
			{
				order = lastOrder++
			};
			filters.Add(seperator);
		}
	}

	internal class FilterButton : UIElement
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

		public override void Click(UIMouseEvent evt)
		{
			active = !active;

			if (active)
				parent.FilterEvent += filter.shouldFilter;
			else
				parent.FilterEvent -= filter.shouldFilter;
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
				Tooltip.SetName(filter.name);
				Tooltip.SetTooltip(filter.description);
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

	internal class FilterSeperator : UIElement
	{
		public string name;

		public int order;

		public FilterSeperator(string name)
		{
			this.name = name;

			Width.Set(180, 0);
			Height.Set(24, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = GetDimensions().ToRectangle();
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, name, GetDimensions().Position() + new Vector2(12, 4), Color.White, 0.8f);
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
