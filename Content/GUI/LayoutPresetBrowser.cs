using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using System.IO;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class LayoutPresetBrowser : Browser
	{
		public override string Name => "Layout Presets";

		public override void PopulateGrid(UIGrid grid)
		{
			grid.Add(new LayoutPresetButton(this, "Simple", Path.Join(Main.SavePath, "DragonLensLayouts", "Simple"), "A simplified layout, perfect for players or mod testers."));
			grid.Add(new LayoutPresetButton(this, "Advanced", Path.Join(Main.SavePath, "DragonLensLayouts", "Advanced"), "An advanced layout with every tool available. Perfect for mod developers or power users."));
			grid.Add(new LayoutPresetButton(this, "HEROs mod imitation", Path.Join(Main.SavePath, "DragonLensLayouts", "HEROs mod imitation"), "A layout attempting to mimic HEROs mod as closely as possible."));
			grid.Add(new LayoutPresetButton(this, "Cheatsheet imitation", Path.Join(Main.SavePath, "DragonLensLayouts", "Cheatsheet imitation"), "A layout attempting to mimic Cheatsheet as closely as possible."));
			grid.Add(new LayoutPresetButton(this, "Empty", Path.Join(Main.SavePath, "DragonLensLayouts", "Empty"), "A clean slate to build your own layout."));
		}

		public override void PostInitialize()
		{
			listMode = true;
		}
	}

	internal class LayoutPresetButton : BrowserButton
	{
		private readonly string name;
		private readonly string tooltip;
		private readonly string presetPath;

		public override string Identifier => name;

		public LayoutPresetButton(Browser parent, string name, string presetPath, string tooltip = "A toolbar layout preset") : base(parent)
		{
			this.name = name;
			this.presetPath = presetPath;
			this.tooltip = tooltip;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ToolbarHandler.LoadFromFile(presetPath);
			UILoader.GetUIState<ToolbarState>().Refresh();

			Main.NewText($"Loaded layout: {name}");
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconArea)
		{
			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip(tooltip);
			}
		}
	}
}
