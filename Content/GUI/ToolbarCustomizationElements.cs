using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using System.IO;
using Terraria.UI;
namespace DragonLens.Content.GUI
{
	internal class RemoveButton : SmartUIElement
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("Remove tool");
				Tooltip.SetTooltip("You can always re-add it from the add tool menu (green tab)!");
			}

			base.Draw(spriteBatch);
		}
	}

	internal class AddButton : SmartUIElement
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("Add tool");
				Tooltip.SetTooltip("Click to add tools to this toolbar");
			}

			base.Draw(spriteBatch);
		}
	}

	internal class RemoveToolbarButton : SmartUIElement
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("Remove toolbar");
				Tooltip.SetTooltip("Remove this entire toolbar!");
			}

			base.Draw(spriteBatch);
		}
	}

	internal class DragButton : SmartUIElement
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("Move toolbar");
				Tooltip.SetTooltip("Click and drag to re-position this toolbar. The toolbar will automatically snap to the edges of the screen if close enough. Right click while dragging to rotate the toolbar.");
			}

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
				string hideOption = Toolbar.automaticHideOption switch
				{
					AutomaticHideOption.Never => "Normal",
					AutomaticHideOption.InventoryOpen => "Inventory closed only",
					AutomaticHideOption.InventoryClosed => "Inventory open only",
					AutomaticHideOption.NoMapScreen => "Map only",
					_ => "ERROR"
				};

				string hideTip = Toolbar.automaticHideOption switch
				{
					AutomaticHideOption.Never => "Always visible during normal gameplay",
					AutomaticHideOption.InventoryOpen => "Only visible if your inventory is closed",
					AutomaticHideOption.InventoryClosed => "Only visible if your inventory is open",
					AutomaticHideOption.NoMapScreen => "Visible on the map -- note: tools which open other windows will require you to close the map to use them!",
					_ => "ERROR"
				};

				string nextOption = Toolbar.automaticHideOption switch
				{
					AutomaticHideOption.Never => "Inventory closed only",
					AutomaticHideOption.InventoryOpen => "Inventory open only",
					AutomaticHideOption.InventoryClosed => "Map only",
					AutomaticHideOption.NoMapScreen => "Normal",
					_ => "ERROR"
				};

				Tooltip.SetName($"Auto-hide: {hideOption}");
				Tooltip.SetTooltip($"Allows you to set visibility rules for this toolbar. NEWBLOCK {hideTip} NEWBLOCK Clicking will change to: {nextOption}");
			}

			base.Draw(spriteBatch);
		}
	}

	internal class NewBarButton : SmartUIElement
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("New toolbar");
				Tooltip.SetTooltip("Create a brand new empty toolbar!");
			}

			base.Draw(spriteBatch);
		}
	}

	internal class SaveLayoutButton : SmartUIElement
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

			Main.NewText("Layout saved!");
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("Save layout");
				Tooltip.SetTooltip("Finish customizing and save your layout");
			}

			base.Draw(spriteBatch);
		}
	}

	internal class LoadLayoutButton : SmartUIElement
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("Load layout");
				Tooltip.SetTooltip("Load an existing layout");
			}
		}
	}

	internal class VisualConfigButton : SmartUIElement
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("Change style");
				Tooltip.SetTooltip("Open the configuration to change the visual style of the GUI");
			}

			base.Draw(spriteBatch);
		}
	}

	internal class FunctionalConfigButton : SmartUIElement
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

			if (IsMouseHovering)
			{
				Tooltip.SetName("Tool options");
				Tooltip.SetTooltip("Open the configuration to change tool functionality/defaults");
			}

			base.Draw(spriteBatch);
		}
	}
}