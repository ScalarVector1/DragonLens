using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
				Width.Set(position.X + 26, 0);
				Height.Set(92, 0);
			}
			else
			{
				Height.Set(position.Y + 26, 0);
				Width.Set(92, 0);
			}

			Recalculate();
			AdjustDimensions();
			AddCollapseTab();

			Recalculate();

			basePos = GetDimensions().Position();
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
		/// Adds the collapse tab button
		/// </summary>
		private void AddCollapseTab()
		{
			var collapseButton = new HideTab(this);
			collapseButton.OnClick += (UIMouseEvent mouseEvent, UIElement element) => toolbar.collapsed = !toolbar.collapsed;

			AddTabButton(collapseButton, 0.5f);
		}

		private void AddTabButton(UIElement element, float offset)
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

			base.Update(gameTime);
		}

		public void Customize()
		{
			AddTabButton(new AddButton(this), 0.25f);
			AddTabButton(new DragButton(this), 0.75f);

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
			if (!parent.toolbar.collapsed)
				tool.OnActivate();
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
			tool.DrawIcon(spriteBatch, GetDimensions().Position() + Vector2.One * 7);

			if (IsMouseHovering)
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
			Toolbar.collapsed = !Toolbar.collapsed;

			switch (Toolbar.CollapseDirection)
			{
				case CollapseDirection.Left:
					parent.offsetTarget = new Vector2(Toolbar.collapsed ? -62 : 0, 0);
					break;

				case CollapseDirection.Right:
					parent.offsetTarget = new Vector2(Toolbar.collapsed ? 62 : 0, 0);
					break;

				case CollapseDirection.Up:
					parent.offsetTarget = new Vector2(0, Toolbar.collapsed ? -62 : 0);
					break;

				case CollapseDirection.Down:
					parent.offsetTarget = new Vector2(0, Toolbar.collapsed ? 62 : 0);
					break;

				case CollapseDirection.Floating:

					break;
			}

			parent.Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Tab").Value;

			float rotation;

			if (Toolbar.orientation == Orientation.Horizontal)
				rotation = Toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = Toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, ModContent.GetInstance<GUIConfig>().buttonColor, rotation, tex.Size() / 2f, 1, 0, 0);

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

				DraggedToolbar.relativePosition.X = MathHelper.Clamp(Main.MouseScreen.X / Main.screenWidth, 0, 1);
				DraggedToolbar.relativePosition.Y = MathHelper.Clamp(Main.MouseScreen.Y / Main.screenHeight, 0, 1);

				if (Main.MouseScreen.X / Main.screenWidth < 0.05f && DraggedToolbar.orientation == Orientation.Vertical)
					DraggedToolbar.relativePosition.X = 0;

				if (Main.MouseScreen.X / Main.screenWidth > 0.95f && DraggedToolbar.orientation == Orientation.Vertical)
					DraggedToolbar.relativePosition.X = 1;

				if (Main.MouseScreen.Y / Main.screenHeight < 0.05f && DraggedToolbar.orientation == Orientation.Horizontal)
					DraggedToolbar.relativePosition.Y = 0;

				if (Main.MouseScreen.Y / Main.screenHeight > 0.95f && DraggedToolbar.orientation == Orientation.Horizontal)
					DraggedToolbar.relativePosition.Y = 1;

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
}
