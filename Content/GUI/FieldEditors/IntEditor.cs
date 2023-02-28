using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class IntEditor : FieldEditor<int>
	{
		public TextField entry;

		public IntEditor(string name, Action<int> onValueChanged, int initialValue) : base(70, name, onValueChanged, initialValue)
		{
			entry = new(InputType.integer);
			entry.Left.Set(10, 0);
			entry.Top.Set(32, 0);
			entry.currentValue = initialValue.ToString();
			Append(entry);
		}

		public override void Update(GameTime gameTime)
		{
			if (entry.updated)
				onValueChanged(int.TryParse(entry.currentValue, out int value) ? value : 0);

			base.Update(gameTime);
		}

		public override void SafeDraw(SpriteBatch sprite)
		{
			base.SafeDraw(sprite);
		}
	}
}
