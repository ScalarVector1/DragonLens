using System;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class Vector2Editor : FieldEditor<Vector2>
	{
		public TextField xEntry;
		public TextField yEntry;

		public override bool Editing => xEntry.typing || yEntry.typing;

		public Vector2Editor(string name, Action<Vector2> onValueChanged, Vector2 initialValue, Func<Vector2> listenForUpdate = null, string description = "") : base(94, name, onValueChanged, listenForUpdate, initialValue, description)
		{
			xEntry = new(InputType.number);
			xEntry.Left.Set(30, 0);
			xEntry.Top.Set(32, 0);
			xEntry.Width.Set(110, 0);
			xEntry.currentValue = initialValue.X.ToString();
			Append(xEntry);

			yEntry = new(InputType.number);
			yEntry.Left.Set(30, 0);
			yEntry.Top.Set(60, 0);
			yEntry.Width.Set(110, 0);
			yEntry.currentValue = initialValue.Y.ToString();
			Append(yEntry);
		}

		public override void OnRecieveNewValue(Vector2 newValue)
		{
			xEntry.currentValue = newValue.X.ToString();
			yEntry.currentValue = newValue.Y.ToString();
		}

		public override void EditorUpdate(GameTime gameTime)
		{
			if (xEntry.updated || yEntry.updated)
			{
				bool xValid = float.TryParse(xEntry.currentValue, out float x);
				bool yValid = float.TryParse(yEntry.currentValue, out float y);
				onValueChanged(new Vector2(x, y));
				value = new Vector2(x, y);
			}
		}

		public override void SafeDraw(SpriteBatch sprite)
		{
			Utils.DrawBorderString(sprite, "X", xEntry.GetDimensions().ToRectangle().TopLeft() + new Vector2(-16, 4), Color.White, 0.8f);
			Utils.DrawBorderString(sprite, "Y", yEntry.GetDimensions().ToRectangle().TopLeft() + new Vector2(-16, 4), Color.White, 0.8f);
		}
	}
}