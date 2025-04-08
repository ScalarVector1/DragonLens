using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Helpers;
using System.Linq;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	// used for easier localization code
	internal abstract class LocalizedCustomizationElement : SmartUIElement
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

	/// <summary>
	/// Individual tool button remove button
	/// </summary>
	internal class RemoveButton : LocalizedCustomizationElement
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
			Texture2D tex = Assets.GUI.Remove.Value;
			spriteBatch.Draw(tex, GetDimensions().Position(), Color.White);

			base.Draw(spriteBatch);
		}
	}

	/// <summary>
	/// Individual tool button dragger
	/// </summary>
	internal class ToolButtonDragger : LocalizedCustomizationElement
	{
		private readonly ToolButton parent;

		public bool dragging;
		public static Vector2 dragOffset;

		ToolbarElement ParentToolbar => parent.parent;

		public ToolButtonDragger(ToolButton parent)
		{
			this.parent = parent;

			Width.Set(parent.Width.Pixels, 0);
			Height.Set(parent.Height.Pixels, 0);
			Left.Set(0, 0);
			Top.Set(0, 0);
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			dragging = true;
			dragOffset = parent.GetDimensions().Center() - Main.MouseScreen;
		}

		public override void SafeMouseUp(UIMouseEvent evt)
		{
			dragging = false;
			dragOffset = Vector2.Zero;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (dragging)
			{
				int index = ParentToolbar.toolbar.toolList.IndexOf(parent.tool);

				if (ParentToolbar.toolbar.orientation == Orientation.Horizontal)
				{
					if (Main.MouseScreen.X > parent.GetDimensions().X + parent.GetDimensions().Width + 8 && index < parent.parent.toolbar.toolList.Count - 1)
						SwapTools(index + 1);
					else if (Main.MouseScreen.X < parent.GetDimensions().X - 8 && index > 0)
						SwapTools(index - 1);
				}
				else
				{
					if (Main.MouseScreen.Y > parent.GetDimensions().Y + parent.GetDimensions().Height + 8 && index < parent.parent.toolbar.toolList.Count - 1)
						SwapTools(index + 1);
					else if (Main.MouseScreen.Y < parent.GetDimensions().Y - 8 && index > 0)
						SwapTools(index - 1);
				}
			}
			else
			{
				Width.Set(parent.Width.Pixels, 0);
				Height.Set(parent.Height.Pixels, 0);
				Left.Set(0, 0);
				Top.Set(0, 0);
				Recalculate();
			}
		}

		private void SwapTools(int index2)
		{
			int index = ParentToolbar.toolbar.toolList.IndexOf(parent.tool);

			Core.Systems.ToolSystem.Tool otherTool = ParentToolbar.toolbar.toolList[index2];
			UIElement other = ParentToolbar.Children.FirstOrDefault(n => n is ToolButton && (n as ToolButton).tool == otherTool);

			(ParentToolbar.toolbar.toolList[index2], ParentToolbar.toolbar.toolList[index]) = (ParentToolbar.toolbar.toolList[index], ParentToolbar.toolbar.toolList[index2]);

			StyleDimension temp = parent.Left;
			StyleDimension temp2 = parent.Top;

			parent.Left.Set(other.Left.Pixels, other.Left.Percent);
			parent.Top.Set(other.Top.Pixels, other.Top.Percent);

			other.Left.Set(temp.Pixels, temp.Percent);
			other.Top.Set(temp2.Pixels, temp2.Percent);

			parent.Recalculate();
			other.Recalculate();
		}
	}

	/// <summary>
	/// Full toolbar dragger element
	/// </summary>
	internal class ToolbarDragger : LocalizedCustomizationElement
	{
		public ToolbarElement parent;

		public bool debounce;

		public bool dragging;
		public Vector2 dragOffset;
		public ToolbarElement draggedElement;

		public Toolbar Toolbar => parent.toolbar;
		public Toolbar DraggedToolbar => draggedElement.toolbar;

		public ToolbarDragger(ToolbarElement parent)
		{
			this.parent = parent;

			Width.Set(parent.Width.Pixels, 0);
			Height.Set(parent.Height.Pixels, 0);
			Left.Set(0, 0);
			Top.Set(0, 0);

		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			dragging = true;
			draggedElement = parent;
			parent.beingDragged = true;
			dragOffset = parent.GetDimensions().Center() - Main.MouseScreen;
			
			draggedElement.Refresh();
		}

		public override void SafeMouseUp(UIMouseEvent evt)
		{
			dragging = false;
			parent.beingDragged = false;

			parent.Refresh();

			dragOffset = Vector2.Zero;
			draggedElement = null;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (debounce && !Main.mouseRight && Main.mouseRightRelease)
				debounce = false;

			// This logic exists because this dragger is always present, not just when customizing. Has to be done due to
			// append order effecting click priority :/
			if (!CustomizeTool.customizing)
			{
				Width.Set(0, 0);
				Height.Set(0, 0);
				return;
			}

			if (dragging && draggedElement != null)
			{
				if (Main.mouseRight && !debounce)
				{
					if (DraggedToolbar.orientation == Orientation.Horizontal)
					{
						DraggedToolbar.orientation = Orientation.Vertical;
						draggedElement.Refresh();
						debounce = true;
					}
					else
					{
						DraggedToolbar.orientation = Orientation.Horizontal;
						draggedElement.Refresh();
						debounce = true;
					}
				}

				float relW = draggedElement.Width.Pixels / Main.screenWidth;
				float relH = draggedElement.Height.Pixels / Main.screenHeight;

				DraggedToolbar.relativePosition.X = MathHelper.Clamp((Main.MouseScreen.X + dragOffset.X) / Main.screenWidth, 0 + relW / 2, 1 - relW / 2);
				DraggedToolbar.relativePosition.Y = MathHelper.Clamp((Main.MouseScreen.Y + dragOffset.Y) / Main.screenHeight, 0 + relH / 2, 1 - relH / 2);

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

				if (DraggedToolbar.CollapseDirection != DraggedToolbar.lastKnownCollapse)
				{
					draggedElement.Refresh();
					DraggedToolbar.lastKnownCollapse = DraggedToolbar.CollapseDirection;
				}

				draggedElement.Recalculate();
				draggedElement.UpdatePosition();
			}
			else
			{
				Width.Set(parent.Width.Pixels, 0);
				Height.Set(parent.Height.Pixels, 0);
				Left.Set(0, 0);
				Top.Set(0, 0);
				Recalculate();
			}
		}
	}

	/// <summary>
	/// Tab the user can press to add a tool to a toolbar
	/// </summary>
	internal class AddButton : LocalizedCustomizationElement
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
			Texture2D tex = Assets.GUI.SpecialTabs.Value;
			var source = new Rectangle(0, 0, 60, 24);

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), source, new Color(100, 200, 100), rotation, new Vector2(30, 0), 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	/// <summary>
	/// Tab the user can press to remove a toolbar
	/// </summary>
	internal class RemoveToolbarButton : LocalizedCustomizationElement
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
			Texture2D tex = Assets.GUI.SpecialTabs.Value;
			var source = new Rectangle(60, 0, 60, 24);

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), source, new Color(200, 100, 100), rotation, new Vector2(30, 0), 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}

	/// <summary>
	/// Tab the user can press to toggle the hide state
	/// </summary>
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
			Texture2D tex = Assets.GUI.SpecialTabs.Value;
			var source = new Rectangle(120, 0, 60, 24);

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), source, new Color(200, 200, 100), rotation, new Vector2(30, 0), 1, 0, 0);

			if (IsMouseHovering)
			{
				string hideOption = LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.HideOptionButton.{Toolbar.automaticHideOption}.Name");

				string hideTip = LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.HideOptionButton.{Toolbar.automaticHideOption}.Tip");

				var nextOptionEnum = (AutomaticHideOption)(((int)Toolbar.automaticHideOption + 1) % 4);
				string nextOption = LocalizationHelper.GetGUIText($"ToolbarCustomizationElements.HideOptionButton.{nextOptionEnum}.Name");

				Tooltip.SetName(LocalizationHelper.GetGUIText("ToolbarCustomizationElements.HideOptionButton.Name", hideOption));
				Tooltip.SetTooltip(LocalizationHelper.GetGUIText("ToolbarCustomizationElements.HideOptionButton.Tooltip", hideTip, nextOption));
			}

			base.Draw(spriteBatch);
		}
	}
}