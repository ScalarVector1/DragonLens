using Terraria.ModLoader.Config;

namespace DragonLens.Configs
{
	public class ToolConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Godmode default")]
		[Tooltip("If godmode should be turned on by default at startup")]
		public bool defaultGodmode;

		[Label("Preload spawners")]
		[Tooltip("Load entries for spawners at mod load instead of when used. Prevents in-game freezes but may increase load time.")]
		public bool preloadSpawners;
	}
}