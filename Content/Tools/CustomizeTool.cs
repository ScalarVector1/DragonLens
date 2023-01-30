﻿using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
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
				UILoader.GetUIState<ToolbarState>().Customize();
			else
				UILoader.GetUIState<ToolbarState>().FinishCustomize();

			Main.NewText("Test tool used!");
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
			for (int k = 0; k < ToolLoader.Tools.Count; k++)
			{
				grid.Add(new ToolBrowserButton(ToolLoader.Tools[k]));
			}
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

		public override string Identifier => tool.Name;

		public ToolBrowserButton(Tool tool)
		{
			this.tool = tool;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			tool.DrawIcon(spriteBatch, GetDimensions().Position());
		}

		public override void Click(UIMouseEvent evt)
		{
			ToolBrowser.TrackedToolbar.AddTool(tool);

			UILoader.GetUIState<ToolbarState>().Refresh();
			UILoader.GetUIState<ToolbarState>().Customize();
		}

		public override int CompareTo(object obj)
		{
			return tool.Name.CompareTo((obj as ToolBrowserButton).tool.Name);
		}
	}
}
