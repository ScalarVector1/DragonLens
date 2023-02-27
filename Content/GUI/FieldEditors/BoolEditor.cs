using DragonLens.Configs;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class BoolEditor : FieldEditor
	{
		public TextField entry;

		public BoolEditor(string name, Action<object> onValueChanged, bool initialValue) : base(70, name, onValueChanged, initialValue) { }

		public override void Click(UIMouseEvent evt)
		{
			value = !(bool)value;
			onValueChanged((bool)value);
		}

		public override void SafeDraw(SpriteBatch sprite)
		{
			Utils.DrawBorderString(sprite, $"{(bool)value}", GetDimensions().Position() + new Vector2(12, 38), Color.White, 0.8f);

			var box = GetDimensions().ToRectangle();
			box.Width = 40;
			box.Height = 15;
			box.Offset(new Point(95, 40));
			GUIHelper.DrawBox(sprite, box, ModContent.GetInstance<GUIConfig>().backgroundColor);

			if (value is bool && (bool)value)
			{
				box.Width = 15;
				box.Offset(new Point(25, 0));
				GUIHelper.DrawBox(sprite, box, ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());
			}
			else
			{
				box.Width = 15;
				GUIHelper.DrawBox(sprite, box, ModContent.GetInstance<GUIConfig>().buttonColor);
			}
		}
	}
}
