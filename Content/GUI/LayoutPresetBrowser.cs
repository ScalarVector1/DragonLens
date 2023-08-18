using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class LayoutPresetBrowser : Browser
	{
		public override string Name => LocalizationHelper.GetGUIText("LayoutPresetBrowser.Name");

		public override Vector2 DefaultPosition => new(0.6f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")) + 1;
		}

		public override void PopulateGrid(UIGrid grid)
		{
			grid.Add(new LayoutPresetButton(this, "Simple", Path.Join(Main.SavePath, "DragonLensLayouts", "Simple")));
			grid.Add(new LayoutPresetButton(this, "Advanced", Path.Join(Main.SavePath, "DragonLensLayouts", "Advanced")));
			grid.Add(new LayoutPresetButton(this, "HEROsMod", Path.Join(Main.SavePath, "DragonLensLayouts", "HEROs mod imitation")));
			grid.Add(new LayoutPresetButton(this, "Cheatsheet", Path.Join(Main.SavePath, "DragonLensLayouts", "Cheatsheet imitation")));
			grid.Add(new LayoutPresetButton(this, "Empty", Path.Join(Main.SavePath, "DragonLensLayouts", "Empty")));
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

		public LayoutPresetButton(Browser parent, string name, string presetPath, string tooltip) : base(parent)
		{
			this.name = name;
			this.presetPath = presetPath;
			this.tooltip = tooltip;
		}

		public LayoutPresetButton(Browser parent, string localizationKey, string presetPath) : this(parent, LocalizationHelper.GetGUIText($"Layout.{localizationKey}.Name"), presetPath, LocalizationHelper.GetGUIText($"Layout.{localizationKey}.Tooltip"))
		{
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ToolbarHandler.LoadFromFile(presetPath);
			UILoader.GetUIState<ToolbarState>().Refresh();

			Main.NewText(LocalizationHelper.GetGUIText("LayoutPresetBrowser.LoadedLayout", name));
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