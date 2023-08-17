using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Helpers;
using System.IO;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class NewBarButton : ToolbarCustomizationElement
	{
		public override void SafeClick(UIMouseEvent evt)
		{
			ToolbarHandler.activeToolbars.Add(new Toolbar(new Vector2(0.5f, 0.6f), Orientation.Horizontal, Main.mapFullscreen ? AutomaticHideOption.NoMapScreen : AutomaticHideOption.Never));

			UILoader.GetUIState<ToolbarState>().Refresh();
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawTarget = GetDimensions().ToRectangle();
			drawTarget = new Rectangle(drawTarget.X, drawTarget.Y, 48, 48);

			drawTarget.Inflate(-6, -16);
			GUIHelper.DrawBox(spriteBatch, drawTarget, ThemeHandler.ButtonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/NewBar").Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class SaveLayoutButton : ToolbarCustomizationElement
	{
		public override void SafeClick(UIMouseEvent evt)
		{
			UILoader.GetUIState<ToolbarState>().FinishCustomize();
			ToolbarHandler.ExportToFile(Path.Join(Main.SavePath, "DragonLensLayouts", "Current"));

			UILoader.GetUIState<LayoutPresetBrowser>().visible = false;
			UILoader.GetUIState<ThemeMenu>().visible = false;

			CustomizeTool.customizing = false;

			UILoader.GetUIState<ToolbarState>().Refresh();

			Main.NewText(LocalizationHelper.GetGUIText("ToolbarCustomizationElements.SaveLayoutButton.LayoutSaved"));
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/SaveLayout").Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class LoadLayoutButton : ToolbarCustomizationElement
	{
		public override void SafeClick(UIMouseEvent evt)
		{
			LayoutPresetBrowser state = UILoader.GetUIState<LayoutPresetBrowser>();
			state.visible = !state.visible;

			BrowserButton.drawDelayTimer = 2;

			if (!state.initialized)
			{
				UILoader.GetUIState<LayoutPresetBrowser>().Refresh();
				state.initialized = true;
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/LoadLayout").Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class VisualConfigButton : ToolbarCustomizationElement
	{
		public override void SafeClick(UIMouseEvent evt)
		{
			UILoader.GetUIState<ThemeMenu>().visible = true;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/StyleButton").Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class FunctionalConfigButton : ToolbarCustomizationElement
	{
		public override void SafeClick(UIMouseEvent evt)
		{
			Helpers.GUIHelper.OpenConfig(ModContent.GetInstance<ToolConfig>());
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			Texture2D tex = ThemeHandler.GetIcon("Customize");
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}
}