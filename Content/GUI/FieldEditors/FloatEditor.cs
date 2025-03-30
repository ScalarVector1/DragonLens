using System;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class FloatEditor : FieldEditor<float>
	{
		public TextField entry;
		public bool typing;

		public override bool Editing => typing;

		public FloatEditor(string name, Action<float> onValueChanged, float initialValue, Func<float> listenForUpdates = null, string description = "") : base(70, name, onValueChanged, listenForUpdates, initialValue, description)
		{
			entry = new(InputType.number);
			entry.Left.Set(10, 0);
			entry.Top.Set(32, 0);
			entry.currentValue = initialValue.ToString();
			Append(entry);
		}

		public override void OnRecieveNewValue(float newValue)
		{
			entry.currentValue = newValue.ToString();
		}

		public override void EditorUpdate(GameTime gameTime)
		{
			if (entry.typing)
				typing = true;

			if (typing && !entry.typing)
			{
				onValueChanged(float.TryParse(entry.currentValue, out float value) ? value : 0);
				typing = false;
			}
		}
	}

	internal class DoubleEditor : FieldEditor<double>
	{
		public TextField entry;
		public bool typing;

		public override bool Editing => typing;

		public DoubleEditor(string name, Action<double> onValueChanged, double initialValue, Func<double> listenForUpdates = null, string description = "") : base(70, name, onValueChanged, listenForUpdates, initialValue, description)
		{
			entry = new(InputType.number);
			entry.Left.Set(10, 0);
			entry.Top.Set(32, 0);
			entry.currentValue = initialValue.ToString();
			Append(entry);
		}

		public override void OnRecieveNewValue(double newValue)
		{
			entry.currentValue = newValue.ToString();
		}

		public override void EditorUpdate(GameTime gameTime)
		{
			if (entry.typing)
				typing = true;

			if (typing && !entry.typing)
			{
				onValueChanged(double.TryParse(entry.currentValue, out double value) ? value : 0);
				typing = false;
			}
		}
	}
}