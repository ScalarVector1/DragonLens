using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools
{
	internal class CustomizeTool : Tool
	{
		public static bool customizing;

		public override string IconKey => "Customize";

		public override string DisplayName => "Customize tool";

		public override string Description => "Customize your toolbar layout!";

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
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
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

		public override string Name => "Add tool";

		public override string IconTexture => "Customize";

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<ToolBrowserButton>();
			for (int k = 0; k < ToolHandler.Tools.Count; k++)
			{
				buttons.Add(new ToolBrowserButton(ToolHandler.Tools[k], this));
			}

			grid.AddRange(buttons);
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

		public ToolBrowserButton(Tool tool, Browser browser) : base(browser)
		{
			this.tool = tool;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Rectangle target = iconBox;
			target.Inflate(-4, -4);

			Texture2D icon = ThemeHandler.GetIcon(tool.IconKey);
			float scale = 1;

			if (icon.Width > target.Width || icon.Height > target.Height)
				scale = icon.Width > icon.Height ? target.Width / icon.Width : target.Height / icon.Height;

			spriteBatch.Draw(icon, target.Center(), null, Color.White, 0, icon.Size() / 2f, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(tool.DisplayName);
				Tooltip.SetTooltip(tool.Description);
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ToolBrowser.TrackedToolbar.AddTool(tool);

			UILoader.GetUIState<ToolbarState>().Refresh();
		}

		public override int CompareTo(object obj)
		{
			return tool.DisplayName.CompareTo((obj as ToolBrowserButton).tool.DisplayName);
		}
	}
}
