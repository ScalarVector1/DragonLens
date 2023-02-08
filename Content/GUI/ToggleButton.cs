using DragonLens.Configs;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class ToggleButton : UIElement
	{
		public string iconTexture;
		public Func<bool> isOn;

		public ToggleButton(string iconTexture, Func<bool> isOn)
		{
			this.iconTexture = iconTexture;
			this.isOn = isOn;
			Width.Set(32, 0);
			Height.Set(32, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			spriteBatch.Draw(ModContent.Request<Texture2D>(iconTexture).Value, GetDimensions().Position(), Color.White);

			if (isOn())
				GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());
		}
	}
}
