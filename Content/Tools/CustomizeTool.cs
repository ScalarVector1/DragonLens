﻿using DragonLens.Content.Filters.ToolFilters;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools
{
	internal class CustomizeTool : Tool
	{
		public static bool customizing;

		public override string IconKey => "Customize";

		public override void OnActivate()
		{
			if (!customizing)
			{
				customizing = true;

				if (customizing)
					UILoader.GetUIState<ToolbarState>().Customize();
			}
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			if (customizing)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

				Texture2D tex = Assets.Misc.GlowAlpha.Value;
				Color color = Color.White;
				color.A = 0;
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class ToolBrowser : Browser
	{
		public static ToolbarElement trackedElement;

		public static Toolbar TrackedToolbar => trackedElement?.toolbar;

		public override string Name => LocalizationHelper.GetToolText("CustomizeTool.ToolBrowser");

		public override string IconTexture => "Customize";

		public override Vector2 DefaultPosition => new(0.6f, 0.3f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")) + 1;
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<ToolBrowserButton>();

			foreach (Tool tool in ModContent.GetContent<Tool>())
			{
				buttons.Add(new ToolBrowserButton(tool, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			// Add only mods that have tools
			foreach (Mod mod in ModContent.GetContent<Tool>().Select(t => t.Mod).Distinct())
			{
				filters.AddFilter(new ToolModFilter(mod));
			}
		}

		public override void SetupSorts()
		{
			SortModes.Add(new("Alphabetical", (a, b) => a.Identifier.CompareTo(b.Identifier)));

			SortFunction = SortModes.First().Function;
		}

		public static void OpenForToolbar(ToolbarElement bar)
		{
			trackedElement = bar;
			UILoader.GetUIState<ToolBrowser>().visible = true;

			ToolBrowser state = UILoader.GetUIState<ToolBrowser>();

			if (!state.initialized)
			{
				UILoader.GetUIState<ToolBrowser>().Refresh();
				state.initialized = true;
			}
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			// Have the browser follow expected visibility on map VS non-map bars
			if (TrackedToolbar != null && TrackedToolbar.automaticHideOption != AutomaticHideOption.NoMapScreen && Main.mapFullscreen)
				visible = false;

			if (TrackedToolbar != null && TrackedToolbar.automaticHideOption == AutomaticHideOption.NoMapScreen && !Main.mapFullscreen)
				visible = false;

			base.DraggableUdpate(gameTime);
		}
	}

	internal class ToolBrowserButton : BrowserButton
	{
		public Tool tool;

		public override string Identifier => tool.DisplayName;
		public override string Key => tool.Mod?.Name ?? "Terraria" + ":" + tool.Name;

		public ToolBrowserButton(Tool tool, Browser browser) : base(browser)
		{
			this.tool = tool;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Rectangle target = iconBox;
			target.Inflate(-4, -4);

			Texture2D icon = ThemeHandler.GetIcon(tool.IconKey);
			float scale = iconBox.Width / 52f;

			if (icon.Width > 32 || icon.Height > 32)
				scale *= 32f / Math.Max(icon.Width, icon.Height);

			spriteBatch.Draw(icon, target.Center(), null, Color.White, 0, icon.Size() / 2f, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(tool.DisplayName);
				Tooltip.SetTooltip(tool.Description);
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ToolBrowser.TrackedToolbar?.AddTool(tool);

			UILoader.GetUIState<ToolbarState>()?.Refresh();
		}
	}
}