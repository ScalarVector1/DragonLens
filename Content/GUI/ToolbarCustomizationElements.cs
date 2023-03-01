using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class RemoveButton : UIElement
	{
		private readonly ToolButton parent;

		public RemoveButton(ToolButton parent)
		{
			this.parent = parent;

			Width.Set(16, 0);
			Height.Set(16, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			parent.parent.toolbar.toolList.Remove(parent.tool);
			UILoader.GetUIState<ToolbarState>().Refresh();
			UILoader.GetUIState<ToolbarState>().Customize();
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

	internal class AddButton : UIElement
	{
		public ToolbarElement parent;

		public Toolbar Toolbar => parent.toolbar;

		public AddButton(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void Click(UIMouseEvent evt)
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

	internal class RemoveToolbarButton : UIElement
	{
		public ToolbarElement parent;

		public Toolbar Toolbar => parent.toolbar;

		public RemoveToolbarButton(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void Click(UIMouseEvent evt)
		{
			ToolbarHandler.activeToolbars.Remove(Toolbar);
			UILoader.GetUIState<ToolbarState>().Refresh();
			UILoader.GetUIState<ToolbarState>().Customize();
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

	internal class DragButton : UIElement
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

		public override void MouseDown(UIMouseEvent evt)
		{
			dragging = true;
			draggedElement = parent;
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			dragging = false;
			parent.Refresh();
			parent.Customize();

			draggedElement = null;
		}

		public override void Update(GameTime gameTime)
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

				if (Main.MouseScreen.X / Main.screenWidth < 0.025f)
				{
					DraggedToolbar.relativePosition.X = 0;
					DraggedToolbar.orientation = Orientation.Vertical;
				}

				if (Main.MouseScreen.X / Main.screenWidth > 0.975f)
				{
					DraggedToolbar.relativePosition.X = 1;
					DraggedToolbar.orientation = Orientation.Vertical;
				}

				if (Main.MouseScreen.Y / Main.screenHeight < 0.025f)
				{
					DraggedToolbar.relativePosition.Y = 0;
					DraggedToolbar.orientation = Orientation.Horizontal;
				}

				if (Main.MouseScreen.Y / Main.screenHeight > 0.975f)
				{
					DraggedToolbar.relativePosition.Y = 1;
					DraggedToolbar.orientation = Orientation.Horizontal;
				}

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

	internal class HideOptionButton : UIElement
	{
		public ToolbarElement parent;

		public Toolbar Toolbar => parent.toolbar;

		public HideOptionButton(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void Click(UIMouseEvent evt)
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

	internal class NewBarButton : UIElement
	{
		public NewBarButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			ToolbarHandler.activeToolbars.Add(new Toolbar(new Vector2(0.5f, 0.6f), Orientation.Horizontal, Main.mapFullscreen ? AutomaticHideOption.NoMapScreen : AutomaticHideOption.Never));

			UILoader.GetUIState<ToolbarState>().Refresh();
			UILoader.GetUIState<ToolbarState>().Customize();
		}

		public override void Update(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawTarget = GetDimensions().ToRectangle();

			Helpers.GUIHelper.DrawBox(spriteBatch, drawTarget, ModContent.GetInstance<GUIConfig>().buttonColor);

			drawTarget.Inflate(-6, -16);
			Helpers.GUIHelper.DrawBox(spriteBatch, drawTarget, ModContent.GetInstance<GUIConfig>().buttonColor);

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

	internal class SaveLayoutButton : UIElement
	{
		public SaveLayoutButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			UILoader.GetUIState<ToolbarState>().FinishCustomize();
			ToolbarHandler.ExportToFile(Path.Join(Main.SavePath, "DragonLensLayouts", "Current"));

			CustomizeTool.customizing = false;

			Main.NewText("Layout saved!");
		}

		public override void Update(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

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

	internal class LoadLayoutButton : UIElement
	{
		public LoadLayoutButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			Main.NewText("To be implemented while file browsers are...");
		}

		public override void Update(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/LoadLayout").Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName("Load layout");
				Tooltip.SetTooltip("Load an existing layout");
			}

			base.Draw(spriteBatch);
		}
	}

	internal class VisualConfigButton : UIElement
	{
		public VisualConfigButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			Helpers.GUIHelper.OpenConfig(ModContent.GetInstance<GUIConfig>());
		}

		public override void Update(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

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

	internal class FunctionalConfigButton : UIElement
	{
		public FunctionalConfigButton()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			Helpers.GUIHelper.OpenConfig(ModContent.GetInstance<ToolConfig>());
		}

		public override void Update(GameTime gameTime)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Tools/Customize").Value;
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
