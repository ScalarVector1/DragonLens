using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class ToolbarState : SmartUIState
	{
		private static readonly List<ToolbarElement> toolbarElements = new();

		public static ReadOnlyCollection<ToolbarElement> toolbars = toolbarElements.AsReadOnly();

		public override bool Visible => true;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			Recalculate();
		}

		/// <summary>
		/// Refresh the entire HUD with new toolbars, like when you would want to load a template
		/// </summary>
		public void Refresh()
		{
			RemoveAllChildren();
			toolbarElements.Clear();

			foreach (Toolbar toolbar in ToolbarHandler.activeToolbars)
			{
				if (toolbar is null)
					continue;

				var element = new ToolbarElement(toolbar);
				element.Refresh();

				toolbarElements.Add(element);
				Append(element);
			}

			Recalculate();

			//We want to shove based on size order, larger toolbars should be shoved last
			toolbarElements.Sort((a, b) => a.toolbar.toolList.Count > b.toolbar.toolList.Count ? 1 : -1);

			foreach (ToolbarElement element in toolbarElements)
			{
				element.SmartShove();
			}

			if (CustomizeTool.customizing)
				Customize();

			Recalculate();
		}

		public void Customize()
		{
			foreach (UIElement child in Children)
			{
				if (child is ToolbarElement)
					(child as ToolbarElement).Customize();
			}

			var addButton = new NewBarButton();
			addButton.Left.Set(-24, 0.5f);
			addButton.Top.Set(-24, 0.5f);
			Append(addButton);

			var saveButton = new SaveLayoutButton();
			saveButton.Left.Set(34, 0.5f);
			saveButton.Top.Set(-24, 0.5f);
			Append(saveButton);

			var loadButton = new LoadLayoutButton();
			loadButton.Left.Set(-84, 0.5f);
			loadButton.Top.Set(-24, 0.5f);
			Append(loadButton);

			var styleButton = new VisualConfigButton();
			styleButton.Left.Set(34, 0.5f);
			styleButton.Top.Set(30, 0.5f);
			Append(styleButton);

			var functionButton = new FunctionalConfigButton();
			functionButton.Left.Set(-84, 0.5f);
			functionButton.Top.Set(30, 0.5f);
			Append(functionButton);
		}

		public void FinishCustomize()
		{
			List<UIElement> toRemove = new();

			foreach (UIElement child in Children)
			{
				if (child is ToolbarElement)
					(child as ToolbarElement).FinishCustomize();

				if (child is NewBarButton || child is SaveLayoutButton || child is LoadLayoutButton || child is VisualConfigButton || child is FunctionalConfigButton) //TODO: Make these buttons have a common parent class?
					toRemove.Add(child);
			}

			foreach (UIElement child in toRemove)
			{
				RemoveChild(child);
			}
		}
	}

	/// <summary>
	/// Handles refreshing the toolbar layout when appropriate
	/// </summary>
	internal class ToolbarStateHandler : ModSystem
	{
		int initialTimer = 0;
		float oldUIScale;

		public override void Load()
		{
			Main.OnResolutionChanged += RefreshUI;
			Main.OnPostFullscreenMapDraw += DrawToolbars;
		}

		public override void Unload()
		{
			Main.OnResolutionChanged -= RefreshUI;
			Main.OnPostFullscreenMapDraw -= DrawToolbars;
		}

		private void RefreshUI(Vector2 newSize)
		{
			UILoader.GetUIState<ToolbarState>().Refresh();
		}

		private void DrawToolbars(Vector2 arg1, float arg2)
		{
			UILoader.GetUIState<ToolbarState>().UserInterface.Update(Main._drawInterfaceGameTime);
			UILoader.GetUIState<ToolBrowser>().UserInterface.Update(Main._drawInterfaceGameTime); //We update/draw the tool browser here too to ease customization

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
			UILoader.GetUIState<ToolbarState>().Draw(Main.spriteBatch);

			if (UILoader.GetUIState<ToolBrowser>().Visible)
				UILoader.GetUIState<ToolBrowser>().Draw(Main.spriteBatch);

			UILoader.GetUIState<Tooltip>().Draw(Main.spriteBatch);
			Main.spriteBatch.End();
		}

		public override void PostUpdateEverything()
		{
			if (Main.UIScale != oldUIScale)
			{
				UILoader.GetUIState<ToolbarState>().Refresh();
				oldUIScale = Main.UIScale;
			}

			if (Main.gameMenu)
				initialTimer = 0;
			else
				initialTimer++;

			if (initialTimer == 30)
			{
				UILoader.GetUIState<ToolbarState>().Refresh();

				UILoader.GetUIState<FirstTimeLayoutPresetMenu>().RemoveAllChildren();
				UILoader.GetUIState<FirstTimeLayoutPresetMenu>().OnInitialize();
			}
		}
	}
}
