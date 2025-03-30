using System;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class StringEditor : FieldEditor<string>
	{
		public TextField entry;
		public bool typing;

		public override bool Editing => typing;

		public StringEditor(string name, Action<string> onValueChanged, string initialValue, Func<string> listenForUpdates = null, string description = "") : base(70, name, onValueChanged, listenForUpdates, initialValue, description)
		{
			entry = new(InputType.text);
			entry.Left.Set(10, 0);
			entry.Top.Set(32, 0);
			entry.currentValue = initialValue ?? "";
			Append(entry);
		}

		public override void OnRecieveNewValue(string newValue)
		{
			entry.currentValue = newValue;
		}

		public override void EditorUpdate(GameTime gameTime)
		{
			if (entry.typing)
				typing = true;

			if (typing && !entry.typing)
			{
				onValueChanged(entry.currentValue);
				typing = false;
			}
		}
	}
}