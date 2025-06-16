﻿using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class ThemeMenu : DraggableUIState
	{
		private UIGrid boxes;
		private StyledScrollbar boxScrollBar;
		private UIGrid icons;
		private StyledScrollbar iconScrollBar;

		private ColorEditor backgroundColorEditor;
		private ColorEditor foregroundColorEditor;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 574, 64);

		public override Vector2 DefaultPosition => new(0.3f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")) + 1;
		}

		public override void SafeOnInitialize()
		{
			width = 574;
			height = 400;

			boxScrollBar = new(UserInterface);
			boxScrollBar.Width.Set(24, 0);
			boxScrollBar.Height.Set(300, 0);
			Append(boxScrollBar);

			boxes = new();
			boxes.Width.Set(140, 0);
			boxes.Height.Set(300, 0);
			boxes.SetScrollbar(boxScrollBar);
			Append(boxes);

			iconScrollBar = new(UserInterface);
			iconScrollBar.Width.Set(24, 0);
			iconScrollBar.Height.Set(300, 0);
			Append(iconScrollBar);

			icons = new();
			icons.Width.Set(140, 0);
			icons.Height.Set(300, 0);
			icons.SetScrollbar(iconScrollBar);
			Append(icons);

			backgroundColorEditor = new(LocalizationHelper.GetText("BackgroundColor.Name"),
				(a) => ThemeHandler.currentColorProvider.backgroundColor = a,
				ThemeHandler.BackgroundColor, null, LocalizationHelper.GetText("BackgroundColor.Description"));
			Append(backgroundColorEditor);

			foregroundColorEditor = new(LocalizationHelper.GetText("ButtonColor.Name"),
				(a) => ThemeHandler.currentColorProvider.buttonColor = a,
				ThemeHandler.ButtonColor, null, LocalizationHelper.GetText("ButtonColor.Description"));
			Append(foregroundColorEditor);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			boxScrollBar.Left.Set(newPos.X + 160, 0);
			boxScrollBar.Top.Set(newPos.Y + 80, 0);

			boxes.Left.Set(newPos.X + 16, 0);
			boxes.Top.Set(newPos.Y + 80, 0);

			iconScrollBar.Left.Set(newPos.X + 360, 0);
			iconScrollBar.Top.Set(newPos.Y + 80, 0);

			icons.Left.Set(newPos.X + 216, 0);
			icons.Top.Set(newPos.Y + 80, 0);

			backgroundColorEditor.Left.Set(newPos.X + 400, 0);
			backgroundColorEditor.Top.Set(newPos.Y + 90, 0);

			foregroundColorEditor.Left.Set(newPos.X + 400, 0);
			foregroundColorEditor.Top.Set(newPos.Y + 240, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			// Probably not the best check but is functional for now
			if (boxes.Count <= 0 && icons.Count <= 0)
			{
				RemoveAllChildren();
				OnInitialize(); // We have to re-initialize so the scrollbars set properly and lists populate correctly
				PopulateLists();
			}

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll("DragonLens: Theme Menu");

			var target = new Rectangle((int)basePos.X, (int)basePos.Y, 574, 400);

			GUIHelper.DrawBox(spriteBatch, target, ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 474, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D gridBack = Terraria.GameContent.TextureAssets.MagicPixel.Value;

			var gridBackTarget = boxes.GetDimensions().ToRectangle();
			gridBackTarget.Inflate(4, 4);
			spriteBatch.Draw(gridBack, gridBackTarget, Color.Black * 0.25f);

			gridBackTarget = icons.GetDimensions().ToRectangle();
			gridBackTarget.Inflate(4, 4);
			spriteBatch.Draw(gridBack, gridBackTarget, Color.Black * 0.25f);

			Texture2D icon = ThemeHandler.GetIcon("Customize");
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, LocalizationHelper.GetGUIText("ThemeMenu.Name"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			base.Draw(spriteBatch);
		}

		private void PopulateLists()
		{
			foreach (KeyValuePair<string, ThemeBoxProvider> pair in ThemeHandler.allBoxProviders)
			{
				boxes.Add(new BoxProviderButton(pair.Value));
			}

			foreach (KeyValuePair<string, ThemeIconProvider> pair in ThemeHandler.allIconProviders)
			{
				icons.Add(new IconProviderButton(pair.Value));
			}

			Recalculate();
		}
	}

	/// <summary>
	/// A button to select a box provider for your theme
	/// </summary>
	internal class BoxProviderButton : SmartUIElement
	{
		readonly ThemeBoxProvider theme;

		public BoxProviderButton(ThemeBoxProvider theme)
		{
			this.theme = theme;
			Width.Set(64, 0);
			Height.Set(64, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var target = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, target, ThemeHandler.ButtonColor);

			if (ThemeHandler.currentBoxProvider == theme)
				GUIHelper.DrawOutline(spriteBatch, target, GUIHelper.InvertColor(ThemeHandler.ButtonColor));

			target.Inflate(-12, -12);
			theme.DrawBox(spriteBatch, target, ThemeHandler.ButtonColor);

			if (IsMouseHovering && !Main.mouseLeft)
			{
				Tooltip.SetName(theme.Name);
				Tooltip.SetTooltip(theme.Description);
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ThemeHandler.SetBoxProvider(theme);
		}
	}

	/// <summary>
	/// A button to select an icon provider for your theme
	/// </summary>
	internal class IconProviderButton : SmartUIElement
	{
		readonly ThemeIconProvider theme;

		public IconProviderButton(ThemeIconProvider theme)
		{
			this.theme = theme;
			Width.Set(64, 0);
			Height.Set(64, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var target = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, target, ThemeHandler.ButtonColor);

			if (ThemeHandler.currentIconProvider == theme)
				GUIHelper.DrawOutline(spriteBatch, target, GUIHelper.InvertColor(ThemeHandler.ButtonColor));

			Texture2D tex = theme.GetIcon("ItemSpawner");
			spriteBatch.Draw(tex, target.Center.ToVector2(), null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);

			if (IsMouseHovering && !Main.mouseLeft)
			{
				Tooltip.SetName(theme.Name);
				Tooltip.SetTooltip(theme.Description);
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ThemeHandler.SetIconProvider(theme);
		}
	}
}