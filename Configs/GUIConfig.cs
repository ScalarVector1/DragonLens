using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;

namespace DragonLens.Configs
{
	public class GUIConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Background color")]
		[Tooltip("The color of DragonLens' backgrounds")]
		public Color backgroundColor;

		[Label("Button color")]
		[Tooltip("The color of DragonLens' buttons")]
		public Color buttonColor;
	}
}