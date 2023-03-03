using Microsoft.Xna.Framework;
using System;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class StringEditor : FieldEditor<string>
	{
		public TextField entry;

		public StringEditor(string name, Action<string> onValueChanged, string initialValue, string description = "") : base(70, name, onValueChanged, initialValue, description)
		{
			entry = new(InputType.text);
			entry.Left.Set(10, 0);
			entry.Top.Set(32, 0);
			entry.currentValue = initialValue.ToString();
			Append(entry);
		}

		public override void Update(GameTime gameTime)
		{
			if (entry.updated)
				onValueChanged(entry.currentValue);

			base.Update(gameTime);
		}
	}
}
