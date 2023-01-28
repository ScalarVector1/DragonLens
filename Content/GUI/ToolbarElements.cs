using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
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

			foreach (Tool tool in toolbar.tools)
			{
				ToolButton button = new(tool, position);
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
			var collapseButton = new UIImageButton(ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Tab"));
			collapseButton.Width.Set(30, 0);
			collapseButton.Height.Set(30, 0);

			if (toolbar.orientation == Orientation.Horizontal)
			{
				collapseButton.Left.Set(0, 0.5f);
				collapseButton.Top.Set(-30, 1f);
			}
			else
			{
				if (toolbar.relativePosition.X < 0.5f)
					collapseButton.Left.Set(-30, 1f);
				else
					collapseButton.Left.Set(30, 0f);

				collapseButton.Top.Set(0, 0.5f);
			}

			collapseButton.OnClick += (UIMouseEvent mouseEvent, UIElement element) => toolbar.hidden = !toolbar.hidden;

			Append(collapseButton);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), new Color(20, 50, 80));
			base.Draw(spriteBatch);
		}
	}

	internal class ToolButton : UIElement
	{
		public Tool tool;

		public ToolButton(Tool tool, Vector2 pos)
		{
			this.tool = tool;

			Left.Set(pos.X, 0);
			Top.Set(pos.Y, 0);
			Width.Set(38, 0);
			Height.Set(38, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			tool.OnActivate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle());
			tool.DrawIcon(spriteBatch, GetDimensions().Position() + Vector2.One * 3);

			if (IsMouseHovering)
				Utils.DrawBorderString(spriteBatch, tool.Name, Main.MouseScreen + Vector2.One * 16, Color.White);

			base.Draw(spriteBatch);
		}
	}
}
