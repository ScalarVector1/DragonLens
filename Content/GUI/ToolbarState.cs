using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.GameInput;
using Terraria.ID;
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

		public override void SafeUpdate(GameTime gameTime)
		{
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

		/// <summary>
		/// Handles updating the logic for the collapse animation of toolbars
		/// </summary>
		public void UpdateCollapse()
		{
			toolbarElements.ForEach(n => n.UpdateTargetOffset());
		}

		/// <summary>
		/// Triggers the toolbar state to enter customize mode
		/// </summary>
		public void Customize()
		{
			foreach (UIElement child in Children)
			{
				if (child is ToolbarElement)
					(child as ToolbarElement).Customize();
			}

			var addButton = new NewBarButton();
			addButton.Left.Set(-100, 0.5f);
			addButton.Top.Set(-100, 0.5f);
			Append(addButton);

			var styleButton = new VisualConfigButton();
			styleButton.Left.Set(-100, 0.5f);
			styleButton.Top.Set(-50, 0.5f);
			Append(styleButton);

			var functionButton = new FunctionalConfigButton();
			functionButton.Left.Set(-100, 0.5f);
			functionButton.Top.Set(0, 0.5f);
			Append(functionButton);

			var loadButton = new LoadLayoutButton();
			loadButton.Left.Set(-100, 0.5f);
			loadButton.Top.Set(50, 0.5f);
			Append(loadButton);

			var saveButton = new SaveLayoutButton();
			saveButton.Left.Set(-100, 0.5f);
			saveButton.Top.Set(100, 0.5f);
			Append(saveButton);
		}

		/// <summary>
		/// Ends customization and removes all customization elements
		/// </summary>
		public void FinishCustomize()
		{
			List<UIElement> toRemove = new();

			foreach (UIElement child in Children)
			{
				if (child is ToolbarElement)
					(child as ToolbarElement).FinishCustomize();

				if (child is NewBarButton || child is SaveLayoutButton || child is LoadLayoutButton || child is VisualConfigButton || child is FunctionalConfigButton)
					toRemove.Add(child);
			}

			foreach (UIElement child in toRemove)
			{
				RemoveChild(child);
			}
		}

		/// <summary>
		/// Draws the black background while customizing to emphasize the customization process
		/// </summary>
		/// <param name="spriteBatch"></param>
		public override void Draw(SpriteBatch spriteBatch)
		{
			if (CustomizeTool.customizing)
			{
				Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
				spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), null, Color.Black * 0.75f);
			}

			base.Draw(spriteBatch);
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
			int savedX = Main.mouseX; //vanilla uses something strange for mouse pos here so preserve it
			int savedY = Main.mouseY;

			PlayerInput.SetZoom_UI();

			// Have to check if _drawInterfaceGameTime is null otherwise there is a crash with world gen preview mod
			UILoader.GetUIState<ToolbarState>()?.UserInterface?.Update(Main._drawInterfaceGameTime ?? new GameTime());
			UILoader.GetUIState<ToolBrowser>()?.UserInterface?.Update(Main._drawInterfaceGameTime ?? new GameTime()); //We update/draw the tool browser here too to ease customization

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
			UILoader.GetUIState<ToolbarState>()?.Draw(Main.spriteBatch);

			if (UILoader.GetUIState<ToolBrowser>()?.Visible ?? false)
				UILoader.GetUIState<ToolBrowser>()?.Draw(Main.spriteBatch);

			UILoader.GetUIState<Tooltip>()?.Draw(Main.spriteBatch);
			Main.spriteBatch.End();

			Main.mouseX = savedX;
			Main.mouseY = savedY;
		}

		public override void PostUpdateEverything()
		{
			if (Main.netMode == NetmodeID.Server)
				return;

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