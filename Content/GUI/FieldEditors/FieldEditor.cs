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
	internal abstract class FieldEditor : UIElement
	{
		public readonly string name;
		public object value;

		protected readonly Action<object> onValueChanged;

		public FieldEditor(int height, string name, Action<object> onValueChanged, object initialValue = null)
		{
			Width.Set(150, 0);
			Height.Set(height, 0);
			this.name = name;
			this.onValueChanged = onValueChanged;
			value = initialValue;
		}

		public sealed override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = GetDimensions().ToRectangle();
			backTarget.Height = 24;
			backTarget.Offset(new Point(4, 4));
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, name, GetDimensions().Position() + new Vector2(8, 8), Color.White, 0.8f);

			base.Draw(spriteBatch);

			SafeDraw(spriteBatch);
		}

		public virtual void SafeDraw(SpriteBatch sprite) { }
	}
}
