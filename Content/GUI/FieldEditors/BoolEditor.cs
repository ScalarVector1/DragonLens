using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System;
using Terraria.UI;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class BoolEditor : FieldEditor<bool>
	{
		public BoolEditor(string name, Action<bool> onValueChanged, bool initialValue, Func<bool> listenForUpdate = null, string description = "") : base(70, name, onValueChanged, listenForUpdate, initialValue, description) { }

		public override void SafeClick(UIMouseEvent evt)
		{
			value = !value;
			onValueChanged(value);
		}

		public override void SafeDraw(SpriteBatch sprite)
		{
			Utils.DrawBorderString(sprite, $"{value}", GetDimensions().Position() + new Vector2(12, 38), Color.White, 0.8f);

			var box = GetDimensions().ToRectangle();
			box.Width = 40;
			box.Height = 15;
			box.Offset(new Point(95, 40));
			GUIHelper.DrawBox(sprite, box, ThemeHandler.BackgroundColor);

			if (value)
			{
				box.Width = 15;
				box.Offset(new Point(25, 0));
				GUIHelper.DrawBox(sprite, box, ThemeHandler.ButtonColor.InvertColor());
			}
			else
			{
				box.Width = 15;
				GUIHelper.DrawBox(sprite, box, ThemeHandler.ButtonColor);
			}
		}
	}
}