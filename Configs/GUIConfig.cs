using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DragonLens.Configs
{
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

		[Label("Browser button size")]
		[Tooltip("Modifies the size of browser buttons.\nlarger sizes may be easier to see but require more scrolling.")]
		[Range(36, 108)]
		[Slider]
		[DefaultValue(36)]
		public int browserButtonSize;
	}
}