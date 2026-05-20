using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;

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
		/// Custom sub tooltip getter if relevant
		/// </summary>
		public Func<string> getInfo;

		public Asset<Texture2D> iconAsset;
		public Func<Asset<Texture2D>> getIconTexture;
		public Func<string> getTooltip;
		public bool drawHighlight = true;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="iconTexture">The texture of the icon to draw on the button</param>
		/// <param name="isOn">How the button should determine if it is 'on' or not. While on, it will draw a colored outline around itself.</param>
		/// <param name="tooltip">What this button should say when hovered over</param>
		public ToggleButton(string iconTexture, Func<bool> isOn, string tooltip = "", Func<string> getInfo = null, bool drawHighlight = true)
		{
			this.iconTexture = iconTexture;
			this.iconAsset = ModContent.Request<Texture2D>(iconTexture);
			this.isOn = isOn;
			this.tooltip = tooltip;
			this.getInfo = getInfo;
			this.drawHighlight = drawHighlight;

			Width.Set(32, 0);
			Height.Set(32, 0);
		}

		public ToggleButton(Asset<Texture2D> iconTexture, Func<bool> isOn, string tooltip = "", Func<string> getInfo = null, bool drawHighlight = true)
		{
			this.iconAsset = iconTexture;
			this.isOn = isOn;
			this.tooltip = tooltip;
			this.getInfo = getInfo;
			this.drawHighlight = drawHighlight;

			Width.Set(32, 0);
			Height.Set(32, 0);
		}

		public ToggleButton(Func<Asset<Texture2D>> getIconTexture, Func<bool> isOn, Func<string> getTooltip, Func<string> getInfo = null, bool drawHighlight = true)
		{
			this.getIconTexture = getIconTexture;
			this.isOn = isOn;
			this.getTooltip = getTooltip;
			this.getInfo = getInfo;
			this.drawHighlight = drawHighlight;

			Width.Set(32, 0);
			Height.Set(32, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			Texture2D tex = (getIconTexture != null ? getIconTexture() : iconAsset).Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);

			if (drawHighlight && isOn())
				GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor.InvertColor());

#if DEBUG

			/*
			 * This debug block is:
			 * pretty good for debugging when having multiple states open and seeing when
			 * e.g filter toggle button goes out of focus
			 */
			//if (drawHighlight && isOn() && CanShowTooltip)
			//GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor.InvertColor());
#endif

			string hoverName = getTooltip?.Invoke() ?? tooltip;
			if (IsMouseHovering && hoverName != "")
			{
				Tooltip.SetName(hoverName);
				Tooltip.SetTooltip(getInfo?.Invoke() ?? LocalizationHelper.GetGUIText($"ToggleButton.{(isOn() ? "On" : "Off")}"));
			}
		}
	}
}