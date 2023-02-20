using Terraria.ModLoader.Config;

namespace DragonLens.Configs
{
	public class ToolConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Godmode default")]
		[Tooltip("If godmode should be turned on by default at startup")]
		public bool defaultGodmode;
	}
}