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
		[Label("$Mods.DragonLens.BoxStyle.simple")] simple,
		[Label("$Mods.DragonLens.BoxStyle.vanilla")] vanilla
	}

	public enum IconStyle
	{
		[Label("$Mods.DragonLens.IconStyle.basic")] basic,
		[Label("$Mods.DragonLens.IconStyle.HEROs")] HEROs
	}

	public class GUIConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("$Mods.DragonLens.GUIConfig.backgroundColor.Label")]
		[Tooltip("$Mods.DragonLens.GUIConfig.backgroundColor.Tooltip")]
		[DefaultValue("25, 35, 100, 200")]
		public Color backgroundColor;

		[Label("$Mods.DragonLens.GUIConfig.buttonColor.Label")]
		[Tooltip("$Mods.DragonLens.GUIConfig.buttonColor.Tooltip")]
		[DefaultValue("45, 55, 130, 200")]
		public Color buttonColor;

		//Placeholder for better theme menu later. Ignore this shitcode!
		[Label("$Mods.DragonLens.GUIConfig.boxStyle.Label")]
		[Tooltip("$Mods.DragonLens.GUIConfig.boxStyle.Tooltip")]
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

		[Label("$Mods.DragonLens.GUIConfig.iconStyle.Label")]
		[Tooltip("$Mods.DragonLens.GUIConfig.iconStyle.Tooltip")]
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
	}
}