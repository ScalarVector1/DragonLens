using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	/// <summary>
	/// This element is the frontend display half for a Toolbar, displaying the buttons
	/// </summary>
	internal class ToolbarElement : SmartUIElement
	{
		/// <summary>
		/// The toolbar that this UI is tracking and should draw
		/// </summary>
		public Toolbar toolbar;
		public ToolbarDragger dragger;
		public List<ToolButton> buttons = new();

		public AddButton addButton;
		public RemoveToolbarButton removeButton;
		public HideOptionButton hideOptionButton;

		public HideTab hideButton;

		public Vector2 basePos;
		public Vector2 offset;
		public Vector2 offsetTarget;

		public Vector2 overlapAdjust;

		public bool beingDragged;
		public bool wasRefreshed;

		public ToolbarElement(Toolbar toolbar)
		{
			this.toolbar = toolbar;
			dragger = new(this);

			addButton = new AddButton(this);
			removeButton = new RemoveToolbarButton(this);
			hideOptionButton = new HideOptionButton(this);

			hideButton = new HideTab(this);

			Left.Set(0, toolbar.relativePosition.X);
			Top.Set(0, toolbar.relativePosition.Y);
		}

		/// <summary>
		/// Re-calculates which buttons this element should have based on the toolbar it's tracking
		/// </summary>
		public void Refresh()
		{
			Vector2 position = Vector2.Zero;

			if (toolbar.orientation == Orientation.Horizontal)
				position += new Vector2(36, 100 / 2 - 46 / 2);
			else
				position += new Vector2(100 / 2 - 46 / 2, 36);

			foreach (ToolButton button in buttons)
			{
				//button.pos
				button.Left.Set(position.X, 0);
				button.Top.Set(position.Y, 0);

				if (toolbar.orientation == Orientation.Horizontal)
					position.X += 50;
				else
					position.Y += 50;
			}

			if (toolbar.orientation == Orientation.Horizontal)
			{
				Width.Set(Math.Max(position.X + 34, 200), 0);
				Height.Set(100, 0);
			}
			else
			{
				Height.Set(Math.Max(position.Y + 34, 200), 0);
				Width.Set(100, 0);
			}

			UpdatePosition();
			wasRefreshed = true;
		}

		/// <summary>
		/// Populates this toolbar for the first time
		/// </summary>
		public void Populate()
		{
			RemoveAllChildren();

			Append(dragger); // Important this goes under everything so its last precedence.
							 // Sucks this has to be here but oh well. Terraria hates letting you re-order UI elements :/

			Vector2 position = Vector2.Zero;

			if (toolbar.orientation == Orientation.Horizontal)
				position += new Vector2(36, 100 / 2 - 46 / 2);
			else
				position += new Vector2(100 / 2 - 46 / 2, 36);

			foreach (Tool tool in toolbar.toolList)
			{
				ToolButton button = new(tool, position, this);
				buttons.Add(button);
				Append(button);

				if (toolbar.orientation == Orientation.Horizontal)
					position.X += 50;
				else
					position.Y += 50;
			}

			if (toolbar.orientation == Orientation.Horizontal)
			{
				Width.Set(Math.Max(position.X + 34, 200), 0);
				Height.Set(100, 0);
			}
			else
			{
				Height.Set(Math.Max(position.Y + 34, 200), 0);
				Width.Set(100, 0);
			}

			UpdatePosition();
			AddCollapseTab();
		}

		/// <summary>
		/// Calls position constraints together for convenience and while dragging
		/// </summary>
		public void UpdatePosition()
		{
			Left.Set(0, toolbar.relativePosition.X);
			Top.Set(0, toolbar.relativePosition.Y);

			PositionTab(addButton, 0.2f);
			PositionTab(removeButton, 0.5f);
			PositionTab(hideOptionButton, 0.8f);

			PositionTab(hideButton, 0.5f);

			Recalculate();
			UpdateTargetOffset();
			offset = offsetTarget;
			AdjustDimensions();

			Recalculate();

			basePos = GetDimensions().Position();
		}

		/// <summary>
		/// Calculates offsets for when a toolbar is collapsed
		/// </summary>
		public void UpdateTargetOffset()
		{
			switch (toolbar.CollapseDirection)
			{
				case CollapseDirection.Left:
					offsetTarget = new Vector2(toolbar.collapsed ? -62 : 0, 0);
					break;

				case CollapseDirection.Right:
					offsetTarget = new Vector2(toolbar.collapsed ? 62 : 0, 0);
					break;

				case CollapseDirection.Up:
					offsetTarget = new Vector2(0, toolbar.collapsed ? -62 : 0);
					break;

				case CollapseDirection.Down:
					offsetTarget = new Vector2(0, toolbar.collapsed ? 62 : 0);
					break;

				case CollapseDirection.Floating:
					offsetTarget = Vector2.Zero;
					break;
			}
		}

		/// <summary>
		/// Centers the toolbar based on it's orientation, and applies offsets for bars snapped to edges
		/// </summary>
		private void AdjustDimensions()
		{
			CalculatedStyle dims = GetDimensions();

			Left.Set(-dims.Width / 2, toolbar.relativePosition.X);
			Top.Set(-dims.Height / 2, toolbar.relativePosition.Y);

			switch (toolbar.CollapseDirection)
			{
				case CollapseDirection.Left:
					Left.Set(-19, 0);
					break;

				case CollapseDirection.Right:
					Left.Set(-92 + 11, 1);
					break;

				case CollapseDirection.Up:
					Top.Set(-19, 0);
					break;

				case CollapseDirection.Down:
					Top.Set(-92 + 11, 1);
					break;

				case CollapseDirection.Floating:
					break;
			}
		}

		/// <summary>
		/// Adds the collapse tab button
		/// </summary>
		private void AddCollapseTab()
		{
			if (toolbar.CollapseDirection == CollapseDirection.Floating) //floating bars dont get a collapse tab
				return;

			AddTabButton(hideButton, 0.5f);

			if (toolbar.orientation == Orientation.Horizontal)
			{
				hideButton.Width.Set(90, 0);
				hideButton.Left.Set(-45, 0.5f);
			}
			else
			{
				hideButton.Height.Set(90, 0);
				hideButton.Top.Set(-45, 0.5f);
			}
		}

		/// <summary>
		/// Adds a tab button for when customizing
		/// </summary>
		/// <param name="element">The tab to add</param>
		/// <param name="offset">How far along the bar it should be as a percentage</param>
		private void AddTabButton(UIElement element, float offset)
		{
			PositionTab(element, offset);
			Append(element);
		}

		public override void Update(GameTime gameTime)
		{
			if (toolbar is null)
			{
				FirstTimeSetupSystem.PanicToSetup();
				return;
			}

			if (!toolbar.Invisible)
			{
				Left.Set(basePos.X + offset.X, 0);
				Top.Set(basePos.Y + offset.Y, 0);

				//For the opening and closing animation
				if (offset != offsetTarget)
				{
					if (Vector2.Distance(offset, offsetTarget) < 0.1f)
						offset = offsetTarget;
					else
						offset += (offsetTarget - offset) * 0.08f;
				}

				if (IsMouseHovering)
					Main.LocalPlayer.mouseInterface = true;

				if (!wasRefreshed)
					base.Update(gameTime);
			}
			else //This is here to prevent invisible toolbars from interfering with mouse interactions, since terraria is terrible about that
			{
				Left.Set(0, -1);
				Top.Set(0, -1);
			}

			wasRefreshed = false;
		}

		/// <summary>
		/// Positions a tab relative to the toolbar
		/// </summary>
		/// <param name="element"></param>
		/// <param name="offset"></param>
		public void PositionTab(UIElement element, float offset)
		{
			element.Width.Set(60, 0);
			element.Height.Set(60, 0);

			if (toolbar.orientation == Orientation.Horizontal)
			{
				element.Left.Set(-30, offset);

				if (toolbar.relativePosition.Y > 0.5f)
					element.Top.Set(-35, 0f);
				else
					element.Top.Set(-25, 1f);
			}
			else
			{
				if (toolbar.relativePosition.X < 0.5f)
					element.Left.Set(-25, 1f);
				else
					element.Left.Set(-35, 0f);

				element.Top.Set(-30, offset);
			}
		}

		/// <summary>
		/// Trigger customization mode for this element
		/// </summary>
		public void Customize()
		{
			AddTabButton(addButton, 0.2f);
			AddTabButton(removeButton, 0.5f);
			AddTabButton(hideOptionButton, 0.8f);

			RemoveChild(hideButton);

			foreach (UIElement child in Children)
			{
				if (child is ToolButton)
					(child as ToolButton).Customize();
			}
		}

		/// <summary>
		/// Leave customization mode for this element
		/// </summary>
		public void FinishCustomize()
		{
			RemoveChild(addButton);
			RemoveChild(removeButton);
			RemoveChild(hideOptionButton);

			if (toolbar.CollapseDirection != CollapseDirection.Floating)
				AddCollapseTab();

			foreach (UIElement child in Children)
			{
				if (child is ToolButton)
					(child as ToolButton).FinishCustomize();
			}

			Refresh();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (toolbar is null)
			{
				FirstTimeSetupSystem.PanicToSetup();
				return;
			}

			if (!toolbar.Invisible)
			{
				var bgTarget = GetDimensions().ToRectangle();
				bgTarget.Inflate(-19, -19);

				Helpers.GUIHelper.DrawBox(spriteBatch, bgTarget, ThemeHandler.BackgroundColor);

				base.Draw(spriteBatch);
				Recalculate();
			}
		}
	}

	/// <summary>
	/// A button which can be used to activate a tool when clicked on
	/// </summary>
	internal class ToolButton : SmartUIElement
	{
		public Tool tool;

		public ToolbarElement parent;

		public ToolButton(Tool tool, Vector2 pos, ToolbarElement parent)
		{
			this.tool = tool;

			Left.Set(pos.X, 0);
			Top.Set(pos.Y, 0);
			Width.Set(46, 0);
			Height.Set(46, 0);
			this.parent = parent;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!PermissionHandler.CanUseTools(Main.LocalPlayer))
			{
				Main.NewText(LocalizationHelper.GetText("Permission.NotAdmin"), Color.Red);
				return;
			}

			if (!parent.toolbar.collapsed && !CustomizeTool.customizing)
				tool.OnActivate();
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			if (!PermissionHandler.CanUseTools(Main.LocalPlayer))
			{
				Main.NewText(LocalizationHelper.GetText("Permission.NotAdmin"), Color.Red);
				return;
			}

			if (!parent.toolbar.collapsed && tool.HasRightClick && !CustomizeTool.customizing)
				tool.OnRightClick();
		}

		public void Customize()
		{
			Append(new ToolButtonDragger(this));

			var rb = new RemoveButton(this);
			rb.Left.Set(4, 0);
			rb.Top.Set(4, 0);
			Append(rb);
		}

		public void FinishCustomize()
		{
			RemoveAllChildren();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (parent.toolbar.collapsed && parent.toolbar.CollapseDirection == CollapseDirection.Floating)
				return;

			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			var innerRect = GetDimensions().ToRectangle();
			innerRect.Inflate(-4, -4);

			tool.DrawIcon(spriteBatch, innerRect);

			if (IsMouseHovering && !parent.toolbar.collapsed)
			{
				Tooltip.SetName(tool.DisplayName);
				Tooltip.SetTooltip(tool.Description);
			}

			base.Draw(spriteBatch);
		}
	}

	/// <summary>
	/// The tab the user can click on to hide a hotbar
	/// </summary>
	internal class HideTab : SmartUIElement
	{
		public ToolbarElement parent;

		public Toolbar Toolbar => parent.toolbar;

		public HideTab(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (CustomizeTool.customizing)
				return;

			Toolbar.collapsed = !Toolbar.collapsed;

			parent.UpdateTargetOffset();
			parent.Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (CustomizeTool.customizing)
				return;

			Texture2D tex = Assets.GUI.TabWide.Value;

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, ThemeHandler.ButtonColor, rotation, new Vector2(45, 0), 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}
}