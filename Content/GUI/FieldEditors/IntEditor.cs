using Microsoft.Xna.Framework;
using System;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class IntEditor : FieldEditor<int>
	{
		public TextField entry;

		public override bool Editing => entry.typing;

		public IntEditor(string name, Action<int> onValueChanged, int initialValue, Func<int> listenForUpdate = null, string description = "") : base(70, name, onValueChanged, listenForUpdate, initialValue, description)
		{
			entry = new(InputType.integer);
			entry.Left.Set(10, 0);
			entry.Top.Set(32, 0);
			entry.currentValue = initialValue.ToString();
			Append(entry);
		}

		public override void OnRecieveNewValue(int newValue)
		{
			entry.currentValue = newValue.ToString();
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (entry.updated)
				onValueChanged(int.TryParse(entry.currentValue, out int value) ? value : 0);
		}
	}
}
