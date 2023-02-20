using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools
{
	internal class CustomizeTool : Tool
	{
		public static bool customizing;

		public override string Texture => "DragonLens/Assets/Tools/TestTool";

		public override string DisplayName => "Customize tool";

		public override string Description => "Customize your toolbar layout!";

		public override void OnActivate()
		{
			customizing = !customizing;

			if (customizing)
			{
				UILoader.GetUIState<ToolbarState>().Customize();
			}
			else
			{
				UILoader.GetUIState<ToolbarState>().FinishCustomize();
				ToolbarHandler.ExportToFile(Path.Join(Main.SavePath, "DragonLensLayouts", "Current"));
				Main.NewText("Layout saved!");
			}
		}
	}

	internal class ToolBrowser : Browser
	{
		public static ToolbarElement trackedElement;

		public static Toolbar TrackedToolbar => trackedElement.toolbar;

		public override string Name => "Add tool";

		public override string IconTexture => "DragonLens/Assets/Tools/TestTool";

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
			spriteBatch.Draw(ModContent.Request<Texture2D>(tool.Texture).Value, target, Color.White);

			if (IsMouseHovering)
			{
				Tooltip.SetName(tool.DisplayName);
				Tooltip.SetTooltip(tool.Description);
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			ToolBrowser.TrackedToolbar.AddTool(tool);

			UILoader.GetUIState<ToolbarState>().Refresh();
			UILoader.GetUIState<ToolbarState>().Customize();
		}

		public override int CompareTo(object obj)
		{
			return tool.DisplayName.CompareTo((obj as ToolBrowserButton).tool.DisplayName);
		}
	}
}
