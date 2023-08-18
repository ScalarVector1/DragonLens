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
	internal abstract class CustomizeMenuLine : LocalizedCustomizationElement
	{
		public CustomizeMenuLine()
		{
			Width.Set(200, 0);
			Height.Set(48, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawTarget = GetDimensions().ToRectangle();
			var buttonTarget = new Rectangle(drawTarget.X, drawTarget.Y, 48, 48);

			GUIHelper.DrawBox(spriteBatch, drawTarget, ThemeHandler.BackgroundColor);
			GUIHelper.DrawBox(spriteBatch, buttonTarget, ThemeHandler.ButtonColor);

			Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.{GetType().Name}.Name"), new Vector2(drawTarget.X + 54, drawTarget.Y + 28), Color.White, 1, 0, 0.5f);

			if (IsMouseHovering)
			{
				Tooltip.SetName(LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.{GetType().Name}.Name"));
				Tooltip.SetTooltip(LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.{GetType().Name}.Tooltip"));
			}

			base.Draw(spriteBatch);
		}
	}

	internal class NewBarButton : CustomizeMenuLine
	{
		public override void SafeClick(UIMouseEvent evt)
		{
			ToolbarHandler.activeToolbars.Add(new Toolbar(new Vector2(0.5f, 0.2f), Orientation.Horizontal, Main.mapFullscreen ? AutomaticHideOption.NoMapScreen : AutomaticHideOption.Never));

			UILoader.GetUIState<ToolbarState>().Refresh();
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			var drawTarget = GetDimensions().ToRectangle();
			drawTarget = new Rectangle(drawTarget.X, drawTarget.Y, 48, 48);

			drawTarget.Inflate(-6, -16);
			GUIHelper.DrawBox(spriteBatch, drawTarget, ThemeHandler.ButtonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/NewBar").Value;
			spriteBatch.Draw(tex, GetDimensions().Position() + Vector2.One * 24, null, Color.White, 0, tex.Size() / 2, 1, 0, 0);
		}
	}

	internal class SaveLayoutButton : CustomizeMenuLine
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
			base.Draw(spriteBatch);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/SaveLayout").Value;
			spriteBatch.Draw(tex, GetDimensions().Position() + Vector2.One * 24, null, Color.White, 0, tex.Size() / 2, 1, 0, 0);
		}
	}

	internal class LoadLayoutButton : CustomizeMenuLine
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
			base.Draw(spriteBatch);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/LoadLayout").Value;
			spriteBatch.Draw(tex, GetDimensions().Position() + Vector2.One * 24, null, Color.White, 0, tex.Size() / 2, 1, 0, 0);
		}
	}

	internal class VisualConfigButton : CustomizeMenuLine
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
			base.Draw(spriteBatch);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/StyleButton").Value;
			spriteBatch.Draw(tex, GetDimensions().Position() + Vector2.One * 24, null, Color.White, 0, tex.Size() / 2, 1, 0, 0);
		}
	}

	internal class FunctionalConfigButton : CustomizeMenuLine
	{
		public override void SafeClick(UIMouseEvent evt)
		{
			GUIHelper.OpenConfig(ModContent.GetInstance<ToolConfig>());
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			Texture2D tex = ThemeHandler.GetIcon("Customize");
			spriteBatch.Draw(tex, GetDimensions().Position() + Vector2.One * 24, null, Color.White, 0, tex.Size() / 2, 1, 0, 0);
		}
	}
}