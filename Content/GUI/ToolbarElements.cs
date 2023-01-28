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

			Vector2 position = Vector2.One * 10;

			foreach (Tool tool in toolbar.toolList)
			{
				ToolButton button = new(tool, position, this);
				Append(button);

				if (toolbar.orientation == Orientation.Horizontal)
					position.X += 42;
				else
					position.Y += 42;
			}

			if (toolbar.orientation == Orientation.Horizontal)
			{
				Width.Set(position.X + 10, 0);
				Height.Set(72, 0);
			}
			else
			{
				Height.Set(position.Y + 10, 0);
				Width.Set(72, 0);
			}

			AddCollapseTab();
			Recalculate();
		}

		private void AddCollapseTab()
		{
			var collapseButton = new HideTab(this);
			collapseButton.Width.Set(30, 0);
			collapseButton.Height.Set(30, 0);

			if (toolbar.orientation == Orientation.Horizontal)
			{
				collapseButton.Left.Set(-15, 0.5f);

				if (toolbar.relativePosition.Y < 0.5f)
					collapseButton.Top.Set(15, 0f);
				else
					collapseButton.Top.Set(-15, 1f);
			}
			else
			{
				if (toolbar.relativePosition.X < 0.5f)
					collapseButton.Left.Set(-15, 1f);
				else
					collapseButton.Left.Set(15, 0f);

				collapseButton.Top.Set(-15, 0.5f);
			}

			collapseButton.OnClick += (UIMouseEvent mouseEvent, UIElement element) => toolbar.collapsed = !toolbar.collapsed;

			Append(collapseButton);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!toolbar.Invisible)
			{
				var bgTarget = GetDimensions().ToRectangle();

				if (toolbar.orientation == Orientation.Horizontal)
					bgTarget.Height -= 14;
				else
					bgTarget.Width -= 14;

				Helpers.GUIHelper.DrawBox(spriteBatch, bgTarget, new Color(20, 50, 80));

				base.Draw(spriteBatch);
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
			Width.Set(38, 0);
			Height.Set(38, 0);
			this.parent = parent;
		}

		public override void Click(UIMouseEvent evt)
		{
			if (!parent.toolbar.collapsed)
				tool.OnActivate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!parent.toolbar.collapsed)
			{
				Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle());
				tool.DrawIcon(spriteBatch, GetDimensions().Position() + Vector2.One * 3);

				if (IsMouseHovering)
					Utils.DrawBorderString(spriteBatch, tool.Name, Main.MouseScreen + Vector2.One * 16, Color.White);

				base.Draw(spriteBatch);
			}
		}
	}

	internal class HideTab : UIElement
	{
		public ToolbarElement parent;

		public HideTab(ToolbarElement parent)
		{
			this.parent = parent;
		}

		public override void Click(UIMouseEvent evt)
		{
			parent.toolbar.collapsed = !parent.toolbar.collapsed;

			if (parent.toolbar.orientation == Orientation.Horizontal)
				parent.Height.Set(parent.toolbar.collapsed ? 20 : 72, 0);
			else
				parent.Width.Set(parent.toolbar.collapsed ? 20 : 72, 0);

			parent.Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Tab").Value;

			float rotation;

			if (parent.toolbar.orientation == Orientation.Horizontal)
				rotation = parent.toolbar.relativePosition.Y > 0.5f ? 0 : 3.14f;
			else
				rotation = parent.toolbar.relativePosition.X > 0.5f ? 1.57f * 3 : 1.57f;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, rotation, tex.Size() / 2f, 1, 0, 0);

			base.Draw(spriteBatch);
		}
	}
}
