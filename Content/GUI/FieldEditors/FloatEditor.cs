using Microsoft.Xna.Framework;
using System;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class FloatEditor : FieldEditor<float>
	{
		public TextField entry;

		public FloatEditor(string name, Action<float> onValueChanged, float initialValue, string description = "") : base(70, name, onValueChanged, initialValue, description)
		{
			entry = new(InputType.number);
			entry.Left.Set(10, 0);
			entry.Top.Set(32, 0);
			entry.currentValue = initialValue.ToString();
			Append(entry);
		}

		public override void Update(GameTime gameTime)
		{
			if (entry.updated)
				onValueChanged(float.TryParse(entry.currentValue, out float value) ? value : 0);

			base.Update(gameTime);
		}
	}
}
