using DragonLens.Configs;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Helpers;
using System;

namespace DragonLens.Content.GUI
{
	/// <summary>
	/// A simple button used for on/off states
	/// </summary>
	internal class ToggleButton : SmartUIElement
	{
		/// <summary>
		/// The texture of the icon to draw on the button
		/// </summary>
		public string iconTexture;
		/// <summary>
		/// How the button should determine if it is 'on' or not. While on, it will draw a colored outline around itself.
		/// </summary>
		public Func<bool> isOn;
		/// <summary>
		/// What this button should say when hovered over
		/// </summary>
		public string tooltip;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="iconTexture">The texture of the icon to draw on the button</param>
		/// <param name="isOn">How the button should determine if it is 'on' or not. While on, it will draw a colored outline around itself.</param>
		/// <param name="tooltip">What this button should say when hovered over</param>
		public ToggleButton(string iconTexture, Func<bool> isOn, string tooltip = "")
		{
			this.iconTexture = iconTexture;
			this.isOn = isOn;
			Width.Set(32, 0);
			Height.Set(32, 0);
			this.tooltip = tooltip;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>(iconTexture).Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			if (isOn())
				GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

			if (IsMouseHovering && tooltip != "")
			{
				Tooltip.SetName(tooltip);
				Tooltip.SetTooltip(isOn() ? "On" : "Off");
			}
		}
	}
}
