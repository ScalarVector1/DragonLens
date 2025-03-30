using System;
using Terraria.DataStructures;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class Point16Editor : FieldEditor<Point16>
	{
		public TextField xEntry;
		public TextField yEntry;

		public bool typing;

		public override bool Editing => typing;

		public Point16Editor(string name, Action<Point16> onValueChanged, Point16 initialValue, Func<Point16> listenForUpdate = null, string description = "") : base(94, name, onValueChanged, listenForUpdate, initialValue, description)
		{
			xEntry = new(InputType.integer);
			xEntry.Left.Set(30, 0);
			xEntry.Top.Set(32, 0);
			xEntry.Width.Set(110, 0);
			xEntry.currentValue = initialValue.X.ToString();
			Append(xEntry);

			yEntry = new(InputType.integer);
			yEntry.Left.Set(30, 0);
			yEntry.Top.Set(60, 0);
			yEntry.Width.Set(110, 0);
			yEntry.currentValue = initialValue.Y.ToString();
			Append(yEntry);
		}

		public override void OnRecieveNewValue(Point16 newValue)
		{
			if (!typing)
			{
				xEntry.currentValue = newValue.X.ToString();
				yEntry.currentValue = newValue.Y.ToString();
			}
		}

		public override void EditorUpdate(GameTime gameTime)
		{
			if (xEntry.typing || yEntry.typing)
				typing = true;

			if (typing && !xEntry.typing && !yEntry.typing)
			{
				bool xValid = int.TryParse(xEntry.currentValue, out int x);
				bool yValid = int.TryParse(yEntry.currentValue, out int y);
				onValueChanged(new Point16(x, y));
				value = new Point16(x, y);

				typing = false;
			}
		}

		public override void SafeDraw(SpriteBatch sprite)
		{
			Utils.DrawBorderString(sprite, "X", xEntry.GetDimensions().ToRectangle().TopLeft() + new Vector2(-16, 4), Color.White, 0.8f);
			Utils.DrawBorderString(sprite, "Y", yEntry.GetDimensions().ToRectangle().TopLeft() + new Vector2(-16, 4), Color.White, 0.8f);
		}
	}
}