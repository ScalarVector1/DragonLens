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
	// used for easier localization code
	internal abstract class ToolbarCustomizationElement : SmartUIElement
	{
		public override void Draw(SpriteBatch spriteBatch)
		{
			if (IsMouseHovering)
			{
				Tooltip.SetName(LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.{GetType().Name}.Name"));
				Tooltip.SetTooltip(LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.{GetType().Name}.Tooltip"));
			}

			base.Draw(spriteBatch);
		}
	}

	internal class RemoveButton : ToolbarCustomizationElement
	{
		private readonly ToolButton parent;

		public RemoveButton(ToolButton parent)
		{
			this.parent = parent;

			Width.Set(16, 0);
			Height.Set(16, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			parent.parent.toolbar.toolList.Remove(parent.tool);
			UILoader.GetUIState<ToolbarState>().Refresh();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Remove").Value;
			spriteBatch.Draw(tex, GetDimensions().Position(), Color.White);

			base.Draw(spriteBatch);
		}
	}

	internal class AddButton : ToolbarCustomizationElement
	{
		public ToolbarElement parent;

		public Toolbar Toolbar => parent.toolbar;

		public AddButton(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ToolBrowser.OpenForToolbar(parent);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Tab").Value;

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.LimeGreen, rotation, tex.Size() / 2f, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class RemoveToolbarButton : ToolbarCustomizationElement
	{
		public ToolbarElement parent;

		public Toolbar Toolbar => parent.toolbar;

		public RemoveToolbarButton(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ToolbarHandler.activeToolbars.Remove(Toolbar);
			UILoader.GetUIState<ToolbarState>().Refresh();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Tab").Value;

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Red, rotation, tex.Size() / 2f, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class DragButton : ToolbarCustomizationElement
	{
		public ToolbarElement parent;

		public static bool dragging;
		public static ToolbarElement draggedElement;

		public Toolbar Toolbar => parent.toolbar;
		public Toolbar DraggedToolbar => draggedElement.toolbar;

		public DragButton(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			dragging = true;
			draggedElement = parent;
			parent.beingDragged = true;
		}

		public override void SafeMouseUp(UIMouseEvent evt)
		{
			dragging = false;
			parent.Refresh();
			parent.Customize();
			parent.beingDragged = false;

			draggedElement = null;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (dragging && draggedElement != null)
			{
				if (Main.mouseRight && Main.mouseRightRelease)
				{
					if (DraggedToolbar.orientation == Orientation.Horizontal)
					{
						DraggedToolbar.orientation = Orientation.Vertical;
						Main.mouseRightRelease = false; //failsafe for slow updates
					}
					else
					{
						DraggedToolbar.orientation = Orientation.Horizontal;
						Main.mouseRightRelease = false;
					}
				}

				float relW = draggedElement.Width.Pixels / Main.screenWidth;
				float relH = draggedElement.Height.Pixels / Main.screenHeight;

				DraggedToolbar.relativePosition.X = MathHelper.Clamp(Main.MouseScreen.X / Main.screenWidth, 0 + relW / 2, 1 - relW / 2);
				DraggedToolbar.relativePosition.Y = MathHelper.Clamp(Main.MouseScreen.Y / Main.screenHeight, 0 + relH / 2, 1 - relH / 2);

				if (Main.MouseScreen.X / Main.screenWidth < 0.025f && draggedElement.Width.Pixels < Main.screenHeight)
				{
					DraggedToolbar.relativePosition.X = 0;
					DraggedToolbar.orientation = Orientation.Vertical;
				}
				else if (Main.MouseScreen.X / Main.screenWidth > 0.975f && draggedElement.Width.Pixels < Main.screenHeight)
				{
					DraggedToolbar.relativePosition.X = 1;
					DraggedToolbar.orientation = Orientation.Vertical;
				}
				else if (Main.MouseScreen.Y / Main.screenHeight < 0.025f && draggedElement.Height.Pixels < Main.screenWidth)
				{
					DraggedToolbar.relativePosition.Y = 0;
					DraggedToolbar.orientation = Orientation.Horizontal;
				}
				else if (Main.MouseScreen.Y / Main.screenHeight > 0.975f && draggedElement.Height.Pixels < Main.screenWidth)
				{
					DraggedToolbar.relativePosition.Y = 1;
					DraggedToolbar.orientation = Orientation.Horizontal;
				}

				DraggedToolbar.collapsed = false;
				draggedElement.Refresh();
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Tab").Value;

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Blue, rotation, tex.Size() / 2f, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class HideOptionButton : SmartUIElement
	{
		public ToolbarElement parent;

		public Toolbar Toolbar => parent.toolbar;

		public HideOptionButton(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			Toolbar.automaticHideOption++;

			if (Toolbar.automaticHideOption > AutomaticHideOption.NoMapScreen)
				Toolbar.automaticHideOption = AutomaticHideOption.Never;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Tab").Value;

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Yellow, rotation, tex.Size() / 2f, 1, 0, 0);

			if (IsMouseHovering)
			{
				string hideOption = LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.HideOptionButton.{Toolbar.automaticHideOption.ToString()}.Name");

				string hideTip = LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.HideOptionButton.{Toolbar.automaticHideOption.ToString()}.Tip");

				var nextOptionEnum = (AutomaticHideOption)(((int)Toolbar.automaticHideOption + 1) % 4);
				string nextOption = LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.HideOptionButton.{nextOptionEnum.ToString()}.Name");

				Tooltip.SetName(LocalizationHelper.GetGUIText("ToolbarCustomizationElements.HideOptionButton.Name", hideOption));
				Tooltip.SetTooltip(LocalizationHelper.GetGUIText("ToolbarCustomizationElements.HideOptionButton.Tooltip", hideTip, nextOption));
			}

			base.Draw(spriteBatch);
		}
	}

	internal class NewBarButton : ToolbarCustomizationElement
	{
		public NewBarButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

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

			Helpers.GUIHelper.DrawBox(spriteBatch, drawTarget, ThemeHandler.ButtonColor);

			drawTarget.Inflate(-6, -16);
			Helpers.GUIHelper.DrawBox(spriteBatch, drawTarget, ThemeHandler.ButtonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/NewBar").Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class SaveLayoutButton : ToolbarCustomizationElement
	{
		public SaveLayoutButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

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
		public LoadLayoutButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

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
		public VisualConfigButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

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
		public FunctionalConfigButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

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