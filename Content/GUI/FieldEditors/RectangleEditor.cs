using System;
using Terraria.DataStructures;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class RectangleEditor : FieldEditor<Rectangle>
	{
		public TextField xEntry;
		public TextField yEntry;
		public TextField wEntry;
		public TextField hEntry;

		public bool typing;

		public override bool Editing => typing;

		public RectangleEditor(string name, Action<Rectangle> onValueChanged, Rectangle initialValue, Func<Rectangle> listenForUpdate = null, string description = "") : base(150, name, onValueChanged, listenForUpdate, initialValue, description)
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

			wEntry = new(InputType.integer);
			wEntry.Left.Set(30, 0);
			wEntry.Top.Set(88, 0);
			wEntry.Width.Set(110, 0);
			wEntry.currentValue = initialValue.Width.ToString();
			Append(wEntry);

			hEntry = new(InputType.integer);
			hEntry.Left.Set(30, 0);
			hEntry.Top.Set(116, 0);
			hEntry.Width.Set(110, 0);
			hEntry.currentValue = initialValue.Height.ToString();
			Append(hEntry);
		}

		public override void OnRecieveNewValue(Rectangle newValue)
		{
			if (!typing)
			{
				xEntry.currentValue = newValue.X.ToString();
				yEntry.currentValue = newValue.Y.ToString();
				wEntry.currentValue= newValue.Width.ToString();
				hEntry.currentValue= newValue.Height.ToString();
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
				bool wValid = int.TryParse(wEntry.currentValue,out int w);
				bool hValid = int.TryParse(hEntry.currentValue,out int h);

				onValueChanged(new Rectangle(x, y, w, h));
				value = new Rectangle(x, y, w, h);

				typing = false;
			}
		}

		public override void SafeDraw(SpriteBatch sprite)
		{
			Utils.DrawBorderString(sprite, "X", xEntry.GetDimensions().ToRectangle().TopLeft() + new Vector2(-16, 4), Color.White, 0.8f);
			Utils.DrawBorderString(sprite, "Y", yEntry.GetDimensions().ToRectangle().TopLeft() + new Vector2(-16, 4), Color.White, 0.8f);
			Utils.DrawBorderString(sprite, "W", wEntry.GetDimensions().ToRectangle().TopLeft() + new Vector2(-16, 4), Color.White, 0.8f);
			Utils.DrawBorderString(sprite, "H", hEntry.GetDimensions().ToRectangle().TopLeft() + new Vector2(-16, 4), Color.White, 0.8f);
		}
	}
}