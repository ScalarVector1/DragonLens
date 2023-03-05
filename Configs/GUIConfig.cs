using DragonLens.Content.Themes.BoxProviders;
using DragonLens.Content.Themes.IconProviders;
using DragonLens.Core.Systems.ThemeSystem;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;

namespace DragonLens.Configs
{
	public enum BoxStyle
	{
		simple,
		vanilla
	}

	public enum IconStyle
	{
		basic,
		HEROs
	}

	public class GUIConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Background color")]
		[Tooltip("The color of DragonLens' backgrounds")]
		[DefaultValue("25, 35, 100, 200")]
		public Color backgroundColor;

		[Label("Button color")]
		[Tooltip("The color of DragonLens' buttons")]
		[DefaultValue("45, 55, 130, 200")]
		public Color buttonColor;

		//Placeholder for better theme menu later. Ignore this shitcode!
		[Label("Box style")]
		[Tooltip("The style of DragonLens' boxes")]
		public BoxStyle boxStyle
		{
			get => ThemeHandler.currentBoxProvider is SimpleBoxes ? BoxStyle.simple : BoxStyle.vanilla;
			set
			{
				if (Main.gameMenu)
					return;
				else
					ThemeHandler.currentBoxProvider = value == BoxStyle.simple ? new SimpleBoxes() : new VanillaBoxes();
			}
		}

		[Label("Icon pack")]
		[Tooltip("The icon pack to use for tools")]
		public IconStyle iconStyle
		{
			get => ThemeHandler.currentIconProvider is DefaultIcons ? IconStyle.basic : IconStyle.HEROs;
			set
			{
				if (Main.gameMenu)
					return;
				else
					ThemeHandler.currentIconProvider = value == IconStyle.basic ? new DefaultIcons() : new HEROsIcons();
			}
		}

		[Label("Browser button size")]
		[Tooltip("Modifies the size of browser buttons.\nlarger sizes may be easier to see but require more scrolling.")]
		[Range(36, 108)]
		[Slider]
		[DefaultValue(36)]
		public int browserButtonSize;
	}
}