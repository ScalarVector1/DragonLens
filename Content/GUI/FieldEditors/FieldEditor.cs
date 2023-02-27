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
	/// <summary>
	/// A UI element for changing the value of 'something'. 
	/// </summary>
	internal abstract class FieldEditor : UIElement
	{
		/// <summary>
		/// The name that gets displated above the panel to the user
		/// </summary>
		public readonly string name;

		/// <summary>
		/// The current value this editor believes the field its tied to to have. This wont update in real time so be careful
		/// </summary>
		public object value;

		/// <summary>
		/// The callback that should happen when this editor thinks the value its tracking has changed. You'll likely need to cast the object parameter to the correct type.
		/// </summary>
		protected readonly Action<object> onValueChanged;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="height">Height of the panel</param>
		/// <param name="name">The name that gets displated above the panel to the user</param>
		/// <param name="onValueChanged">The callback that should happen when this editor thinks the value its tracking has changed. You'll likely need to cast the object parameter to the correct type.</param>
		/// <param name="initialValue">A hint for what the initial value of the field tracked by this editor is</param>
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
