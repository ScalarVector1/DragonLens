using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	/// <summary>
	/// This element is the frontend display half for a Toolbar, displaying the buttons
	/// </summary>
	internal class ToolbarElement : UIElement
	{
		/// <summary>
		/// The toolbar that this UI is tracking and should draw
		/// </summary>
		public Toolbar toolbar;

		public Vector2 basePos;
		public Vector2 offset;
		public Vector2 offsetTarget;

		public Vector2 overlapAdjust;

		public bool beingDragged;

		public ToolbarElement(Toolbar toolbar)
		{
			this.toolbar = toolbar;

			Left.Set(0, toolbar.relativePosition.X);
			Top.Set(0, toolbar.relativePosition.Y);
		}

		/// <summary>
		/// Re-calculates which buttons this element should have based on the toolbar it's tracking
		/// </summary>
		public void Refresh()
		{
			if (toolbar is null) // We loaded a null toolbar, panic!
			{
				FirstTimeSetupSystem.PanicToSetup();
				return;
			}

			RemoveAllChildren();

			Vector2 position = Vector2.Zero;

			if (toolbar.orientation == Orientation.Horizontal)
				position += new Vector2(32, 92 / 2 - 46 / 2);
			else
				position += new Vector2(92 / 2 - 46 / 2, 32);

			foreach (Tool tool in toolbar.toolList)
			{
				ToolButton button = new(tool, position, this);
				Append(button);

				if (toolbar.orientation == Orientation.Horizontal)
					position.X += 50;
				else
					position.Y += 50;
			}

			if (toolbar.orientation == Orientation.Horizontal)
			{
				Width.Set(Math.Max(position.X + 26, 200), 0);
				Height.Set(92, 0);
			}
			else
			{
				Height.Set(Math.Max(position.Y + 26, 200), 0);
				Width.Set(92, 0);
			}

			Recalculate();
			UpdateTargetOffset();
			offset = offsetTarget;
			AdjustDimensions();
			AddCollapseTab();

			Recalculate();

			basePos = GetDimensions().Position();
		}

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
					Left.Set(-15, 0);
					break;

				case CollapseDirection.Right:
					Left.Set(-92 + 15, 1);
					break;

				case CollapseDirection.Up:
					Top.Set(-15, 0);
					break;

				case CollapseDirection.Down:
					Top.Set(-92 + 15, 1);
					break;

				case CollapseDirection.Floating:
					break;
			}
		}

		/// <summary>
		/// Shoves this toolbar over to ensure it isnt intersecting any other toolbars
		/// </summary>
		public void SmartShove()
		{
			return; //Temporarily disabled...

			overlapAdjust = Vector2.Zero;

			foreach (ToolbarElement potentialIntersect in ToolbarState.toolbars)
			{
				if (potentialIntersect == this) //Cant overlap itself
					continue;

				if (toolbar.automaticHideOption != potentialIntersect.toolbar.automaticHideOption) //Overlaps with different hide options are allowed
					continue;

				var thisDims = GetDimensions().ToRectangle();
				var thatDims = potentialIntersect.GetDimensions().ToRectangle();

				if (thisDims.Intersects(thatDims))
				{
					Main.NewText($"Intersection detected! Your toolbars are being automatically shifted to prevent overlapping bars...");

					if (toolbar.orientation == Orientation.Horizontal)
					{
						if (thisDims.X > thatDims.X)
							Left.Set(thatDims.X + thatDims.Width, 0);
						else
							Left.Set(thatDims.X - thisDims.Width, 0);

						Recalculate();
						toolbar.relativePosition.X = GetDimensions().Center().X / Main.screenWidth;
					}
					else
					{
						if (thisDims.Y > thatDims.Y)
							Top.Set(thatDims.Y + thatDims.Height, 0);
						else
							Top.Set(thatDims.Y - thisDims.Height, 0);

						Recalculate();
						toolbar.relativePosition.Y = GetDimensions().Center().Y / Main.screenHeight;
					}
				}
			}

			Refresh();
		}

		/// <summary>
		/// Adds the collapse tab button
		/// </summary>
		private void AddCollapseTab()
		{
			var collapseButton = new HideTab(this);
			collapseButton.OnClick += (UIMouseEvent mouseEvent, UIElement element) => toolbar.collapsed = !toolbar.collapsed;

			AddTabButton(collapseButton, 0.5f);

			if (toolbar.orientation == Orientation.Horizontal)
			{
				collapseButton.Width.Set(90, 0);
				collapseButton.Left.Set(-45, 0.5f);
			}
			else
			{
				collapseButton.Height.Set(90, 0);
				collapseButton.Top.Set(-45, 0.5f);
			}
		}

		private void AddTabButton(UIElement element, float offset, int size = 30)
		{
			element.Width.Set(30, 0);
			element.Height.Set(30, 0);

			if (toolbar.orientation == Orientation.Horizontal)
			{
				element.Left.Set(-15, offset);

				if (toolbar.relativePosition.Y > 0.5f)
					element.Top.Set(-15, 0f);
				else
					element.Top.Set(-15, 1f);
			}
			else
			{
				if (toolbar.relativePosition.X < 0.5f)
					element.Left.Set(-15, 1f);
				else
					element.Left.Set(-15, 0f);

				element.Top.Set(-15, offset);
			}

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

				if (!beingDragged) // I dont know why this causes errors, but it does sometimes...
					base.Update(gameTime);
			}
			else //This is here to prevent invisible toolbars from interfering with mouse interactions, since terraria is terrible about that
			{
				Left.Set(0, -1);
				Top.Set(0, -1);
			}
		}

		public void Customize()
		{
			AddTabButton(new AddButton(this), 0.2f);
			AddTabButton(new RemoveToolbarButton(this), 0.4f);
			AddTabButton(new DragButton(this), 0.6f);
			AddTabButton(new HideOptionButton(this), 0.8f);

			foreach (UIElement child in Children)
			{
				if (child is ToolButton)
					(child as ToolButton).Customize();
			}
		}

		public void FinishCustomize()
		{
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
				//base.Draw(spriteBatch);
				FirstTimeSetupSystem.PanicToSetup();
				return;
			}

			if (!toolbar.Invisible)
			{
				var bgTarget = GetDimensions().ToRectangle();
				bgTarget.Inflate(-15, -15);

				Helpers.GUIHelper.DrawBox(spriteBatch, bgTarget, ModContent.GetInstance<GUIConfig>().backgroundColor);

				base.Draw(spriteBatch);
				Recalculate();
			}
		}
	}

	internal class ToolButton : UIElement
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

		public override void Click(UIMouseEvent evt)
		{
			if (!PermissionHandler.CanUseTools(Main.LocalPlayer))
			{
				Main.NewText("You are not an admin!", Color.Red);
				return;
			}

			if (!parent.toolbar.collapsed)
				tool.OnActivate();
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (!PermissionHandler.CanUseTools(Main.LocalPlayer))
			{
				Main.NewText("You are not an admin!", Color.Red);
				return;
			}

			if (!parent.toolbar.collapsed && tool.HasRightClick)
				tool.OnRightClick();
		}

		public void Customize()
		{
			Append(new RemoveButton(this));
		}

		public void FinishCustomize()
		{
			RemoveAllChildren();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (parent.toolbar.collapsed && parent.toolbar.CollapseDirection == CollapseDirection.Floating)
				return;

			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

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

	internal class HideTab : UIElement
	{
		public ToolbarElement parent;

		public Toolbar Toolbar => parent.toolbar;

		public HideTab(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void Click(UIMouseEvent evt)
		{
			if (CustomizeTool.customizing)
				return;

			if (Toolbar.CollapseDirection != CollapseDirection.Floating)
				Toolbar.collapsed = !Toolbar.collapsed;

			parent.UpdateTargetOffset();
			parent.Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (CustomizeTool.customizing)
				return;

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/TabWide").Value;

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, ModContent.GetInstance<GUIConfig>().buttonColor, rotation, new Vector2(45, 75), 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}
}
