using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using System.IO;
using Terraria.UI;
namespace DragonLens.Content.GUI
{
	internal class FirstTimeLayoutPresetMenu : SmartUIState
	{
		public override bool Visible => FirstTimeSetupSystem.trueFirstTime;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			var newButton = new FirstTimeLayoutButton("Simple", Path.Join(Main.SavePath, "DragonLensLayouts", "Simple"), "DragonLens/Assets/Misc/PreviewSimple");
			newButton.Left.Set(-220, 0.5f);
			newButton.Top.Set(-262, 0.5f);
			Append(newButton);

			newButton = new FirstTimeLayoutButton("Advanced", Path.Join(Main.SavePath, "DragonLensLayouts", "Advanced"), "DragonLens/Assets/Misc/PreviewAdv");
			newButton.Left.Set(20, 0.5f);
			newButton.Top.Set(-262, 0.5f);
			Append(newButton);

			newButton = new FirstTimeLayoutButton("HEROsMod", Path.Join(Main.SavePath, "DragonLensLayouts", "HEROs mod imitation"), "DragonLens/Assets/Misc/PreviewHeros");
			newButton.Left.Set(-220, 0.5f);
			newButton.Top.Set(20, 0.5f);
			Append(newButton);

			newButton = new FirstTimeLayoutButton("Cheatsheet", Path.Join(Main.SavePath, "DragonLensLayouts", "Cheatsheet imitation"), "DragonLens/Assets/Misc/PreviewCheatsheet");
			newButton.Left.Set(20, 0.5f);
			newButton.Top.Set(20, 0.5f);
			Append(newButton);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Utils.DrawBorderStringBig(spriteBatch, LocalizationHelper.GetGUIText("FirstTimeLayoutPresetMenu.Welcome"), GetDimensions().Center() + new Vector2(0, -380), Color.White, 1, 0.5f, 0);
			Utils.DrawBorderStringBig(spriteBatch, LocalizationHelper.GetGUIText("FirstTimeLayoutPresetMenu.SelectLayout"), GetDimensions().Center() + new Vector2(0, -320), Color.LightGray, 0.6f, 0.5f, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class FirstTimeLayoutButton : SmartUIElement
	{
		private readonly string name;
		private readonly string tooltip;
		private readonly string presetPath;
		private readonly string texPath;

		public FirstTimeLayoutButton(string name, string presetPath, string texPath, string tooltip)
		{
			this.name = name;
			this.presetPath = presetPath;
			this.tooltip = tooltip;
			this.texPath = texPath;

			Width.Set(218, 0);
			Height.Set(244, 0);
		}

		public FirstTimeLayoutButton(string localizationKey, string presetPath, string texPath) : this(LocalizationHelper.GetGUIText($"Layout.{localizationKey}.Name"), presetPath, texPath, LocalizationHelper.GetGUIText($"Layout.{localizationKey}.Tooltip"))
		{
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ToolbarHandler.LoadFromFile(presetPath);

			ToolbarHandler.ExportToFile(Path.Join(Main.SavePath, "DragonLensLayouts", "Current"));

			UILoader.GetUIState<ToolbarState>().Refresh();

			Main.NewText(LocalizationHelper.GetGUIText("FirstTimeLayoutPresetMenu.Selected", name));
			FirstTimeSetupSystem.trueFirstTime = false;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.BackgroundColor);

			Utils.DrawBorderString(spriteBatch, name, GetDimensions().Position() + Vector2.One * 8, Color.White);

			Texture2D tex = ModContent.Request<Texture2D>(texPath).Value;
			spriteBatch.Draw(tex, GetDimensions().Position() + new Vector2(8, 36), null, Color.White, 0, Vector2.Zero, 0.5f, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(name);
				Tooltip.SetTooltip(tooltip);
			}
		}
	}
}